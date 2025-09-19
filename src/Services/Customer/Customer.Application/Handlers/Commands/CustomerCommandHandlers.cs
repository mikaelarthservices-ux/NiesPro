using AutoMapper;
using BuildingBlocks.Application.Commands;
using Customer.Application.Commands;
using Customer.Application.DTOs;
using Customer.Domain.Entities;
using Customer.Domain.Events;
using Customer.Domain.Repositories;
using Customer.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Customer.Application.Handlers.Commands;

/// <summary>
/// Handler pour la création d'un nouveau client
/// </summary>
public class CreateCustomerCommandHandler : ICommandHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomerProfileRepository _profileRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateCustomerCommandHandler> _logger;

    public CreateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        ICustomerProfileRepository profileRepository,
        IMapper mapper,
        ILogger<CreateCustomerCommandHandler> logger)
    {
        _customerRepository = customerRepository;
        _profileRepository = profileRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Création d'un nouveau client avec email {Email}", request.Email);

        // Vérifier l'unicité de l'email
        var emailExists = await _customerRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (emailExists != null)
        {
            throw new InvalidOperationException($"Un client avec l'email {request.Email} existe déjà");
        }

        // Vérifier l'unicité du téléphone si fourni
        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            var phoneExists = await _customerRepository.GetByPhoneAsync(request.Phone, cancellationToken);
            if (phoneExists != null)
            {
                throw new InvalidOperationException($"Un client avec le téléphone {request.Phone} existe déjà");
            }
        }

        // Créer les informations personnelles
        var personalInfo = new PersonalInfo(
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.Gender);

        // Créer les informations de contact
        var contactInfo = new ContactInfo(
            request.Email,
            request.Phone);

        // Créer le client
        var customer = new Entities.Customer(
            personalInfo,
            contactInfo,
            request.RegistrationSource ?? "web",
            request.ReferralCode);

        // Configurer les préférences marketing
        customer.SetMarketingConsent(request.AcceptMarketing);

        // Ajouter l'événement de domaine
        customer.AddDomainEvent(new CustomerRegisteredEvent(
            customer.Id,
            customer.ContactInfo.Email,
            customer.PersonalInfo.FirstName,
            customer.PersonalInfo.LastName,
            customer.ContactInfo.Phone,
            customer.RegistrationDate,
            customer.RegistrationSource ?? "unknown",
            customer.IsEmailVerified,
            customer.ReferralCode));

        // Sauvegarder le client
        await _customerRepository.AddAsync(customer, cancellationToken);

        // Créer le profil si des informations sont fournies
        if (request.DietaryRestrictions?.Any() == true ||
            request.Allergies?.Any() == true ||
            request.PreferredAmbiance.HasValue ||
            request.PreferredTimeSlot.HasValue)
        {
            var profile = new CustomerProfile(customer.Id);

            if (request.DietaryRestrictions?.Any() == true)
            {
                foreach (var restriction in request.DietaryRestrictions)
                {
                    profile.AddDietaryRestriction(restriction);
                }
            }

            if (request.Allergies?.Any() == true)
            {
                foreach (var allergy in request.Allergies)
                {
                    profile.AddAllergy(allergy);
                }
            }

            if (request.PreferredAmbiance.HasValue)
            {
                profile.SetAmbiancePreference(request.PreferredAmbiance.Value);
            }

            if (request.PreferredTimeSlot.HasValue)
            {
                profile.SetTimeSlotPreference(request.PreferredTimeSlot.Value);
            }

            await _profileRepository.AddAsync(profile, cancellationToken);
        }

        _logger.LogInformation("Client créé avec succès avec ID {CustomerId}", customer.Id);

        return _mapper.Map<CustomerDto>(customer);
    }
}

/// <summary>
/// Handler pour la mise à jour d'un client
/// </summary>
public class UpdateCustomerCommandHandler : ICommandHandler<UpdateCustomerCommand, CustomerDto>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateCustomerCommandHandler> _logger;

    public UpdateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IMapper mapper,
        ILogger<UpdateCustomerCommandHandler> logger)
    {
        _customerRepository = customerRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CustomerDto> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Mise à jour du client {CustomerId}", request.CustomerId);

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            throw new InvalidOperationException($"Client {request.CustomerId} non trouvé");
        }

        var changes = new Dictionary<string, object>();

        // Mettre à jour les informations personnelles si nécessaire
        if (!string.IsNullOrWhiteSpace(request.FirstName) ||
            !string.IsNullOrWhiteSpace(request.LastName) ||
            request.DateOfBirth.HasValue ||
            request.Gender.HasValue)
        {
            var newPersonalInfo = new PersonalInfo(
                request.FirstName ?? customer.PersonalInfo.FirstName,
                request.LastName ?? customer.PersonalInfo.LastName,
                request.DateOfBirth ?? customer.PersonalInfo.DateOfBirth,
                request.Gender ?? customer.PersonalInfo.Gender);

            if (!customer.PersonalInfo.Equals(newPersonalInfo))
            {
                if (request.FirstName != null) changes["FirstName"] = request.FirstName;
                if (request.LastName != null) changes["LastName"] = request.LastName;
                if (request.DateOfBirth.HasValue) changes["DateOfBirth"] = request.DateOfBirth.Value;
                if (request.Gender.HasValue) changes["Gender"] = request.Gender.Value;

                customer.UpdatePersonalInfo(newPersonalInfo);
            }
        }

        // Mettre à jour les informations de contact si nécessaire
        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            // Vérifier l'unicité du téléphone
            var phoneExists = await _customerRepository.IsPhoneUniqueAsync(request.Phone, customer.Id, cancellationToken);
            if (!phoneExists)
            {
                throw new InvalidOperationException($"Le téléphone {request.Phone} est déjà utilisé par un autre client");
            }

            var newContactInfo = new ContactInfo(
                customer.ContactInfo.Email,
                request.Phone);

            if (!customer.ContactInfo.Equals(newContactInfo))
            {
                changes["Phone"] = request.Phone;
                customer.UpdateContactInfo(newContactInfo);
            }
        }

        // Mettre à jour les préférences marketing si nécessaire
        if (request.AcceptMarketing.HasValue &&
            request.AcceptMarketing.Value != customer.AcceptMarketing)
        {
            changes["AcceptMarketing"] = request.AcceptMarketing.Value;
            customer.SetMarketingConsent(request.AcceptMarketing.Value);
        }

        // Ajouter l'événement de domaine si des changements ont été apportés
        if (changes.Any())
        {
            customer.AddDomainEvent(new CustomerProfileUpdatedEvent(
                customer.Id,
                changes,
                "system", // TODO: Récupérer l'utilisateur actuel
                "Mise à jour du profil client"));
        }

        await _customerRepository.UpdateAsync(customer, cancellationToken);

        _logger.LogInformation("Client {CustomerId} mis à jour avec succès", customer.Id);

        return _mapper.Map<CustomerDto>(customer);
    }
}

/// <summary>
/// Handler pour la désactivation d'un client
/// </summary>
public class DeactivateCustomerCommandHandler : ICommandHandler<DeactivateCustomerCommand, bool>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<DeactivateCustomerCommandHandler> _logger;

    public DeactivateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        ILogger<DeactivateCustomerCommandHandler> logger)
    {
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(DeactivateCustomerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Désactivation du client {CustomerId}", request.CustomerId);

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            throw new InvalidOperationException($"Client {request.CustomerId} non trouvé");
        }

        customer.Deactivate(request.Reason);

        customer.AddDomainEvent(new CustomerDeactivatedEvent(
            customer.Id,
            request.Reason,
            "system", // TODO: Récupérer l'utilisateur actuel
            request.IsTemporary,
            request.ReactivationDate));

        await _customerRepository.UpdateAsync(customer, cancellationToken);

        _logger.LogInformation("Client {CustomerId} désactivé avec succès", customer.Id);

        return true;
    }
}

/// <summary>
/// Handler pour la réactivation d'un client
/// </summary>
public class ReactivateCustomerCommandHandler : ICommandHandler<ReactivateCustomerCommand, bool>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<ReactivateCustomerCommandHandler> _logger;

    public ReactivateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        ILogger<ReactivateCustomerCommandHandler> logger)
    {
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(ReactivateCustomerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Réactivation du client {CustomerId}", request.CustomerId);

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            throw new InvalidOperationException($"Client {request.CustomerId} non trouvé");
        }

        var daysDeactivated = customer.LastVisitDate.HasValue 
            ? (DateTime.UtcNow - customer.LastVisitDate.Value).Days 
            : 0;

        customer.Reactivate();

        customer.AddDomainEvent(new CustomerReactivatedEvent(
            customer.Id,
            "system", // TODO: Récupérer l'utilisateur actuel
            request.ReactivationReason,
            daysDeactivated));

        await _customerRepository.UpdateAsync(customer, cancellationToken);

        _logger.LogInformation("Client {CustomerId} réactivé avec succès", customer.Id);

        return true;
    }
}

/// <summary>
/// Handler pour la vérification de l'email d'un client
/// </summary>
public class VerifyCustomerEmailCommandHandler : ICommandHandler<VerifyCustomerEmailCommand, bool>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<VerifyCustomerEmailCommandHandler> _logger;

    public VerifyCustomerEmailCommandHandler(
        ICustomerRepository customerRepository,
        ILogger<VerifyCustomerEmailCommandHandler> logger)
    {
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(VerifyCustomerEmailCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Vérification de l'email pour le client {CustomerId}", request.CustomerId);

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            throw new InvalidOperationException($"Client {request.CustomerId} non trouvé");
        }

        // TODO: Valider le token de vérification
        // Pour l'instant, on accepte tous les tokens non vides
        if (string.IsNullOrWhiteSpace(request.VerificationToken))
        {
            throw new InvalidOperationException("Token de vérification invalide");
        }

        customer.VerifyEmail();

        await _customerRepository.UpdateAsync(customer, cancellationToken);

        _logger.LogInformation("Email vérifié avec succès pour le client {CustomerId}", customer.Id);

        return true;
    }
}

/// <summary>
/// Handler pour l'enregistrement d'une visite client
/// </summary>
public class RecordCustomerVisitCommandHandler : ICommandHandler<RecordCustomerVisitCommand, bool>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<RecordCustomerVisitCommandHandler> _logger;

    public RecordCustomerVisitCommandHandler(
        ICustomerRepository customerRepository,
        ILogger<RecordCustomerVisitCommandHandler> logger)
    {
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(RecordCustomerVisitCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Enregistrement d'une visite pour le client {CustomerId}", request.CustomerId);

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            throw new InvalidOperationException($"Client {request.CustomerId} non trouvé");
        }

        customer.RecordVisit(request.VisitDate);

        await _customerRepository.UpdateAsync(customer, cancellationToken);

        _logger.LogInformation("Visite enregistrée avec succès pour le client {CustomerId}", customer.Id);

        return true;
    }
}