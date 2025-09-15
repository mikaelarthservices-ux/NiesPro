namespace Payment.Application.DTOs.FraudDetection;

public class FraudAnalysisResultDto
{
    public Guid TransactionId { get; set; }
    public decimal RiskScore { get; set; }
    public string Decision { get; set; } = string.Empty;
    public List<string> RiskFactors { get; set; } = new();
    public string AnalyzedBy { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; }
}

public class SuspiciousTransactionDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public decimal RiskScore { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string MerchantName { get; set; } = string.Empty;
}

public class FraudMetricsDto
{
    public string Period { get; set; } = string.Empty;
    public decimal FraudRate { get; set; }
    public int TotalFraudCases { get; set; }
    public decimal AverageRiskScore { get; set; }
    public int BlockedTransactions { get; set; }
}

public class FraudRuleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class FraudEnginePerformanceTestDto
{
    public int TransactionsProcessed { get; set; }
    public double AverageProcessingTimeMs { get; set; }
    public double MaxProcessingTimeMs { get; set; }
    public double MinProcessingTimeMs { get; set; }
    public int ErrorCount { get; set; }
    public DateTime TestStarted { get; set; }
    public DateTime TestCompleted { get; set; }
}