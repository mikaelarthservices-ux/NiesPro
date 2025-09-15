using MediatR;
using Payment.Application.DTOs.Compliance;

namespace Payment.Application.Queries.Compliance;

public class GetComplianceStatusQuery : IRequest<ComplianceStatusDto>
{
}

public class GetAuditHistoryQuery : IRequest<PagedResult<ComplianceAuditSummaryDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetComplianceMetricsQuery : IRequest<ComplianceMetricsDto>
{
    public string Period { get; set; } = "Last30Days";
}