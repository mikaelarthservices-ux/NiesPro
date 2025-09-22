using NiesPro.Contracts.Common;
using NiesPro.Contracts.Infrastructure;
using NiesPro.Contracts.Primitives;
using Restaurant.Domain.Enums;
using Restaurant.Domain.Events;
using Restaurant.Domain.ValueObjects;

namespace Restaurant.Domain.Entities;

/// <summary>
/// Entité représentant un membre du personnel
/// </summary>
public sealed class Staff : Entity, IAggregateRoot
{
    public string EmployeeId { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public Address? Address { get; private set; }
    public StaffRole Role { get; private set; }
    public StaffStatus Status { get; private set; }
    public EmploymentType EmploymentType { get; private set; }
    public Department Department { get; private set; }

    // Informations d'emploi
    public DateTime HireDate { get; private set; }
    public DateTime? TerminationDate { get; private set; }
    public decimal HourlyRate { get; private set; }
    public decimal? MonthlyBonus { get; private set; }
    public int VacationDaysPerYear { get; private set; }
    public int UsedVacationDays { get; private set; }
    public int SickDaysUsed { get; private set; }

    // Horaires et disponibilités
    public WorkSchedule WorkSchedule { get; private set; }
    public bool IsAvailableToday { get; private set; }
    public DateTime? LastClockIn { get; private set; }
    public DateTime? LastClockOut { get; private set; }
    public WorkingStatus CurrentWorkingStatus { get; private set; }

    // Compétences et certifications
    public List<StaffSkill> Skills { get; private set; } = new();
    public List<string> Certifications { get; private set; } = new();
    public List<string> Languages { get; private set; } = new();
    public SkillLevel OverallSkillLevel { get; private set; }

    // Performance et évaluations
    public decimal? PerformanceRating { get; private set; }
    public DateTime? LastPerformanceReview { get; private set; }
    public int CompletedOrders { get; private set; }
    public decimal TotalRevenue { get; private set; }
    public decimal AverageOrderValue { get; private set; }
    public decimal CustomerSatisfactionScore { get; private set; }

    // Informations personnelles
    public DateTime DateOfBirth { get; private set; }
    public Gender? Gender { get; private set; }
    public string? EmergencyContactName { get; private set; }
    public string? EmergencyContactPhone { get; private set; }
    public string? Notes { get; private set; }
    public string? ProfileImageUrl { get; private set; }

    // Gestion disciplinaire et récompenses
    public int WarningCount { get; private set; }
    public int CommendationCount { get; private set; }
    public DateTime? LastWarningDate { get; private set; }
    public DateTime? LastCommendationDate { get; private set; }

    // Accès et sécurité
    public List<SystemPermission> Permissions { get; private set; } = new();
    public string? BadgeNumber { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public bool RequiresPasswordReset { get; private set; }

    private readonly List<StaffShift> _shifts = new();
    public IReadOnlyList<StaffShift> Shifts => _shifts.AsReadOnly();

    private readonly List<StaffTimeEntry> _timeEntries = new();
    public IReadOnlyList<StaffTimeEntry> TimeEntries => _timeEntries.AsReadOnly();

    private readonly List<StaffEvaluation> _evaluations = new();
    public IReadOnlyList<StaffEvaluation> Evaluations => _evaluations.AsReadOnly();

    private readonly List<StaffTraining> _trainings = new();
    public IReadOnlyList<StaffTraining> Trainings => _trainings.AsReadOnly();

    private readonly List<StaffVacationRequest> _vacationRequests = new();
    public IReadOnlyList<StaffVacationRequest> VacationRequests => _vacationRequests.AsReadOnly();

    private Staff() { } // EF Constructor

    public Staff(
        string employeeId,
        string firstName,
        string lastName,
        string email,
        StaffRole role,
        EmploymentType employmentType,
        DateTime hireDate,
        decimal hourlyRate,
        WorkSchedule workSchedule,
        string? phoneNumber = null,
        Address? address = null)
    {
        if (string.IsNullOrWhiteSpace(employeeId))
            throw new ArgumentException("Employee ID cannot be null or empty", nameof(employeeId));

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty", nameof(lastName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        if (hourlyRate < 0)
            throw new ArgumentException("Hourly rate cannot be negative", nameof(hourlyRate));

        Id = Guid.NewGuid();
        EmployeeId = employeeId.Trim();
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
        PhoneNumber = phoneNumber?.Trim();
        Address = address;
        Role = role;
        EmploymentType = employmentType;
        HireDate = hireDate;
        HourlyRate = hourlyRate;
        WorkSchedule = workSchedule ?? throw new ArgumentNullException(nameof(workSchedule));
        Status = StaffStatus.Active;
        CurrentWorkingStatus = WorkingStatus.OffDuty;
        VacationDaysPerYear = employmentType == EmploymentType.FullTime ? 25 : 15;
        UsedVacationDays = 0;
        SickDaysUsed = 0;
        CompletedOrders = 0;
        TotalRevenue = 0;
        WarningCount = 0;
        CommendationCount = 0;
        RequiresPasswordReset = true;
        
        // Déterminer le département basé sur le rôle
        Department = DetermineDepartment(role);
        
        // Compétences de base selon le rôle
        InitializeSkillsByRole(role);
        
        // Permissions de base
        InitializePermissionsByRole(role);
        
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new StaffHiredEvent(
            Id,
            EmployeeId,
            $"{FirstName} {LastName}",
            Role,
            Department,
            HireDate));
    }

    /// <summary>
    /// Mettre à jour les informations personnelles
    /// </summary>
    public void UpdatePersonalInformation(
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? phoneNumber = null,
        Address? address = null,
        DateTime? dateOfBirth = null,
        Gender? gender = null)
    {
        if (!string.IsNullOrWhiteSpace(firstName))
            FirstName = firstName.Trim();

        if (!string.IsNullOrWhiteSpace(lastName))
            LastName = lastName.Trim();

        if (!string.IsNullOrWhiteSpace(email))
            Email = email.Trim().ToLowerInvariant();

        PhoneNumber = phoneNumber?.Trim();
        Address = address;

        if (dateOfBirth.HasValue)
            DateOfBirth = dateOfBirth.Value;

        if (gender.HasValue)
            Gender = gender.Value;

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mettre à jour les informations d'urgence
    /// </summary>
    public void UpdateEmergencyContact(string contactName, string contactPhone)
    {
        if (string.IsNullOrWhiteSpace(contactName))
            throw new ArgumentException("Emergency contact name cannot be null or empty", nameof(contactName));

        if (string.IsNullOrWhiteSpace(contactPhone))
            throw new ArgumentException("Emergency contact phone cannot be null or empty", nameof(contactPhone));

        EmergencyContactName = contactName.Trim();
        EmergencyContactPhone = contactPhone.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Changer le statut du personnel
    /// </summary>
    public void ChangeStatus(StaffStatus newStatus, string? reason = null, DateTime? effectiveDate = null)
    {
        if (Status == newStatus)
            return;

        var previousStatus = Status;
        Status = newStatus;
        
        if (newStatus == StaffStatus.Terminated && !TerminationDate.HasValue)
            TerminationDate = effectiveDate ?? DateTime.UtcNow;

        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StaffStatusChangedEvent(
            Id,
            EmployeeId,
            $"{FirstName} {LastName}",
            previousStatus,
            newStatus,
            reason,
            UpdatedAt.Value));
    }

    /// <summary>
    /// Promouvoir ou changer de rôle
    /// </summary>
    public void ChangeRole(StaffRole newRole, Department? newDepartment = null, decimal? newHourlyRate = null, string? reason = null)
    {
        var previousRole = Role;
        var previousDepartment = Department;
        var previousRate = HourlyRate;

        Role = newRole;
        Department = newDepartment ?? DetermineDepartment(newRole);
        
        if (newHourlyRate.HasValue && newHourlyRate.Value >= 0)
            HourlyRate = newHourlyRate.Value;

        // Mettre à jour les permissions
        UpdatePermissionsByRole(newRole);
        
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StaffRoleChangedEvent(
            Id,
            EmployeeId,
            $"{FirstName} {LastName}",
            previousRole,
            newRole,
            previousDepartment,
            Department,
            previousRate,
            HourlyRate,
            reason,
            UpdatedAt.Value));
    }

    /// <summary>
    /// Pointer à l'arrivée
    /// </summary>
    public void ClockIn(DateTime? clockInTime = null)
    {
        if (CurrentWorkingStatus == WorkingStatus.OnDuty)
            throw new InvalidOperationException("Staff member is already clocked in");

        var clockTime = clockInTime ?? DateTime.UtcNow;
        LastClockIn = clockTime;
        CurrentWorkingStatus = WorkingStatus.OnDuty;
        IsAvailableToday = true;
        UpdatedAt = DateTime.UtcNow;

        // Créer une entrée de temps
        var timeEntry = new StaffTimeEntry(Id, clockTime, TimeEntryType.ClockIn);
        _timeEntries.Add(timeEntry);

        AddDomainEvent(new StaffClockedInEvent(
            Id,
            EmployeeId,
            clockTime,
            WorkSchedule.IsOnSchedule(clockTime)));
    }

    /// <summary>
    /// Pointer à la sortie
    /// </summary>
    public void ClockOut(DateTime? clockOutTime = null)
    {
        if (CurrentWorkingStatus != WorkingStatus.OnDuty)
            throw new InvalidOperationException("Staff member is not clocked in");

        var clockTime = clockOutTime ?? DateTime.UtcNow;
        LastClockOut = clockTime;
        CurrentWorkingStatus = WorkingStatus.OffDuty;
        UpdatedAt = DateTime.UtcNow;

        // Créer une entrée de temps
        var timeEntry = new StaffTimeEntry(Id, clockTime, TimeEntryType.ClockOut);
        _timeEntries.Add(timeEntry);

        // Calculer les heures travaillées
        if (LastClockIn.HasValue)
        {
            var hoursWorked = (clockTime - LastClockIn.Value).TotalHours;
            AddDomainEvent(new StaffClockedOutEvent(
                Id,
                EmployeeId,
                clockTime,
                hoursWorked));
        }
    }

    /// <summary>
    /// Prendre une pause
    /// </summary>
    public void StartBreak(BreakType breakType)
    {
        if (CurrentWorkingStatus != WorkingStatus.OnDuty)
            throw new InvalidOperationException("Staff member must be on duty to take a break");

        CurrentWorkingStatus = WorkingStatus.OnBreak;
        UpdatedAt = DateTime.UtcNow;

        var timeEntry = new StaffTimeEntry(Id, DateTime.UtcNow, TimeEntryType.BreakStart, breakType.ToString());
        _timeEntries.Add(timeEntry);
    }

    /// <summary>
    /// Terminer une pause
    /// </summary>
    public void EndBreak()
    {
        if (CurrentWorkingStatus != WorkingStatus.OnBreak)
            throw new InvalidOperationException("Staff member is not on break");

        CurrentWorkingStatus = WorkingStatus.OnDuty;
        UpdatedAt = DateTime.UtcNow;

        var timeEntry = new StaffTimeEntry(Id, DateTime.UtcNow, TimeEntryType.BreakEnd);
        _timeEntries.Add(timeEntry);
    }

    /// <summary>
    /// Ajouter une compétence
    /// </summary>
    public void AddSkill(string skillName, SkillLevel level, DateTime? certifiedDate = null)
    {
        if (string.IsNullOrWhiteSpace(skillName))
            throw new ArgumentException("Skill name cannot be null or empty", nameof(skillName));

        if (Skills.Any(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Skill '{skillName}' already exists");

        var skill = new StaffSkill(skillName, level, certifiedDate);
        Skills.Add(skill);
        
        UpdateOverallSkillLevel();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mettre à jour une compétence
    /// </summary>
    public void UpdateSkill(string skillName, SkillLevel newLevel)
    {
        var skill = Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));
        if (skill == null)
            throw new ArgumentException($"Skill '{skillName}' not found", nameof(skillName));

        skill.UpdateLevel(newLevel);
        UpdateOverallSkillLevel();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Demander des congés
    /// </summary>
    public void RequestVacation(DateTime startDate, DateTime endDate, VacationType vacationType, string? reason = null)
    {
        if (startDate >= endDate)
            throw new ArgumentException("Start date must be before end date");

        if (startDate < DateTime.UtcNow.Date)
            throw new ArgumentException("Cannot request vacation for past dates");

        var daysRequested = (endDate - startDate).Days + 1;
        var remainingVacationDays = VacationDaysPerYear - UsedVacationDays;

        if (vacationType == VacationType.PaidVacation && daysRequested > remainingVacationDays)
            throw new InvalidOperationException("Insufficient vacation days remaining");

        var request = new StaffVacationRequest(Id, startDate, endDate, vacationType, reason);
        _vacationRequests.Add(request);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StaffVacationRequestedEvent(
            Id,
            EmployeeId,
            startDate,
            endDate,
            daysRequested,
            vacationType,
            UpdatedAt.Value));
    }

    /// <summary>
    /// Approuver/Rejeter une demande de congé
    /// </summary>
    public void ProcessVacationRequest(Guid requestId, bool approved, Guid approvedBy, string? comments = null)
    {
        var request = _vacationRequests.FirstOrDefault(r => r.Id == requestId);
        if (request == null)
            throw new ArgumentException("Vacation request not found", nameof(requestId));

        request.Process(approved, approvedBy, comments);

        if (approved && request.VacationType == VacationType.PaidVacation)
        {
            UsedVacationDays += request.DaysRequested;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Ajouter une formation
    /// </summary>
    public void AddTraining(string trainingName, DateTime startDate, DateTime? endDate = null, TrainingStatus status = TrainingStatus.Scheduled)
    {
        if (string.IsNullOrWhiteSpace(trainingName))
            throw new ArgumentException("Training name cannot be null or empty", nameof(trainingName));

        var training = new StaffTraining(Id, trainingName, startDate, endDate, status);
        _trainings.Add(training);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StaffTrainingScheduledEvent(
            Id,
            EmployeeId,
            trainingName,
            startDate,
            endDate,
            UpdatedAt.Value));
    }

    /// <summary>
    /// Compléter une formation
    /// </summary>
    public void CompleteTraining(Guid trainingId, DateTime completionDate, decimal? score = null)
    {
        var training = _trainings.FirstOrDefault(t => t.Id == trainingId);
        if (training == null)
            throw new ArgumentException("Training not found", nameof(trainingId));

        training.Complete(completionDate, score);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StaffTrainingCompletedEvent(
            Id,
            EmployeeId,
            training.Name,
            completionDate,
            score,
            UpdatedAt.Value));
    }

    /// <summary>
    /// Ajouter un avertissement
    /// </summary>
    public void AddWarning(string reason, Guid issuedBy, WarningType warningType = WarningType.Verbal)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Warning reason cannot be null or empty", nameof(reason));

        WarningCount++;
        LastWarningDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StaffWarningIssuedEvent(
            Id,
            EmployeeId,
            $"{FirstName} {LastName}",
            warningType,
            reason,
            issuedBy,
            LastWarningDate.Value));
    }

    /// <summary>
    /// Ajouter une félicitation
    /// </summary>
    public void AddCommendation(string reason, Guid issuedBy, decimal? bonusAmount = null)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Commendation reason cannot be null or empty", nameof(reason));

        CommendationCount++;
        LastCommendationDate = DateTime.UtcNow;
        
        if (bonusAmount.HasValue && bonusAmount.Value > 0)
            MonthlyBonus = (MonthlyBonus ?? 0) + bonusAmount.Value;

        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StaffCommendationIssuedEvent(
            Id,
            EmployeeId,
            $"{FirstName} {LastName}",
            reason,
            issuedBy,
            bonusAmount,
            LastCommendationDate.Value));
    }

    /// <summary>
    /// Enregistrer une commande complétée
    /// </summary>
    public void RecordCompletedOrder(decimal orderValue)
    {
        CompletedOrders++;
        TotalRevenue += orderValue;
        AverageOrderValue = CompletedOrders > 0 ? TotalRevenue / CompletedOrders : 0;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mettre à jour l'évaluation de performance
    /// </summary>
    public void UpdatePerformanceRating(decimal rating, Guid evaluatedBy, string? comments = null)
    {
        if (rating < 0 || rating > 5)
            throw new ArgumentException("Rating must be between 0 and 5", nameof(rating));

        PerformanceRating = rating;
        LastPerformanceReview = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        var evaluation = new StaffEvaluation(Id, rating, evaluatedBy, comments, LastPerformanceReview.Value);
        _evaluations.Add(evaluation);

        AddDomainEvent(new StaffPerformanceEvaluatedEvent(
            Id,
            EmployeeId,
            rating,
            evaluatedBy,
            LastPerformanceReview.Value));
    }

    /// <summary>
    /// Vérifier si le personnel est disponible à une heure donnée
    /// </summary>
    public bool IsAvailableAt(DateTime dateTime)
    {
        if (Status != StaffStatus.Active)
            return false;

        // Vérifier les congés approuvés
        var hasApprovedVacation = _vacationRequests.Any(r => 
            r.IsApproved && 
            dateTime.Date >= r.StartDate.Date && 
            dateTime.Date <= r.EndDate.Date);

        if (hasApprovedVacation)
            return false;

        // Vérifier l'horaire de travail
        return WorkSchedule.IsScheduledAt(dateTime);
    }

    /// <summary>
    /// Calculer les heures travaillées dans une période
    /// </summary>
    public decimal CalculateHoursWorked(DateTime startDate, DateTime endDate)
    {
        var relevantEntries = _timeEntries
            .Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate)
            .OrderBy(e => e.Timestamp)
            .ToList();

        decimal totalHours = 0;
        DateTime? currentClockIn = null;

        foreach (var entry in relevantEntries)
        {
            switch (entry.Type)
            {
                case TimeEntryType.ClockIn:
                    currentClockIn = entry.Timestamp;
                    break;
                case TimeEntryType.ClockOut when currentClockIn.HasValue:
                    totalHours += (decimal)(entry.Timestamp - currentClockIn.Value).TotalHours;
                    currentClockIn = null;
                    break;
            }
        }

        return totalHours;
    }

    /// <summary>
    /// Calculer le salaire pour une période
    /// </summary>
    public decimal CalculateSalary(DateTime startDate, DateTime endDate)
    {
        var hoursWorked = CalculateHoursWorked(startDate, endDate);
        var baseSalary = hoursWorked * HourlyRate;
        var bonuses = MonthlyBonus ?? 0;
        
        return baseSalary + bonuses;
    }

    /// <summary>
    /// Obtenir le nom complet
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Vérifier si c'est un superviseur
    /// </summary>
    public bool IsSupervisor => Role == StaffRole.Manager || Role == StaffRole.ShiftSupervisor || Role == StaffRole.HeadChef;

    /// <summary>
    /// Obtenir les jours de congé restants
    /// </summary>
    public int RemainingVacationDays => Math.Max(0, VacationDaysPerYear - UsedVacationDays);

    private Department DetermineDepartment(StaffRole role)
    {
        return role switch
        {
            StaffRole.Chef or StaffRole.SousChef or StaffRole.HeadChef or StaffRole.LineChef => Department.Kitchen,
            StaffRole.Waiter or StaffRole.Bartender or StaffRole.Host => Department.Service,
            StaffRole.Manager or StaffRole.ShiftSupervisor => Department.Management,
            StaffRole.Dishwasher or StaffRole.Cleaner => Department.Maintenance,
            StaffRole.Cashier => Department.Service,
            _ => Department.Service
        };
    }

    private void InitializeSkillsByRole(StaffRole role)
    {
        var baseSkills = role switch
        {
            StaffRole.Chef => new[] { "Cooking", "Food Safety", "Menu Planning" },
            StaffRole.Waiter => new[] { "Customer Service", "Order Taking", "Food Service" },
            StaffRole.Bartender => new[] { "Cocktail Making", "Wine Knowledge", "Customer Service" },
            StaffRole.Manager => new[] { "Leadership", "Staff Management", "Financial Management" },
            _ => new[] { "Customer Service", "Teamwork" }
        };

        foreach (var skill in baseSkills)
        {
            Skills.Add(new StaffSkill(skill, SkillLevel.Beginner));
        }

        UpdateOverallSkillLevel();
    }

    private void InitializePermissionsByRole(StaffRole role)
    {
        var basePermissions = role switch
        {
            StaffRole.Manager => new[]
            {
                SystemPermission.ViewReports,
                SystemPermission.ManageStaff,
                SystemPermission.ManageOrders,
                SystemPermission.ManageInventory,
                SystemPermission.ViewFinancials
            },
            StaffRole.Chef or StaffRole.HeadChef => new[]
            {
                SystemPermission.ManageOrders,
                SystemPermission.ViewInventory,
                SystemPermission.ManageKitchen
            },
            StaffRole.Waiter => new[]
            {
                SystemPermission.TakeOrders,
                SystemPermission.ViewOrders,
                SystemPermission.ProcessPayments
            },
            _ => new[] { SystemPermission.ViewOrders }
        };

        Permissions = basePermissions.ToList();
    }

    private void UpdatePermissionsByRole(StaffRole role)
    {
        InitializePermissionsByRole(role);
        UpdatedAt = DateTime.UtcNow;
    }

    private void UpdateOverallSkillLevel()
    {
        if (!Skills.Any())
        {
            OverallSkillLevel = SkillLevel.Beginner;
            return;
        }

        var averageLevel = Skills.Average(s => (int)s.Level);
        OverallSkillLevel = (SkillLevel)Math.Round(averageLevel);
    }
}

/// <summary>
/// Équipe de travail d'un membre du personnel
/// </summary>
public sealed class StaffShift : Entity
{
    public Guid StaffId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public DateTime Date { get; private set; }
    public ShiftType ShiftType { get; private set; }
    public ShiftStatus Status { get; private set; }
    public Department Department { get; private set; }
    public string? Notes { get; private set; }
    public Guid? AssignedBy { get; private set; }

    private StaffShift() { } // EF Constructor

    public StaffShift(Guid staffId, DateTime date, DateTime startTime, DateTime endTime, ShiftType shiftType, Department department)
    {
        Id = Guid.NewGuid();
        StaffId = staffId;
        Date = date.Date;
        StartTime = startTime;
        EndTime = endTime;
        ShiftType = shiftType;
        Department = department;
        Status = ShiftStatus.Scheduled;
        CreatedAt = DateTime.UtcNow;
    }

    public void ChangeStatus(ShiftStatus newStatus, string? notes = null)
    {
        Status = newStatus;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public TimeSpan Duration => EndTime - StartTime;
    public bool IsToday => Date.Date == DateTime.UtcNow.Date;
}

/// <summary>
/// Entrée de temps d'un membre du personnel
/// </summary>
public sealed class StaffTimeEntry : Entity
{
    public Guid StaffId { get; private set; }
    public DateTime Timestamp { get; private set; }
    public TimeEntryType Type { get; private set; }
    public string? Notes { get; private set; }
    public Location? Location { get; private set; }

    private StaffTimeEntry() { } // EF Constructor

    public StaffTimeEntry(Guid staffId, DateTime timestamp, TimeEntryType type, string? notes = null)
    {
        Id = Guid.NewGuid();
        StaffId = staffId;
        Timestamp = timestamp;
        Type = type;
        Notes = notes;
        CreatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Évaluation d'un membre du personnel
/// </summary>
public sealed class StaffEvaluation : Entity
{
    public Guid StaffId { get; private set; }
    public decimal Rating { get; private set; }
    public Guid EvaluatedBy { get; private set; }
    public DateTime EvaluationDate { get; private set; }
    public string? Comments { get; private set; }
    public List<string> Strengths { get; private set; } = new();
    public List<string> AreasForImprovement { get; private set; } = new();

    private StaffEvaluation() { } // EF Constructor

    public StaffEvaluation(Guid staffId, decimal rating, Guid evaluatedBy, string? comments, DateTime evaluationDate)
    {
        Id = Guid.NewGuid();
        StaffId = staffId;
        Rating = rating;
        EvaluatedBy = evaluatedBy;
        Comments = comments;
        EvaluationDate = evaluationDate;
        CreatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Formation d'un membre du personnel
/// </summary>
public sealed class StaffTraining : Entity
{
    public Guid StaffId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public TrainingStatus Status { get; private set; }
    public DateTime? CompletionDate { get; private set; }
    public decimal? Score { get; private set; }

    private StaffTraining() { } // EF Constructor

    public StaffTraining(Guid staffId, string name, DateTime startDate, DateTime? endDate, TrainingStatus status)
    {
        Id = Guid.NewGuid();
        StaffId = staffId;
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
        Status = status;
        CreatedAt = DateTime.UtcNow;
    }

    public void Complete(DateTime completionDate, decimal? score = null)
    {
        Status = TrainingStatus.Completed;
        CompletionDate = completionDate;
        Score = score;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Demande de congé d'un membre du personnel
/// </summary>
public sealed class StaffVacationRequest : Entity
{
    public Guid StaffId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public VacationType VacationType { get; private set; }
    public VacationRequestStatus Status { get; private set; }
    public string? Reason { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? Comments { get; private set; }

    private StaffVacationRequest() { } // EF Constructor

    public StaffVacationRequest(Guid staffId, DateTime startDate, DateTime endDate, VacationType vacationType, string? reason)
    {
        Id = Guid.NewGuid();
        StaffId = staffId;
        StartDate = startDate;
        EndDate = endDate;
        VacationType = vacationType;
        Status = VacationRequestStatus.Pending;
        Reason = reason;
        CreatedAt = DateTime.UtcNow;
    }

    public void Process(bool approved, Guid approvedBy, string? comments = null)
    {
        Status = approved ? VacationRequestStatus.Approved : VacationRequestStatus.Rejected;
        ApprovedBy = approvedBy;
        ProcessedAt = DateTime.UtcNow;
        Comments = comments;
        UpdatedAt = DateTime.UtcNow;
    }

    public int DaysRequested => (EndDate - StartDate).Days + 1;
    public bool IsApproved => Status == VacationRequestStatus.Approved;
}