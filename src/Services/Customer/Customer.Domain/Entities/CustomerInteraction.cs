using BuildingBlocks.Domain.Entities;
using Customer.Domain.Enums;
using Customer.Domain.Events;

namespace Customer.Domain.Entities;

/// <summary>
/// Interaction entre le restaurant et le client
/// </summary>
public class CustomerInteraction : Entity<Guid>
{
    public Guid CustomerId { get; private set; }
    public InteractionType Type { get; private set; }
    public InteractionChannel Channel { get; private set; }
    public DateTime InteractionDate { get; private set; }
    public string Subject { get; private set; }
    public string? Description { get; private set; }
    public InteractionOutcome Outcome { get; private set; }
    public string? StaffMemberId { get; private set; }
    public string? StaffMemberName { get; private set; }
    public TimeSpan? Duration { get; private set; }
    public decimal? AssociatedAmount { get; private set; }
    public string? ReferenceId { get; private set; }
    public int? SatisfactionRating { get; private set; }
    public string? CustomerFeedback { get; private set; }
    public string? InternalNotes { get; private set; }
    public string? Tags { get; private set; }
    public bool RequiresFollowUp { get; private set; }
    public DateTime? FollowUpDate { get; private set; }
    public bool IsFollowUpCompleted { get; private set; }
    public string? Attachments { get; private set; }
    public string? Metadata { get; private set; }

    protected CustomerInteraction() { }

    public CustomerInteraction(
        Guid customerId,
        InteractionType type,
        InteractionChannel channel,
        string subject,
        string? description = null,
        string? staffMemberId = null,
        string? staffMemberName = null)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject cannot be empty", nameof(subject));

        CustomerId = customerId;
        Type = type;
        Channel = channel;
        Subject = subject.Trim();
        Description = description?.Trim();
        StaffMemberId = staffMemberId?.Trim();
        StaffMemberName = staffMemberName?.Trim();
        InteractionDate = DateTime.UtcNow;
        Outcome = InteractionOutcome.Pending;
        RequiresFollowUp = false;
        IsFollowUpCompleted = false;
    }

    // Méthodes métier pour la gestion de l'interaction
    public void UpdateBasicInfo(string subject, string? description)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject cannot be empty", nameof(subject));

        Subject = subject.Trim();
        Description = description?.Trim();
    }

    public void UpdateStaffInfo(string? staffMemberId, string? staffMemberName)
    {
        StaffMemberId = staffMemberId?.Trim();
        StaffMemberName = staffMemberName?.Trim();
    }

    public void SetDuration(TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
            throw new ArgumentException("Duration cannot be negative", nameof(duration));

        Duration = duration;
    }

    public void SetAssociatedAmount(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        AssociatedAmount = amount;
    }

    public void SetReferenceId(string? referenceId)
    {
        ReferenceId = referenceId?.Trim();
    }

    // Méthodes métier pour la satisfaction et feedback
    public void RecordSatisfactionRating(int rating, string? feedback = null)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Satisfaction rating must be between 1 and 5", nameof(rating));

        SatisfactionRating = rating;
        CustomerFeedback = feedback?.Trim();

        // Déclencher un événement de feedback
        AddDomainEvent(new CustomerFeedbackRecordedEvent(
            CustomerId, Id, Type, rating, feedback, InteractionDate));
    }

    public void UpdateCustomerFeedback(string? feedback)
    {
        CustomerFeedback = feedback?.Trim();
    }

    public void AddInternalNotes(string notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            throw new ArgumentException("Notes cannot be empty", nameof(notes));

        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
        var newNote = $"[{timestamp}] {notes.Trim()}";

        InternalNotes = string.IsNullOrWhiteSpace(InternalNotes) 
            ? newNote 
            : $"{InternalNotes}\n{newNote}";
    }

    public void UpdateInternalNotes(string? notes)
    {
        InternalNotes = notes?.Trim();
    }

    // Méthodes métier pour les tags
    public void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentException("Tag cannot be empty", nameof(tag));

        var tagToAdd = tag.Trim().ToLowerInvariant();
        var currentTags = GetTagsList();

        if (!currentTags.Contains(tagToAdd))
        {
            currentTags.Add(tagToAdd);
            Tags = string.Join(",", currentTags);
        }
    }

    public void RemoveTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return;

        var tagToRemove = tag.Trim().ToLowerInvariant();
        var currentTags = GetTagsList();

        if (currentTags.Remove(tagToRemove))
        {
            Tags = currentTags.Any() ? string.Join(",", currentTags) : null;
        }
    }

    public void SetTags(IEnumerable<string> tags)
    {
        if (tags == null)
        {
            Tags = null;
            return;
        }

        var validTags = tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

        Tags = validTags.Any() ? string.Join(",", validTags) : null;
    }

    public List<string> GetTagsList()
    {
        if (string.IsNullOrWhiteSpace(Tags))
            return new List<string>();

        return Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                  .Select(t => t.Trim())
                  .Where(t => !string.IsNullOrWhiteSpace(t))
                  .ToList();
    }

    // Méthodes métier pour le suivi
    public void MarkAsCompleted(InteractionOutcome outcome, string? notes = null)
    {
        if (outcome == InteractionOutcome.Pending)
            throw new ArgumentException("Cannot mark as completed with pending outcome", nameof(outcome));

        Outcome = outcome;
        
        if (!string.IsNullOrWhiteSpace(notes))
        {
            AddInternalNotes($"Interaction terminée: {notes}");
        }

        // Si c'était en échec, marquer pour suivi
        if (outcome == InteractionOutcome.Failure)
        {
            RequireFollowUp("Échec de l'interaction - suivi nécessaire");
        }
    }

    public void RequireFollowUp(string reason, DateTime? followUpDate = null)
    {
        RequiresFollowUp = true;
        FollowUpDate = followUpDate ?? DateTime.UtcNow.AddDays(7); // Par défaut, suivi dans 7 jours
        IsFollowUpCompleted = false;

        AddInternalNotes($"Suivi requis: {reason}");

        // Déclencher un événement de suivi requis
        AddDomainEvent(new CustomerInteractionFollowUpRequiredEvent(
            CustomerId, Id, reason, FollowUpDate.Value, InteractionDate));
    }

    public void CompleteFollowUp(string? notes = null)
    {
        if (!RequiresFollowUp)
            throw new InvalidOperationException("No follow-up was required");

        IsFollowUpCompleted = true;

        if (!string.IsNullOrWhiteSpace(notes))
        {
            AddInternalNotes($"Suivi complété: {notes}");
        }
        else
        {
            AddInternalNotes("Suivi complété");
        }
    }

    public void PostponeFollowUp(DateTime newFollowUpDate, string? reason = null)
    {
        if (!RequiresFollowUp)
            throw new InvalidOperationException("No follow-up was required");

        if (newFollowUpDate <= DateTime.UtcNow)
            throw new ArgumentException("Follow-up date must be in the future", nameof(newFollowUpDate));

        FollowUpDate = newFollowUpDate;

        var reasonText = !string.IsNullOrWhiteSpace(reason) ? $": {reason}" : "";
        AddInternalNotes($"Suivi reporté au {newFollowUpDate:yyyy-MM-dd}{reasonText}");
    }

    // Méthodes métier pour les pièces jointes et métadonnées
    public void AddAttachment(string attachmentInfo)
    {
        if (string.IsNullOrWhiteSpace(attachmentInfo))
            throw new ArgumentException("Attachment info cannot be empty", nameof(attachmentInfo));

        var attachment = attachmentInfo.Trim();
        
        if (string.IsNullOrWhiteSpace(Attachments))
        {
            Attachments = attachment;
        }
        else
        {
            Attachments = $"{Attachments};{attachment}";
        }
    }

    public void SetMetadata(string? metadata)
    {
        Metadata = metadata?.Trim();
    }

    // Propriétés calculées et d'analyse
    public bool IsPositiveInteraction => SatisfactionRating.HasValue && SatisfactionRating.Value >= 4;
    public bool IsNegativeInteraction => SatisfactionRating.HasValue && SatisfactionRating.Value <= 2;
    public bool HasFeedback => !string.IsNullOrWhiteSpace(CustomerFeedback);
    public bool HasAttachments => !string.IsNullOrWhiteSpace(Attachments);
    public bool IsOverdue => RequiresFollowUp && FollowUpDate.HasValue && DateTime.UtcNow > FollowUpDate.Value && !IsFollowUpCompleted;
    public bool IsRecent => (DateTime.UtcNow - InteractionDate).TotalDays <= 7;

    public TimeSpan? TimeToComplete => Outcome != InteractionOutcome.Pending ? Duration : null;

    public int DaysUntilFollowUp
    {
        get
        {
            if (!RequiresFollowUp || !FollowUpDate.HasValue)
                return 0;

            return Math.Max(0, (FollowUpDate.Value - DateTime.UtcNow).Days);
        }
    }

    public bool IsComplaint => Type == InteractionType.Complaint;
    public bool IsCompliment => Type == InteractionType.Compliment;
    public bool IsSupport => Type == InteractionType.Support;

    public string GetOutcomeDescription()
    {
        return Outcome switch
        {
            InteractionOutcome.Success => "Succès",
            InteractionOutcome.Failure => "Échec",
            InteractionOutcome.Pending => "En cours",
            InteractionOutcome.Cancelled => "Annulé",
            InteractionOutcome.Postponed => "Reporté",
            InteractionOutcome.Transferred => "Transféré",
            _ => "Inconnu"
        };
    }

    public string GetChannelDescription()
    {
        return Channel switch
        {
            InteractionChannel.Website => "Site web",
            InteractionChannel.MobileApp => "Application mobile",
            InteractionChannel.Phone => "Téléphone",
            InteractionChannel.Email => "Email",
            InteractionChannel.SMS => "SMS",
            InteractionChannel.InPerson => "En personne",
            InteractionChannel.SocialMedia => "Réseaux sociaux",
            InteractionChannel.LiveChat => "Chat en ligne",
            InteractionChannel.Mail => "Courrier",
            _ => "Autre"
        };
    }

    public Dictionary<string, object> GetInteractionMetrics()
    {
        return new Dictionary<string, object>
        {
            { "Type", Type.ToString() },
            { "Channel", Channel.ToString() },
            { "Outcome", Outcome.ToString() },
            { "Duration", Duration?.TotalMinutes ?? 0 },
            { "SatisfactionRating", SatisfactionRating ?? 0 },
            { "HasFeedback", HasFeedback },
            { "RequiresFollowUp", RequiresFollowUp },
            { "IsCompleted", Outcome != InteractionOutcome.Pending },
            { "IsPositive", IsPositiveInteraction },
            { "DaysAgo", (DateTime.UtcNow - InteractionDate).Days }
        };
    }
}