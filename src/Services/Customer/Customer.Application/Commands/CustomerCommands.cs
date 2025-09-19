using BuildingBlocks.Application.Commands;
using Customer.Application.DTOs;
using Customer.Domain.Enums;
using FluentValidation;

namespace Customer.Application.Commands;

/// <summary>
/// Commande pour créer un nouveau client
/// </summary>
public class CreateCustomerCommand : ICommand<CustomerDto>
{
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Phone { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public Gender? Gender { get; init; }
    public string? RegistrationSource { get; init; }
    public string? ReferralCode { get; init; }
    public bool AcceptMarketing { get; init; } = false;
    public bool AcceptTerms { get; init; } = true;
    
    // Informations de profil optionnelles
    public List<DietaryRestriction>? DietaryRestrictions { get; init; }
    public List<string>? Allergies { get; init; }
    public AmbiancePreference? PreferredAmbiance { get; init; }
    public PreferredTimeSlot? PreferredTimeSlot { get; init; }
}

/// <summary>
/// Validateur pour CreateCustomerCommand
/// </summary>
public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("L'email est obligatoire")
            .EmailAddress()
            .WithMessage("Format d'email invalide")
            .MaximumLength(255)
            .WithMessage("L'email ne peut pas dépasser 255 caractères");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("Le prénom est obligatoire")
            .MinimumLength(2)
            .WithMessage("Le prénom doit contenir au moins 2 caractères")
            .MaximumLength(100)
            .WithMessage("Le prénom ne peut pas dépasser 100 caractères");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Le nom est obligatoire")
            .MinimumLength(2)
            .WithMessage("Le nom doit contenir au moins 2 caractères")
            .MaximumLength(100)
            .WithMessage("Le nom ne peut pas dépasser 100 caractères");

        RuleFor(x => x.Phone)
            .Matches(@"^[\+]?[0-9\-\.\s\(\)]+$")
            .WithMessage("Format de téléphone invalide")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today)
            .WithMessage("La date de naissance doit être antérieure à aujourd'hui")
            .GreaterThan(DateTime.Today.AddYears(-120))
            .WithMessage("La date de naissance doit être réaliste")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.AcceptTerms)
            .Equal(true)
            .WithMessage("L'acceptation des conditions générales est obligatoire");

        RuleFor(x => x.RegistrationSource)
            .MaximumLength(100)
            .WithMessage("La source d'enregistrement ne peut pas dépasser 100 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.RegistrationSource));

        RuleFor(x => x.ReferralCode)
            .MaximumLength(50)
            .WithMessage("Le code de parrainage ne peut pas dépasser 50 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.ReferralCode));

        RuleFor(x => x.Allergies)
            .Must(allergies => allergies == null || allergies.All(a => !string.IsNullOrWhiteSpace(a)))
            .WithMessage("Les allergies ne peuvent pas être vides");
    }
}

/// <summary>
/// Commande pour mettre à jour un client
/// </summary>
public class UpdateCustomerCommand : ICommand<CustomerDto>
{
    public Guid CustomerId { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Phone { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public Gender? Gender { get; init; }
    public bool? AcceptMarketing { get; init; }
}

/// <summary>
/// Validateur pour UpdateCustomerCommand
/// </summary>
public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");

        RuleFor(x => x.FirstName)
            .MinimumLength(2)
            .WithMessage("Le prénom doit contenir au moins 2 caractères")
            .MaximumLength(100)
            .WithMessage("Le prénom ne peut pas dépasser 100 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.LastName)
            .MinimumLength(2)
            .WithMessage("Le nom doit contenir au moins 2 caractères")
            .MaximumLength(100)
            .WithMessage("Le nom ne peut pas dépasser 100 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));

        RuleFor(x => x.Phone)
            .Matches(@"^[\+]?[0-9\-\.\s\(\)]+$")
            .WithMessage("Format de téléphone invalide")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today)
            .WithMessage("La date de naissance doit être antérieure à aujourd'hui")
            .GreaterThan(DateTime.Today.AddYears(-120))
            .WithMessage("La date de naissance doit être réaliste")
            .When(x => x.DateOfBirth.HasValue);
    }
}

/// <summary>
/// Commande pour désactiver un client
/// </summary>
public class DeactivateCustomerCommand : ICommand<bool>
{
    public Guid CustomerId { get; init; }
    public required string Reason { get; init; }
    public bool IsTemporary { get; init; } = false;
    public DateTime? ReactivationDate { get; init; }
}

/// <summary>
/// Validateur pour DeactivateCustomerCommand
/// </summary>
public class DeactivateCustomerCommandValidator : AbstractValidator<DeactivateCustomerCommand>
{
    public DeactivateCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("La raison de désactivation est obligatoire")
            .MaximumLength(500)
            .WithMessage("La raison ne peut pas dépasser 500 caractères");

        RuleFor(x => x.ReactivationDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("La date de réactivation doit être future")
            .When(x => x.IsTemporary && x.ReactivationDate.HasValue);
    }
}

/// <summary>
/// Commande pour réactiver un client
/// </summary>
public class ReactivateCustomerCommand : ICommand<bool>
{
    public Guid CustomerId { get; init; }
    public string? ReactivationReason { get; init; }
}

/// <summary>
/// Validateur pour ReactivateCustomerCommand
/// </summary>
public class ReactivateCustomerCommandValidator : AbstractValidator<ReactivateCustomerCommand>
{
    public ReactivateCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");

        RuleFor(x => x.ReactivationReason)
            .MaximumLength(500)
            .WithMessage("La raison de réactivation ne peut pas dépasser 500 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.ReactivationReason));
    }
}

/// <summary>
/// Commande pour vérifier l'email d'un client
/// </summary>
public class VerifyCustomerEmailCommand : ICommand<bool>
{
    public Guid CustomerId { get; init; }
    public required string VerificationToken { get; init; }
}

/// <summary>
/// Validateur pour VerifyCustomerEmailCommand
/// </summary>
public class VerifyCustomerEmailCommandValidator : AbstractValidator<VerifyCustomerEmailCommand>
{
    public VerifyCustomerEmailCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");

        RuleFor(x => x.VerificationToken)
            .NotEmpty()
            .WithMessage("Le token de vérification est obligatoire");
    }
}

/// <summary>
/// Commande pour mettre à jour le profil client
/// </summary>
public class UpdateCustomerProfileCommand : ICommand<CustomerProfileDto>
{
    public Guid CustomerId { get; init; }
    public List<DietaryRestriction>? DietaryRestrictions { get; init; }
    public List<string>? Allergies { get; init; }
    public AmbiancePreference? PreferredAmbiance { get; init; }
    public PreferredTimeSlot? PreferredTimeSlot { get; init; }
    public int? PreferredTableSize { get; init; }
    public string? PreferredTableLocation { get; init; }
    public string? SpecialRequests { get; init; }
    public string? Notes { get; init; }
    public CommunicationPreference? PreferredCommunication { get; init; }
    public CommunicationFrequency? CommunicationFrequency { get; init; }
    public List<CommunicationPreference>? AllowedChannels { get; init; }
}

/// <summary>
/// Validateur pour UpdateCustomerProfileCommand
/// </summary>
public class UpdateCustomerProfileCommandValidator : AbstractValidator<UpdateCustomerProfileCommand>
{
    public UpdateCustomerProfileCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");

        RuleFor(x => x.Allergies)
            .Must(allergies => allergies == null || allergies.All(a => !string.IsNullOrWhiteSpace(a)))
            .WithMessage("Les allergies ne peuvent pas être vides");

        RuleFor(x => x.PreferredTableSize)
            .GreaterThan(0)
            .WithMessage("La taille de table préférée doit être positive")
            .LessThanOrEqualTo(20)
            .WithMessage("La taille de table préférée ne peut pas dépasser 20 personnes")
            .When(x => x.PreferredTableSize.HasValue);

        RuleFor(x => x.PreferredTableLocation)
            .MaximumLength(200)
            .WithMessage("L'emplacement préféré ne peut pas dépasser 200 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.PreferredTableLocation));

        RuleFor(x => x.SpecialRequests)
            .MaximumLength(1000)
            .WithMessage("Les demandes spéciales ne peuvent pas dépasser 1000 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.SpecialRequests));

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .WithMessage("Les notes ne peuvent pas dépasser 2000 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));

        RuleFor(x => x.AllowedChannels)
            .Must(channels => channels == null || channels.Count > 0)
            .WithMessage("Au moins un canal de communication doit être autorisé")
            .When(x => x.AllowedChannels != null);
    }
}

/// <summary>
/// Commande pour enregistrer une visite client
/// </summary>
public class RecordCustomerVisitCommand : ICommand<bool>
{
    public Guid CustomerId { get; init; }
    public DateTime VisitDate { get; init; } = DateTime.UtcNow;
    public string? Location { get; init; }
    public decimal? AmountSpent { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Validateur pour RecordCustomerVisitCommand
/// </summary>
public class RecordCustomerVisitCommandValidator : AbstractValidator<RecordCustomerVisitCommand>
{
    public RecordCustomerVisitCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");

        RuleFor(x => x.VisitDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddHours(1))
            .WithMessage("La date de visite ne peut pas être dans le futur");

        RuleFor(x => x.AmountSpent)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Le montant dépensé ne peut pas être négatif")
            .When(x => x.AmountSpent.HasValue);

        RuleFor(x => x.Location)
            .MaximumLength(200)
            .WithMessage("L'emplacement ne peut pas dépasser 200 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.Location));

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Les notes ne peuvent pas dépasser 1000 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}