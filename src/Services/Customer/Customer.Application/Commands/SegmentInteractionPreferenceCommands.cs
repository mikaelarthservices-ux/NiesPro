using BuildingBlocks.Application.Commands;
using Customer.Application.DTOs;
using Customer.Domain.Enums;
using FluentValidation;

namespace Customer.Application.Commands;

/// <summary>
/// Commande pour créer un segment client
/// </summary>
public class CreateCustomerSegmentCommand : ICommand<CustomerSegmentDto>
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public SegmentType Type { get; init; } = SegmentType.Behavioral;
    public bool IsActive { get; init; } = true;
    public bool IsAutomatic { get; init; } = true;
    public decimal MinMatchScore { get; init; } = 0.7m;
    
    // Critères de segmentation
    public List<CreateSegmentCriterionDto> Criteria { get; init; } = new();
    
    // Configuration de rafraîchissement
    public int RefreshIntervalHours { get; init; } = 24;
    public bool AutoRefresh { get; init; } = true;
    
    // Métadonnées
    public string? BusinessPurpose { get; init; }
    public List<string> Tags { get; init; } = new();
}

/// <summary>
/// Validateur pour CreateCustomerSegmentCommand
/// </summary>
public class CreateCustomerSegmentCommandValidator : AbstractValidator<CreateCustomerSegmentCommand>
{
    public CreateCustomerSegmentCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Le nom du segment est obligatoire")
            .MaximumLength(200)
            .WithMessage("Le nom ne peut pas dépasser 200 caractères");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("La description ne peut pas dépasser 1000 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.MinMatchScore)
            .InclusiveBetween(0.1m, 1.0m)
            .WithMessage("Le score minimum de correspondance doit être entre 0.1 et 1.0");

        RuleFor(x => x.RefreshIntervalHours)
            .GreaterThan(0)
            .WithMessage("L'intervalle de rafraîchissement doit être positif")
            .LessThanOrEqualTo(8760)
            .WithMessage("L'intervalle de rafraîchissement ne peut pas dépasser 1 an");

        RuleFor(x => x.Criteria)
            .NotEmpty()
            .WithMessage("Au moins un critère doit être défini")
            .When(x => x.IsAutomatic);

        RuleFor(x => x.BusinessPurpose)
            .MaximumLength(500)
            .WithMessage("L'objectif métier ne peut pas dépasser 500 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.BusinessPurpose));

        RuleFor(x => x.Tags)
            .Must(tags => tags.All(tag => !string.IsNullOrWhiteSpace(tag)))
            .WithMessage("Les tags ne peuvent pas être vides")
            .Must(tags => tags.Count <= 10)
            .WithMessage("Maximum 10 tags autorisés");
    }
}

/// <summary>
/// Commande pour assigner un client à un segment
/// </summary>
public class AssignCustomerSegmentCommand : ICommand<bool>
{
    public Guid CustomerId { get; init; }
    public Guid SegmentId { get; init; }
    public string? Reason { get; init; }
    public bool OverrideAutomatic { get; init; } = false;
}

/// <summary>
/// Validateur pour AssignCustomerSegmentCommand
/// </summary>
public class AssignCustomerSegmentCommandValidator : AbstractValidator<AssignCustomerSegmentCommand>
{
    public AssignCustomerSegmentCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");

        RuleFor(x => x.SegmentId)
            .NotEmpty()
            .WithMessage("L'identifiant du segment est obligatoire");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Une raison est obligatoire pour l'assignation manuelle")
            .MaximumLength(500)
            .WithMessage("La raison ne peut pas dépasser 500 caractères")
            .When(x => x.OverrideAutomatic);
    }
}

/// <summary>
/// Commande pour retirer un client d'un segment
/// </summary>
public class RemoveCustomerSegmentCommand : ICommand<bool>
{
    public Guid CustomerId { get; init; }
    public Guid SegmentId { get; init; }
    public required string Reason { get; init; }
}

/// <summary>
/// Validateur pour RemoveCustomerSegmentCommand
/// </summary>
public class RemoveCustomerSegmentCommandValidator : AbstractValidator<RemoveCustomerSegmentCommand>
{
    public RemoveCustomerSegmentCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");

        RuleFor(x => x.SegmentId)
            .NotEmpty()
            .WithMessage("L'identifiant du segment est obligatoire");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("La raison de suppression est obligatoire")
            .MaximumLength(500)
            .WithMessage("La raison ne peut pas dépasser 500 caractères");
    }
}

/// <summary>
/// Commande pour rafraîchir un segment automatique
/// </summary>
public class RefreshCustomerSegmentCommand : ICommand<int>
{
    public Guid SegmentId { get; init; }
    public bool ForceRefresh { get; init; } = false;
}

/// <summary>
/// Validateur pour RefreshCustomerSegmentCommand
/// </summary>
public class RefreshCustomerSegmentCommandValidator : AbstractValidator<RefreshCustomerSegmentCommand>
{
    public RefreshCustomerSegmentCommandValidator()
    {
        RuleFor(x => x.SegmentId)
            .NotEmpty()
            .WithMessage("L'identifiant du segment est obligatoire");
    }
}

/// <summary>
/// Commande pour créer une interaction client
/// </summary>
public class CreateCustomerInteractionCommand : ICommand<CustomerInteractionDto>
{
    public Guid CustomerId { get; init; }
    public InteractionType Type { get; init; }
    public InteractionChannel Channel { get; init; }
    public required string Subject { get; init; }
    public string? Description { get; init; }
    public string? Staff { get; init; }
    public int? SatisfactionRating { get; init; }
    public bool RequiresFollowUp { get; init; } = false;
    public DateTime? FollowUpDate { get; init; }
    public List<string> Tags { get; init; } = new();
    public string? Notes { get; init; }
}

/// <summary>
/// Validateur pour CreateCustomerInteractionCommand
/// </summary>
public class CreateCustomerInteractionCommandValidator : AbstractValidator<CreateCustomerInteractionCommand>
{
    public CreateCustomerInteractionCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");

        RuleFor(x => x.Subject)
            .NotEmpty()
            .WithMessage("Le sujet de l'interaction est obligatoire")
            .MaximumLength(200)
            .WithMessage("Le sujet ne peut pas dépasser 200 caractères");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("La description ne peut pas dépasser 2000 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Staff)
            .MaximumLength(100)
            .WithMessage("Le nom du personnel ne peut pas dépasser 100 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.Staff));

        RuleFor(x => x.SatisfactionRating)
            .InclusiveBetween(1, 5)
            .WithMessage("La note de satisfaction doit être entre 1 et 5")
            .When(x => x.SatisfactionRating.HasValue);

        RuleFor(x => x.FollowUpDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("La date de suivi doit être future")
            .When(x => x.RequiresFollowUp && x.FollowUpDate.HasValue);

        RuleFor(x => x.FollowUpDate)
            .NotNull()
            .WithMessage("Une date de suivi est obligatoire quand un suivi est requis")
            .When(x => x.RequiresFollowUp);

        RuleFor(x => x.Tags)
            .Must(tags => tags.All(tag => !string.IsNullOrWhiteSpace(tag)))
            .WithMessage("Les tags ne peuvent pas être vides")
            .Must(tags => tags.Count <= 10)
            .WithMessage("Maximum 10 tags autorisés");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Les notes ne peuvent pas dépasser 1000 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}

/// <summary>
/// Commande pour mettre à jour une interaction
/// </summary>
public class UpdateCustomerInteractionCommand : ICommand<CustomerInteractionDto>
{
    public Guid InteractionId { get; init; }
    public string? Description { get; init; }
    public InteractionOutcome? Outcome { get; init; }
    public int? SatisfactionRating { get; init; }
    public bool? RequiresFollowUp { get; init; }
    public DateTime? FollowUpDate { get; init; }
    public List<string>? Tags { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Validateur pour UpdateCustomerInteractionCommand
/// </summary>
public class UpdateCustomerInteractionCommandValidator : AbstractValidator<UpdateCustomerInteractionCommand>
{
    public UpdateCustomerInteractionCommandValidator()
    {
        RuleFor(x => x.InteractionId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'interaction est obligatoire");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("La description ne peut pas dépasser 2000 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.SatisfactionRating)
            .InclusiveBetween(1, 5)
            .WithMessage("La note de satisfaction doit être entre 1 et 5")
            .When(x => x.SatisfactionRating.HasValue);

        RuleFor(x => x.FollowUpDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("La date de suivi doit être future")
            .When(x => x.RequiresFollowUp == true && x.FollowUpDate.HasValue);

        RuleFor(x => x.Tags)
            .Must(tags => tags!.All(tag => !string.IsNullOrWhiteSpace(tag)))
            .WithMessage("Les tags ne peuvent pas être vides")
            .Must(tags => tags!.Count <= 10)
            .WithMessage("Maximum 10 tags autorisés")
            .When(x => x.Tags != null);

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Les notes ne peuvent pas dépasser 1000 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}

/// <summary>
/// Commande pour compléter une interaction
/// </summary>
public class CompleteInteractionCommand : ICommand<bool>
{
    public Guid InteractionId { get; init; }
    public InteractionOutcome Outcome { get; init; }
    public string? Resolution { get; init; }
    public int? SatisfactionRating { get; init; }
    public bool ScheduleFollowUp { get; init; } = false;
    public DateTime? FollowUpDate { get; init; }
    public string? FollowUpReason { get; init; }
}

/// <summary>
/// Validateur pour CompleteInteractionCommand
/// </summary>
public class CompleteInteractionCommandValidator : AbstractValidator<CompleteInteractionCommand>
{
    public CompleteInteractionCommandValidator()
    {
        RuleFor(x => x.InteractionId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'interaction est obligatoire");

        RuleFor(x => x.Resolution)
            .MaximumLength(1000)
            .WithMessage("La résolution ne peut pas dépasser 1000 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.Resolution));

        RuleFor(x => x.SatisfactionRating)
            .InclusiveBetween(1, 5)
            .WithMessage("La note de satisfaction doit être entre 1 et 5")
            .When(x => x.SatisfactionRating.HasValue);

        RuleFor(x => x.FollowUpDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("La date de suivi doit être future")
            .When(x => x.ScheduleFollowUp && x.FollowUpDate.HasValue);

        RuleFor(x => x.FollowUpDate)
            .NotNull()
            .WithMessage("Une date de suivi est obligatoire")
            .When(x => x.ScheduleFollowUp);

        RuleFor(x => x.FollowUpReason)
            .NotEmpty()
            .WithMessage("Une raison de suivi est obligatoire")
            .MaximumLength(500)
            .WithMessage("La raison de suivi ne peut pas dépasser 500 caractères")
            .When(x => x.ScheduleFollowUp);
    }
}

/// <summary>
/// Commande pour ajouter une préférence client
/// </summary>
public class AddCustomerPreferenceCommand : ICommand<CustomerPreferenceDto>
{
    public Guid CustomerId { get; init; }
    public PreferenceType Type { get; init; }
    public required string Key { get; init; }
    public required string Value { get; init; }
    public string? DisplayValue { get; init; }
    public int Priority { get; init; } = 0;
    public string? Source { get; init; }
    public decimal? Confidence { get; init; }
    public string? Context { get; init; }
    public DateTime? ExpirationDate { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Validateur pour AddCustomerPreferenceCommand
/// </summary>
public class AddCustomerPreferenceCommandValidator : AbstractValidator<AddCustomerPreferenceCommand>
{
    public AddCustomerPreferenceCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");

        RuleFor(x => x.Key)
            .NotEmpty()
            .WithMessage("La clé de préférence est obligatoire")
            .MaximumLength(100)
            .WithMessage("La clé ne peut pas dépasser 100 caractères");

        RuleFor(x => x.Value)
            .NotEmpty()
            .WithMessage("La valeur de préférence est obligatoire")
            .MaximumLength(500)
            .WithMessage("La valeur ne peut pas dépasser 500 caractères");

        RuleFor(x => x.DisplayValue)
            .MaximumLength(500)
            .WithMessage("La valeur d'affichage ne peut pas dépasser 500 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.DisplayValue));

        RuleFor(x => x.Priority)
            .InclusiveBetween(-100, 100)
            .WithMessage("La priorité doit être entre -100 et 100");

        RuleFor(x => x.Source)
            .MaximumLength(100)
            .WithMessage("La source ne peut pas dépasser 100 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.Source));

        RuleFor(x => x.Confidence)
            .InclusiveBetween(0.0m, 1.0m)
            .WithMessage("La confiance doit être entre 0.0 et 1.0")
            .When(x => x.Confidence.HasValue);

        RuleFor(x => x.Context)
            .MaximumLength(500)
            .WithMessage("Le contexte ne peut pas dépasser 500 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.Context));

        RuleFor(x => x.ExpirationDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("La date d'expiration doit être future")
            .When(x => x.ExpirationDate.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Les notes ne peuvent pas dépasser 1000 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}

/// <summary>
/// Commande pour mettre à jour une préférence client
/// </summary>
public class UpdateCustomerPreferenceCommand : ICommand<CustomerPreferenceDto>
{
    public Guid PreferenceId { get; init; }
    public string? Value { get; init; }
    public string? DisplayValue { get; init; }
    public int? Priority { get; init; }
    public string? Source { get; init; }
    public decimal? Confidence { get; init; }
    public string? Context { get; init; }
    public DateTime? ExpirationDate { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Validateur pour UpdateCustomerPreferenceCommand
/// </summary>
public class UpdateCustomerPreferenceCommandValidator : AbstractValidator<UpdateCustomerPreferenceCommand>
{
    public UpdateCustomerPreferenceCommandValidator()
    {
        RuleFor(x => x.PreferenceId)
            .NotEmpty()
            .WithMessage("L'identifiant de préférence est obligatoire");

        RuleFor(x => x.Value)
            .NotEmpty()
            .WithMessage("La valeur ne peut pas être vide")
            .MaximumLength(500)
            .WithMessage("La valeur ne peut pas dépasser 500 caractères")
            .When(x => x.Value != null);

        RuleFor(x => x.DisplayValue)
            .MaximumLength(500)
            .WithMessage("La valeur d'affichage ne peut pas dépasser 500 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.DisplayValue));

        RuleFor(x => x.Priority)
            .InclusiveBetween(-100, 100)
            .WithMessage("La priorité doit être entre -100 et 100")
            .When(x => x.Priority.HasValue);

        RuleFor(x => x.Source)
            .MaximumLength(100)
            .WithMessage("La source ne peut pas dépasser 100 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.Source));

        RuleFor(x => x.Confidence)
            .InclusiveBetween(0.0m, 1.0m)
            .WithMessage("La confiance doit être entre 0.0 et 1.0")
            .When(x => x.Confidence.HasValue);

        RuleFor(x => x.Context)
            .MaximumLength(500)
            .WithMessage("Le contexte ne peut pas dépasser 500 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.Context));

        RuleFor(x => x.ExpirationDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("La date d'expiration doit être future")
            .When(x => x.ExpirationDate.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Les notes ne peuvent pas dépasser 1000 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}