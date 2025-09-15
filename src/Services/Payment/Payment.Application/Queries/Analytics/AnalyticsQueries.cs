using MediatR;
using Payment.Application.DTOs.Analytics;

namespace Payment.Application.Queries.Analytics;

public class GetPaymentMetricsQuery : IRequest<PaymentMetricsDto>
{
    public string Period { get; set; } = "Last30Days";
    public Guid? MerchantId { get; set; }
}

public class GetFraudStatisticsQuery : IRequest<FraudStatisticsDto>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class GetPaymentTrendsQuery : IRequest<PaymentTrendsDto>
{
    public string Timeframe { get; set; } = "Monthly";
}

public class GetPaymentMethodAnalysisQuery : IRequest<PaymentMethodAnalysisDto>
{
    public string Period { get; set; } = "Last30Days";
}

public class GetRealTimeDashboardQuery : IRequest<RealTimeDashboardDto>
{
}

public class GetAnalyticsAlertsQuery : IRequest<List<AnalyticsAlertDto>>
{
    public string? Severity { get; set; }
}