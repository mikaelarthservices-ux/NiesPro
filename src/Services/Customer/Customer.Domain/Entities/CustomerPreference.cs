using BuildingBlocks.Domain.Entities;
using Customer.Domain.Enums;

namespace Customer.Domain.Entities;

/// <summary>
/// Préférence spécifique d'un client
/// </summary>
public class CustomerPreference : Entity<Guid>
{
    public Guid CustomerId { get; private set; }
    public PreferenceType Type { get; private set; }
    public string Key { get; private set; }
    public string Value { get; private set; }
    public string? DisplayValue { get; private set; }
    public int Priority { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime LastUsedDate { get; private set; }
    public int UsageCount { get; private set; }
    public string? Source { get; private set; }
    public decimal? Confidence { get; private set; }
    public string? Context { get; private set; }
    public DateTime? ExpirationDate { get; private set; }
    public string? Notes { get; private set; }

    protected CustomerPreference() { }

    public CustomerPreference(
        Guid customerId,
        PreferenceType type,
        string key,
        string value,
        string? displayValue = null,
        int priority = 0,
        string? source = null,
        decimal? confidence = null)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be empty", nameof(value));

        if (confidence.HasValue && (confidence.Value < 0 || confidence.Value > 1))
            throw new ArgumentException("Confidence must be between 0 and 1", nameof(confidence));

        CustomerId = customerId;
        Type = type;
        Key = key.Trim();
        Value = value.Trim();
        DisplayValue = displayValue?.Trim() ?? Value;
        Priority = priority;
        IsActive = true;
        CreatedDate = DateTime.UtcNow;
        LastUsedDate = DateTime.UtcNow;
        UsageCount = 0;
        Source = source?.Trim();
        Confidence = confidence;
    }

    // Méthodes métier pour la gestion de la préférence
    public void UpdateValue(string value, string? displayValue = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be empty", nameof(value));

        Value = value.Trim();
        DisplayValue = displayValue?.Trim() ?? Value;
        LastUsedDate = DateTime.UtcNow;
        
        // Augmenter la confiance si la préférence est mise à jour manuellement
        if (Confidence.HasValue && Confidence.Value < 1.0m)
        {
            Confidence = Math.Min(1.0m, Confidence.Value + 0.1m);
        }
    }

    public void UpdatePriority(int priority)
    {
        Priority = priority;
    }

    public void UpdateSource(string? source)
    {
        Source = source?.Trim();
    }

    public void UpdateConfidence(decimal confidence)
    {
        if (confidence < 0 || confidence > 1)
            throw new ArgumentException("Confidence must be between 0 and 1", nameof(confidence));

        Confidence = confidence;
    }

    public void UpdateContext(string? context)
    {
        Context = context?.Trim();
    }

    public void SetExpirationDate(DateTime? expirationDate)
    {
        if (expirationDate.HasValue && expirationDate.Value <= DateTime.UtcNow)
            throw new ArgumentException("Expiration date must be in the future", nameof(expirationDate));

        ExpirationDate = expirationDate;
    }

    public void AddNotes(string notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            throw new ArgumentException("Notes cannot be empty", nameof(notes));

        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
        var newNote = $"[{timestamp}] {notes.Trim()}";

        Notes = string.IsNullOrWhiteSpace(Notes) 
            ? newNote 
            : $"{Notes}\n{newNote}";
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
    }

    // Méthodes métier pour l'utilisation
    public void RecordUsage()
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot record usage for inactive preference");

        if (IsExpired)
            throw new InvalidOperationException("Cannot record usage for expired preference");

        UsageCount++;
        LastUsedDate = DateTime.UtcNow;

        // Augmenter légèrement la confiance à chaque utilisation
        if (Confidence.HasValue && Confidence.Value < 1.0m)
        {
            Confidence = Math.Min(1.0m, Confidence.Value + 0.01m);
        }
    }

    public void Activate()
    {
        if (IsExpired)
            throw new InvalidOperationException("Cannot activate expired preference");

        IsActive = true;
    }

    public void Deactivate(string? reason = null)
    {
        IsActive = false;
        
        if (!string.IsNullOrWhiteSpace(reason))
        {
            AddNotes($"Désactivé: {reason}");
        }
    }

    // Propriétés calculées
    public bool IsExpired => ExpirationDate.HasValue && DateTime.UtcNow > ExpirationDate.Value;
    public bool IsRecentlyUsed => (DateTime.UtcNow - LastUsedDate).TotalDays <= 30;
    public bool IsFrequentlyUsed => UsageCount >= 10;
    public bool IsHighConfidence => Confidence.HasValue && Confidence.Value >= 0.8m;
    public bool IsLowConfidence => Confidence.HasValue && Confidence.Value <= 0.3m;
    public bool IsValid => IsActive && !IsExpired;

    public int DaysSinceCreation => (DateTime.UtcNow - CreatedDate).Days;
    public int DaysSinceLastUse => (DateTime.UtcNow - LastUsedDate).Days;
    public int DaysUntilExpiration => ExpirationDate.HasValue 
        ? Math.Max(0, (ExpirationDate.Value - DateTime.UtcNow).Days) 
        : int.MaxValue;

    public decimal CalculateRelevanceScore()
    {
        var baseScore = 0.5m;

        // Bonus pour l'activité
        if (IsActive) baseScore += 0.2m;

        // Bonus pour l'utilisation récente
        if (IsRecentlyUsed) baseScore += 0.1m;

        // Bonus pour l'utilisation fréquente
        if (IsFrequentlyUsed) baseScore += 0.1m;

        // Bonus pour la confiance élevée
        if (Confidence.HasValue)
        {
            baseScore += Confidence.Value * 0.3m;
        }

        // Bonus pour la priorité
        if (Priority > 0)
        {
            baseScore += Math.Min(0.2m, Priority * 0.02m);
        }

        // Malus pour l'expiration proche
        if (ExpirationDate.HasValue && DaysUntilExpiration <= 7)
        {
            baseScore -= 0.1m;
        }

        // Malus pour l'inactivité prolongée
        if (DaysSinceLastUse > 90)
        {
            baseScore -= 0.2m;
        }

        return Math.Max(0m, Math.Min(1m, baseScore));
    }

    // Méthodes de comparaison et groupement
    public bool IsSimilarTo(CustomerPreference other)
    {
        if (other == null)
            return false;

        return Type == other.Type && 
               string.Equals(Key, other.Key, StringComparison.OrdinalIgnoreCase);
    }

    public bool ConflictsWith(CustomerPreference other)
    {
        if (other == null || !IsSimilarTo(other))
            return false;

        // Conflit si même type/clé mais valeurs différentes et les deux sont actives
        return IsActive && other.IsActive && 
               !string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    // Méthodes spécifiques par type de préférence
    public static CustomerPreference CreateCulinaryPreference(
        Guid customerId, 
        string dishName, 
        bool isLiked,
        decimal confidence = 0.7m)
    {
        var type = PreferenceType.Culinary;
        var key = isLiked ? "favorite_dish" : "disliked_dish";
        var source = "user_feedback";

        return new CustomerPreference(
            customerId, type, key, dishName, dishName, 
            isLiked ? 1 : -1, source, confidence);
    }

    public static CustomerPreference CreateTablePreference(
        Guid customerId,
        string tableLocation,
        string reason = "user_specified")
    {
        return new CustomerPreference(
            customerId, PreferenceType.Table, "preferred_location", 
            tableLocation, tableLocation, 0, reason, 0.9m);
    }

    public static CustomerPreference CreateTimingPreference(
        Guid customerId,
        PreferredTimeSlot timeSlot,
        string source = "usage_pattern")
    {
        return new CustomerPreference(
            customerId, PreferenceType.Timing, "preferred_time_slot", 
            timeSlot.ToString(), GetTimeSlotDisplayName(timeSlot), 0, source, 0.6m);
    }

    public static CustomerPreference CreateAmbiancePreference(
        Guid customerId,
        AmbiancePreference ambiance,
        decimal confidence = 0.7m)
    {
        return new CustomerPreference(
            customerId, PreferenceType.Ambiance, "preferred_ambiance", 
            ambiance.ToString(), GetAmbianceDisplayName(ambiance), 0, "profile", confidence);
    }

    public static CustomerPreference CreateCommunicationPreference(
        Guid customerId,
        CommunicationPreference channel,
        CommunicationFrequency frequency)
    {
        var preference = new CustomerPreference(
            customerId, PreferenceType.Communication, "preferred_channel", 
            channel.ToString(), GetChannelDisplayName(channel), 0, "user_settings", 1.0m);

        preference.UpdateContext($"frequency:{frequency}");
        return preference;
    }

    // Méthodes utilitaires privées
    private static string GetTimeSlotDisplayName(PreferredTimeSlot timeSlot)
    {
        return timeSlot switch
        {
            PreferredTimeSlot.Breakfast => "Petit-déjeuner",
            PreferredTimeSlot.Lunch => "Déjeuner",
            PreferredTimeSlot.Afternoon => "Après-midi",
            PreferredTimeSlot.Dinner => "Dîner",
            PreferredTimeSlot.LateNight => "Tard le soir",
            PreferredTimeSlot.Flexible => "Flexible",
            _ => timeSlot.ToString()
        };
    }

    private static string GetAmbianceDisplayName(AmbiancePreference ambiance)
    {
        return ambiance switch
        {
            AmbiancePreference.Quiet => "Calme",
            AmbiancePreference.Lively => "Animé",
            AmbiancePreference.Romantic => "Romantique",
            AmbiancePreference.Family => "Familial",
            AmbiancePreference.Business => "Professionnel",
            AmbiancePreference.Casual => "Décontracté",
            AmbiancePreference.Upscale => "Haut de gamme",
            _ => ambiance.ToString()
        };
    }

    private static string GetChannelDisplayName(CommunicationPreference channel)
    {
        return channel switch
        {
            CommunicationPreference.Email => "Email",
            CommunicationPreference.SMS => "SMS",
            CommunicationPreference.Phone => "Téléphone",
            CommunicationPreference.Push => "Notifications push",
            CommunicationPreference.Mail => "Courrier",
            CommunicationPreference.All => "Tous les canaux",
            CommunicationPreference.None => "Aucun",
            _ => channel.ToString()
        };
    }

    public override string ToString()
    {
        return $"{Type}: {Key} = {DisplayValue ?? Value}";
    }
}