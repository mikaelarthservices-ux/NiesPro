using BuildingBlocks.Application.Queries;
using Customer.Application.DTOs;
using Customer.Domain.Enums;
using FluentValidation;

namespace Customer.Application.Queries;

/// <summary>
/// Requête pour obtenir un programme de fidélité par ID
/// </summary>
public class GetLoyaltyProgramByIdQuery : IQuery<LoyaltyProgramDto?>
{
    public Guid ProgramId { get; init; }
    public bool IncludeStatistics { get; init; } = true;
    public bool IncludeRewards { get; init; } = true;
}

/// <summary>
/// Validateur pour GetLoyaltyProgramByIdQuery
/// </summary>
public class GetLoyaltyProgramByIdQueryValidator : AbstractValidator<GetLoyaltyProgramByIdQuery>
{
    public GetLoyaltyProgramByIdQueryValidator()
    {
        RuleFor(x => x.ProgramId)
            .NotEmpty()
            .WithMessage("L'identifiant du programme est obligatoire");
    }
}

/// <summary>
/// Requête pour obtenir tous les programmes de fidélité actifs
/// </summary>
public class GetActiveLoyaltyProgramsQuery : IQuery<List<LoyaltyProgramDto>>
{
    public bool IncludeStatistics { get; init; } = false;
    public bool IncludeRewards { get; init; } = false;
}

/// <summary>
/// Requête pour obtenir les programmes de fidélité d'un client
/// </summary>
public class GetCustomerLoyaltyProgramsQuery : IQuery<List<LoyaltyProgramDto>>
{
    public Guid CustomerId { get; init; }
    public bool OnlyActive { get; init; } = true;
}

/// <summary>
/// Validateur pour GetCustomerLoyaltyProgramsQuery
/// </summary>
public class GetCustomerLoyaltyProgramsQueryValidator : AbstractValidator<GetCustomerLoyaltyProgramsQuery>
{
    public GetCustomerLoyaltyProgramsQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");
    }
}

/// <summary>
/// Requête pour obtenir les statistiques de fidélité d'un client
/// </summary>
public class GetCustomerLoyaltyStatsQuery : IQuery<List<CustomerLoyaltyStatsDto>>
{
    public Guid CustomerId { get; init; }
    public Guid? ProgramId { get; init; }
}

/// <summary>
/// Validateur pour GetCustomerLoyaltyStatsQuery
/// </summary>
public class GetCustomerLoyaltyStatsQueryValidator : AbstractValidator<GetCustomerLoyaltyStatsQuery>
{
    public GetCustomerLoyaltyStatsQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");
    }
}

/// <summary>
/// Requête pour obtenir les récompenses disponibles pour un client
/// </summary>
public class GetAvailableRewardsQuery : IQuery<List<LoyaltyRewardDto>>
{
    public Guid CustomerId { get; init; }
    public Guid ProgramId { get; init; }
    public RewardType? Type { get; init; }
    public int? MaxPointsCost { get; init; }
}

/// <summary>
/// Validateur pour GetAvailableRewardsQuery
/// </summary>
public class GetAvailableRewardsQueryValidator : AbstractValidator<GetAvailableRewardsQuery>
{
    public GetAvailableRewardsQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");

        RuleFor(x => x.ProgramId)
            .NotEmpty()
            .WithMessage("L'identifiant du programme est obligatoire");

        RuleFor(x => x.MaxPointsCost)
            .GreaterThan(0)
            .WithMessage("Le coût maximum en points doit être positif")
            .When(x => x.MaxPointsCost.HasValue);
    }
}

/// <summary>
/// Requête pour obtenir les récompenses d'un programme
/// </summary>
public class GetProgramRewardsQuery : IQuery<List<LoyaltyRewardDto>>
{
    public Guid ProgramId { get; init; }
    public bool OnlyActive { get; init; } = true;
    public RewardType? Type { get; init; }
}

/// <summary>
/// Validateur pour GetProgramRewardsQuery
/// </summary>
public class GetProgramRewardsQueryValidator : AbstractValidator<GetProgramRewardsQuery>
{
    public GetProgramRewardsQueryValidator()
    {
        RuleFor(x => x.ProgramId)
            .NotEmpty()
            .WithMessage("L'identifiant du programme est obligatoire");
    }
}

/// <summary>
/// Requête pour obtenir l'historique des transactions de fidélité
/// </summary>
public class GetLoyaltyTransactionHistoryQuery : IQuery<PagedResult<LoyaltyTransactionDto>>
{
    public Guid CustomerId { get; init; }
    public Guid? ProgramId { get; init; }
    public LoyaltyTransactionType? Type { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

/// <summary>
/// Validateur pour GetLoyaltyTransactionHistoryQuery
/// </summary>
public class GetLoyaltyTransactionHistoryQueryValidator : AbstractValidator<GetLoyaltyTransactionHistoryQuery>
{
    public GetLoyaltyTransactionHistoryQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Le numéro de page doit être positif");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("La taille de page doit être entre 1 et 100");

        RuleFor(x => x.FromDate)
            .LessThan(x => x.ToDate)
            .WithMessage("La date de début doit être antérieure à la date de fin")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);
    }
}

/// <summary>
/// Requête pour obtenir les statistiques d'un programme de fidélité
/// </summary>
public class GetLoyaltyProgramStatsQuery : IQuery<LoyaltyProgramStatsDto>
{
    public Guid ProgramId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public bool IncludeTrends { get; init; } = true;
}

/// <summary>
/// Validateur pour GetLoyaltyProgramStatsQuery
/// </summary>
public class GetLoyaltyProgramStatsQueryValidator : AbstractValidator<GetLoyaltyProgramStatsQuery>
{
    public GetLoyaltyProgramStatsQueryValidator()
    {
        RuleFor(x => x.ProgramId)
            .NotEmpty()
            .WithMessage("L'identifiant du programme est obligatoire");

        RuleFor(x => x.FromDate)
            .LessThan(x => x.ToDate)
            .WithMessage("La date de début doit être antérieure à la date de fin")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);
    }
}

/// <summary>
/// Requête pour obtenir les clients à risque de désengagement
/// </summary>
public class GetLoyaltyRiskCustomersQuery : IQuery<List<CustomerSummaryDto>>
{
    public Guid ProgramId { get; init; }
    public int DaysWithoutActivity { get; init; } = 90;
    public int MinPointsThreshold { get; init; } = 100;
}

/// <summary>
/// Validateur pour GetLoyaltyRiskCustomersQuery
/// </summary>
public class GetLoyaltyRiskCustomersQueryValidator : AbstractValidator<GetLoyaltyRiskCustomersQuery>
{
    public GetLoyaltyRiskCustomersQueryValidator()
    {
        RuleFor(x => x.ProgramId)
            .NotEmpty()
            .WithMessage("L'identifiant du programme est obligatoire");

        RuleFor(x => x.DaysWithoutActivity)
            .GreaterThan(0)
            .WithMessage("Le nombre de jours sans activité doit être positif");

        RuleFor(x => x.MinPointsThreshold)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Le seuil de points minimum ne peut pas être négatif");
    }
}

/// <summary>
/// Requête pour obtenir les récompenses expirant bientôt
/// </summary>
public class GetExpiringRewardsQuery : IQuery<List<LoyaltyRewardDto>>
{
    public int DaysFromNow { get; init; } = 30;
    public Guid? ProgramId { get; init; }
}

/// <summary>
/// Validateur pour GetExpiringRewardsQuery
/// </summary>
public class GetExpiringRewardsQueryValidator : AbstractValidator<GetExpiringRewardsQuery>
{
    public GetExpiringRewardsQueryValidator()
    {
        RuleFor(x => x.DaysFromNow)
            .GreaterThan(0)
            .WithMessage("Le nombre de jours doit être positif")
            .LessThanOrEqualTo(365)
            .WithMessage("Le nombre de jours ne peut pas dépasser 365");
    }
}

/// <summary>
/// Requête pour obtenir les récompenses les plus populaires
/// </summary>
public class GetMostPopularRewardsQuery : IQuery<List<PopularRewardDto>>
{
    public Guid ProgramId { get; init; }
    public int Top { get; init; } = 10;
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

/// <summary>
/// Validateur pour GetMostPopularRewardsQuery
/// </summary>
public class GetMostPopularRewardsQueryValidator : AbstractValidator<GetMostPopularRewardsQuery>
{
    public GetMostPopularRewardsQueryValidator()
    {
        RuleFor(x => x.ProgramId)
            .NotEmpty()
            .WithMessage("L'identifiant du programme est obligatoire");

        RuleFor(x => x.Top)
            .InclusiveBetween(1, 50)
            .WithMessage("Le nombre de résultats doit être entre 1 et 50");

        RuleFor(x => x.FromDate)
            .LessThan(x => x.ToDate)
            .WithMessage("La date de début doit être antérieure à la date de fin")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);
    }
}

/// <summary>
/// Requête pour vérifier si un client peut utiliser une récompense
/// </summary>
public class CanRedeemRewardQuery : IQuery<RewardEligibilityDto>
{
    public Guid CustomerId { get; init; }
    public Guid RewardId { get; init; }
    public int Quantity { get; init; } = 1;
}

/// <summary>
/// Validateur pour CanRedeemRewardQuery
/// </summary>
public class CanRedeemRewardQueryValidator : AbstractValidator<CanRedeemRewardQuery>
{
    public CanRedeemRewardQueryValidator()
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
            .WithMessage("La quantité ne peut pas dépasser 10");
    }
}

/// <summary>
/// DTO pour l'éligibilité à une récompense
/// </summary>
public class RewardEligibilityDto
{
    public bool IsEligible { get; init; }
    public List<string> Reasons { get; init; } = new();
    public int RequiredPoints { get; init; }
    public int AvailablePoints { get; init; }
    public int MissingPoints { get; init; }
    public bool MeetsRequirements { get; init; }
    public List<string> MissingRequirements { get; init; } = new();
}