using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Infrastructure.Data;
using Restaurant.Domain.Entities;
using Restaurant.Domain.Repositories;
using Restaurant.Infrastructure.Data;

namespace Restaurant.Infrastructure.Repositories;

/// <summary>
/// Implémentation du repository pour les Commandes de Cuisine
/// </summary>
public class KitchenOrderRepository : Repository<KitchenOrder>, IKitchenOrderRepository
{
    public KitchenOrderRepository(RestaurantDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtenir les commandes par statut
    /// </summary>
    public async Task<IReadOnlyList<KitchenOrder>> GetOrdersByStatusAsync(
        Domain.Enums.KitchenOrderStatus status,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<KitchenOrder>()
            .Include(o => o.OrderItems)
            .Where(o => o.Status == status)
            .OrderBy(o => o.EstimatedReadyTime)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir les commandes actives pour une section de cuisine
    /// </summary>
    public async Task<IReadOnlyList<KitchenOrder>> GetActiveOrdersBySectionAsync(
        Domain.Enums.KitchenSection kitchenSection,
        CancellationToken cancellationToken = default)
    {
        var activeStatuses = new[]
        {
            Domain.Enums.KitchenOrderStatus.Pending,
            Domain.Enums.KitchenOrderStatus.Accepted,
            Domain.Enums.KitchenOrderStatus.InPreparation
        };

        return await Context.Set<KitchenOrder>()
            .Include(o => o.OrderItems)
            .Where(o => o.KitchenSection == kitchenSection && 
                       activeStatuses.Contains(o.Status))
            .OrderBy(o => o.Priority)
            .ThenBy(o => o.EstimatedReadyTime)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir les commandes par priorité
    /// </summary>
    public async Task<IReadOnlyList<KitchenOrder>> GetOrdersByPriorityAsync(
        Domain.Enums.OrderPriority priority,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<KitchenOrder>()
            .Include(o => o.OrderItems)
            .Where(o => o.Priority == priority)
            .OrderBy(o => o.OrderedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir les commandes pour une table
    /// </summary>
    public async Task<IReadOnlyList<KitchenOrder>> GetOrdersByTableAsync(
        Guid tableId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<KitchenOrder>()
            .Include(o => o.OrderItems)
            .Where(o => o.TableId == tableId)
            .OrderByDescending(o => o.OrderedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir les commandes en retard
    /// </summary>
    public async Task<IReadOnlyList<KitchenOrder>> GetDelayedOrdersAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var activeStatuses = new[]
        {
            Domain.Enums.KitchenOrderStatus.Pending,
            Domain.Enums.KitchenOrderStatus.Accepted,
            Domain.Enums.KitchenOrderStatus.InPreparation
        };

        return await Context.Set<KitchenOrder>()
            .Include(o => o.OrderItems)
            .Where(o => activeStatuses.Contains(o.Status) && 
                       o.EstimatedReadyTime < now)
            .OrderBy(o => o.EstimatedReadyTime)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir une commande par numéro
    /// </summary>
    public async Task<KitchenOrder?> GetByOrderNumberAsync(
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<KitchenOrder>()
            .Include(o => o.OrderItems)
            .Include(o => o.OrderLogs)
            .Include(o => o.Modifications)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }

    /// <summary>
    /// Obtenir les commandes assignées à un chef
    /// </summary>
    public async Task<IReadOnlyList<KitchenOrder>> GetOrdersByChefAsync(
        Guid chefId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<KitchenOrder>()
            .Include(o => o.OrderItems)
            .Where(o => o.ChefId == chefId)
            .OrderBy(o => o.EstimatedReadyTime)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir les statistiques des commandes pour une période
    /// </summary>
    public async Task<Dictionary<Domain.Enums.KitchenOrderStatus, int>> GetOrderStatisticsAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<KitchenOrder>()
            .Where(o => o.OrderedAt >= fromDate && o.OrderedAt <= toDate)
            .GroupBy(o => o.Status)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }
}

/// <summary>
/// Implémentation du repository pour le Personnel
/// </summary>
public class StaffRepository : Repository<Staff>, IStaffRepository
{
    public StaffRepository(RestaurantDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtenir le personnel actif
    /// </summary>
    public async Task<IReadOnlyList<Staff>> GetActiveStaffAsync(
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Staff>()
            .Where(s => s.IsActive && s.WorkStatus == Domain.Enums.WorkStatus.Active)
            .OrderBy(s => s.Department)
            .ThenBy(s => s.Position)
            .ThenBy(s => s.LastName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir le personnel par département
    /// </summary>
    public async Task<IReadOnlyList<Staff>> GetStaffByDepartmentAsync(
        Domain.Enums.Department department,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Staff>()
            .Where(s => s.Department == department && s.IsActive)
            .OrderBy(s => s.Position)
            .ThenBy(s => s.LastName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir le personnel par poste
    /// </summary>
    public async Task<IReadOnlyList<Staff>> GetStaffByPositionAsync(
        Domain.Enums.StaffPosition position,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Staff>()
            .Where(s => s.Position == position && s.IsActive)
            .OrderBy(s => s.LastName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir le personnel disponible pour un créneau
    /// </summary>
    public async Task<IReadOnlyList<Staff>> GetAvailableStaffAsync(
        DateTime date,
        TimeSpan startTime,
        TimeSpan endTime,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Staff>()
            .Where(s => s.IsActive && 
                       s.WorkStatus == Domain.Enums.WorkStatus.Active &&
                       s.WorkDays.Contains(date.DayOfWeek) &&
                       s.ShiftStart <= startTime &&
                       s.ShiftEnd >= endTime)
            .Where(s => !Context.Set<StaffSchedule>()
                .Any(schedule => schedule.StaffId == s.Id &&
                                schedule.Date == date.Date &&
                                schedule.StartTime < endTime &&
                                schedule.EndTime > startTime &&
                                schedule.Status != Domain.Enums.ScheduleStatus.Absent))
            .OrderBy(s => s.Department)
            .ThenBy(s => s.Position)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir un membre du personnel par identifiant employé
    /// </summary>
    public async Task<Staff?> GetByEmployeeIdAsync(
        string employeeId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Staff>()
            .Include(s => s.Schedules)
            .Include(s => s.Performances)
            .Include(s => s.Trainings)
            .FirstOrDefaultAsync(s => s.EmployeeId == employeeId, cancellationToken);
    }

    /// <summary>
    /// Rechercher le personnel par nom
    /// </summary>
    public async Task<IReadOnlyList<Staff>> SearchByNameAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Staff>()
            .Where(s => s.FirstName.Contains(searchTerm) || 
                       s.LastName.Contains(searchTerm) ||
                       (s.FirstName + " " + s.LastName).Contains(searchTerm))
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir le personnel avec des compétences spécifiques
    /// </summary>
    public async Task<IReadOnlyList<Staff>> GetStaffWithSkillsAsync(
        List<Domain.Enums.StaffSkill> requiredSkills,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Staff>()
            .Where(s => s.IsActive && 
                       requiredSkills.All(skill => s.Skills.Contains(skill)))
            .OrderBy(s => s.ExperienceLevel)
            .ThenBy(s => s.LastName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir les superviseurs
    /// </summary>
    public async Task<IReadOnlyList<Staff>> GetSupervisorsAsync(
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Staff>()
            .Where(s => s.IsActive && s.IsSupervisor)
            .OrderBy(s => s.Department)
            .ThenBy(s => s.LastName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir les formateurs
    /// </summary>
    public async Task<IReadOnlyList<Staff>> GetTrainersAsync(
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Staff>()
            .Where(s => s.IsActive && s.IsTrainer)
            .OrderBy(s => s.Department)
            .ThenBy(s => s.LastName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir les statistiques du personnel
    /// </summary>
    public async Task<Dictionary<Domain.Enums.Department, int>> GetStaffStatisticsByDepartmentAsync(
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Staff>()
            .Where(s => s.IsActive)
            .GroupBy(s => s.Department)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }

    /// <summary>
    /// Obtenir le personnel nécessitant une formation
    /// </summary>
    public async Task<IReadOnlyList<Staff>> GetStaffNeedingTrainingAsync(
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Staff>()
            .Where(s => s.IsActive && 
                       (!s.TrainingCompleted || s.NeedsAdditionalTraining))
            .OrderBy(s => s.HireDate)
            .ToListAsync(cancellationToken);
    }
}