using BuildingBlocks.Application.Queries;
using Customer.Application.DTOs;
using Customer.Domain.Enums;
using FluentValidation;

namespace Customer.Application.Queries;

/// <summary>
/// Requête pour obtenir un client par ID
/// </summary>
public class GetCustomerByIdQuery : IQuery<CustomerDto?>
{
    public Guid CustomerId { get; init; }
    public bool IncludeProfile { get; init; } = true;
    public bool IncludeLoyaltyStats { get; init; } = true;
    public bool IncludeSegments { get; init; } = true;
}

/// <summary>
/// Validateur pour GetCustomerByIdQuery
/// </summary>
public class GetCustomerByIdQueryValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");
    }
}

/// <summary>
/// Requête pour obtenir un client par email
/// </summary>
public class GetCustomerByEmailQuery : IQuery<CustomerDto?>
{
    public required string Email { get; init; }
    public bool IncludeProfile { get; init; } = true;
    public bool IncludeLoyaltyStats { get; init; } = true;
    public bool IncludeSegments { get; init; } = true;
}

/// <summary>
/// Validateur pour GetCustomerByEmailQuery
/// </summary>
public class GetCustomerByEmailQueryValidator : AbstractValidator<GetCustomerByEmailQuery>
{
    public GetCustomerByEmailQueryValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("L'email est obligatoire")
            .EmailAddress()
            .WithMessage("Format d'email invalide");
    }
}

/// <summary>
/// Requête pour rechercher des clients
/// </summary>
public class SearchCustomersQuery : IQuery<PagedResult<CustomerSummaryDto>>
{
    public string? SearchTerm { get; init; }
    public CustomerStatus? Status { get; init; }
    public DateTime? RegisteredAfter { get; init; }
    public DateTime? RegisteredBefore { get; init; }
    public bool? HasPhone { get; init; }
    public bool? IsEmailVerified { get; init; }
    public List<Guid>? SegmentIds { get; init; }
    public int? MinLoyaltyPoints { get; init; }
    public int? MaxLoyaltyPoints { get; init; }
    public string? SortBy { get; init; } = "RegistrationDate";
    public bool SortDescending { get; init; } = true;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

/// <summary>
/// Validateur pour SearchCustomersQuery
/// </summary>
public class SearchCustomersQueryValidator : AbstractValidator<SearchCustomersQuery>
{
    public SearchCustomersQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Le numéro de page doit être positif");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("La taille de page doit être entre 1 et 100");

        RuleFor(x => x.RegisteredAfter)
            .LessThan(x => x.RegisteredBefore)
            .WithMessage("La date de début doit être antérieure à la date de fin")
            .When(x => x.RegisteredAfter.HasValue && x.RegisteredBefore.HasValue);

        RuleFor(x => x.MinLoyaltyPoints)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Le minimum de points doit être positif")
            .LessThanOrEqualTo(x => x.MaxLoyaltyPoints ?? int.MaxValue)
            .WithMessage("Le minimum doit être inférieur au maximum")
            .When(x => x.MinLoyaltyPoints.HasValue);

        RuleFor(x => x.MaxLoyaltyPoints)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Le maximum de points doit être positif")
            .When(x => x.MaxLoyaltyPoints.HasValue);

        RuleFor(x => x.SortBy)
            .Must(sortBy => new[] { "RegistrationDate", "LastVisitDate", "Email", "FullName", "TotalLoyaltyPoints" }
                .Contains(sortBy, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Critère de tri invalide")
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy));
    }
}

/// <summary>
/// Requête pour obtenir les statistiques clients
/// </summary>
public class GetCustomerStatsQuery : IQuery<CustomerStatsDto>
{
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public List<Guid>? SegmentIds { get; init; }
    public bool IncludeRegistrationTrends { get; init; } = true;
}

/// <summary>
/// Validateur pour GetCustomerStatsQuery
/// </summary>
public class GetCustomerStatsQueryValidator : AbstractValidator<GetCustomerStatsQuery>
{
    public GetCustomerStatsQueryValidator()
    {
        RuleFor(x => x.FromDate)
            .LessThan(x => x.ToDate)
            .WithMessage("La date de début doit être antérieure à la date de fin")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);
    }
}

/// <summary>
/// Requête pour obtenir le profil d'un client
/// </summary>
public class GetCustomerProfileQuery : IQuery<CustomerProfileDto?>
{
    public Guid CustomerId { get; init; }
}

/// <summary>
/// Validateur pour GetCustomerProfileQuery
/// </summary>
public class GetCustomerProfileQueryValidator : AbstractValidator<GetCustomerProfileQuery>
{
    public GetCustomerProfileQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant du client est obligatoire");
    }
}

/// <summary>
/// Requête pour obtenir les clients avec des profils incomplets
/// </summary>
public class GetIncompleteProfilesQuery : IQuery<PagedResult<CustomerSummaryDto>>
{
    public decimal MinCompletionPercentage { get; init; } = 50m;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

/// <summary>
/// Validateur pour GetIncompleteProfilesQuery
/// </summary>
public class GetIncompleteProfilesQueryValidator : AbstractValidator<GetIncompleteProfilesQuery>
{
    public GetIncompleteProfilesQueryValidator()
    {
        RuleFor(x => x.MinCompletionPercentage)
            .InclusiveBetween(0m, 100m)
            .WithMessage("Le pourcentage de complétion doit être entre 0 et 100");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Le numéro de page doit être positif");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("La taille de page doit être entre 1 et 100");
    }
}

/// <summary>
/// Requête pour obtenir les clients avec des anniversaires
/// </summary>
public class GetCustomerBirthdaysQuery : IQuery<List<CustomerSummaryDto>>
{
    public int DaysFromNow { get; init; } = 0;
    public int DaysRange { get; init; } = 7;
}

/// <summary>
/// Validateur pour GetCustomerBirthdaysQuery
/// </summary>
public class GetCustomerBirthdaysQueryValidator : AbstractValidator<GetCustomerBirthdaysQuery>
{
    public GetCustomerBirthdaysQueryValidator()
    {
        RuleFor(x => x.DaysFromNow)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Le nombre de jours ne peut pas être négatif");

        RuleFor(x => x.DaysRange)
            .InclusiveBetween(1, 365)
            .WithMessage("La plage de jours doit être entre 1 et 365");
    }
}

/// <summary>
/// Requête pour obtenir les clients inactifs
/// </summary>
public class GetInactiveCustomersQuery : IQuery<PagedResult<CustomerSummaryDto>>
{
    public int DaysInactive { get; init; } = 90;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

/// <summary>
/// Validateur pour GetInactiveCustomersQuery
/// </summary>
public class GetInactiveCustomersQueryValidator : AbstractValidator<GetInactiveCustomersQuery>
{
    public GetInactiveCustomersQueryValidator()
    {
        RuleFor(x => x.DaysInactive)
            .GreaterThan(0)
            .WithMessage("Le nombre de jours d'inactivité doit être positif");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Le numéro de page doit être positif");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("La taille de page doit être entre 1 et 100");
    }
}

/// <summary>
/// Requête pour vérifier l'unicité d'un email
/// </summary>
public class CheckEmailUniquenessQuery : IQuery<bool>
{
    public required string Email { get; init; }
    public Guid? ExcludeCustomerId { get; init; }
}

/// <summary>
/// Validateur pour CheckEmailUniquenessQuery
/// </summary>
public class CheckEmailUniquenessQueryValidator : AbstractValidator<CheckEmailUniquenessQuery>
{
    public CheckEmailUniquenessQueryValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("L'email est obligatoire")
            .EmailAddress()
            .WithMessage("Format d'email invalide");
    }
}

/// <summary>
/// Classe utilitaire pour les résultats paginés
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; init; } = new();
    public int TotalItems { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}