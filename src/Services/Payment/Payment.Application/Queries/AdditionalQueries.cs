using MediatR;
using Payment.Application.DTOs;
using Payment.Application.DTOs.FraudDetection;

namespace Payment.Application.Queries;

/// <summary>
/// Requête pour obtenir les statistiques des moyens de paiement
/// </summary>
public class GetPaymentMethodStatsQuery : IRequest<PaymentMethodStatsDto>
{
    /// <summary>
    /// Identifiant du commerçant
    /// </summary>
    public Guid? MerchantId { get; set; }

    /// <summary>
    /// Identifiant du client
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Date de début
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Date de fin
    /// </summary>
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Requête pour obtenir les statistiques des transactions
/// </summary>
public class GetTransactionStatsQuery : IRequest<TransactionStatsDto>
{
    /// <summary>
    /// Identifiant du commerçant
    /// </summary>
    public Guid? MerchantId { get; set; }

    /// <summary>
    /// Date de début
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Date de fin
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Date de début (alias pour compatibilité)
    /// </summary>
    public DateTime FromDate
    {
        get => StartDate;
        set => StartDate = value;
    }

    /// <summary>
    /// Date de fin (alias pour compatibilité)
    /// </summary>
    public DateTime ToDate
    {
        get => EndDate;
        set => EndDate = value;
    }
}

/// <summary>
/// Requête pour obtenir les transactions par client
/// </summary>
public class GetTransactionsByCustomerQuery : IRequest<PagedResult<TransactionSummaryDto>>
{
    /// <summary>
    /// Identifiant du client
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Page à récupérer
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Nombre d'éléments par page
    /// </summary>
    public int PageSize { get; set; } = 20;

    public GetTransactionsByCustomerQuery(Guid customerId)
    {
        CustomerId = customerId;
    }

    public GetTransactionsByCustomerQuery() { }
}