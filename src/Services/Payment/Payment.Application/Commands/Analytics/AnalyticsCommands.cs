using MediatR;
using Payment.Application.DTOs.Analytics;

namespace Payment.Application.Commands.Analytics;

public class GenerateFinancialReportCommand : IRequest<FinancialReportDto>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? GeneratedBy { get; set; }
    public Guid? MerchantId { get; set; }
}

public class GenerateCustomReportCommand : IRequest<CustomReportDto>
{
    public string ReportName { get; set; } = string.Empty;
    public List<string>? Metrics { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? GeneratedBy { get; set; }
}

public class ExportAnalyticsDataCommand : IRequest<ExportAnalyticsDataResult>
{
    public string ReportType { get; set; } = string.Empty;
    public string Format { get; set; } = "PDF";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? RequestedBy { get; set; }
}

public class ConfigureAlertThresholdsCommand : IRequest
{
    public string? ConfiguredBy { get; set; }
    public List<AlertThreshold> Thresholds { get; set; } = new();
}

public class AlertThreshold
{
    public string MetricName { get; set; } = string.Empty;
    public decimal ThresholdValue { get; set; }
    public string Severity { get; set; } = string.Empty;
}