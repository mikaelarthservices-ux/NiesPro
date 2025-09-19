using BuildingBlocks.Application.Queries;
using Customer.Application.DTOs;
using Customer.Domain.Enums;
using FluentValidation;

namespace Customer.Application.Queries;

/// <summary>
/// Requête pour obtenir les préférences d'un client
/// </summary>
public class GetCustomerPreferencesQuery : IQuery<List<CustomerPreferenceDto>>
{
    public Guid CustomerId { get; init; }
    public PreferenceType? Type { get; init; }
    public bool OnlyActive { get; init; } = true;
    public bool OnlyValid { get; init; } = true;
}

/// <summary>
/// Validateur pour GetCustomerPreferencesQuery
/// </summary>
public class GetCustomerPreferencesQueryValidator : AbstractValidator<GetCustomerPreferencesQuery>
{
    public GetCustomerPreferencesQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");
    }
}

/// <summary>
/// Requête pour obtenir une préférence par ID
/// </summary>
public class GetPreferenceByIdQuery : IQuery<CustomerPreferenceDto?>
{
    public Guid PreferenceId { get; init; }
}

/// <summary>
/// Validateur pour GetPreferenceByIdQuery
/// </summary>
public class GetPreferenceByIdQueryValidator : AbstractValidator<GetPreferenceByIdQuery>
{
    public GetPreferenceByIdQueryValidator()
    {
        RuleFor(x => x.PreferenceId)
            .NotEmpty()
            .WithMessage("L'identifiant de la préférence est obligatoire");
    }
}

/// <summary>
/// Requête pour rechercher des préférences
/// </summary>
public class SearchPreferencesQuery : IQuery<PagedResult<CustomerPreferenceDto>>
{
    public Guid? CustomerId { get; init; }
    public PreferenceType? Type { get; init; }
    public string? Key { get; init; }
    public string? Value { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsValid { get; init; }
    public decimal? MinConfidence { get; init; }
    public decimal? MaxConfidence { get; init; }
    public DateTime? CreatedAfter { get; init; }
    public DateTime? CreatedBefore { get; init; }
    public DateTime? UsedAfter { get; init; }
    public string? Source { get; init; }
    public string? SortBy { get; init; } = "CreatedDate";
    public bool SortDescending { get; init; } = true;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

/// <summary>
/// Validateur pour SearchPreferencesQuery
/// </summary>
public class SearchPreferencesQueryValidator : AbstractValidator<SearchPreferencesQuery>
{
    public SearchPreferencesQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Le numéro de page doit être positif");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("La taille de page doit être entre 1 et 100");

        RuleFor(x => x.MinConfidence)
            .InclusiveBetween(0.0m, 1.0m)
            .WithMessage("La confiance minimum doit être entre 0.0 et 1.0")
            .LessThanOrEqualTo(x => x.MaxConfidence ?? 1.0m)
            .WithMessage("La confiance minimum doit être inférieure à la confiance maximum")
            .When(x => x.MinConfidence.HasValue);

        RuleFor(x => x.MaxConfidence)
            .InclusiveBetween(0.0m, 1.0m)
            .WithMessage("La confiance maximum doit être entre 0.0 et 1.0")
            .When(x => x.MaxConfidence.HasValue);

        RuleFor(x => x.CreatedAfter)
            .LessThan(x => x.CreatedBefore)
            .WithMessage("La date de début doit être antérieure à la date de fin")
            .When(x => x.CreatedAfter.HasValue && x.CreatedBefore.HasValue);

        RuleFor(x => x.SortBy)
            .Must(sortBy => new[] { "CreatedDate", "LastUsedDate", "UsageCount", "Confidence", "Priority" }
                .Contains(sortBy, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Critère de tri invalide")
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy));
    }
}

/// <summary>
/// Requête pour obtenir les préférences culinaires d'un client
/// </summary>
public class GetCulinaryPreferencesQuery : IQuery<List<CulinaryPreferenceDto>>
{
    public Guid CustomerId { get; init; }
    public bool OnlyLiked { get; init; } = false;
    public decimal MinConfidence { get; init; } = 0.5m;
}

/// <summary>
/// Validateur pour GetCulinaryPreferencesQuery
/// </summary>
public class GetCulinaryPreferencesQueryValidator : AbstractValidator<GetCulinaryPreferencesQuery>
{
    public GetCulinaryPreferencesQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");

        RuleFor(x => x.MinConfidence)
            .InclusiveBetween(0.0m, 1.0m)
            .WithMessage("La confiance minimum doit être entre 0.0 et 1.0");
    }
}

/// <summary>
/// Requête pour obtenir les préférences de table d'un client
/// </summary>
public class GetTablePreferencesQuery : IQuery<TablePreferenceDto?>
{
    public Guid CustomerId { get; init; }
}

/// <summary>
/// Validateur pour GetTablePreferencesQuery
/// </summary>
public class GetTablePreferencesQueryValidator : AbstractValidator<GetTablePreferencesQuery>
{
    public GetTablePreferencesQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");
    }
}

/// <summary>
/// Requête pour obtenir les préférences d'ambiance d'un client
/// </summary>
public class GetAmbiancePreferencesQuery : IQuery<AmbiancePreferenceDto?>
{
    public Guid CustomerId { get; init; }
}

/// <summary>
/// Validateur pour GetAmbiancePreferencesQuery
/// </summary>
public class GetAmbiancePreferencesQueryValidator : AbstractValidator<GetAmbiancePreferencesQuery>
{
    public GetAmbiancePreferencesQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");
    }
}

/// <summary>
/// Requête pour obtenir les conflits de préférences d'un client
/// </summary>
public class GetPreferenceConflictsQuery : IQuery<PreferenceConflictDto?>
{
    public Guid CustomerId { get; init; }
}

/// <summary>
/// Validateur pour GetPreferenceConflictsQuery
/// </summary>
public class GetPreferenceConflictsQueryValidator : AbstractValidator<GetPreferenceConflictsQuery>
{
    public GetPreferenceConflictsQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");
    }
}

/// <summary>
/// Requête pour obtenir les statistiques de préférences
/// </summary>
public class GetPreferenceStatsQuery : IQuery<PreferenceStatsDto>
{
    public PreferenceType? Type { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

/// <summary>
/// Validateur pour GetPreferenceStatsQuery
/// </summary>
public class GetPreferenceStatsQueryValidator : AbstractValidator<GetPreferenceStatsQuery>
{
    public GetPreferenceStatsQueryValidator()
    {
        RuleFor(x => x.FromDate)
            .LessThan(x => x.ToDate)
            .WithMessage("La date de début doit être antérieure à la date de fin")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);
    }
}

/// <summary>
/// Requête pour obtenir les préférences expirant bientôt
/// </summary>
public class GetExpiringPreferencesQuery : IQuery<List<CustomerPreferenceDto>>
{
    public int DaysFromNow { get; init; } = 30;
    public Guid? CustomerId { get; init; }
}

/// <summary>
/// Validateur pour GetExpiringPreferencesQuery
/// </summary>
public class GetExpiringPreferencesQueryValidator : AbstractValidator<GetExpiringPreferencesQuery>
{
    public GetExpiringPreferencesQueryValidator()
    {
        RuleFor(x => x.DaysFromNow)
            .GreaterThan(0)
            .WithMessage("Le nombre de jours doit être positif")
            .LessThanOrEqualTo(365)
            .WithMessage("Le nombre de jours ne peut pas dépasser 365");
    }
}

/// <summary>
/// Requête pour l'analyse comportementale d'un client
/// </summary>
public class GetCustomerBehaviorAnalysisQuery : IQuery<CustomerBehaviorAnalysisDto>
{
    public Guid CustomerId { get; init; }
    public int AnalysisPeriodDays { get; init; } = 90;
    public bool IncludePredictions { get; init; } = true;
    public bool IncludeRecommendations { get; init; } = true;
}

/// <summary>
/// Validateur pour GetCustomerBehaviorAnalysisQuery
/// </summary>
public class GetCustomerBehaviorAnalysisQueryValidator : AbstractValidator<GetCustomerBehaviorAnalysisQuery>
{
    public GetCustomerBehaviorAnalysisQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");

        RuleFor(x => x.AnalysisPeriodDays)
            .InclusiveBetween(7, 365)
            .WithMessage("La période d'analyse doit être entre 7 et 365 jours");
    }
}

/// <summary>
/// Requête pour obtenir les métriques de performance d'un client
/// </summary>
public class GetCustomerPerformanceQuery : IQuery<CustomerPerformanceDto>
{
    public Guid CustomerId { get; init; }
    public DateTime? CalculationDate { get; init; }
}

/// <summary>
/// Validateur pour GetCustomerPerformanceQuery
/// </summary>
public class GetCustomerPerformanceQueryValidator : AbstractValidator<GetCustomerPerformanceQuery>
{
    public GetCustomerPerformanceQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");
    }
}

/// <summary>
/// Requête pour obtenir les recommandations personnalisées d'un client
/// </summary>
public class GetCustomerRecommendationsQuery : IQuery<CustomerRecommendationsDto>
{
    public Guid CustomerId { get; init; }
    public int MaxProductRecommendations { get; init; } = 10;
    public int MaxMarketingActions { get; init; } = 5;
    public int MaxEngagementActions { get; init; } = 3;
    public int MaxNextBestActions { get; init; } = 5;
}

/// <summary>
/// Validateur pour GetCustomerRecommendationsQuery
/// </summary>
public class GetCustomerRecommendationsQueryValidator : AbstractValidator<GetCustomerRecommendationsQuery>
{
    public GetCustomerRecommendationsQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");

        RuleFor(x => x.MaxProductRecommendations)
            .InclusiveBetween(1, 50)
            .WithMessage("Le nombre de recommandations produits doit être entre 1 et 50");

        RuleFor(x => x.MaxMarketingActions)
            .InclusiveBetween(1, 20)
            .WithMessage("Le nombre d'actions marketing doit être entre 1 et 20");

        RuleFor(x => x.MaxEngagementActions)
            .InclusiveBetween(1, 10)
            .WithMessage("Le nombre d'actions d'engagement doit être entre 1 et 10");

        RuleFor(x => x.MaxNextBestActions)
            .InclusiveBetween(1, 20)
            .WithMessage("Le nombre de prochaines meilleures actions doit être entre 1 et 20");
    }
}

/// <summary>
/// Requête pour obtenir le vue d'ensemble analytics des clients
/// </summary>
public class GetCustomerAnalyticsOverviewQuery : IQuery<CustomerAnalyticsOverviewDto>
{
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public bool IncludeTrends { get; init; } = true;
    public int TrendPeriodDays { get; init; } = 30;
}

/// <summary>
/// Validateur pour GetCustomerAnalyticsOverviewQuery
/// </summary>
public class GetCustomerAnalyticsOverviewQueryValidator : AbstractValidator<GetCustomerAnalyticsOverviewQuery>
{
    public GetCustomerAnalyticsOverviewQueryValidator()
    {
        RuleFor(x => x.FromDate)
            .LessThan(x => x.ToDate)
            .WithMessage("La date de début doit être antérieure à la date de fin")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);

        RuleFor(x => x.TrendPeriodDays)
            .InclusiveBetween(7, 365)
            .WithMessage("La période de tendance doit être entre 7 et 365 jours");
    }
}

/// <summary>
/// Requête pour obtenir les clients similaires
/// </summary>
public class GetSimilarCustomersQuery : IQuery<List<CustomerSummaryDto>>
{
    public Guid CustomerId { get; init; }
    public int MaxResults { get; init; } = 10;
    public decimal MinSimilarityScore { get; init; } = 0.7m;
}

/// <summary>
/// Validateur pour GetSimilarCustomersQuery
/// </summary>
public class GetSimilarCustomersQueryValidator : AbstractValidator<GetSimilarCustomersQuery>
{
    public GetSimilarCustomersQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");

        RuleFor(x => x.MaxResults)
            .InclusiveBetween(1, 50)
            .WithMessage("Le nombre maximum de résultats doit être entre 1 et 50");

        RuleFor(x => x.MinSimilarityScore)
            .InclusiveBetween(0.1m, 1.0m)
            .WithMessage("Le score de similarité minimum doit être entre 0.1 et 1.0");
    }
}

/// <summary>
/// Requête pour calculer la probabilité de churn d'un client
/// </summary>
public class GetCustomerChurnProbabilityQuery : IQuery<decimal>
{
    public Guid CustomerId { get; init; }
}

/// <summary>
/// Validateur pour GetCustomerChurnProbabilityQuery
/// </summary>
public class GetCustomerChurnProbabilityQueryValidator : AbstractValidator<GetCustomerChurnProbabilityQuery>
{
    public GetCustomerChurnProbabilityQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");
    }
}

/// <summary>
/// Requête pour prédire la valeur vie client
/// </summary>
public class GetCustomerLifetimeValueQuery : IQuery<decimal>
{
    public Guid CustomerId { get; init; }
    public int PredictionMonths { get; init; } = 12;
}

/// <summary>
/// Validateur pour GetCustomerLifetimeValueQuery
/// </summary>
public class GetCustomerLifetimeValueQueryValidator : AbstractValidator<GetCustomerLifetimeValueQuery>
{
    public GetCustomerLifetimeValueQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");

        RuleFor(x => x.PredictionMonths)
            .InclusiveBetween(1, 60)
            .WithMessage("La période de prédiction doit être entre 1 et 60 mois");
    }
}

/// <summary>
/// Requête pour obtenir les clients à haut potentiel
/// </summary>
public class GetHighPotentialCustomersQuery : IQuery<List<CustomerSummaryDto>>
{
    public decimal MinLifetimeValuePrediction { get; init; } = 1000m;
    public decimal MaxChurnProbability { get; init; } = 0.3m;
    public int MaxResults { get; init; } = 50;
}

/// <summary>
/// Validateur pour GetHighPotentialCustomersQuery
/// </summary>
public class GetHighPotentialCustomersQueryValidator : AbstractValidator<GetHighPotentialCustomersQuery>
{
    public GetHighPotentialCustomersQueryValidator()
    {
        RuleFor(x => x.MinLifetimeValuePrediction)
            .GreaterThan(0)
            .WithMessage("La valeur vie minimum doit être positive");

        RuleFor(x => x.MaxChurnProbability)
            .InclusiveBetween(0.0m, 1.0m)
            .WithMessage("La probabilité de churn maximum doit être entre 0.0 et 1.0");

        RuleFor(x => x.MaxResults)
            .InclusiveBetween(1, 100)
            .WithMessage("Le nombre maximum de résultats doit être entre 1 et 100");
    }
}