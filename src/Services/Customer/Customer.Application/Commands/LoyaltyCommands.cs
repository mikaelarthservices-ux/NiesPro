using BuildingBlocks.Application.Commands;
using Customer.Application.DTOs;
using Customer.Domain.Enums;
using FluentValidation;

namespace Customer.Application.Commands;

/// <summary>
/// Commande pour gagner des points de fidélité
/// </summary>
public class EarnLoyaltyPointsCommand : ICommand<LoyaltyTransactionDto>
{
    public Guid CustomerId { get; init; }
    public Guid ProgramId { get; init; }
    public int Points { get; init; }
    public required string Source { get; init; }
    public string? SourceReference { get; init; }
    public decimal? TransactionAmount { get; init; }
    public string? Description { get; init; }
}

/// <summary>
/// Validateur pour EarnLoyaltyPointsCommand
/// </summary>
public class EarnLoyaltyPointsCommandValidator : AbstractValidator<EarnLoyaltyPointsCommand>
{
    public EarnLoyaltyPointsCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");

        RuleFor(x => x.ProgramId)
            .NotEmpty()
            .WithMessage("L'identifiant du programme est obligatoire");

        RuleFor(x => x.Points)
            .GreaterThan(0)
            .WithMessage("Le nombre de points doit être positif")
            .LessThanOrEqualTo(10000)
            .WithMessage("Le nombre de points ne peut pas dépasser 10000 par transaction");

        RuleFor(x => x.Source)
            .NotEmpty()
            .WithMessage("La source est obligatoire")
            .MaximumLength(100)
            .WithMessage("La source ne peut pas dépasser 100 caractères");

        RuleFor(x => x.SourceReference)
            .MaximumLength(200)
            .WithMessage("La référence source ne peut pas dépasser 200 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.SourceReference));

        RuleFor(x => x.TransactionAmount)
            .GreaterThan(0)
            .WithMessage("Le montant de transaction doit être positif")
            .When(x => x.TransactionAmount.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("La description ne peut pas dépasser 500 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

/// <summary>
/// Commande pour utiliser des points de fidélité
/// </summary>
public class RedeemLoyaltyPointsCommand : ICommand<LoyaltyTransactionDto>
{
    public Guid CustomerId { get; init; }
    public Guid RewardId { get; init; }
    public int Quantity { get; init; } = 1;
    public string? TransactionReference { get; init; }
}

/// <summary>
/// Validateur pour RedeemLoyaltyPointsCommand
/// </summary>
public class RedeemLoyaltyPointsCommandValidator : AbstractValidator<RedeemLoyaltyPointsCommand>
{
    public RedeemLoyaltyPointsCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");

        RuleFor(x => x.RewardId)
            .NotEmpty()
            .WithMessage("L'identifiant de la récompense est obligatoire");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("La quantité doit être positive")
            .LessThanOrEqualTo(10)
            .WithMessage("La quantité ne peut pas dépasser 10 par transaction");

        RuleFor(x => x.TransactionReference)
            .MaximumLength(200)
            .WithMessage("La référence de transaction ne peut pas dépasser 200 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.TransactionReference));
    }
}

/// <summary>
/// Commande pour créer un programme de fidélité
/// </summary>
public class CreateLoyaltyProgramCommand : ICommand<LoyaltyProgramDto>
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public LoyaltyProgramType Type { get; init; } = LoyaltyProgramType.Points;
    public bool IsActive { get; init; } = true;
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    
    // Configuration des tiers
    public List<CreateLoyaltyTierDto> Tiers { get; init; } = new();
    
    // Règles de gain de points
    public decimal PointsPerEuro { get; init; } = 1.0m;
    public int BonusRegistrationPoints { get; init; } = 0;
    public int BonusBirthdayPoints { get; init; } = 0;
    
    // Configuration d'expiration
    public int? PointsExpirationMonths { get; init; }
    public bool NotifyBeforeExpiration { get; init; } = true;
    public int NotificationDaysBefore { get; init; } = 30;
}

/// <summary>
/// Validateur pour CreateLoyaltyProgramCommand
/// </summary>
public class CreateLoyaltyProgramCommandValidator : AbstractValidator<CreateLoyaltyProgramCommand>
{
    public CreateLoyaltyProgramCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Le nom du programme est obligatoire")
            .MaximumLength(200)
            .WithMessage("Le nom ne peut pas dépasser 200 caractères");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("La description ne peut pas dépasser 1000 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate)
            .WithMessage("La date de début doit être antérieure à la date de fin")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.PointsPerEuro)
            .GreaterThan(0)
            .WithMessage("Le ratio points par euro doit être positif")
            .LessThanOrEqualTo(100)
            .WithMessage("Le ratio points par euro ne peut pas dépasser 100");

        RuleFor(x => x.BonusRegistrationPoints)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Les points bonus d'inscription ne peuvent pas être négatifs")
            .LessThanOrEqualTo(10000)
            .WithMessage("Les points bonus d'inscription ne peuvent pas dépasser 10000");

        RuleFor(x => x.BonusBirthdayPoints)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Les points bonus d'anniversaire ne peuvent pas être négatifs")
            .LessThanOrEqualTo(5000)
            .WithMessage("Les points bonus d'anniversaire ne peuvent pas dépasser 5000");

        RuleFor(x => x.PointsExpirationMonths)
            .GreaterThan(0)
            .WithMessage("La durée d'expiration doit être positive")
            .LessThanOrEqualTo(120)
            .WithMessage("La durée d'expiration ne peut pas dépasser 10 ans")
            .When(x => x.PointsExpirationMonths.HasValue);

        RuleFor(x => x.NotificationDaysBefore)
            .GreaterThan(0)
            .WithMessage("Le délai de notification doit être positif")
            .LessThanOrEqualTo(90)
            .WithMessage("Le délai de notification ne peut pas dépasser 90 jours");

        RuleFor(x => x.Tiers)
            .Must(tiers => tiers.Count > 0)
            .WithMessage("Au moins un tier doit être défini")
            .Must(tiers => tiers.Select(t => t.Name).Distinct().Count() == tiers.Count)
            .WithMessage("Les noms des tiers doivent être uniques")
            .Must(tiers => tiers.OrderBy(t => t.RequiredPoints).SequenceEqual(tiers))
            .WithMessage("Les tiers doivent être ordonnés par points requis croissants");
    }
}

/// <summary>
/// Commande pour créer une récompense de fidélité
/// </summary>
public class CreateLoyaltyRewardCommand : ICommand<LoyaltyRewardDto>
{
    public Guid ProgramId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public RewardType Type { get; init; }
    public int PointsCost { get; init; }
    public decimal? MonetaryValue { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int? MaxRedemptions { get; init; }
    public int? MaxRedemptionsPerCustomer { get; init; }
    public List<string> RequiredTiers { get; init; } = new();
    public string? Terms { get; init; }
    public string? ImageUrl { get; init; }
}

/// <summary>
/// Validateur pour CreateLoyaltyRewardCommand
/// </summary>
public class CreateLoyaltyRewardCommandValidator : AbstractValidator<CreateLoyaltyRewardCommand>
{
    public CreateLoyaltyRewardCommandValidator()
    {
        RuleFor(x => x.ProgramId)
            .NotEmpty()
            .WithMessage("L'identifiant du programme est obligatoire");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Le nom de la récompense est obligatoire")
            .MaximumLength(200)
            .WithMessage("Le nom ne peut pas dépasser 200 caractères");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("La description ne peut pas dépasser 1000 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.PointsCost)
            .GreaterThan(0)
            .WithMessage("Le coût en points doit être positif")
            .LessThanOrEqualTo(1000000)
            .WithMessage("Le coût en points ne peut pas dépasser 1 million");

        RuleFor(x => x.MonetaryValue)
            .GreaterThan(0)
            .WithMessage("La valeur monétaire doit être positive")
            .When(x => x.MonetaryValue.HasValue);

        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate)
            .WithMessage("La date de début doit être antérieure à la date de fin")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.MaxRedemptions)
            .GreaterThan(0)
            .WithMessage("Le nombre maximum d'utilisations doit être positif")
            .When(x => x.MaxRedemptions.HasValue);

        RuleFor(x => x.MaxRedemptionsPerCustomer)
            .GreaterThan(0)
            .WithMessage("Le nombre maximum d'utilisations par client doit être positif")
            .LessThanOrEqualTo(x => x.MaxRedemptions ?? int.MaxValue)
            .WithMessage("Le nombre maximum par client ne peut pas dépasser le maximum global")
            .When(x => x.MaxRedemptionsPerCustomer.HasValue);

        RuleFor(x => x.Terms)
            .MaximumLength(2000)
            .WithMessage("Les conditions ne peuvent pas dépasser 2000 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.Terms));

        RuleFor(x => x.ImageUrl)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("L'URL de l'image doit être valide")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));
    }
}

/// <summary>
/// Commande pour mettre à jour un programme de fidélité
/// </summary>
public class UpdateLoyaltyProgramCommand : ICommand<LoyaltyProgramDto>
{
    public Guid ProgramId { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? IsActive { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public decimal? PointsPerEuro { get; init; }
    public int? BonusRegistrationPoints { get; init; }
    public int? BonusBirthdayPoints { get; init; }
    public int? PointsExpirationMonths { get; init; }
    public bool? NotifyBeforeExpiration { get; init; }
    public int? NotificationDaysBefore { get; init; }
}

/// <summary>
/// Validateur pour UpdateLoyaltyProgramCommand
/// </summary>
public class UpdateLoyaltyProgramCommandValidator : AbstractValidator<UpdateLoyaltyProgramCommand>
{
    public UpdateLoyaltyProgramCommandValidator()
    {
        RuleFor(x => x.ProgramId)
            .NotEmpty()
            .WithMessage("L'identifiant du programme est obligatoire");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Le nom du programme ne peut pas être vide")
            .MaximumLength(200)
            .WithMessage("Le nom ne peut pas dépasser 200 caractères")
            .When(x => x.Name != null);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("La description ne peut pas dépasser 1000 caractères")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate)
            .WithMessage("La date de début doit être antérieure à la date de fin")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.PointsPerEuro)
            .GreaterThan(0)
            .WithMessage("Le ratio points par euro doit être positif")
            .LessThanOrEqualTo(100)
            .WithMessage("Le ratio points par euro ne peut pas dépasser 100")
            .When(x => x.PointsPerEuro.HasValue);

        RuleFor(x => x.BonusRegistrationPoints)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Les points bonus d'inscription ne peuvent pas être négatifs")
            .LessThanOrEqualTo(10000)
            .WithMessage("Les points bonus d'inscription ne peuvent pas dépasser 10000")
            .When(x => x.BonusRegistrationPoints.HasValue);

        RuleFor(x => x.BonusBirthdayPoints)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Les points bonus d'anniversaire ne peuvent pas être négatifs")
            .LessThanOrEqualTo(5000)
            .WithMessage("Les points bonus d'anniversaire ne peuvent pas dépasser 5000")
            .When(x => x.BonusBirthdayPoints.HasValue);
    }
}