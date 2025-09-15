using MediatR;
using Payment.Application.DTOs.FraudDetection;

namespace Payment.Application.Queries.FraudDetection;

public class GetSuspiciousTransactionsQuery : IRequest<PagedResult<SuspiciousTransactionDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public decimal? MinRiskScore { get; set; }
    public string? Status { get; set; }
}

public class GetFraudMetricsQuery : IRequest<FraudMetricsDto>
{
    public string Period { get; set; } = "Last30Days";
}

public class GetFraudRulesQuery : IRequest<List<FraudRuleDto>>
{
}