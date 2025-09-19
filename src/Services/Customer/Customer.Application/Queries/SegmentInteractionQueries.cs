using BuildingBlocks.Application.Queries;
using Customer.Application.DTOs;
using Customer.Domain.Enums;
using FluentValidation;

namespace Customer.Application.Queries;

/// <summary>
/// Requête pour obtenir un segment par ID
/// </summary>
public class GetCustomerSegmentByIdQuery : IQuery<CustomerSegmentDto?>
{
    public Guid SegmentId { get; init; }
    public bool IncludePerformance { get; init; } = true;
    public bool IncludeCustomerCount { get; init; } = true;
}

/// <summary>
/// Validateur pour GetCustomerSegmentByIdQuery
/// </summary>
public class GetCustomerSegmentByIdQueryValidator : AbstractValidator<GetCustomerSegmentByIdQuery>
{
    public GetCustomerSegmentByIdQueryValidator()
    {
        RuleFor(x => x.SegmentId)
            .NotEmpty()
            .WithMessage("L'identifiant du segment est obligatoire");
    }
}

/// <summary>
/// Requête pour obtenir tous les segments actifs
/// </summary>
public class GetActiveCustomerSegmentsQuery : IQuery<List<CustomerSegmentDto>>
{
    public SegmentType? Type { get; init; }
    public bool IncludePerformance { get; init; } = false;
}

/// <summary>
/// Requête pour obtenir les segments d'un client
/// </summary>
public class GetCustomerSegmentsQuery : IQuery<List<CustomerSegmentSummaryDto>>
{
    public Guid CustomerId { get; init; }
    public bool OnlyActive { get; init; } = true;
}

/// <summary>
/// Validateur pour GetCustomerSegmentsQuery
/// </summary>
public class GetCustomerSegmentsQueryValidator : AbstractValidator<GetCustomerSegmentsQuery>
{
    public GetCustomerSegmentsQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");
    }
}

/// <summary>
/// Requête pour évaluer un client pour tous les segments
/// </summary>
public class EvaluateCustomerSegmentsQuery : IQuery<CustomerSegmentEvaluationDto>
{
    public Guid CustomerId { get; init; }
    public decimal MinMatchScore { get; init; } = 0.5m;
    public bool IncludePotentialSegments { get; init; } = true;
}

/// <summary>
/// Validateur pour EvaluateCustomerSegmentsQuery
/// </summary>
public class EvaluateCustomerSegmentsQueryValidator : AbstractValidator<EvaluateCustomerSegmentsQuery>
{
    public EvaluateCustomerSegmentsQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");

        RuleFor(x => x.MinMatchScore)
            .InclusiveBetween(0.1m, 1.0m)
            .WithMessage("Le score minimum doit être entre 0.1 et 1.0");
    }
}

/// <summary>
/// Requête pour obtenir les clients d'un segment
/// </summary>
public class GetSegmentCustomersQuery : IQuery<PagedResult<CustomerSummaryDto>>
{
    public Guid SegmentId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
    public string? SortBy { get; init; } = "MatchScore";
    public bool SortDescending { get; init; } = true;
}

/// <summary>
/// Validateur pour GetSegmentCustomersQuery
/// </summary>
public class GetSegmentCustomersQueryValidator : AbstractValidator<GetSegmentCustomersQuery>
{
    public GetSegmentCustomersQueryValidator()
    {
        RuleFor(x => x.SegmentId)
            .NotEmpty()
            .WithMessage("L'identifiant du segment est obligatoire");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Le numéro de page doit être positif");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("La taille de page doit être entre 1 et 100");

        RuleFor(x => x.SortBy)
            .Must(sortBy => new[] { "MatchScore", "AssignedDate", "CustomerName", "TotalSpent" }
                .Contains(sortBy, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Critère de tri invalide")
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy));
    }
}

/// <summary>
/// Requête pour obtenir les segments nécessitant un rafraîchissement
/// </summary>
public class GetSegmentsNeedingRefreshQuery : IQuery<List<CustomerSegmentDto>>
{
    public bool ForceCheck { get; init; } = false;
}

/// <summary>
/// Requête pour obtenir l'historique des interactions d'un client
/// </summary>
public class GetCustomerInteractionHistoryQuery : IQuery<CustomerInteractionHistoryDto>
{
    public Guid CustomerId { get; init; }
    public int RecentInteractionsLimit { get; init; } = 10;
    public bool IncludeTrends { get; init; } = true;
}

/// <summary>
/// Validateur pour GetCustomerInteractionHistoryQuery
/// </summary>
public class GetCustomerInteractionHistoryQueryValidator : AbstractValidator<GetCustomerInteractionHistoryQuery>
{
    public GetCustomerInteractionHistoryQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");

        RuleFor(x => x.RecentInteractionsLimit)
            .InclusiveBetween(1, 50)
            .WithMessage("La limite d'interactions récentes doit être entre 1 et 50");
    }
}

/// <summary>
/// Requête pour rechercher des interactions
/// </summary>
public class SearchInteractionsQuery : IQuery<PagedResult<CustomerInteractionDto>>
{
    public Guid? CustomerId { get; init; }
    public InteractionType? Type { get; init; }
    public InteractionChannel? Channel { get; init; }
    public InteractionOutcome? Outcome { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? Staff { get; init; }
    public int? MinSatisfactionRating { get; init; }
    public int? MaxSatisfactionRating { get; init; }
    public bool? RequiresFollowUp { get; init; }
    public bool? IsOverdue { get; init; }
    public bool? IsCompleted { get; init; }
    public List<string>? Tags { get; init; }
    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; } = "InteractionDate";
    public bool SortDescending { get; init; } = true;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

/// <summary>
/// Validateur pour SearchInteractionsQuery
/// </summary>
public class SearchInteractionsQueryValidator : AbstractValidator<SearchInteractionsQuery>
{
    public SearchInteractionsQueryValidator()
    {
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

        RuleFor(x => x.MinSatisfactionRating)
            .InclusiveBetween(1, 5)
            .WithMessage("La note minimum doit être entre 1 et 5")
            .LessThanOrEqualTo(x => x.MaxSatisfactionRating ?? 5)
            .WithMessage("La note minimum doit être inférieure à la note maximum")
            .When(x => x.MinSatisfactionRating.HasValue);

        RuleFor(x => x.MaxSatisfactionRating)
            .InclusiveBetween(1, 5)
            .WithMessage("La note maximum doit être entre 1 et 5")
            .When(x => x.MaxSatisfactionRating.HasValue);

        RuleFor(x => x.SortBy)
            .Must(sortBy => new[] { "InteractionDate", "Type", "Channel", "Staff", "SatisfactionRating" }
                .Contains(sortBy, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Critère de tri invalide")
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy));
    }
}

/// <summary>
/// Requête pour obtenir les interactions en attente de suivi
/// </summary>
public class GetPendingFollowUpsQuery : IQuery<PagedResult<CustomerInteractionDto>>
{
    public string? Staff { get; init; }
    public bool OnlyOverdue { get; init; } = false;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

/// <summary>
/// Validateur pour GetPendingFollowUpsQuery
/// </summary>
public class GetPendingFollowUpsQueryValidator : AbstractValidator<GetPendingFollowUpsQuery>
{
    public GetPendingFollowUpsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Le numéro de page doit être positif");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("La taille de page doit être entre 1 et 100");
    }
}

/// <summary>
/// Requête pour obtenir les statistiques d'interactions
/// </summary>
public class GetInteractionStatsQuery : IQuery<InteractionStatsDto>
{
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? Staff { get; init; }
    public Guid? CustomerId { get; init; }
}

/// <summary>
/// Validateur pour GetInteractionStatsQuery
/// </summary>
public class GetInteractionStatsQueryValidator : AbstractValidator<GetInteractionStatsQuery>
{
    public GetInteractionStatsQueryValidator()
    {
        RuleFor(x => x.FromDate)
            .LessThan(x => x.ToDate)
            .WithMessage("La date de début doit être antérieure à la date de fin")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);
    }
}

/// <summary>
/// Requête pour obtenir une interaction par ID
/// </summary>
public class GetInteractionByIdQuery : IQuery<CustomerInteractionDto?>
{
    public Guid InteractionId { get; init; }
}

/// <summary>
/// Validateur pour GetInteractionByIdQuery
/// </summary>
public class GetInteractionByIdQueryValidator : AbstractValidator<GetInteractionByIdQuery>
{
    public GetInteractionByIdQueryValidator()
    {
        RuleFor(x => x.InteractionId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'interaction est obligatoire");
    }
}

/// <summary>
/// Requête pour obtenir la dernière interaction d'un client
/// </summary>
public class GetLastCustomerInteractionQuery : IQuery<CustomerInteractionDto?>
{
    public Guid CustomerId { get; init; }
    public InteractionType? Type { get; init; }
}

/// <summary>
/// Validateur pour GetLastCustomerInteractionQuery
/// </summary>
public class GetLastCustomerInteractionQueryValidator : AbstractValidator<GetLastCustomerInteractionQuery>
{
    public GetLastCustomerInteractionQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");
    }
}