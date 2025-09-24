using NiesPro.Contracts.Infrastructure;
using Restaurant.Domain.Enums;

namespace Restaurant.Domain.Entities;

/// <summary>
/// Entité représentant un planning de travail pour un membre du personnel
/// </summary>
public class StaffSchedule : Entity
{
    /// <summary>
    /// Identifiant du membre du personnel
    /// </summary>
    public Guid StaffId { get; private set; }

    /// <summary>
    /// Date du planning
    /// </summary>
    public DateTime Date { get; private set; }

    /// <summary>
    /// Type de service (matin, midi, soir, nuit)
    /// </summary>
    public ShiftType ShiftType { get; private set; }

    /// <summary>
    /// Heure de début prévue
    /// </summary>
    public TimeSpan StartTime { get; private set; }

    /// <summary>
    /// Heure de fin prévue
    /// </summary>
    public TimeSpan EndTime { get; private set; }

    /// <summary>
    /// Statut du planning
    /// </summary>
    public ScheduleStatus Status { get; private set; }

    /// <summary>
    /// Heure de début réelle
    /// </summary>
    public TimeSpan? ActualStartTime { get; private set; }

    /// <summary>
    /// Heure de fin réelle
    /// </summary>
    public TimeSpan? ActualEndTime { get; private set; }

    /// <summary>
    /// Durée de pause en minutes
    /// </summary>
    public int? BreakDuration { get; private set; }

    /// <summary>
    /// Heures supplémentaires
    /// </summary>
    public decimal? OvertimeHours { get; private set; }

    /// <summary>
    /// Notes sur le planning
    /// </summary>
    public string? Notes { get; private set; }

    // Constructeur privé pour EF Core
    private StaffSchedule() { }

    /// <summary>
    /// Constructeur pour créer un nouveau planning
    /// </summary>
    public StaffSchedule(
        Guid staffId,
        DateTime date,
        ShiftType shiftType,
        TimeSpan startTime,
        TimeSpan endTime,
        string? notes = null)
    {
        StaffId = staffId;
        Date = date.Date; // On ne garde que la date
        ShiftType = shiftType;
        StartTime = startTime;
        EndTime = endTime;
        Status = ScheduleStatus.Scheduled;
        Notes = notes;

        ValidateSchedule();
    }

    /// <summary>
    /// Confirmer l'arrivée
    /// </summary>
    public void CheckIn(TimeSpan actualStartTime)
    {
        if (Status != ScheduleStatus.Scheduled)
            throw new InvalidOperationException($"Cannot check in with status {Status}");

        ActualStartTime = actualStartTime;
        Status = ScheduleStatus.CheckedIn;
    }

    /// <summary>
    /// Confirmer le départ
    /// </summary>
    public void CheckOut(TimeSpan actualEndTime, int? breakDuration = null)
    {
        if (Status != ScheduleStatus.CheckedIn)
            throw new InvalidOperationException($"Cannot check out with status {Status}");

        ActualEndTime = actualEndTime;
        BreakDuration = breakDuration;
        Status = ScheduleStatus.CheckedOut;

        CalculateOvertimeHours();
    }

    /// <summary>
    /// Marquer comme absent
    /// </summary>
    public void MarkAbsent(string reason)
    {
        Status = ScheduleStatus.Absent;
        Notes = reason;
    }

    /// <summary>
    /// Calculer les heures supplémentaires
    /// </summary>
    private void CalculateOvertimeHours()
    {
        if (ActualStartTime.HasValue && ActualEndTime.HasValue)
        {
            var scheduledHours = (EndTime - StartTime).TotalHours;
            var actualHours = (ActualEndTime.Value - ActualStartTime.Value).TotalHours;
            
            if (BreakDuration.HasValue)
                actualHours -= BreakDuration.Value / 60.0; // Convertir minutes en heures

            var overtime = actualHours - scheduledHours;
            OvertimeHours = overtime > 0 ? (decimal)overtime : 0;
        }
    }

    /// <summary>
    /// Valider le planning
    /// </summary>
    private void ValidateSchedule()
    {
        if (EndTime <= StartTime)
            throw new ArgumentException("End time must be after start time");

        if (Date < DateTime.Today)
            throw new ArgumentException("Cannot schedule in the past");
    }
}

/// <summary>
/// Entité représentant une évaluation de performance du personnel
/// </summary>
public class StaffPerformance : Entity
{
    /// <summary>
    /// Identifiant du membre du personnel évalué
    /// </summary>
    public Guid StaffId { get; private set; }

    /// <summary>
    /// Date de l'évaluation
    /// </summary>
    public DateTime EvaluationDate { get; private set; }

    /// <summary>
    /// Identifiant de l'évaluateur
    /// </summary>
    public Guid EvaluatedBy { get; private set; }

    /// <summary>
    /// Note globale (1-5)
    /// </summary>
    public int OverallRating { get; private set; }

    /// <summary>
    /// Qualité du travail (1-5)
    /// </summary>
    public int? QualityOfWork { get; private set; }

    /// <summary>
    /// Productivité (1-5)
    /// </summary>
    public int? Productivity { get; private set; }

    /// <summary>
    /// Fiabilité (1-5)
    /// </summary>
    public int? Reliability { get; private set; }

    /// <summary>
    /// Travail d'équipe (1-5)
    /// </summary>
    public int? TeamWork { get; private set; }

    /// <summary>
    /// Service client (1-5)
    /// </summary>
    public int? CustomerService { get; private set; }

    /// <summary>
    /// Initiative (1-5)
    /// </summary>
    public int? Initiative { get; private set; }

    /// <summary>
    /// Communication (1-5)
    /// </summary>
    public int? Communication { get; private set; }

    /// <summary>
    /// Développement professionnel (1-5)
    /// </summary>
    public int? ProfessionalDevelopment { get; private set; }

    /// <summary>
    /// Points forts
    /// </summary>
    public string? Strengths { get; private set; }

    /// <summary>
    /// Axes d'amélioration
    /// </summary>
    public string? AreasForImprovement { get; private set; }

    /// <summary>
    /// Objectifs
    /// </summary>
    public string? Goals { get; private set; }

    /// <summary>
    /// Commentaires du manager
    /// </summary>
    public string? ManagerComments { get; private set; }

    /// <summary>
    /// Commentaires de l'employé
    /// </summary>
    public string? EmployeeComments { get; private set; }

    /// <summary>
    /// Début de la période d'évaluation
    /// </summary>
    public DateTime PeriodStart { get; private set; }

    /// <summary>
    /// Fin de la période d'évaluation
    /// </summary>
    public DateTime PeriodEnd { get; private set; }

    // Constructeur privé pour EF Core
    private StaffPerformance() { }

    /// <summary>
    /// Constructeur pour créer une nouvelle évaluation
    /// </summary>
    public StaffPerformance(
        Guid staffId,
        Guid evaluatedBy,
        DateTime evaluationDate,
        DateTime periodStart,
        DateTime periodEnd,
        int overallRating)
    {
        StaffId = staffId;
        EvaluatedBy = evaluatedBy;
        EvaluationDate = evaluationDate;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        OverallRating = overallRating;

        ValidateEvaluation();
    }

    /// <summary>
    /// Mettre à jour les notes détaillées
    /// </summary>
    public void UpdateDetailedRatings(
        int? qualityOfWork = null,
        int? productivity = null,
        int? reliability = null,
        int? teamWork = null,
        int? customerService = null,
        int? initiative = null,
        int? communication = null,
        int? professionalDevelopment = null)
    {
        QualityOfWork = qualityOfWork;
        Productivity = productivity;
        Reliability = reliability;
        TeamWork = teamWork;
        CustomerService = customerService;
        Initiative = initiative;
        Communication = communication;
        ProfessionalDevelopment = professionalDevelopment;

        ValidateRatings();
    }

    /// <summary>
    /// Mettre à jour les commentaires
    /// </summary>
    public void UpdateComments(
        string? strengths = null,
        string? areasForImprovement = null,
        string? goals = null,
        string? managerComments = null,
        string? employeeComments = null)
    {
        Strengths = strengths;
        AreasForImprovement = areasForImprovement;
        Goals = goals;
        ManagerComments = managerComments;
        EmployeeComments = employeeComments;
    }

    /// <summary>
    /// Valider l'évaluation
    /// </summary>
    private void ValidateEvaluation()
    {
        if (PeriodEnd <= PeriodStart)
            throw new ArgumentException("Period end must be after period start");

        if (EvaluationDate < PeriodEnd)
            throw new ArgumentException("Evaluation date must be after period end");

        if (OverallRating < 1 || OverallRating > 5)
            throw new ArgumentException("Overall rating must be between 1 and 5");
    }

    /// <summary>
    /// Valider les notes
    /// </summary>
    private void ValidateRatings()
    {
        var ratings = new[] { QualityOfWork, Productivity, Reliability, TeamWork, 
                             CustomerService, Initiative, Communication, ProfessionalDevelopment };

        foreach (var rating in ratings.Where(r => r.HasValue))
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("All ratings must be between 1 and 5");
        }
    }
}

/// <summary>
/// Entité représentant une formation du personnel
/// </summary>
public class StaffTraining : Entity
{
    /// <summary>
    /// Identifiant du membre du personnel
    /// </summary>
    public Guid StaffId { get; private set; }

    /// <summary>
    /// Nom de la formation
    /// </summary>
    public string TrainingName { get; private set; } = string.Empty;

    /// <summary>
    /// Type de formation
    /// </summary>
    public TrainingType TrainingType { get; private set; }

    /// <summary>
    /// Statut de la formation
    /// </summary>
    public TrainingStatus Status { get; private set; }

    /// <summary>
    /// Date de début
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// Date de fin prévue
    /// </summary>
    public DateTime? EndDate { get; private set; }

    /// <summary>
    /// Date de completion
    /// </summary>
    public DateTime? CompletionDate { get; private set; }

    /// <summary>
    /// Date d'expiration de la certification
    /// </summary>
    public DateTime? ExpirationDate { get; private set; }

    /// <summary>
    /// Durée en heures
    /// </summary>
    public int? Duration { get; private set; }

    /// <summary>
    /// Instructeur
    /// </summary>
    public string? Instructor { get; private set; }

    /// <summary>
    /// Fournisseur de formation
    /// </summary>
    public string? Provider { get; private set; }

    /// <summary>
    /// Lieu
    /// </summary>
    public string? Location { get; private set; }

    /// <summary>
    /// Coût
    /// </summary>
    public decimal? Cost { get; private set; }

    /// <summary>
    /// Score obtenu
    /// </summary>
    public decimal? Score { get; private set; }

    /// <summary>
    /// Score minimum pour réussir
    /// </summary>
    public decimal? PassingScore { get; private set; }

    /// <summary>
    /// Certification obtenue
    /// </summary>
    public bool CertificationObtained { get; private set; }

    /// <summary>
    /// Numéro de certification
    /// </summary>
    public string? CertificationNumber { get; private set; }

    /// <summary>
    /// Formation récurrente
    /// </summary>
    public bool IsRecurring { get; private set; }

    /// <summary>
    /// Formation obligatoire
    /// </summary>
    public bool IsMandatory { get; private set; }

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Objectifs
    /// </summary>
    public string? Objectives { get; private set; }

    /// <summary>
    /// Feedback
    /// </summary>
    public string? Feedback { get; private set; }

    /// <summary>
    /// Notes
    /// </summary>
    public string? Notes { get; private set; }

    // Constructeur privé pour EF Core
    private StaffTraining() { }

    /// <summary>
    /// Constructeur pour créer une nouvelle formation
    /// </summary>
    public StaffTraining(
        Guid staffId,
        string trainingName,
        TrainingType trainingType,
        DateTime startDate,
        bool isMandatory = false,
        string? description = null)
    {
        StaffId = staffId;
        TrainingName = trainingName ?? throw new ArgumentNullException(nameof(trainingName));
        TrainingType = trainingType;
        StartDate = startDate;
        IsMandatory = isMandatory;
        Description = description;
        Status = TrainingStatus.Scheduled;
    }

    /// <summary>
    /// Commencer la formation
    /// </summary>
    public void StartTraining()
    {
        if (Status != TrainingStatus.Scheduled)
            throw new InvalidOperationException($"Cannot start training with status {Status}");

        Status = TrainingStatus.InProgress;
    }

    /// <summary>
    /// Compléter la formation
    /// </summary>
    public void CompleteTraining(
        DateTime completionDate,
        decimal? score = null,
        bool certificationObtained = false,
        string? certificationNumber = null)
    {
        if (Status != TrainingStatus.InProgress)
            throw new InvalidOperationException($"Cannot complete training with status {Status}");

        CompletionDate = completionDate;
        Score = score;
        CertificationObtained = certificationObtained;
        CertificationNumber = certificationNumber;
        Status = TrainingStatus.Completed;

        // Vérifier si la formation est réussie
        if (PassingScore.HasValue && score.HasValue && score < PassingScore)
        {
            Status = TrainingStatus.Failed;
        }
    }

    /// <summary>
    /// Marquer comme échouée
    /// </summary>
    public void MarkAsFailed(string? feedback = null)
    {
        Status = TrainingStatus.Failed;
        Feedback = feedback;
    }

    /// <summary>
    /// Annuler la formation
    /// </summary>
    public void CancelTraining(string reason)
    {
        Status = TrainingStatus.Cancelled;
        Notes = reason;
    }

    /// <summary>
    /// Mettre à jour les détails de la formation
    /// </summary>
    public void UpdateTrainingDetails(
        DateTime? endDate = null,
        int? duration = null,
        string? instructor = null,
        string? provider = null,
        string? location = null,
        decimal? cost = null,
        decimal? passingScore = null,
        DateTime? expirationDate = null)
    {
        EndDate = endDate;
        Duration = duration;
        Instructor = instructor;
        Provider = provider;
        Location = location;
        Cost = cost;
        PassingScore = passingScore;
        ExpirationDate = expirationDate;
    }
}