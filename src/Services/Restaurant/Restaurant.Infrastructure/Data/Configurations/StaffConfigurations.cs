using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.Domain.Entities;
using Restaurant.Domain.Enums;

namespace Restaurant.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité Staff
/// </summary>
public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        // Configuration de la table
        builder.ToTable("Staff");
        
        // Clé primaire
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnName("Id")
            .IsRequired();

        // Propriétés d'identification requises
        builder.Property(s => s.EmployeeId)
            .HasColumnName("EmployeeId")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.FirstName)
            .HasColumnName("FirstName")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.LastName)
            .HasColumnName("LastName")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Position)
            .HasColumnName("Position")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.Department)
            .HasColumnName("Department")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        // Informations de contact
        builder.Property(s => s.Email)
            .HasColumnName("Email")
            .HasMaxLength(200);

        builder.Property(s => s.PhoneNumber)
            .HasColumnName("PhoneNumber")
            .HasMaxLength(20);

        builder.Property(s => s.Address)
            .HasColumnName("Address")
            .HasMaxLength(500);

        // Informations d'emploi
        builder.Property(s => s.HireDate)
            .HasColumnName("HireDate")
            .IsRequired();

        builder.Property(s => s.EndDate)
            .HasColumnName("EndDate");

        builder.Property(s => s.IsActive)
            .HasColumnName("IsActive")
            .HasDefaultValue(true);

        builder.Property(s => s.WorkStatus)
            .HasColumnName("WorkStatus")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        // Informations salariales
        builder.Property(s => s.HourlyRate)
            .HasColumnName("HourlyRate")
            .HasPrecision(8, 2);

        builder.Property(s => s.MonthlyRate)
            .HasColumnName("MonthlyRate")
            .HasPrecision(10, 2);

        builder.Property(s => s.YearlyRate)
            .HasColumnName("YearlyRate")
            .HasPrecision(12, 2);

        // Horaires de travail
        builder.Property(s => s.ShiftStart)
            .HasColumnName("ShiftStart");

        builder.Property(s => s.ShiftEnd)
            .HasColumnName("ShiftEnd");

        // Collections d'énumérations
        builder.Property(s => s.WorkDays)
            .HasConversion(
                v => string.Join(',', v.Select(d => (int)d)),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => (DayOfWeek)int.Parse(s))
                      .ToList())
            .HasColumnName("WorkDays")
            .HasMaxLength(50);

        builder.Property(s => s.Skills)
            .HasConversion(
                v => string.Join(',', v.Select(skill => (int)skill)),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => (StaffSkill)int.Parse(s))
                      .ToList())
            .HasColumnName("Skills")
            .HasMaxLength(500);

        builder.Property(s => s.Certifications)
            .HasConversion(
                v => string.Join(',', v.Select(cert => (int)cert)),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => (StaffCertification)int.Parse(s))
                      .ToList())
            .HasColumnName("Certifications")
            .HasMaxLength(500);

        builder.Property(s => s.Languages)
            .HasConversion(
                v => string.Join(',', v.Select(lang => (int)lang)),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => (Language)int.Parse(s))
                      .ToList())
            .HasColumnName("Languages")
            .HasMaxLength(200);

        // Informations supplémentaires
        builder.Property(s => s.EmergencyContactName)
            .HasColumnName("EmergencyContactName")
            .HasMaxLength(200);

        builder.Property(s => s.EmergencyContactPhone)
            .HasColumnName("EmergencyContactPhone")
            .HasMaxLength(20);

        builder.Property(s => s.Notes)
            .HasColumnName("Notes")
            .HasMaxLength(2000);

        // Performance et qualité
        builder.Property(s => s.PerformanceRating)
            .HasColumnName("PerformanceRating");

        builder.Property(s => s.ExperienceLevel)
            .HasColumnName("ExperienceLevel")
            .HasMaxLength(50)
            .HasConversion<string>();

        builder.Property(s => s.IsTrainer)
            .HasColumnName("IsTrainer")
            .HasDefaultValue(false);

        builder.Property(s => s.IsSupervisor)
            .HasColumnName("IsSupervisor")
            .HasDefaultValue(false);

        builder.Property(s => s.CanWorkOvertime)
            .HasColumnName("CanWorkOvertime")
            .HasDefaultValue(false);

        builder.Property(s => s.IsReliable)
            .HasColumnName("IsReliable")
            .HasDefaultValue(true);

        // Informations de formation
        builder.Property(s => s.TrainingCompleted)
            .HasColumnName("TrainingCompleted")
            .HasDefaultValue(false);

        builder.Property(s => s.TrainingCompletedDate)
            .HasColumnName("TrainingCompletedDate");

        builder.Property(s => s.NeedsAdditionalTraining)
            .HasColumnName("NeedsAdditionalTraining")
            .HasDefaultValue(false);

        // Timestamps
        builder.Property(s => s.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("UpdatedAt");

        // Index
        builder.HasIndex(s => s.EmployeeId)
            .IsUnique()
            .HasDatabaseName("IX_Staff_EmployeeId");

        builder.HasIndex(s => s.Email)
            .IsUnique()
            .HasFilter("Email IS NOT NULL")
            .HasDatabaseName("IX_Staff_Email");

        builder.HasIndex(s => s.PhoneNumber)
            .HasDatabaseName("IX_Staff_PhoneNumber");

        builder.HasIndex(s => s.Position)
            .HasDatabaseName("IX_Staff_Position");

        builder.HasIndex(s => s.Department)
            .HasDatabaseName("IX_Staff_Department");

        builder.HasIndex(s => s.IsActive)
            .HasDatabaseName("IX_Staff_IsActive");

        builder.HasIndex(s => s.WorkStatus)
            .HasDatabaseName("IX_Staff_WorkStatus");

        builder.HasIndex(s => s.HireDate)
            .HasDatabaseName("IX_Staff_HireDate");

        builder.HasIndex(s => new { s.Position, s.IsActive })
            .HasDatabaseName("IX_Staff_Position_IsActive");

        builder.HasIndex(s => new { s.Department, s.IsActive })
            .HasDatabaseName("IX_Staff_Department_IsActive");

        builder.HasIndex(s => new { s.WorkStatus, s.IsActive })
            .HasDatabaseName("IX_Staff_WorkStatus_IsActive");

        // Relations
        builder.HasMany<StaffSchedule>()
            .WithOne()
            .HasForeignKey(schedule => schedule.StaffId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_StaffSchedules_Staff");

        builder.HasMany<StaffPerformance>()
            .WithOne()
            .HasForeignKey(perf => perf.StaffId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_StaffPerformances_Staff");

        builder.HasMany<StaffTraining>()
            .WithOne()
            .HasForeignKey(training => training.StaffId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_StaffTrainings_Staff");

        // Contraintes
        builder.HasCheckConstraint("CK_Staff_HourlyRate", "HourlyRate IS NULL OR HourlyRate >= 0");
        builder.HasCheckConstraint("CK_Staff_MonthlyRate", "MonthlyRate IS NULL OR MonthlyRate >= 0");
        builder.HasCheckConstraint("CK_Staff_YearlyRate", "YearlyRate IS NULL OR YearlyRate >= 0");
        builder.HasCheckConstraint("CK_Staff_PerformanceRating", "PerformanceRating IS NULL OR (PerformanceRating >= 1 AND PerformanceRating <= 5)");
        builder.HasCheckConstraint("CK_Staff_EndDate", "EndDate IS NULL OR EndDate > HireDate");
        builder.HasCheckConstraint("CK_Staff_TrainingDate", "TrainingCompletedDate IS NULL OR TrainingCompletedDate >= HireDate");

        // Configuration des données par défaut
        builder.Property(s => s.IsActive).HasDefaultValue(true);
        builder.Property(s => s.IsTrainer).HasDefaultValue(false);
        builder.Property(s => s.IsSupervisor).HasDefaultValue(false);
        builder.Property(s => s.CanWorkOvertime).HasDefaultValue(false);
        builder.Property(s => s.IsReliable).HasDefaultValue(true);
        builder.Property(s => s.TrainingCompleted).HasDefaultValue(false);
        builder.Property(s => s.NeedsAdditionalTraining).HasDefaultValue(false);
    }
}

/// <summary>
/// Configuration EF Core pour l'entité StaffSchedule
/// </summary>
public class StaffScheduleConfiguration : IEntityTypeConfiguration<StaffSchedule>
{
    public void Configure(EntityTypeBuilder<StaffSchedule> builder)
    {
        // Configuration de la table
        builder.ToTable("StaffSchedules");
        
        // Clé primaire
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnName("Id")
            .IsRequired();

        // Propriétés requises
        builder.Property(s => s.StaffId)
            .HasColumnName("StaffId")
            .IsRequired();

        builder.Property(s => s.Date)
            .HasColumnName("Date")
            .IsRequired();

        builder.Property(s => s.ShiftType)
            .HasColumnName("ShiftType")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.StartTime)
            .HasColumnName("StartTime")
            .IsRequired();

        builder.Property(s => s.EndTime)
            .HasColumnName("EndTime")
            .IsRequired();

        builder.Property(s => s.Status)
            .HasColumnName("Status")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        // Propriétés optionnelles
        builder.Property(s => s.ActualStartTime)
            .HasColumnName("ActualStartTime");

        builder.Property(s => s.ActualEndTime)
            .HasColumnName("ActualEndTime");

        builder.Property(s => s.BreakDuration)
            .HasColumnName("BreakDuration");

        builder.Property(s => s.OvertimeHours)
            .HasColumnName("OvertimeHours");

        builder.Property(s => s.Notes)
            .HasColumnName("Notes")
            .HasMaxLength(1000);

        // Timestamps
        builder.Property(s => s.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("UpdatedAt");

        // Index
        builder.HasIndex(s => s.StaffId)
            .HasDatabaseName("IX_StaffSchedules_StaffId");

        builder.HasIndex(s => s.Date)
            .HasDatabaseName("IX_StaffSchedules_Date");

        builder.HasIndex(s => s.ShiftType)
            .HasDatabaseName("IX_StaffSchedules_ShiftType");

        builder.HasIndex(s => s.Status)
            .HasDatabaseName("IX_StaffSchedules_Status");

        builder.HasIndex(s => new { s.StaffId, s.Date })
            .IsUnique()
            .HasDatabaseName("IX_StaffSchedules_StaffId_Date");

        // Relations
        builder.HasOne<Staff>()
            .WithMany()
            .HasForeignKey(s => s.StaffId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_StaffSchedules_Staff");

        // Contraintes
        builder.HasCheckConstraint("CK_StaffSchedules_EndTime", "EndTime > StartTime");
        builder.HasCheckConstraint("CK_StaffSchedules_ActualEndTime", "ActualEndTime IS NULL OR ActualStartTime IS NULL OR ActualEndTime > ActualStartTime");
        builder.HasCheckConstraint("CK_StaffSchedules_BreakDuration", "BreakDuration IS NULL OR BreakDuration >= 0");
        builder.HasCheckConstraint("CK_StaffSchedules_OvertimeHours", "OvertimeHours IS NULL OR OvertimeHours >= 0");
    }
}

/// <summary>
/// Configuration EF Core pour l'entité StaffPerformance
/// </summary>
public class StaffPerformanceConfiguration : IEntityTypeConfiguration<StaffPerformance>
{
    public void Configure(EntityTypeBuilder<StaffPerformance> builder)
    {
        // Configuration de la table
        builder.ToTable("StaffPerformances");
        
        // Clé primaire
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("Id")
            .IsRequired();

        // Propriétés requises
        builder.Property(p => p.StaffId)
            .HasColumnName("StaffId")
            .IsRequired();

        builder.Property(p => p.EvaluationDate)
            .HasColumnName("EvaluationDate")
            .IsRequired();

        builder.Property(p => p.EvaluatedBy)
            .HasColumnName("EvaluatedBy")
            .IsRequired();

        builder.Property(p => p.OverallRating)
            .HasColumnName("OverallRating")
            .IsRequired();

        // Évaluations spécifiques
        builder.Property(p => p.QualityOfWork)
            .HasColumnName("QualityOfWork");

        builder.Property(p => p.Productivity)
            .HasColumnName("Productivity");

        builder.Property(p => p.Reliability)
            .HasColumnName("Reliability");

        builder.Property(p => p.TeamWork)
            .HasColumnName("TeamWork");

        builder.Property(p => p.CustomerService)
            .HasColumnName("CustomerService");

        builder.Property(p => p.Initiative)
            .HasColumnName("Initiative");

        builder.Property(p => p.Communication)
            .HasColumnName("Communication");

        builder.Property(p => p.ProfessionalDevelopment)
            .HasColumnName("ProfessionalDevelopment");

        // Commentaires
        builder.Property(p => p.Strengths)
            .HasColumnName("Strengths")
            .HasMaxLength(2000);

        builder.Property(p => p.AreasForImprovement)
            .HasColumnName("AreasForImprovement")
            .HasMaxLength(2000);

        builder.Property(p => p.Goals)
            .HasColumnName("Goals")
            .HasMaxLength(2000);

        builder.Property(p => p.ManagerComments)
            .HasColumnName("ManagerComments")
            .HasMaxLength(2000);

        builder.Property(p => p.EmployeeComments)
            .HasColumnName("EmployeeComments")
            .HasMaxLength(2000);

        // Période d'évaluation
        builder.Property(p => p.PeriodStart)
            .HasColumnName("PeriodStart")
            .IsRequired();

        builder.Property(p => p.PeriodEnd)
            .HasColumnName("PeriodEnd")
            .IsRequired();

        // Timestamps
        builder.Property(p => p.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("UpdatedAt");

        // Index
        builder.HasIndex(p => p.StaffId)
            .HasDatabaseName("IX_StaffPerformances_StaffId");

        builder.HasIndex(p => p.EvaluationDate)
            .HasDatabaseName("IX_StaffPerformances_EvaluationDate");

        builder.HasIndex(p => p.EvaluatedBy)
            .HasDatabaseName("IX_StaffPerformances_EvaluatedBy");

        builder.HasIndex(p => p.OverallRating)
            .HasDatabaseName("IX_StaffPerformances_OverallRating");

        builder.HasIndex(p => new { p.StaffId, p.EvaluationDate })
            .HasDatabaseName("IX_StaffPerformances_StaffId_EvaluationDate");

        // Relations
        builder.HasOne<Staff>()
            .WithMany()
            .HasForeignKey(p => p.StaffId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_StaffPerformances_Staff");

        // Contraintes
        builder.HasCheckConstraint("CK_StaffPerformances_OverallRating", "OverallRating >= 1 AND OverallRating <= 5");
        builder.HasCheckConstraint("CK_StaffPerformances_QualityOfWork", "QualityOfWork IS NULL OR (QualityOfWork >= 1 AND QualityOfWork <= 5)");
        builder.HasCheckConstraint("CK_StaffPerformances_Productivity", "Productivity IS NULL OR (Productivity >= 1 AND Productivity <= 5)");
        builder.HasCheckConstraint("CK_StaffPerformances_Reliability", "Reliability IS NULL OR (Reliability >= 1 AND Reliability <= 5)");
        builder.HasCheckConstraint("CK_StaffPerformances_TeamWork", "TeamWork IS NULL OR (TeamWork >= 1 AND TeamWork <= 5)");
        builder.HasCheckConstraint("CK_StaffPerformances_CustomerService", "CustomerService IS NULL OR (CustomerService >= 1 AND CustomerService <= 5)");
        builder.HasCheckConstraint("CK_StaffPerformances_Initiative", "Initiative IS NULL OR (Initiative >= 1 AND Initiative <= 5)");
        builder.HasCheckConstraint("CK_StaffPerformances_Communication", "Communication IS NULL OR (Communication >= 1 AND Communication <= 5)");
        builder.HasCheckConstraint("CK_StaffPerformances_ProfessionalDevelopment", "ProfessionalDevelopment IS NULL OR (ProfessionalDevelopment >= 1 AND ProfessionalDevelopment <= 5)");
        builder.HasCheckConstraint("CK_StaffPerformances_Period", "PeriodEnd > PeriodStart");
    }
}

/// <summary>
/// Configuration EF Core pour l'entité StaffTraining
/// </summary>
public class StaffTrainingConfiguration : IEntityTypeConfiguration<StaffTraining>
{
    public void Configure(EntityTypeBuilder<StaffTraining> builder)
    {
        // Configuration de la table
        builder.ToTable("StaffTrainings");
        
        // Clé primaire
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("Id")
            .IsRequired();

        // Propriétés requises
        builder.Property(t => t.StaffId)
            .HasColumnName("StaffId")
            .IsRequired();

        builder.Property(t => t.TrainingName)
            .HasColumnName("TrainingName")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.TrainingType)
            .HasColumnName("TrainingType")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.Status)
            .HasColumnName("Status")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.StartDate)
            .HasColumnName("StartDate")
            .IsRequired();

        // Propriétés optionnelles
        builder.Property(t => t.EndDate)
            .HasColumnName("EndDate");

        builder.Property(t => t.CompletionDate)
            .HasColumnName("CompletionDate");

        builder.Property(t => t.ExpirationDate)
            .HasColumnName("ExpirationDate");

        builder.Property(t => t.Duration)
            .HasColumnName("Duration");

        builder.Property(t => t.Instructor)
            .HasColumnName("Instructor")
            .HasMaxLength(200);

        builder.Property(t => t.Provider)
            .HasColumnName("Provider")
            .HasMaxLength(200);

        builder.Property(t => t.Location)
            .HasColumnName("Location")
            .HasMaxLength(200);

        builder.Property(t => t.Cost)
            .HasColumnName("Cost")
            .HasPrecision(10, 2);

        builder.Property(t => t.Score)
            .HasColumnName("Score");

        builder.Property(t => t.PassingScore)
            .HasColumnName("PassingScore");

        builder.Property(t => t.CertificationObtained)
            .HasColumnName("CertificationObtained")
            .HasDefaultValue(false);

        builder.Property(t => t.CertificationNumber)
            .HasColumnName("CertificationNumber")
            .HasMaxLength(100);

        builder.Property(t => t.IsRecurring)
            .HasColumnName("IsRecurring")
            .HasDefaultValue(false);

        builder.Property(t => t.IsMandatory)
            .HasColumnName("IsMandatory")
            .HasDefaultValue(false);

        builder.Property(t => t.Description)
            .HasColumnName("Description")
            .HasMaxLength(2000);

        builder.Property(t => t.Objectives)
            .HasColumnName("Objectives")
            .HasMaxLength(2000);

        builder.Property(t => t.Feedback)
            .HasColumnName("Feedback")
            .HasMaxLength(2000);

        builder.Property(t => t.Notes)
            .HasColumnName("Notes")
            .HasMaxLength(2000);

        // Timestamps
        builder.Property(t => t.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("UpdatedAt");

        // Index
        builder.HasIndex(t => t.StaffId)
            .HasDatabaseName("IX_StaffTrainings_StaffId");

        builder.HasIndex(t => t.TrainingType)
            .HasDatabaseName("IX_StaffTrainings_TrainingType");

        builder.HasIndex(t => t.Status)
            .HasDatabaseName("IX_StaffTrainings_Status");

        builder.HasIndex(t => t.StartDate)
            .HasDatabaseName("IX_StaffTrainings_StartDate");

        builder.HasIndex(t => t.CompletionDate)
            .HasDatabaseName("IX_StaffTrainings_CompletionDate");

        builder.HasIndex(t => t.ExpirationDate)
            .HasDatabaseName("IX_StaffTrainings_ExpirationDate");

        builder.HasIndex(t => t.IsMandatory)
            .HasDatabaseName("IX_StaffTrainings_IsMandatory");

        builder.HasIndex(t => new { t.StaffId, t.TrainingType })
            .HasDatabaseName("IX_StaffTrainings_StaffId_TrainingType");

        // Relations
        builder.HasOne<Staff>()
            .WithMany()
            .HasForeignKey(t => t.StaffId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_StaffTrainings_Staff");

        // Contraintes
        builder.HasCheckConstraint("CK_StaffTrainings_EndDate", "EndDate IS NULL OR EndDate >= StartDate");
        builder.HasCheckConstraint("CK_StaffTrainings_CompletionDate", "CompletionDate IS NULL OR CompletionDate >= StartDate");
        builder.HasCheckConstraint("CK_StaffTrainings_Cost", "Cost IS NULL OR Cost >= 0");
        builder.HasCheckConstraint("CK_StaffTrainings_Score", "Score IS NULL OR Score >= 0");
        builder.HasCheckConstraint("CK_StaffTrainings_PassingScore", "PassingScore IS NULL OR PassingScore >= 0");

        // Configuration des données par défaut
        builder.Property(t => t.CertificationObtained).HasDefaultValue(false);
        builder.Property(t => t.IsRecurring).HasDefaultValue(false);
        builder.Property(t => t.IsMandatory).HasDefaultValue(false);
    }
}