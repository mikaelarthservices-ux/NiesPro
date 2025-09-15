using MediatR;
using Payment.Application.DTOs.FraudDetection;

namespace Payment.Application.Commands.FraudDetection;

public class AnalyzeTransactionFraudCommand : IRequest<FraudAnalysisResultDto>
{
    public Guid TransactionId { get; set; }
    public string? AnalyzedBy { get; set; }
}

public class UpdateFraudAlertStatusCommand : IRequest<bool>
{
    public Guid AlertId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
    public string? Reason { get; set; }
}

public class AddToWhitelistCommand : IRequest
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? AddedBy { get; set; }
}

public class AddToBlacklistCommand : IRequest
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? AddedBy { get; set; }
}

public class CreateFraudRuleCommand : IRequest<FraudRuleDto>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string? CreatedBy { get; set; }
}

public class UpdateFraudRuleCommand : IRequest<FraudRuleDto?>
{
    public Guid RuleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool IsActive { get; set; }
    public string? UpdatedBy { get; set; }
}

public class DeleteFraudRuleCommand : IRequest<bool>
{
    public Guid RuleId { get; set; }
    public string? DeletedBy { get; set; }
}

public class RunFraudEnginePerformanceTestCommand : IRequest<FraudEnginePerformanceTestDto>
{
    public int TransactionCount { get; set; } = 100;
    public string? InitiatedBy { get; set; }
}