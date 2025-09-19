using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using CustomerService.Domain.Entities;
using CustomerService.Domain.Repositories;
using CustomerService.Domain.Events;
using CustomerService.Application.DTOs.SegmentInteraction;
using CustomerService.Application.Commands.SegmentInteractionPreference;
using CustomerService.Domain.ValueObjects;
using CustomerService.Domain.Enums;

namespace CustomerService.Application.Handlers.Commands
{
    // ========================================================================================
    // SEGMENT COMMAND HANDLERS - GESTION SOPHISTIQUÉE DE LA SEGMENTATION CLIENT
    // ========================================================================================

    /// <summary>
    /// Handler pour la création de segments clients avec critères avancés
    /// </summary>
    public class CreateCustomerSegmentCommandHandler : IRequestHandler<CreateCustomerSegmentCommand, CustomerSegmentDto>
    {
        private readonly ICustomerSegmentRepository _segmentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateCustomerSegmentCommandHandler> _logger;
        private readonly IValidator<CreateCustomerSegmentCommand> _validator;

        public CreateCustomerSegmentCommandHandler(
            ICustomerSegmentRepository segmentRepository,
            IMapper mapper,
            ILogger<CreateCustomerSegmentCommandHandler> logger,
            IValidator<CreateCustomerSegmentCommand> validator)
        {
            _segmentRepository = segmentRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<CustomerSegmentDto> Handle(CreateCustomerSegmentCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Création d'un segment client : {Name}", request.Name);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour la création de segment : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Vérification de l'unicité du nom
            var existingSegment = await _segmentRepository.GetByNameAsync(request.Name);
            if (existingSegment != null)
            {
                _logger.LogError("Un segment avec le nom '{Name}' existe déjà", request.Name);
                throw new InvalidOperationException($"Un segment avec le nom '{request.Name}' existe déjà");
            }

            // Création du segment
            var segment = CustomerSegment.Create(
                request.Name,
                request.Description,
                request.Criteria,
                request.IsActive,
                request.Priority,
                request.AutoAssign,
                request.Tags?.ToList() ?? new List<string>());

            // Sauvegarde
            await _segmentRepository.AddAsync(segment);

            _logger.LogInformation("Segment client créé avec succès : {SegmentId}", segment.Id);

            return _mapper.Map<CustomerSegmentDto>(segment);
        }
    }

    /// <summary>
    /// Handler pour l'assignation manuelle de clients à un segment
    /// </summary>
    public class AssignCustomersToSegmentCommandHandler : IRequestHandler<AssignCustomersToSegmentCommand, bool>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerSegmentRepository _segmentRepository;
        private readonly ILogger<AssignCustomersToSegmentCommandHandler> _logger;
        private readonly IValidator<AssignCustomersToSegmentCommand> _validator;
        private readonly IMediator _mediator;

        public AssignCustomersToSegmentCommandHandler(
            ICustomerRepository customerRepository,
            ICustomerSegmentRepository segmentRepository,
            ILogger<AssignCustomersToSegmentCommandHandler> logger,
            IValidator<AssignCustomersToSegmentCommand> validator,
            IMediator mediator)
        {
            _customerRepository = customerRepository;
            _segmentRepository = segmentRepository;
            _logger = logger;
            _validator = validator;
            _mediator = mediator;
        }

        public async Task<bool> Handle(AssignCustomersToSegmentCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Assignation de {Count} clients au segment {SegmentId}", 
                request.CustomerIds.Count, request.SegmentId);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour l'assignation de segment : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Récupération du segment
            var segment = await _segmentRepository.GetByIdAsync(request.SegmentId);
            if (segment == null)
            {
                _logger.LogError("Segment non trouvé : {SegmentId}", request.SegmentId);
                throw new ArgumentException($"Segment non trouvé : {request.SegmentId}");
            }

            var successCount = 0;
            var failureCount = 0;

            // Assignation pour chaque client
            foreach (var customerId in request.CustomerIds)
            {
                try
                {
                    var customer = await _customerRepository.GetByIdAsync(customerId);
                    if (customer == null)
                    {
                        _logger.LogWarning("Client non trouvé lors de l'assignation : {CustomerId}", customerId);
                        failureCount++;
                        continue;
                    }

                    // Assignation au segment
                    customer.AssignToSegment(segment.Id, segment.Name, request.Reason);
                    await _customerRepository.UpdateAsync(customer);

                    // Publication de l'événement
                    await _mediator.Publish(new CustomerAssignedToSegmentEvent(
                        customerId,
                        segment.Id,
                        segment.Name,
                        request.Reason,
                        DateTime.UtcNow), cancellationToken);

                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de l'assignation du client {CustomerId} au segment {SegmentId}", 
                        customerId, request.SegmentId);
                    failureCount++;
                }
            }

            // Mise à jour des statistiques du segment
            segment.UpdateStatistics(successCount, 0); // +successCount clients assignés
            await _segmentRepository.UpdateAsync(segment);

            _logger.LogInformation("Assignation terminée - Succès: {Success}, Échecs: {Failures}", 
                successCount, failureCount);

            return failureCount == 0;
        }
    }

    /// <summary>
    /// Handler pour la mise à jour automatique des segments basée sur les critères
    /// </summary>
    public class RefreshSegmentMembershipsCommandHandler : IRequestHandler<RefreshSegmentMembershipsCommand, SegmentRefreshResultDto>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerSegmentRepository _segmentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<RefreshSegmentMembershipsCommandHandler> _logger;
        private readonly IValidator<RefreshSegmentMembershipsCommand> _validator;
        private readonly IMediator _mediator;

        public RefreshSegmentMembershipsCommandHandler(
            ICustomerRepository customerRepository,
            ICustomerSegmentRepository segmentRepository,
            IMapper mapper,
            ILogger<RefreshSegmentMembershipsCommandHandler> logger,
            IValidator<RefreshSegmentMembershipsCommand> validator,
            IMediator mediator)
        {
            _customerRepository = customerRepository;
            _segmentRepository = segmentRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
            _mediator = mediator;
        }

        public async Task<SegmentRefreshResultDto> Handle(RefreshSegmentMembershipsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Rafraîchissement des segments démarré - Segment spécifique : {SegmentId}", 
                request.SegmentId);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour le rafraîchissement de segments : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            var result = new SegmentRefreshResultDto
            {
                StartTime = DateTime.UtcNow,
                SegmentResults = new List<SegmentRefreshDetailDto>()
            };

            // Récupération des segments à traiter
            var segments = request.SegmentId.HasValue
                ? new[] { await _segmentRepository.GetByIdAsync(request.SegmentId.Value) }.Where(s => s != null)
                : await _segmentRepository.GetActiveSegmentsAsync();

            foreach (var segment in segments)
            {
                var segmentResult = new SegmentRefreshDetailDto
                {
                    SegmentId = segment.Id,
                    SegmentName = segment.Name,
                    StartTime = DateTime.UtcNow
                };

                try
                {
                    // Récupération de tous les clients
                    var allCustomers = await _customerRepository.GetAllActiveAsync();
                    
                    var customersToAdd = new List<Guid>();
                    var customersToRemove = new List<Guid>();

                    foreach (var customer in allCustomers)
                    {
                        var meetsaCriteria = segment.EvaluateCriteria(customer);
                        var currentlyInSegment = customer.IsInSegment(segment.Id);

                        if (meetsaCriteria && !currentlyInSegment)
                        {
                            customersToAdd.Add(customer.Id);
                        }
                        else if (!meetsaCriteria && currentlyInSegment)
                        {
                            customersToRemove.Add(customer.Id);
                        }
                    }

                    // Ajout des nouveaux clients
                    foreach (var customerId in customersToAdd)
                    {
                        var customer = await _customerRepository.GetByIdAsync(customerId);
                        customer.AssignToSegment(segment.Id, segment.Name, "Assignation automatique - critères remplis");
                        await _customerRepository.UpdateAsync(customer);

                        await _mediator.Publish(new CustomerAssignedToSegmentEvent(
                            customerId,
                            segment.Id,
                            segment.Name,
                            "Assignation automatique - critères remplis",
                            DateTime.UtcNow), cancellationToken);
                    }

                    // Suppression des clients qui ne correspondent plus
                    foreach (var customerId in customersToRemove)
                    {
                        var customer = await _customerRepository.GetByIdAsync(customerId);
                        customer.RemoveFromSegment(segment.Id, "Suppression automatique - critères non remplis");
                        await _customerRepository.UpdateAsync(customer);

                        await _mediator.Publish(new CustomerRemovedFromSegmentEvent(
                            customerId,
                            segment.Id,
                            segment.Name,
                            "Suppression automatique - critères non remplis",
                            DateTime.UtcNow), cancellationToken);
                    }

                    // Mise à jour des statistiques du segment
                    var totalCurrentMembers = await _customerRepository.GetSegmentMemberCountAsync(segment.Id);
                    segment.UpdateStatistics(customersToAdd.Count, customersToRemove.Count);
                    segment.MarkAsRefreshed();
                    await _segmentRepository.UpdateAsync(segment);

                    segmentResult.CustomersAdded = customersToAdd.Count;
                    segmentResult.CustomersRemoved = customersToRemove.Count;
                    segmentResult.CurrentMemberCount = totalCurrentMembers;
                    segmentResult.Success = true;
                    segmentResult.EndTime = DateTime.UtcNow;

                    _logger.LogInformation("Segment {SegmentId} rafraîchi - Ajoutés: {Added}, Supprimés: {Removed}, Total: {Total}", 
                        segment.Id, customersToAdd.Count, customersToRemove.Count, totalCurrentMembers);
                }
                catch (Exception ex)
                {
                    segmentResult.Success = false;
                    segmentResult.ErrorMessage = ex.Message;
                    segmentResult.EndTime = DateTime.UtcNow;

                    _logger.LogError(ex, "Erreur lors du rafraîchissement du segment {SegmentId}", segment.Id);
                }

                result.SegmentResults.Add(segmentResult);
            }

            result.EndTime = DateTime.UtcNow;
            result.TotalSegmentsProcessed = result.SegmentResults.Count;
            result.SuccessfulSegments = result.SegmentResults.Count(r => r.Success);
            result.FailedSegments = result.SegmentResults.Count(r => !r.Success);

            _logger.LogInformation("Rafraîchissement des segments terminé - Traités: {Total}, Succès: {Success}, Échecs: {Failed}", 
                result.TotalSegmentsProcessed, result.SuccessfulSegments, result.FailedSegments);

            return result;
        }
    }

    // ========================================================================================
    // INTERACTION COMMAND HANDLERS - GESTION DES INTERACTIONS CLIENT
    // ========================================================================================

    /// <summary>
    /// Handler pour l'enregistrement d'interactions client
    /// </summary>
    public class RecordCustomerInteractionCommandHandler : IRequestHandler<RecordCustomerInteractionCommand, CustomerInteractionDto>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerInteractionRepository _interactionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<RecordCustomerInteractionCommandHandler> _logger;
        private readonly IValidator<RecordCustomerInteractionCommand> _validator;
        private readonly IMediator _mediator;

        public RecordCustomerInteractionCommandHandler(
            ICustomerRepository customerRepository,
            ICustomerInteractionRepository interactionRepository,
            IMapper mapper,
            ILogger<RecordCustomerInteractionCommandHandler> logger,
            IValidator<RecordCustomerInteractionCommand> validator,
            IMediator mediator)
        {
            _customerRepository = customerRepository;
            _interactionRepository = interactionRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
            _mediator = mediator;
        }

        public async Task<CustomerInteractionDto> Handle(RecordCustomerInteractionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Enregistrement d'interaction pour le client {CustomerId}, type {InteractionType}", 
                request.CustomerId, request.InteractionType);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour l'enregistrement d'interaction : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Récupération du client
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
            {
                _logger.LogError("Client non trouvé : {CustomerId}", request.CustomerId);
                throw new ArgumentException($"Client non trouvé : {request.CustomerId}");
            }

            // Création de l'interaction
            var interaction = CustomerInteraction.Create(
                request.CustomerId,
                request.InteractionType,
                request.Channel,
                request.Subject,
                request.Description,
                request.Outcome,
                request.Duration,
                request.Rating,
                request.Tags?.ToList() ?? new List<string>(),
                request.Metadata?.ToDictionary(kv => kv.Key, kv => kv.Value) ?? new Dictionary<string, object>(),
                request.FollowUpRequired,
                request.FollowUpDate,
                request.AgentId,
                request.AgentName);

            // Sauvegarde
            await _interactionRepository.AddAsync(interaction);

            // Mise à jour des statistiques client
            customer.RecordInteraction(interaction);
            await _customerRepository.UpdateAsync(customer);

            // Publication de l'événement
            await _mediator.Publish(new CustomerInteractionRecordedEvent(
                customer.Id,
                interaction.Id,
                request.InteractionType,
                request.Channel,
                request.Subject,
                request.Outcome,
                request.Duration,
                request.Rating,
                DateTime.UtcNow), cancellationToken);

            _logger.LogInformation("Interaction enregistrée avec succès : {InteractionId}", interaction.Id);

            return _mapper.Map<CustomerInteractionDto>(interaction);
        }
    }

    /// <summary>
    /// Handler pour la mise à jour d'interactions (notamment suivi et résolution)
    /// </summary>
    public class UpdateInteractionStatusCommandHandler : IRequestHandler<UpdateInteractionStatusCommand, CustomerInteractionDto>
    {
        private readonly ICustomerInteractionRepository _interactionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateInteractionStatusCommandHandler> _logger;
        private readonly IValidator<UpdateInteractionStatusCommand> _validator;
        private readonly IMediator _mediator;

        public UpdateInteractionStatusCommandHandler(
            ICustomerInteractionRepository interactionRepository,
            IMapper mapper,
            ILogger<UpdateInteractionStatusCommandHandler> logger,
            IValidator<UpdateInteractionStatusCommand> validator,
            IMediator mediator)
        {
            _interactionRepository = interactionRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
            _mediator = mediator;
        }

        public async Task<CustomerInteractionDto> Handle(UpdateInteractionStatusCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Mise à jour du statut de l'interaction {InteractionId}", request.InteractionId);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour la mise à jour d'interaction : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Récupération de l'interaction
            var interaction = await _interactionRepository.GetByIdAsync(request.InteractionId);
            if (interaction == null)
            {
                _logger.LogError("Interaction non trouvée : {InteractionId}", request.InteractionId);
                throw new ArgumentException($"Interaction non trouvée : {request.InteractionId}");
            }

            // Mise à jour
            interaction.UpdateStatus(
                request.Outcome,
                request.Resolution,
                request.InternalNotes,
                request.FollowUpRequired,
                request.FollowUpDate,
                request.Rating);

            // Sauvegarde
            await _interactionRepository.UpdateAsync(interaction);

            _logger.LogInformation("Statut de l'interaction mis à jour avec succès : {InteractionId}", interaction.Id);

            return _mapper.Map<CustomerInteractionDto>(interaction);
        }
    }
}