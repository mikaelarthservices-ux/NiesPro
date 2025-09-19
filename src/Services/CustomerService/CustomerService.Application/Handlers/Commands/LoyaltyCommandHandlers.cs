using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using CustomerService.Domain.Entities;
using CustomerService.Domain.Repositories;
using CustomerService.Domain.Events;
using CustomerService.Application.DTOs.Loyalty;
using CustomerService.Application.Commands.Loyalty;
using CustomerService.Domain.ValueObjects;
using CustomerService.Domain.Enums;

namespace CustomerService.Application.Handlers.Commands
{
    // ========================================================================================
    // LOYALTY COMMAND HANDLERS - GESTION SOPHISTIQUÉE DES PROGRAMMES DE FIDÉLITÉ
    // ========================================================================================

    /// <summary>
    /// Handler pour l'attribution de points de fidélité avec calculs sophistiqués
    /// </summary>
    public class EarnLoyaltyPointsCommandHandler : IRequestHandler<EarnLoyaltyPointsCommand, LoyaltyTransactionDto>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILoyaltyProgramRepository _loyaltyProgramRepository;
        private readonly ILoyaltyRewardRepository _loyaltyRewardRepository;
        private readonly ICustomerAnalyticsRepository _analyticsRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<EarnLoyaltyPointsCommandHandler> _logger;
        private readonly IValidator<EarnLoyaltyPointsCommand> _validator;
        private readonly IMediator _mediator;

        public EarnLoyaltyPointsCommandHandler(
            ICustomerRepository customerRepository,
            ILoyaltyProgramRepository loyaltyProgramRepository,
            ILoyaltyRewardRepository loyaltyRewardRepository,
            ICustomerAnalyticsRepository analyticsRepository,
            IMapper mapper,
            ILogger<EarnLoyaltyPointsCommandHandler> logger,
            IValidator<EarnLoyaltyPointsCommand> validator,
            IMediator mediator)
        {
            _customerRepository = customerRepository;
            _loyaltyProgramRepository = loyaltyProgramRepository;
            _loyaltyRewardRepository = loyaltyRewardRepository;
            _analyticsRepository = analyticsRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
            _mediator = mediator;
        }

        public async Task<LoyaltyTransactionDto> Handle(EarnLoyaltyPointsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attribution de points de fidélité démarrée pour le client {CustomerId}, montant {Amount}, programme {ProgramId}", 
                request.CustomerId, request.Amount, request.LoyaltyProgramId);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour l'attribution de points : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Récupération du client
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
            {
                _logger.LogError("Client non trouvé : {CustomerId}", request.CustomerId);
                throw new ArgumentException($"Client non trouvé : {request.CustomerId}");
            }

            // Récupération du programme de fidélité
            var loyaltyProgram = await _loyaltyProgramRepository.GetByIdAsync(request.LoyaltyProgramId);
            if (loyaltyProgram == null || !loyaltyProgram.IsActive)
            {
                _logger.LogError("Programme de fidélité non trouvé ou inactif : {ProgramId}", request.LoyaltyProgramId);
                throw new ArgumentException($"Programme de fidélité non trouvé ou inactif : {request.LoyaltyProgramId}");
            }

            // Attribution des points avec calculs sophistiqués
            var transaction = customer.EarnLoyaltyPoints(
                loyaltyProgram,
                request.Amount,
                request.TransactionType,
                request.Description,
                request.Multiplier ?? 1.0m);

            // Mise à jour du client
            await _customerRepository.UpdateAsync(customer);

            // Publication des événements domaine
            await _mediator.Publish(new LoyaltyPointsEarnedEvent(
                customer.Id,
                transaction.PointsEarned,
                transaction.NewTotalPoints,
                request.LoyaltyProgramId,
                request.TransactionType,
                request.Amount,
                transaction.Id,
                DateTime.UtcNow), cancellationToken);

            // Vérification du changement de niveau
            var previousTier = customer.LoyaltyStats.CurrentTier;
            var newTier = loyaltyProgram.CalculateTier(customer.LoyaltyStats.TotalPoints);
            
            if (newTier != previousTier)
            {
                customer.UpdateLoyaltyTier(newTier);
                await _customerRepository.UpdateAsync(customer);

                await _mediator.Publish(new CustomerTierChangedEvent(
                    customer.Id,
                    previousTier,
                    newTier,
                    customer.LoyaltyStats.TotalPoints,
                    DateTime.UtcNow), cancellationToken);

                _logger.LogInformation("Changement de niveau de fidélité pour le client {CustomerId} : {PreviousTier} -> {NewTier}", 
                    customer.Id, previousTier, newTier);
            }

            _logger.LogInformation("Attribution de points terminée avec succès. Transaction : {TransactionId}, Points attribués : {Points}", 
                transaction.Id, transaction.PointsEarned);

            return _mapper.Map<LoyaltyTransactionDto>(transaction);
        }
    }

    /// <summary>
    /// Handler pour l'utilisation/rachat de points de fidélité
    /// </summary>
    public class RedeemLoyaltyPointsCommandHandler : IRequestHandler<RedeemLoyaltyPointsCommand, LoyaltyTransactionDto>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILoyaltyRewardRepository _loyaltyRewardRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<RedeemLoyaltyPointsCommandHandler> _logger;
        private readonly IValidator<RedeemLoyaltyPointsCommand> _validator;
        private readonly IMediator _mediator;

        public RedeemLoyaltyPointsCommandHandler(
            ICustomerRepository customerRepository,
            ILoyaltyRewardRepository loyaltyRewardRepository,
            IMapper mapper,
            ILogger<RedeemLoyaltyPointsCommandHandler> logger,
            IValidator<RedeemLoyaltyPointsCommand> validator,
            IMediator mediator)
        {
            _customerRepository = customerRepository;
            _loyaltyRewardRepository = loyaltyRewardRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
            _mediator = mediator;
        }

        public async Task<LoyaltyTransactionDto> Handle(RedeemLoyaltyPointsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Rachat de points démarré pour le client {CustomerId}, récompense {RewardId}", 
                request.CustomerId, request.RewardId);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour le rachat de points : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Récupération du client
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
            {
                _logger.LogError("Client non trouvé : {CustomerId}", request.CustomerId);
                throw new ArgumentException($"Client non trouvé : {request.CustomerId}");
            }

            // Récupération de la récompense
            var reward = await _loyaltyRewardRepository.GetByIdAsync(request.RewardId);
            if (reward == null || !reward.IsActive)
            {
                _logger.LogError("Récompense non trouvée ou inactive : {RewardId}", request.RewardId);
                throw new ArgumentException($"Récompense non trouvée ou inactive : {request.RewardId}");
            }

            // Vérification de l'éligibilité
            if (!customer.CanRedeemReward(reward))
            {
                _logger.LogWarning("Client {CustomerId} non éligible pour la récompense {RewardId} - Points insuffisants ou conditions non remplies", 
                    request.CustomerId, request.RewardId);
                throw new InvalidOperationException("Points insuffisants ou conditions d'éligibilité non remplies");
            }

            // Rachat des points
            var transaction = customer.RedeemLoyaltyPoints(reward, request.Quantity ?? 1);

            // Mise à jour du client
            await _customerRepository.UpdateAsync(customer);

            // Publication des événements domaine
            await _mediator.Publish(new LoyaltyPointsRedeemedEvent(
                customer.Id,
                transaction.PointsUsed,
                transaction.NewTotalPoints,
                request.RewardId,
                reward.Name,
                transaction.Quantity,
                transaction.Id,
                DateTime.UtcNow), cancellationToken);

            _logger.LogInformation("Rachat de points terminé avec succès. Transaction : {TransactionId}, Points utilisés : {Points}", 
                transaction.Id, transaction.PointsUsed);

            return _mapper.Map<LoyaltyTransactionDto>(transaction);
        }
    }

    /// <summary>
    /// Handler pour la création de programmes de fidélité
    /// </summary>
    public class CreateLoyaltyProgramCommandHandler : IRequestHandler<CreateLoyaltyProgramCommand, LoyaltyProgramDto>
    {
        private readonly ILoyaltyProgramRepository _loyaltyProgramRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateLoyaltyProgramCommandHandler> _logger;
        private readonly IValidator<CreateLoyaltyProgramCommand> _validator;

        public CreateLoyaltyProgramCommandHandler(
            ILoyaltyProgramRepository loyaltyProgramRepository,
            IMapper mapper,
            ILogger<CreateLoyaltyProgramCommandHandler> logger,
            IValidator<CreateLoyaltyProgramCommand> validator)
        {
            _loyaltyProgramRepository = loyaltyProgramRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<LoyaltyProgramDto> Handle(CreateLoyaltyProgramCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Création d'un programme de fidélité : {Name}", request.Name);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour la création de programme : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Vérification de l'unicité du nom
            var existingProgram = await _loyaltyProgramRepository.GetByNameAsync(request.Name);
            if (existingProgram != null)
            {
                _logger.LogError("Un programme avec le nom '{Name}' existe déjà", request.Name);
                throw new InvalidOperationException($"Un programme avec le nom '{request.Name}' existe déjà");
            }

            // Création du programme
            var loyaltyProgram = LoyaltyProgram.Create(
                request.Name,
                request.Description,
                request.ProgramType,
                request.PointsPerEuro,
                request.StartDate,
                request.EndDate,
                request.IsActive,
                request.MinimumPurchaseAmount,
                request.MaximumPointsPerTransaction,
                request.TierThresholds?.ToDictionary(t => t.TierName, t => t.PointsRequired) ?? new Dictionary<string, int>(),
                request.TierMultipliers?.ToDictionary(t => t.TierName, t => t.Multiplier) ?? new Dictionary<string, decimal>());

            // Sauvegarde
            await _loyaltyProgramRepository.AddAsync(loyaltyProgram);

            _logger.LogInformation("Programme de fidélité créé avec succès : {ProgramId}", loyaltyProgram.Id);

            return _mapper.Map<LoyaltyProgramDto>(loyaltyProgram);
        }
    }

    /// <summary>
    /// Handler pour la création de récompenses de fidélité
    /// </summary>
    public class CreateLoyaltyRewardCommandHandler : IRequestHandler<CreateLoyaltyRewardCommand, LoyaltyRewardDto>
    {
        private readonly ILoyaltyRewardRepository _loyaltyRewardRepository;
        private readonly ILoyaltyProgramRepository _loyaltyProgramRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateLoyaltyRewardCommandHandler> _logger;
        private readonly IValidator<CreateLoyaltyRewardCommand> _validator;

        public CreateLoyaltyRewardCommandHandler(
            ILoyaltyRewardRepository loyaltyRewardRepository,
            ILoyaltyProgramRepository loyaltyProgramRepository,
            IMapper mapper,
            ILogger<CreateLoyaltyRewardCommandHandler> logger,
            IValidator<CreateLoyaltyRewardCommand> validator)
        {
            _loyaltyRewardRepository = loyaltyRewardRepository;
            _loyaltyProgramRepository = loyaltyProgramRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<LoyaltyRewardDto> Handle(CreateLoyaltyRewardCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Création d'une récompense de fidélité : {Name}", request.Name);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour la création de récompense : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Vérification de l'existence du programme
            var loyaltyProgram = await _loyaltyProgramRepository.GetByIdAsync(request.LoyaltyProgramId);
            if (loyaltyProgram == null)
            {
                _logger.LogError("Programme de fidélité non trouvé : {ProgramId}", request.LoyaltyProgramId);
                throw new ArgumentException($"Programme de fidélité non trouvé : {request.LoyaltyProgramId}");
            }

            // Création de la récompense
            var loyaltyReward = LoyaltyReward.Create(
                request.LoyaltyProgramId,
                request.Name,
                request.Description,
                request.RewardType,
                request.PointsCost,
                request.Value,
                request.ValidFrom,
                request.ValidUntil,
                request.IsActive,
                request.MaxRedemptionsPerCustomer,
                request.MaxTotalRedemptions,
                request.MinimumTierRequired,
                request.Terms,
                request.ImageUrl);

            // Sauvegarde
            await _loyaltyRewardRepository.AddAsync(loyaltyReward);

            _logger.LogInformation("Récompense de fidélité créée avec succès : {RewardId}", loyaltyReward.Id);

            return _mapper.Map<LoyaltyRewardDto>(loyaltyReward);
        }
    }

    /// <summary>
    /// Handler pour la mise à jour de programmes de fidélité
    /// </summary>
    public class UpdateLoyaltyProgramCommandHandler : IRequestHandler<UpdateLoyaltyProgramCommand, LoyaltyProgramDto>
    {
        private readonly ILoyaltyProgramRepository _loyaltyProgramRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateLoyaltyProgramCommandHandler> _logger;
        private readonly IValidator<UpdateLoyaltyProgramCommand> _validator;

        public UpdateLoyaltyProgramCommandHandler(
            ILoyaltyProgramRepository loyaltyProgramRepository,
            IMapper mapper,
            ILogger<UpdateLoyaltyProgramCommandHandler> logger,
            IValidator<UpdateLoyaltyProgramCommand> validator)
        {
            _loyaltyProgramRepository = loyaltyProgramRepository;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<LoyaltyProgramDto> Handle(UpdateLoyaltyProgramCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Mise à jour du programme de fidélité : {ProgramId}", request.Id);

            // Validation
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Échec de validation pour la mise à jour de programme : {Errors}", errors);
                throw new ValidationException($"Erreurs de validation : {errors}");
            }

            // Récupération du programme
            var loyaltyProgram = await _loyaltyProgramRepository.GetByIdAsync(request.Id);
            if (loyaltyProgram == null)
            {
                _logger.LogError("Programme de fidélité non trouvé : {ProgramId}", request.Id);
                throw new ArgumentException($"Programme de fidélité non trouvé : {request.Id}");
            }

            // Mise à jour
            loyaltyProgram.Update(
                request.Name,
                request.Description,
                request.PointsPerEuro,
                request.StartDate,
                request.EndDate,
                request.IsActive,
                request.MinimumPurchaseAmount,
                request.MaximumPointsPerTransaction,
                request.TierThresholds?.ToDictionary(t => t.TierName, t => t.PointsRequired),
                request.TierMultipliers?.ToDictionary(t => t.TierName, t => t.Multiplier));

            // Sauvegarde
            await _loyaltyProgramRepository.UpdateAsync(loyaltyProgram);

            _logger.LogInformation("Programme de fidélité mis à jour avec succès : {ProgramId}", loyaltyProgram.Id);

            return _mapper.Map<LoyaltyProgramDto>(loyaltyProgram);
        }
    }
}