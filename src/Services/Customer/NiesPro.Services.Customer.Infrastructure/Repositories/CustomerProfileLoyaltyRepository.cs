using Microsoft.EntityFrameworkCore;
using NiesPro.Services.Customer.Domain.Entities;
using NiesPro.Services.Customer.Domain.Repositories;
using NiesPro.Services.Customer.Infrastructure.Data;

namespace NiesPro.Services.Customer.Infrastructure.Repositories;

/// <summary>
/// Implémentation sophistiquée du repository CustomerProfile avec analytics avancées
/// Support : Calculs CLV, Churn Prediction, Behavioral Analytics
/// </summary>
public class CustomerProfileRepository : ICustomerProfileRepository
{
    private readonly CustomerDbContext _context;
    private readonly CustomerReadDbContext _readContext;

    public CustomerProfileRepository(
        CustomerDbContext context, 
        CustomerReadDbContext readContext)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
    }

    // ===== CORE OPERATIONS =====

    public async Task<CustomerProfile?> GetByIdAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _context.CustomerProfiles
            .Include(cp => cp.CommunicationSettings)
            .Include(cp => cp.RiskAssessment)
            .FirstOrDefaultAsync(cp => cp.Id == id, cancellationToken);
    }

    public async Task<CustomerProfile?> GetByCustomerIdAsync(
        Guid customerId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.CustomerProfiles
            .Include(cp => cp.CommunicationSettings)
            .Include(cp => cp.RiskAssessment)
            .FirstOrDefaultAsync(cp => cp.CustomerId == customerId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid customerId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.CustomerProfiles
            .AnyAsync(cp => cp.CustomerId == customerId, cancellationToken);
    }

    public async Task AddAsync(
        CustomerProfile profile, 
        CancellationToken cancellationToken = default)
    {
        if (profile == null)
            throw new ArgumentNullException(nameof(profile));

        await _context.CustomerProfiles.AddAsync(profile, cancellationToken);
    }

    public void Update(CustomerProfile profile)
    {
        if (profile == null)
            throw new ArgumentNullException(nameof(profile));

        _context.CustomerProfiles.Update(profile);
    }

    public void Remove(CustomerProfile profile)
    {
        if (profile == null)
            throw new ArgumentNullException(nameof(profile));

        _context.CustomerProfiles.Remove(profile);
    }

    // ===== ADVANCED QUERIES =====

    public async Task<List<CustomerProfile>> GetVipProfilesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _readContext.CustomerProfiles
            .Where(cp => cp.IsVip)
            .OrderByDescending(cp => cp.CustomerLifetimeValue)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CustomerProfile>> GetHighChurnRiskProfilesAsync(
        decimal riskThreshold = 70m,
        CancellationToken cancellationToken = default)
    {
        return await _readContext.CustomerProfiles
            .Where(cp => cp.ChurnRiskScore >= riskThreshold)
            .OrderByDescending(cp => cp.ChurnRiskScore)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CustomerProfile>> GetTopCustomersByValueAsync(
        int count = 100,
        CancellationToken cancellationToken = default)
    {
        return await _readContext.CustomerProfiles
            .OrderByDescending(cp => cp.CustomerLifetimeValue)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CustomerProfile>> GetIncompleteProfilesAsync(
        decimal completenessThreshold = 50m,
        CancellationToken cancellationToken = default)
    {
        return await _readContext.CustomerProfiles
            .Where(cp => cp.ProfileCompleteness < completenessThreshold)
            .OrderBy(cp => cp.ProfileCompleteness)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<CustomerProfile> Items, int TotalCount)> SearchProfilesAsync(
        decimal? minLifetimeValue = null,
        decimal? maxLifetimeValue = null,
        decimal? minChurnRisk = null,
        decimal? maxChurnRisk = null,
        bool? isVip = null,
        string? sourceChannel = null,
        DateTime? lastOrderFromDate = null,
        DateTime? lastOrderToDate = null,
        int page = 1,
        int pageSize = 20,
        string sortBy = "CustomerLifetimeValue",
        bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        var query = _readContext.CustomerProfiles.AsQueryable();

        // ===== FILTRES =====
        if (minLifetimeValue.HasValue)
        {
            query = query.Where(cp => cp.CustomerLifetimeValue >= minLifetimeValue.Value);
        }

        if (maxLifetimeValue.HasValue)
        {
            query = query.Where(cp => cp.CustomerLifetimeValue <= maxLifetimeValue.Value);
        }

        if (minChurnRisk.HasValue)
        {
            query = query.Where(cp => cp.ChurnRiskScore >= minChurnRisk.Value);
        }

        if (maxChurnRisk.HasValue)
        {
            query = query.Where(cp => cp.ChurnRiskScore <= maxChurnRisk.Value);
        }

        if (isVip.HasValue)
        {
            query = query.Where(cp => cp.IsVip == isVip.Value);
        }

        if (!string.IsNullOrWhiteSpace(sourceChannel))
        {
            query = query.Where(cp => cp.SourceChannel == sourceChannel);
        }

        if (lastOrderFromDate.HasValue)
        {
            query = query.Where(cp => cp.LastOrderDate >= lastOrderFromDate.Value);
        }

        if (lastOrderToDate.HasValue)
        {
            query = query.Where(cp => cp.LastOrderDate <= lastOrderToDate.Value);
        }

        // ===== COMPTAGE =====
        var totalCount = await query.CountAsync(cancellationToken);

        // ===== TRI =====
        query = ApplyProfileSorting(query, sortBy, sortDescending);

        // ===== PAGINATION =====
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    // ===== ANALYTICS METHODS =====

    public async Task<CustomerProfileStatsResult> GetProfileStatsAsync(
        CancellationToken cancellationToken = default)
    {
        var stats = await _readContext.CustomerProfiles
            .GroupBy(cp => 1)
            .Select(g => new CustomerProfileStatsResult
            {
                TotalProfiles = g.Count(),
                VipCustomers = g.Count(cp => cp.IsVip),
                AverageLifetimeValue = g.Average(cp => cp.CustomerLifetimeValue),
                AverageChurnRisk = g.Average(cp => cp.ChurnRiskScore),
                AverageCompleteness = g.Average(cp => cp.ProfileCompleteness),
                AverageSatisfaction = g.Where(cp => cp.SatisfactionScore.HasValue)
                                      .Average(cp => cp.SatisfactionScore.Value),
                HighValueCustomers = g.Count(cp => cp.CustomerLifetimeValue >= 1000),
                HighRiskCustomers = g.Count(cp => cp.ChurnRiskScore >= 70)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return stats ?? new CustomerProfileStatsResult();
    }

    public async Task<List<SourceChannelStatsResult>> GetSourceChannelStatsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _readContext.CustomerProfiles
            .Where(cp => !string.IsNullOrEmpty(cp.SourceChannel))
            .GroupBy(cp => cp.SourceChannel)
            .Select(g => new SourceChannelStatsResult
            {
                SourceChannel = g.Key,
                CustomerCount = g.Count(),
                AverageLifetimeValue = g.Average(cp => cp.CustomerLifetimeValue),
                TotalRevenue = g.Sum(cp => cp.TotalSpent),
                ConversionToVip = (decimal)g.Count(cp => cp.IsVip) / g.Count() * 100
            })
            .OrderByDescending(s => s.CustomerCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ChurnRiskSegmentResult>> GetChurnRiskSegmentsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _readContext.CustomerProfiles
            .GroupBy(cp => cp.ChurnRiskScore >= 80 ? "High Risk" :
                          cp.ChurnRiskScore >= 50 ? "Medium Risk" :
                          cp.ChurnRiskScore >= 20 ? "Low Risk" : "Very Low Risk")
            .Select(g => new ChurnRiskSegmentResult
            {
                RiskSegment = g.Key,
                CustomerCount = g.Count(),
                AverageRiskScore = g.Average(cp => cp.ChurnRiskScore),
                AverageLifetimeValue = g.Average(cp => cp.CustomerLifetimeValue),
                TotalRevenue = g.Sum(cp => cp.TotalSpent)
            })
            .OrderByDescending(r => r.AverageRiskScore)
            .ToListAsync(cancellationToken);
    }

    // ===== BULK OPERATIONS =====

    public async Task<int> BulkUpdateChurnRiskAsync(
        Dictionary<Guid, decimal> riskUpdates,
        CancellationToken cancellationToken = default)
    {
        if (!riskUpdates.Any())
            return 0;

        var affectedCount = 0;
        
        foreach (var update in riskUpdates)
        {
            var count = await _context.CustomerProfiles
                .Where(cp => cp.CustomerId == update.Key)
                .ExecuteUpdateAsync(cp => cp.SetProperty(x => x.ChurnRiskScore, update.Value), 
                    cancellationToken);
            affectedCount += count;
        }

        return affectedCount;
    }

    public async Task<int> BulkUpdateLifetimeValueAsync(
        Dictionary<Guid, decimal> valueUpdates,
        CancellationToken cancellationToken = default)
    {
        if (!valueUpdates.Any())
            return 0;

        var affectedCount = 0;
        
        foreach (var update in valueUpdates)
        {
            var count = await _context.CustomerProfiles
                .Where(cp => cp.CustomerId == update.Key)
                .ExecuteUpdateAsync(cp => cp.SetProperty(x => x.CustomerLifetimeValue, update.Value), 
                    cancellationToken);
            affectedCount += count;
        }

        return affectedCount;
    }

    public async Task<int> BulkUpdateVipStatusAsync(
        List<Guid> customerIds,
        bool isVip,
        CancellationToken cancellationToken = default)
    {
        if (!customerIds.Any())
            return 0;

        return await _context.CustomerProfiles
            .Where(cp => customerIds.Contains(cp.CustomerId))
            .ExecuteUpdateAsync(cp => cp.SetProperty(x => x.IsVip, isVip), 
                cancellationToken);
    }

    // ===== HELPER METHODS =====

    private static IQueryable<CustomerProfile> ApplyProfileSorting(
        IQueryable<CustomerProfile> query,
        string sortBy,
        bool sortDescending)
    {
        return sortBy.ToLowerInvariant() switch
        {
            "customerlifetimevalue" => sortDescending ? query.OrderByDescending(cp => cp.CustomerLifetimeValue) : query.OrderBy(cp => cp.CustomerLifetimeValue),
            "churnriskscore" => sortDescending ? query.OrderByDescending(cp => cp.ChurnRiskScore) : query.OrderBy(cp => cp.ChurnRiskScore),
            "totalspent" => sortDescending ? query.OrderByDescending(cp => cp.TotalSpent) : query.OrderBy(cp => cp.TotalSpent),
            "totalorders" => sortDescending ? query.OrderByDescending(cp => cp.TotalOrders) : query.OrderBy(cp => cp.TotalOrders),
            "averageordervalue" => sortDescending ? query.OrderByDescending(cp => cp.AverageOrderValue) : query.OrderBy(cp => cp.AverageOrderValue),
            "lastorderdate" => sortDescending ? query.OrderByDescending(cp => cp.LastOrderDate) : query.OrderBy(cp => cp.LastOrderDate),
            "profilecompleteness" => sortDescending ? query.OrderByDescending(cp => cp.ProfileCompleteness) : query.OrderBy(cp => cp.ProfileCompleteness),
            "satisfactionscore" => sortDescending ? query.OrderByDescending(cp => cp.SatisfactionScore) : query.OrderBy(cp => cp.SatisfactionScore),
            _ => sortDescending ? query.OrderByDescending(cp => cp.CustomerLifetimeValue) : query.OrderBy(cp => cp.CustomerLifetimeValue)
        };
    }
}

/// <summary>
/// Implémentation du repository LoyaltyProgram avec gestion avancée des programmes
/// </summary>
public class LoyaltyProgramRepository : ILoyaltyProgramRepository
{
    private readonly CustomerDbContext _context;
    private readonly CustomerReadDbContext _readContext;

    public LoyaltyProgramRepository(
        CustomerDbContext context, 
        CustomerReadDbContext readContext)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
    }

    // ===== CORE OPERATIONS =====

    public async Task<LoyaltyProgram?> GetByIdAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _context.LoyaltyPrograms
            .FirstOrDefaultAsync(lp => lp.Id == id, cancellationToken);
    }

    public async Task<LoyaltyProgram?> GetByNameAsync(
        string name, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return await _context.LoyaltyPrograms
            .FirstOrDefaultAsync(lp => lp.Name == name, cancellationToken);
    }

    public async Task<List<LoyaltyProgram>> GetActiveProgramsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _readContext.LoyaltyPrograms
            .Where(lp => lp.IsActive && 
                        (lp.EndDate == null || lp.EndDate > DateTime.UtcNow))
            .OrderBy(lp => lp.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LoyaltyProgram>> GetProgramsByTypeAsync(
        LoyaltyProgramType programType,
        CancellationToken cancellationToken = default)
    {
        return await _readContext.LoyaltyPrograms
            .Where(lp => lp.ProgramType == programType && lp.IsActive)
            .OrderBy(lp => lp.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _context.LoyaltyPrograms
            .AnyAsync(lp => lp.Id == id, cancellationToken);
    }

    public async Task<bool> IsNameUniqueAsync(
        string name, 
        Guid? excludeProgramId = null, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var query = _context.LoyaltyPrograms
            .Where(lp => lp.Name == name);

        if (excludeProgramId.HasValue)
        {
            query = query.Where(lp => lp.Id != excludeProgramId.Value);
        }

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(
        LoyaltyProgram program, 
        CancellationToken cancellationToken = default)
    {
        if (program == null)
            throw new ArgumentNullException(nameof(program));

        await _context.LoyaltyPrograms.AddAsync(program, cancellationToken);
    }

    public void Update(LoyaltyProgram program)
    {
        if (program == null)
            throw new ArgumentNullException(nameof(program));

        _context.LoyaltyPrograms.Update(program);
    }

    public void Remove(LoyaltyProgram program)
    {
        if (program == null)
            throw new ArgumentNullException(nameof(program));

        _context.LoyaltyPrograms.Remove(program);
    }

    // ===== ADVANCED QUERIES =====

    public async Task<List<LoyaltyProgram>> GetProgramsForAutoEnrollmentAsync(
        CancellationToken cancellationToken = default)
    {
        return await _readContext.LoyaltyPrograms
            .Where(lp => lp.IsActive && 
                        lp.AutoEnrollment && 
                        !lp.RequireOptIn &&
                        (lp.EndDate == null || lp.EndDate > DateTime.UtcNow) &&
                        (lp.MaxMembers == null || lp.CurrentMembers < lp.MaxMembers))
            .OrderBy(lp => lp.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LoyaltyProgram>> GetExpiringSoonProgramsAsync(
        int daysFromNow = 30,
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysFromNow);

        return await _readContext.LoyaltyPrograms
            .Where(lp => lp.IsActive && 
                        lp.EndDate.HasValue && 
                        lp.EndDate.Value <= cutoffDate)
            .OrderBy(lp => lp.EndDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<LoyaltyProgramStatsResult> GetProgramStatsAsync(
        Guid programId,
        CancellationToken cancellationToken = default)
    {
        var program = await _readContext.LoyaltyPrograms
            .FirstOrDefaultAsync(lp => lp.Id == programId, cancellationToken);

        if (program == null)
            return new LoyaltyProgramStatsResult();

        // Statistiques calculées (nécessiterait des jointures avec d'autres tables)
        return new LoyaltyProgramStatsResult
        {
            ProgramId = programId,
            ProgramName = program.Name,
            TotalMembers = program.CurrentMembers,
            ProgramType = program.ProgramType.ToString(),
            IsActive = program.IsActive,
            StartDate = program.StartDate,
            EndDate = program.EndDate
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
            var count = await _context.LoyaltyPrograms
                .Where(lp => lp.Id == update.Key)
                .ExecuteUpdateAsync(lp => lp.SetProperty(x => x.CurrentMembers, update.Value), 
                    cancellationToken);
            affectedCount += count;
        }

        return affectedCount;
    }
}

/// <summary>
/// Implémentation du repository LoyaltyReward avec gestion des récompenses
/// </summary>
public class LoyaltyRewardRepository : ILoyaltyRewardRepository
{
    private readonly CustomerDbContext _context;
    private readonly CustomerReadDbContext _readContext;

    public LoyaltyRewardRepository(
        CustomerDbContext context, 
        CustomerReadDbContext readContext)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
    }

    // ===== CORE OPERATIONS =====

    public async Task<LoyaltyReward?> GetByIdAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _context.LoyaltyRewards
            .FirstOrDefaultAsync(lr => lr.Id == id, cancellationToken);
    }

    public async Task<List<LoyaltyReward>> GetByProgramIdAsync(
        Guid programId, 
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = _readContext.LoyaltyRewards
            .Where(lr => lr.ProgramId == programId);

        if (activeOnly)
        {
            query = query.Where(lr => lr.IsActive && 
                                     (lr.EndDate == null || lr.EndDate > DateTime.UtcNow));
        }

        return await query
            .OrderBy(lr => lr.SortOrder)
            .ThenBy(lr => lr.PointsCost)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LoyaltyReward>> GetAvailableRewardsAsync(
        Guid programId,
        LoyaltyTier? requiredTier = null,
        CancellationToken cancellationToken = default)
    {
        var query = _readContext.LoyaltyRewards
            .Where(lr => lr.ProgramId == programId && 
                        lr.IsActive &&
                        (lr.EndDate == null || lr.EndDate > DateTime.UtcNow) &&
                        (lr.MaxRedemptions == null || lr.CurrentRedemptions < lr.MaxRedemptions));

        if (requiredTier.HasValue)
        {
            query = query.Where(lr => lr.RequiredTier == null || lr.RequiredTier == requiredTier.Value);
        }

        return await query
            .OrderBy(lr => lr.SortOrder)
            .ThenBy(lr => lr.PointsCost)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LoyaltyReward>> GetFeaturedRewardsAsync(
        Guid? programId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _readContext.LoyaltyRewards
            .Where(lr => lr.IsFeatured && 
                        lr.IsActive &&
                        (lr.EndDate == null || lr.EndDate > DateTime.UtcNow));

        if (programId.HasValue)
        {
            query = query.Where(lr => lr.ProgramId == programId.Value);
        }

        return await query
            .OrderBy(lr => lr.SortOrder)
            .ThenBy(lr => lr.PointsCost)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        LoyaltyReward reward, 
        CancellationToken cancellationToken = default)
    {
        if (reward == null)
            throw new ArgumentNullException(nameof(reward));

        await _context.LoyaltyRewards.AddAsync(reward, cancellationToken);
    }

    public void Update(LoyaltyReward reward)
    {
        if (reward == null)
            throw new ArgumentNullException(nameof(reward));

        _context.LoyaltyRewards.Update(reward);
    }

    public void Remove(LoyaltyReward reward)
    {
        if (reward == null)
            throw new ArgumentNullException(nameof(reward));

        _context.LoyaltyRewards.Remove(reward);
    }

    // ===== ANALYTICS =====

    public async Task<List<RewardPopularityResult>> GetRewardPopularityAsync(
        Guid? programId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _readContext.LoyaltyRewards.AsQueryable();

        if (programId.HasValue)
        {
            query = query.Where(lr => lr.ProgramId == programId.Value);
        }

        return await query
            .Select(lr => new RewardPopularityResult
            {
                RewardId = lr.Id,
                RewardName = lr.Name,
                RewardType = lr.RewardType.ToString(),
                PointsCost = lr.PointsCost,
                TotalRedemptions = lr.CurrentRedemptions,
                IsActive = lr.IsActive
            })
            .OrderByDescending(r => r.TotalRedemptions)
            .ToListAsync(cancellationToken);
    }
}

// ===== RESULT MODELS =====

public class CustomerProfileStatsResult
{
    public int TotalProfiles { get; set; }
    public int VipCustomers { get; set; }
    public decimal AverageLifetimeValue { get; set; }
    public decimal AverageChurnRisk { get; set; }
    public decimal AverageCompleteness { get; set; }
    public decimal? AverageSatisfaction { get; set; }
    public int HighValueCustomers { get; set; }
    public int HighRiskCustomers { get; set; }
}

public class SourceChannelStatsResult
{
    public string SourceChannel { get; set; } = string.Empty;
    public int CustomerCount { get; set; }
    public decimal AverageLifetimeValue { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal ConversionToVip { get; set; }
}

public class ChurnRiskSegmentResult
{
    public string RiskSegment { get; set; } = string.Empty;
    public int CustomerCount { get; set; }
    public decimal AverageRiskScore { get; set; }
    public decimal AverageLifetimeValue { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class LoyaltyProgramStatsResult
{
    public Guid ProgramId { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string ProgramType { get; set; } = string.Empty;
    public int TotalMembers { get; set; }
    public bool IsActive { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class RewardPopularityResult
{
    public Guid RewardId { get; set; }
    public string RewardName { get; set; } = string.Empty;
    public string RewardType { get; set; } = string.Empty;
    public int PointsCost { get; set; }
    public int TotalRedemptions { get; set; }
    public bool IsActive { get; set; }
}