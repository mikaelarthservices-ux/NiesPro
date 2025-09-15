namespace Payment.Application.DTOs.Compliance;

public class ComplianceAuditReportDto
{
    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int ViolationCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ComplianceStatusDto
{
    public bool IsCompliant { get; set; }
    public decimal ComplianceScore { get; set; }
    public DateTime LastAuditDate { get; set; }
    public List<string> Issues { get; set; } = new();
}

public class TransactionComplianceDto
{
    public Guid TransactionId { get; set; }
    public bool IsCompliant { get; set; }
    public List<string> ViolationReasons { get; set; } = new();
    public string ValidatedBy { get; set; } = string.Empty;
    public DateTime ValidatedAt { get; set; }
}

public class ComplianceAuditSummaryDto
{
    public Guid Id { get; set; }
    public DateTime AuditDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ViolationCount { get; set; }
    public string AuditorName { get; set; } = string.Empty;
}

public class RealTimeComplianceCheckDto
{
    public decimal ComplianceScore { get; set; }
    public bool IsCompliant { get; set; }
    public List<string> Issues { get; set; } = new();
    public DateTime CheckedAt { get; set; }
    public string InitiatedBy { get; set; } = string.Empty;
}

public class ComplianceMetricsDto
{
    public decimal OverallComplianceScore { get; set; }
    public int TotalTransactions { get; set; }
    public int CompliantTransactions { get; set; }
    public int ViolationsCount { get; set; }
    public string Period { get; set; } = string.Empty;
}

public class ExportComplianceDataResult
{
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}