using MediatR;
using Payment.Application.DTOs;
using Payment.Application.DTOs.FraudDetection;

namespace Payment.Application.Queries;

/// <summary>
/// Requête pour obtenir les transactions suspectes
/// </summary>
public class GetSuspiciousTransactionsQuery : IRequest<PagedResult<SuspiciousTransactionDto>>
{
    /// <summary>
    /// Page à récupérer
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Nombre d'éléments par page
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Score de risque minimum
    /// </summary>
    public decimal? MinRiskScore { get; set; }

    /// <summary>
    /// Identifiant du marchand (optionnel)
    /// </summary>
    public Guid? MerchantId { get; set; }

    /// <summary>
    /// Date de début
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Date de fin
    /// </summary>
    public DateTime? EndDate { get; set; }
}