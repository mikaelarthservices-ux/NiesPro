using MediatR;
using Payment.Application.DTOs;
using Payment.Domain.Enums;

namespace Payment.Application.Queries;

/// <summary>
/// Requête pour obtenir un paiement par son identifiant
/// </summary>
public class GetPaymentByIdQuery : IRequest<PaymentDetailDto?>
{
    /// <summary>
    /// Identifiant du paiement
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Inclure les transactions associées
    /// </summary>
    public bool IncludeTransactions { get; set; } = true;

    /// <summary>
    /// Inclure les moyens de paiement utilisés
    /// </summary>
    public bool IncludePaymentMethods { get; set; } = false;

    public GetPaymentByIdQuery(Guid paymentId)
    {
        PaymentId = paymentId;
    }
}

/// <summary>
/// Requête pour obtenir un paiement par son numéro
/// </summary>
public class GetPaymentByNumberQuery : IRequest<PaymentDetailDto?>
{
    /// <summary>
    /// Numéro du paiement
    /// </summary>
    public string PaymentNumber { get; set; }

    /// <summary>
    /// Inclure les transactions associées
    /// </summary>
    public bool IncludeTransactions { get; set; } = true;

    public GetPaymentByNumberQuery(string paymentNumber)
    {
        PaymentNumber = paymentNumber;
    }
}

/// <summary>
/// Requête pour obtenir les paiements d'une commande
/// </summary>
public class GetPaymentsByOrderIdQuery : IRequest<List<PaymentSummaryDto>>
{
    /// <summary>
    /// Identifiant de la commande
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Inclure les paiements annulés
    /// </summary>
    public bool IncludeCancelled { get; set; } = false;

    public GetPaymentsByOrderIdQuery(Guid orderId)
    {
        OrderId = orderId;
    }
}

/// <summary>
/// Requête pour obtenir les paiements d'un client
/// </summary>
public class GetPaymentsByCustomerQuery : IRequest<Payment.Application.DTOs.PagedResult<PaymentSummaryDto>>
{
    /// <summary>
    /// Identifiant du client
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Page à récupérer (base 1)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Nombre d'éléments par page
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Filtrer par statut
    /// </summary>
    public PaymentStatus? Status { get; set; }

    /// <summary>
    /// Date de début pour le filtrage
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Date de fin pour le filtrage
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Montant minimum
    /// </summary>
    public decimal? MinAmount { get; set; }

    /// <summary>
    /// Montant maximum
    /// </summary>
    public decimal? MaxAmount { get; set; }

    /// <summary>
    /// Devise pour les filtres de montant
    /// </summary>
    public string? Currency { get; set; }

    public GetPaymentsByCustomerQuery(Guid customerId)
    {
        CustomerId = customerId;
    }
}

/// <summary>
/// Requête pour obtenir les statistiques de paiement d'un commerçant
/// </summary>
public class GetMerchantPaymentStatsQuery : IRequest<MerchantPaymentStatsDto>
{
    /// <summary>
    /// Identifiant du commerçant
    /// </summary>
    public Guid MerchantId { get; set; }

    /// <summary>
    /// Date de début pour les statistiques
    /// </summary>
    public DateTime FromDate { get; set; }

    /// <summary>
    /// Date de fin pour les statistiques
    /// </summary>
    public DateTime ToDate { get; set; }

    /// <summary>
    /// Devise pour les statistiques
    /// </summary>
    public string Currency { get; set; } = "EUR";

    /// <summary>
    /// Grouper par période
    /// </summary>
    public StatsPeriod GroupBy { get; set; } = StatsPeriod.Today;

    public GetMerchantPaymentStatsQuery(Guid merchantId, DateTime fromDate, DateTime toDate)
    {
        MerchantId = merchantId;
        FromDate = fromDate;
        ToDate = toDate;
    }
}

/// <summary>
/// Requête pour obtenir une transaction par son identifiant
/// </summary>
public class GetTransactionByIdQuery : IRequest<TransactionDetailDto?>
{
    /// <summary>
    /// Identifiant de la transaction
    /// </summary>
    public Guid TransactionId { get; set; }

    /// <summary>
    /// Inclure le paiement parent
    /// </summary>
    public bool IncludePayment { get; set; } = false;

    /// <summary>
    /// Inclure les transactions enfants (remboursements)
    /// </summary>
    public bool IncludeChildTransactions { get; set; } = true;

    public GetTransactionByIdQuery(Guid transactionId)
    {
        TransactionId = transactionId;
    }
}

/// <summary>
/// Requête pour obtenir les transactions d'un paiement
/// </summary>
public class GetTransactionsByPaymentQuery : IRequest<List<TransactionSummaryDto>>
{
    /// <summary>
    /// Identifiant du paiement
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Filtrer par type de transaction
    /// </summary>
    public TransactionType? Type { get; set; }

    /// <summary>
    /// Filtrer par statut
    /// </summary>
    public PaymentStatus? Status { get; set; }

    public GetTransactionsByPaymentQuery(Guid paymentId)
    {
        PaymentId = paymentId;
    }
}

/// <summary>
/// Requête pour obtenir les moyens de paiement d'un client
/// </summary>
public class GetPaymentMethodsByCustomerQuery : IRequest<List<PaymentMethodDto>>
{
    /// <summary>
    /// Identifiant du client
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Inclure les moyens de paiement inactifs
    /// </summary>
    public bool IncludeInactive { get; set; } = false;

    /// <summary>
    /// Filtrer par type de moyen de paiement
    /// </summary>
    public PaymentMethodType? Type { get; set; }

    public GetPaymentMethodsByCustomerQuery(Guid customerId)
    {
        CustomerId = customerId;
    }
}

/// <summary>
/// Requête pour obtenir un moyen de paiement par son identifiant
/// </summary>
public class GetPaymentMethodByIdQuery : IRequest<PaymentMethodDetailDto?>
{
    /// <summary>
    /// Identifiant du moyen de paiement
    /// </summary>
    public Guid PaymentMethodId { get; set; }

    /// <summary>
    /// Inclure les statistiques d'utilisation
    /// </summary>
    public bool IncludeUsageStats { get; set; } = false;

    public GetPaymentMethodByIdQuery(Guid paymentMethodId)
    {
        PaymentMethodId = paymentMethodId;
    }
}

/// <summary>
/// Requête pour rechercher des paiements avec des critères avancés
/// </summary>
public class SearchPaymentsQuery : IRequest<Payment.Application.DTOs.PagedResult<PaymentSummaryDto>>
{
    /// <summary>
    /// Terme de recherche (numéro de paiement, numéro de commande, etc.)
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Identifiant du commerçant
    /// </summary>
    public Guid? MerchantId { get; set; }

    /// <summary>
    /// Identifiant du client
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Statuts à inclure
    /// </summary>
    public List<PaymentStatus>? Statuses { get; set; }

    /// <summary>
    /// Types de moyen de paiement
    /// </summary>
    public List<PaymentMethodType>? PaymentMethodTypes { get; set; }

    /// <summary>
    /// Date de début
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Date de fin
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Montant minimum
    /// </summary>
    public decimal? MinAmount { get; set; }

    /// <summary>
    /// Montant maximum
    /// </summary>
    public decimal? MaxAmount { get; set; }

    /// <summary>
    /// Devise
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Page à récupérer
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Nombre d'éléments par page
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Champ de tri
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Ordre de tri (asc/desc)
    /// </summary>
    public string? SortOrder { get; set; } = "desc";
}

/// <summary>
/// Résultat paginé générique
/// </summary>
/// <typeparam name="T">Type des éléments</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Éléments de la page courante
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Nombre total d'éléments
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Page courante
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Taille de la page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Nombre total de pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Indique s'il y a une page précédente
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Indique s'il y a une page suivante
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
}