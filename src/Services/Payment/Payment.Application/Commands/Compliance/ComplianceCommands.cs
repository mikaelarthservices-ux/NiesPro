using MediatR;
using Payment.Application.DTOs.Compliance;

namespace Payment.Application.Commands.Compliance;

public class GenerateAuditReportCommand : IRequest<ComplianceAuditReportDto>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? RequestedBy { get; set; }
    public string ReportType { get; set; } = "Standard";
}

public class ValidateTransactionComplianceCommand : IRequest<TransactionComplianceDto?>
{
    public Guid TransactionId { get; set; }
    public string? ValidatedBy { get; set; }
}

public class RealTimeComplianceCheckCommand : IRequest<RealTimeComplianceCheckDto>
{
    public string? InitiatedBy { get; set; }
    public List<Guid>? TransactionIds { get; set; }
}

public class ExportComplianceDataCommand : IRequest<ExportComplianceDataResult>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Format { get; set; } = "PDF";
    public string? RequestedBy { get; set; }
}

public class ConfigureComplianceAlertsCommand : IRequest
{
    public string? ConfiguredBy { get; set; }
    public List<AlertConfiguration> AlertConfigurations { get; set; } = new();
}

public class AlertConfiguration
{
    public string AlertType { get; set; } = string.Empty;
    public decimal Threshold { get; set; }
    public bool IsEnabled { get; set; }
}