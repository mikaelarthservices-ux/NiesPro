using Microsoft.EntityFrameworkCore;
using NiesPro.Services.Customer.Domain.Entities;
using NiesPro.Services.Customer.Domain.Repositories;
using NiesPro.Services.Customer.Infrastructure.Data;

namespace NiesPro.Services.Customer.Infrastructure.Repositories;

/// <summary>
/// Implémentation du repository CustomerSegment avec segmentation automatique avancée
/// </summary>
public class CustomerSegmentRepository : ICustomerSegmentRepository
{
    private readonly CustomerDbContext _context;
    private readonly CustomerReadDbContext _readContext;

    public CustomerSegmentRepository(
        CustomerDbContext context, 
        CustomerReadDbContext readContext)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
    }

    // ===== CORE OPERATIONS =====

    public async Task<CustomerSegment?> GetByIdAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _context.CustomerSegments
            .FirstOrDefaultAsync(cs => cs.Id == id, cancellationToken);
    }

    public async Task<CustomerSegment?> GetByNameAsync(
        string name, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return await _context.CustomerSegments
            .FirstOrDefaultAsync(cs => cs.Name == name, cancellationToken);
    }

    public async Task<List<CustomerSegment>> GetActiveSegmentsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _readContext.CustomerSegments
            .Where(cs => cs.IsActive)
            .OrderByDescending(cs => cs.Priority)
            .ThenBy(cs => cs.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CustomerSegment>> GetAutomaticSegmentsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _readContext.CustomerSegments
            .Where(cs => cs.IsActive && cs.IsAutomatic)
            .OrderByDescending(cs => cs.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CustomerSegment>> GetSegmentsByTypeAsync(
        SegmentType segmentType,
        CancellationToken cancellationToken = default)
    {
        return await _readContext.CustomerSegments
            .Where(cs => cs.SegmentType == segmentType && cs.IsActive)
            .OrderByDescending(cs => cs.Priority)
            .ThenBy(cs => cs.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _context.CustomerSegments
            .AnyAsync(cs => cs.Id == id, cancellationToken);
    }

    public async Task<bool> IsNameUniqueAsync(
        string name, 
        Guid? excludeSegmentId = null, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var query = _context.CustomerSegments
            .Where(cs => cs.Name == name);

        if (excludeSegmentId.HasValue)
        {
            query = query.Where(cs => cs.Id != excludeSegmentId.Value);
        }

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(
        CustomerSegment segment, 
        CancellationToken cancellationToken = default)
    {
        if (segment == null)
            throw new ArgumentNullException(nameof(segment));

        await _context.CustomerSegments.AddAsync(segment, cancellationToken);
    }

    public void Update(CustomerSegment segment)
    {
        if (segment == null)
            throw new ArgumentNullException(nameof(segment));

        _context.CustomerSegments.Update(segment);
    }

    public void Remove(CustomerSegment segment)
    {
        if (segment == null)
            throw new ArgumentNullException(nameof(segment));

        _context.CustomerSegments.Remove(segment);
    }

    // ===== ADVANCED SEGMENTATION =====

    public async Task<List<CustomerSegment>> GetSegmentsForEvaluationAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        
        return await _readContext.CustomerSegments
            .Where(cs => cs.IsActive && 
                        cs.IsAutomatic &&
                        (cs.NextEvaluationDate == null || cs.NextEvaluationDate <= now))
            .OrderByDescending(cs => cs.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Guid>> GetCustomersInSegmentAsync(
        Guid segmentId,
        CancellationToken cancellationToken = default)
    {
        return await _readContext.Set<Dictionary<string, object>>("CustomerSegmentMemberships")
            .Where(csm => (Guid)csm["SegmentId"] == segmentId && (bool)csm["IsActive"] == true)
            .Select(csm => (Guid)csm["CustomerId"])
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Guid>> GetSegmentsForCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return await _readContext.Set<Dictionary<string, object>>("CustomerSegmentMemberships")
            .Where(csm => (Guid)csm["CustomerId"] == customerId && (bool)csm["IsActive"] == true)
            .Select(csm => (Guid)csm["SegmentId"])
            .ToListAsync(cancellationToken);
    }

    public async Task AddCustomerToSegmentAsync(
        Guid customerId,
        Guid segmentId,
        string assignedBy,
        CancellationToken cancellationToken = default)
    {
        var membership = new Dictionary<string, object>
        {
            ["CustomerId"] = customerId,
            ["SegmentId"] = segmentId,
            ["AssignedDate"] = DateTime.UtcNow,
            ["AssignedBy"] = assignedBy,
            ["IsActive"] = true
        };

        _context.Set<Dictionary<string, object>>("CustomerSegmentMemberships").Add(membership);
    }

    public async Task RemoveCustomerFromSegmentAsync(
        Guid customerId,
        Guid segmentId,
        CancellationToken cancellationToken = default)
    {
        await _context.Set<Dictionary<string, object>>("CustomerSegmentMemberships")
            .Where(csm => (Guid)csm["CustomerId"] == customerId && (Guid)csm["SegmentId"] == segmentId)
            .ExecuteUpdateAsync(csm => csm.SetProperty("IsActive", false), cancellationToken);
    }

    public async Task<SegmentStatsResult> GetSegmentStatsAsync(
        Guid segmentId,
        CancellationToken cancellationToken = default)
    {
        var segment = await _readContext.CustomerSegments
            .FirstOrDefaultAsync(cs => cs.Id == segmentId, cancellationToken);

        if (segment == null)
            return new SegmentStatsResult();

        return new SegmentStatsResult
        {
            SegmentId = segmentId,
            SegmentName = segment.Name,
            SegmentType = segment.SegmentType.ToString(),
            MemberCount = segment.CurrentMemberCount,
            AverageCustomerValue = segment.AverageCustomerValue,
            ChurnRate = segment.ChurnRate ?? 0,
            ConversionRate = segment.ConversionRate ?? 0
        };
    }

    // ===== BULK OPERATIONS =====

    public async Task<int> BulkUpdateMemberCountsAsync(
        Dictionary<Guid, int> memberCounts,
        CancellationToken cancellationToken = default)
    {
        if (!memberCounts.Any())
            return 0;

        var affectedCount = 0;
        
        foreach (var update in memberCounts)
        {
            var count = await _context.CustomerSegments
                .Where(cs => cs.Id == update.Key)
                .ExecuteUpdateAsync(cs => cs.SetProperty(x => x.CurrentMemberCount, update.Value), 
                    cancellationToken);
            affectedCount += count;
        }

        return affectedCount;
    }

    public async Task<int> BulkUpdateNextEvaluationDatesAsync(
        Dictionary<Guid, DateTime> evaluationDates,
        CancellationToken cancellationToken = default)
    {
        if (!evaluationDates.Any())
            return 0;

        var affectedCount = 0;
        
        foreach (var update in evaluationDates)
        {
            var count = await _context.CustomerSegments
                .Where(cs => cs.Id == update.Key)
                .ExecuteUpdateAsync(cs => cs.SetProperty(x => x.NextEvaluationDate, update.Value), 
                    cancellationToken);
            affectedCount += count;
        }

        return affectedCount;
    }
}

/// <summary>
/// Implémentation du repository CustomerInteraction avec analytics avancées
/// </summary>
public class CustomerInteractionRepository : ICustomerInteractionRepository
{
    private readonly CustomerDbContext _context;
    private readonly CustomerReadDbContext _readContext;

    public CustomerInteractionRepository(
        CustomerDbContext context, 
        CustomerReadDbContext readContext)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
    }

    // ===== CORE OPERATIONS =====

    public async Task<CustomerInteraction?> GetByIdAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _context.CustomerInteractions
            .FirstOrDefaultAsync(ci => ci.Id == id, cancellationToken);
    }

    public async Task<List<CustomerInteraction>> GetByCustomerIdAsync(
        Guid customerId,
        int count = 50,
        CancellationToken cancellationToken = default)
    {
        return await _readContext.CustomerInteractions
            .Where(ci => ci.CustomerId == customerId)
            .OrderByDescending(ci => ci.InteractionDate)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<CustomerInteraction> Items, int TotalCount)> SearchInteractionsAsync(
        Guid? customerId = null,
        InteractionType? interactionType = null,
        InteractionChannel? channel = null,
        InteractionStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? agentId = null,
        bool? requiresFollowUp = null,
        int page = 1,
        int pageSize = 20,
        string sortBy = "InteractionDate",
        bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        var query = _readContext.CustomerInteractions.AsQueryable();

        // ===== FILTRES =====
        if (customerId.HasValue)
        {
            query = query.Where(ci => ci.CustomerId == customerId.Value);
        }

        if (interactionType.HasValue)
        {
            query = query.Where(ci => ci.InteractionType == interactionType.Value);
        }

        if (channel.HasValue)
        {
            query = query.Where(ci => ci.Channel == channel.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(ci => ci.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(ci => ci.InteractionDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(ci => ci.InteractionDate <= toDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(agentId))
        {
            query = query.Where(ci => ci.AgentId == agentId);
        }

        if (requiresFollowUp.HasValue)
        {
            query = query.Where(ci => ci.RequiresFollowUp == requiresFollowUp.Value);
        }

        // ===== COMPTAGE =====
        var totalCount = await query.CountAsync(cancellationToken);

        // ===== TRI =====
        query = ApplyInteractionSorting(query, sortBy, sortDescending);

        // ===== PAGINATION =====
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<List<CustomerInteraction>> GetPendingFollowUpsAsync(
        DateTime? dueBefore = null,
        string? agentId = null,
        CancellationToken cancellationToken = default)
    {
        var dueDate = dueBefore ?? DateTime.UtcNow.AddDays(1);

        var query = _readContext.CustomerInteractions
            .Where(ci => ci.RequiresFollowUp && 
                        ci.FollowUpCompletedDate == null &&
                        ci.FollowUpDate <= dueDate);

        if (!string.IsNullOrWhiteSpace(agentId))
        {
            query = query.Where(ci => ci.FollowUpBy == agentId);
        }

        return await query
            .OrderBy(ci => ci.FollowUpDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _context.CustomerInteractions
            .AnyAsync(ci => ci.Id == id, cancellationToken);
    }

    public async Task AddAsync(
        CustomerInteraction interaction, 
        CancellationToken cancellationToken = default)
    {
        if (interaction == null)
            throw new ArgumentNullException(nameof(interaction));

        await _context.CustomerInteractions.AddAsync(interaction, cancellationToken);
    }

    public void Update(CustomerInteraction interaction)
    {
        if (interaction == null)
            throw new ArgumentNullException(nameof(interaction));

        _context.CustomerInteractions.Update(interaction);
    }

    public void Remove(CustomerInteraction interaction)
    {
        if (interaction == null)
            throw new ArgumentNullException(nameof(interaction));

        _context.CustomerInteractions.Remove(interaction);
    }

    // ===== ANALYTICS =====

    public async Task<InteractionStatsResult> GetInteractionStatsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;

        var stats = await _readContext.CustomerInteractions
            .Where(ci => ci.InteractionDate >= from && ci.InteractionDate <= to)
            .GroupBy(ci => 1)
            .Select(g => new InteractionStatsResult
            {
                TotalInteractions = g.Count(),
                UniqueCustomers = g.Select(ci => ci.CustomerId).Distinct().Count(),
                AverageDuration = g.Where(ci => ci.Duration.HasValue).Average(ci => ci.Duration.Value),
                ResolvedInteractions = g.Count(ci => ci.Status == InteractionStatus.Closed),
                PendingFollowUps = g.Count(ci => ci.RequiresFollowUp && ci.FollowUpCompletedDate == null),
                AverageSatisfaction = g.Where(ci => ci.SatisfactionScore.HasValue)
                                      .Average(ci => ci.SatisfactionScore.Value)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return stats ?? new InteractionStatsResult();
    }

    public async Task<List<ChannelStatsResult>> GetChannelStatsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;

        return await _readContext.CustomerInteractions
            .Where(ci => ci.InteractionDate >= from && ci.InteractionDate <= to)
            .GroupBy(ci => ci.Channel)
            .Select(g => new ChannelStatsResult
            {
                Channel = g.Key.ToString(),
                InteractionCount = g.Count(),
                UniqueCustomers = g.Select(ci => ci.CustomerId).Distinct().Count(),
                AverageDuration = g.Where(ci => ci.Duration.HasValue).Average(ci => ci.Duration.Value),
                ResolutionRate = (decimal)g.Count(ci => ci.Status == InteractionStatus.Closed) / g.Count() * 100,
                AverageSatisfaction = g.Where(ci => ci.SatisfactionScore.HasValue)
                                      .Average(ci => ci.SatisfactionScore.Value)
            })
            .OrderByDescending(cs => cs.InteractionCount)
            .ToListAsync(cancellationToken);
    }

    // ===== HELPER METHODS =====

    private static IQueryable<CustomerInteraction> ApplyInteractionSorting(
        IQueryable<CustomerInteraction> query,
        string sortBy,
        bool sortDescending)
    {
        return sortBy.ToLowerInvariant() switch
        {
            "interactiondate" => sortDescending ? query.OrderByDescending(ci => ci.InteractionDate) : query.OrderBy(ci => ci.InteractionDate),
            "interactiontype" => sortDescending ? query.OrderByDescending(ci => ci.InteractionType) : query.OrderBy(ci => ci.InteractionType),
            "channel" => sortDescending ? query.OrderByDescending(ci => ci.Channel) : query.OrderBy(ci => ci.Channel),
            "status" => sortDescending ? query.OrderByDescending(ci => ci.Status) : query.OrderBy(ci => ci.Status),
            "priority" => sortDescending ? query.OrderByDescending(ci => ci.Priority) : query.OrderBy(ci => ci.Priority),
            "duration" => sortDescending ? query.OrderByDescending(ci => ci.Duration) : query.OrderBy(ci => ci.Duration),
            "satisfactionscore" => sortDescending ? query.OrderByDescending(ci => ci.SatisfactionScore) : query.OrderBy(ci => ci.SatisfactionScore),
            "followupdate" => sortDescending ? query.OrderByDescending(ci => ci.FollowUpDate) : query.OrderBy(ci => ci.FollowUpDate),
            _ => sortDescending ? query.OrderByDescending(ci => ci.InteractionDate) : query.OrderBy(ci => ci.InteractionDate)
        };
    }
}

/// <summary>
/// Implémentation du repository CustomerPreference avec machine learning
/// </summary>
public class CustomerPreferenceRepository : ICustomerPreferenceRepository
{
    private readonly CustomerDbContext _context;
    private readonly CustomerReadDbContext _readContext;

    public CustomerPreferenceRepository(
        CustomerDbContext context, 
        CustomerReadDbContext readContext)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
    }

    // ===== CORE OPERATIONS =====

    public async Task<CustomerPreference?> GetByIdAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _context.CustomerPreferences
            .FirstOrDefaultAsync(cp => cp.Id == id, cancellationToken);
    }

    public async Task<List<CustomerPreference>> GetByCustomerIdAsync(
        Guid customerId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = _readContext.CustomerPreferences
            .Where(cp => cp.CustomerId == customerId);

        if (activeOnly)
        {
            query = query.Where(cp => cp.IsActive && 
                                     (cp.ExpiryDate == null || cp.ExpiryDate > DateTime.UtcNow));
        }

        return await query
            .OrderByDescending(cp => cp.Priority)
            .ThenByDescending(cp => cp.Confidence)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CustomerPreference>> GetByTypeAsync(
        Guid customerId,
        PreferenceType preferenceType,
        CancellationToken cancellationToken = default)
    {
        return await _readContext.CustomerPreferences
            .Where(cp => cp.CustomerId == customerId && 
                        cp.PreferenceType == preferenceType &&
                        cp.IsActive)
            .OrderByDescending(cp => cp.Priority)
            .ThenByDescending(cp => cp.Confidence)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CustomerPreference>> GetInferredPreferencesAsync(
        Guid customerId,
        decimal minimumConfidence = 0.7m,
        CancellationToken cancellationToken = default)
    {
        return await _readContext.CustomerPreferences
            .Where(cp => cp.CustomerId == customerId && 
                        cp.IsInferred &&
                        cp.IsActive &&
                        cp.Confidence >= minimumConfidence)
            .OrderByDescending(cp => cp.Confidence)
            .ThenByDescending(cp => cp.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _context.CustomerPreferences
            .AnyAsync(cp => cp.Id == id, cancellationToken);
    }

    public async Task AddAsync(
        CustomerPreference preference, 
        CancellationToken cancellationToken = default)
    {
        if (preference == null)
            throw new ArgumentNullException(nameof(preference));

        await _context.CustomerPreferences.AddAsync(preference, cancellationToken);
    }

    public void Update(CustomerPreference preference)
    {
        if (preference == null)
            throw new ArgumentNullException(nameof(preference));

        _context.CustomerPreferences.Update(preference);
    }

    public void Remove(CustomerPreference preference)
    {
        if (preference == null)
            throw new ArgumentNullException(nameof(preference));

        _context.CustomerPreferences.Remove(preference);
    }

    // ===== ADVANCED ANALYTICS =====

    public async Task<List<PreferenceAnalyticsResult>> GetPreferenceAnalyticsAsync(
        PreferenceType? preferenceType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-90);
        var to = toDate ?? DateTime.UtcNow;

        var query = _readContext.CustomerPreferences
            .Where(cp => cp.CreatedDate >= from && cp.CreatedDate <= to);

        if (preferenceType.HasValue)
        {
            query = query.Where(cp => cp.PreferenceType == preferenceType.Value);
        }

        return await query
            .GroupBy(cp => new { cp.PreferenceType, cp.Category })
            .Select(g => new PreferenceAnalyticsResult
            {
                PreferenceType = g.Key.PreferenceType.ToString(),
                Category = g.Key.Category ?? "Unknown",
                TotalCustomers = g.Select(cp => cp.CustomerId).Distinct().Count(),
                AverageConfidence = g.Average(cp => cp.Confidence),
                ExplicitPreferences = g.Count(cp => !cp.IsInferred),
                InferredPreferences = g.Count(cp => cp.IsInferred),
                AverageUsageCount = g.Average(cp => cp.UsageCount),
                AverageSuccessRate = g.Where(cp => cp.SuccessRate.HasValue).Average(cp => cp.SuccessRate.Value)
            })
            .OrderByDescending(pa => pa.TotalCustomers)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CustomerPreference>> GetExpiredPreferencesAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _readContext.CustomerPreferences
            .Where(cp => cp.IsActive && 
                        cp.ExpiryDate.HasValue && 
                        cp.ExpiryDate.Value <= now)
            .ToListAsync(cancellationToken);
    }

    // ===== BULK OPERATIONS =====

    public async Task<int> BulkUpdateUsageStatsAsync(
        Dictionary<Guid, (int usageCount, decimal? successRate)> usageStats,
        CancellationToken cancellationToken = default)
    {
        if (!usageStats.Any())
            return 0;

        var affectedCount = 0;
        
        foreach (var update in usageStats)
        {
            var count = await _context.CustomerPreferences
                .Where(cp => cp.Id == update.Key)
                .ExecuteUpdateAsync(cp => cp
                    .SetProperty(x => x.UsageCount, update.Value.usageCount)
                    .SetProperty(x => x.SuccessRate, update.Value.successRate)
                    .SetProperty(x => x.LastUsedDate, DateTime.UtcNow), 
                    cancellationToken);
            affectedCount += count;
        }

        return affectedCount;
    }

    public async Task<int> BulkDeactivateExpiredPreferencesAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _context.CustomerPreferences
            .Where(cp => cp.IsActive && 
                        cp.ExpiryDate.HasValue && 
                        cp.ExpiryDate.Value <= now)
            .ExecuteUpdateAsync(cp => cp.SetProperty(x => x.IsActive, false), 
                cancellationToken);
    }
}

// ===== RESULT MODELS =====

public class SegmentStatsResult
{
    public Guid SegmentId { get; set; }
    public string SegmentName { get; set; } = string.Empty;
    public string SegmentType { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public decimal AverageCustomerValue { get; set; }
    public decimal ChurnRate { get; set; }
    public decimal ConversionRate { get; set; }
}

public class InteractionStatsResult
{
    public int TotalInteractions { get; set; }
    public int UniqueCustomers { get; set; }
    public double? AverageDuration { get; set; }
    public int ResolvedInteractions { get; set; }
    public int PendingFollowUps { get; set; }
    public decimal? AverageSatisfaction { get; set; }
}

public class ChannelStatsResult
{
    public string Channel { get; set; } = string.Empty;
    public int InteractionCount { get; set; }
    public int UniqueCustomers { get; set; }
    public double? AverageDuration { get; set; }
    public decimal ResolutionRate { get; set; }
    public decimal? AverageSatisfaction { get; set; }
}

public class PreferenceAnalyticsResult
{
    public string PreferenceType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int TotalCustomers { get; set; }
    public decimal AverageConfidence { get; set; }
    public int ExplicitPreferences { get; set; }
    public int InferredPreferences { get; set; }
    public double AverageUsageCount { get; set; }
    public decimal? AverageSuccessRate { get; set; }
}