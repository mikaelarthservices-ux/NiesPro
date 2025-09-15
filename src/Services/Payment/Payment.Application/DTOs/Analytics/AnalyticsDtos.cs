namespace Payment.Application.DTOs.Analytics;

public class PaymentMetricsDto
{
    public decimal TotalAmount { get; set; }
    public int TotalTransactions { get; set; }
    public decimal SuccessRate { get; set; }
    public string Period { get; set; } = string.Empty;
    public Guid? MerchantId { get; set; }
}

public class FraudStatisticsDto
{
    public int TotalFraudCases { get; set; }
    public decimal FraudRate { get; set; }
    public decimal AverageRiskScore { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class FinancialReportDto
{
    public int TotalTransactions { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalFees { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
}

public class PaymentTrendsDto
{
    public string Timeframe { get; set; } = string.Empty;
    public List<TrendDataPoint> DataPoints { get; set; } = new();
}

public class TrendDataPoint
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public int TransactionCount { get; set; }
}

public class PaymentMethodAnalysisDto
{
    public string Period { get; set; } = string.Empty;
    public List<PaymentMethodStats> MethodStats { get; set; } = new();
}

public class PaymentMethodStats
{
    public string MethodType { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal SuccessRate { get; set; }
}

public class RealTimeDashboardDto
{
    public int ActiveTransactions { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal FraudRate { get; set; }
    public List<string> Alerts { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class CustomReportDto
{
    public string ReportName { get; set; } = string.Empty;
    public List<ReportMetric> Metrics { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
}

public class ReportMetric
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
}

public class AnalyticsAlertDto
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsResolved { get; set; }
}

public class ExportAnalyticsDataResult
{
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}