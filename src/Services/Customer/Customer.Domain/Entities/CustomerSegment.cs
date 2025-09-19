using BuildingBlocks.Domain.Entities;
using Customer.Domain.Enums;
using Customer.Domain.Events;

namespace Customer.Domain.Entities;

/// <summary>
/// Segment de clientèle pour la segmentation marketing et analytique
/// </summary>
public class CustomerSegment : Entity<Guid>
{
    private readonly List<SegmentCriterion> _criteria = new();
    private readonly List<Guid> _customerIds = new();

    public string Name { get; private set; }
    public string Description { get; private set; }
    public SegmentType Type { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsAutomatic { get; private set; }
    public string? Color { get; private set; }
    public string? Icon { get; private set; }
    public int Priority { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime LastUpdatedDate { get; private set; }
    public DateTime? LastCalculationDate { get; private set; }
    public int CustomerCount { get; private set; }
    public string? Notes { get; private set; }

    // Collections en lecture seule
    public IReadOnlyCollection<SegmentCriterion> Criteria => _criteria.AsReadOnly();
    public IReadOnlyCollection<Guid> CustomerIds => _customerIds.AsReadOnly();

    protected CustomerSegment() { }

    public CustomerSegment(
        string name,
        string description,
        SegmentType type,
        bool isAutomatic = true,
        int priority = 0)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        Name = name.Trim();
        Description = description.Trim();
        Type = type;
        IsActive = true;
        IsAutomatic = isAutomatic;
        Priority = priority;
        CreatedDate = DateTime.UtcNow;
        LastUpdatedDate = DateTime.UtcNow;
        CustomerCount = 0;
    }

    // Méthodes métier pour la gestion du segment
    public void UpdateBasicInfo(string name, string description, int priority)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        Name = name.Trim();
        Description = description.Trim();
        Priority = priority;
        LastUpdatedDate = DateTime.UtcNow;
    }

    public void UpdateAppearance(string? color, string? icon)
    {
        Color = color?.Trim();
        Icon = icon?.Trim();
        LastUpdatedDate = DateTime.UtcNow;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
        LastUpdatedDate = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        LastUpdatedDate = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        LastUpdatedDate = DateTime.UtcNow;
    }

    public void ConvertToManual()
    {
        if (!IsAutomatic)
            return;

        IsAutomatic = false;
        LastUpdatedDate = DateTime.UtcNow;
    }

    public void ConvertToAutomatic()
    {
        if (IsAutomatic)
            return;

        if (!_criteria.Any())
            throw new InvalidOperationException("Cannot convert to automatic without criteria");

        IsAutomatic = true;
        LastUpdatedDate = DateTime.UtcNow;
    }

    // Méthodes métier pour les critères
    public void AddCriterion(SegmentCriterion criterion)
    {
        if (criterion == null)
            throw new ArgumentNullException(nameof(criterion));

        _criteria.Add(criterion);
        LastUpdatedDate = DateTime.UtcNow;

        if (IsAutomatic)
        {
            // Déclencher un recalcul automatique
            MarkForRecalculation();
        }
    }

    public void RemoveCriterion(Guid criterionId)
    {
        var criterion = _criteria.FirstOrDefault(c => c.Id == criterionId);
        if (criterion != null)
        {
            _criteria.Remove(criterion);
            LastUpdatedDate = DateTime.UtcNow;

            if (IsAutomatic)
            {
                MarkForRecalculation();
            }
        }
    }

    public void UpdateCriterion(Guid criterionId, SegmentCriteria criteria, string operator_, object value)
    {
        var criterion = _criteria.FirstOrDefault(c => c.Id == criterionId);
        if (criterion == null)
            throw new ArgumentException("Criterion not found", nameof(criterionId));

        criterion.Update(criteria, operator_, value);
        LastUpdatedDate = DateTime.UtcNow;

        if (IsAutomatic)
        {
            MarkForRecalculation();
        }
    }

    public void ClearCriteria()
    {
        _criteria.Clear();
        LastUpdatedDate = DateTime.UtcNow;

        if (IsAutomatic)
        {
            MarkForRecalculation();
        }
    }

    // Méthodes métier pour la gestion des clients
    public void AddCustomer(Guid customerId)
    {
        if (IsAutomatic)
            throw new InvalidOperationException("Cannot manually add customers to automatic segments");

        if (!_customerIds.Contains(customerId))
        {
            _customerIds.Add(customerId);
            CustomerCount = _customerIds.Count;
            LastUpdatedDate = DateTime.UtcNow;
        }
    }

    public void RemoveCustomer(Guid customerId)
    {
        if (IsAutomatic)
            throw new InvalidOperationException("Cannot manually remove customers from automatic segments");

        if (_customerIds.Remove(customerId))
        {
            CustomerCount = _customerIds.Count;
            LastUpdatedDate = DateTime.UtcNow;
        }
    }

    public void SetCustomers(IEnumerable<Guid> customerIds)
    {
        if (customerIds == null)
            throw new ArgumentNullException(nameof(customerIds));

        _customerIds.Clear();
        _customerIds.AddRange(customerIds.Distinct());
        CustomerCount = _customerIds.Count;
        LastCalculationDate = DateTime.UtcNow;
        LastUpdatedDate = DateTime.UtcNow;
    }

    public void ClearCustomers()
    {
        _customerIds.Clear();
        CustomerCount = 0;
        LastUpdatedDate = DateTime.UtcNow;

        if (IsAutomatic)
        {
            LastCalculationDate = DateTime.UtcNow;
        }
    }

    // Méthodes d'analyse et validation
    public bool ContainsCustomer(Guid customerId)
    {
        return _customerIds.Contains(customerId);
    }

    public bool HasCriteria => _criteria.Any();

    public bool IsValidForAutomatic => HasCriteria && IsActive;

    public bool NeedsRecalculation
    {
        get
        {
            if (!IsAutomatic || !IsActive)
                return false;

            // Recalculer si jamais calculé ou si plus ancien que 24h
            return !LastCalculationDate.HasValue ||
                   DateTime.UtcNow.Subtract(LastCalculationDate.Value).TotalHours >= 24;
        }
    }

    public TimeSpan? TimeSinceLastCalculation
    {
        get
        {
            if (!LastCalculationDate.HasValue)
                return null;

            return DateTime.UtcNow - LastCalculationDate.Value;
        }
    }

    // Méthodes de comparaison de segments
    public decimal CalculateSimilarityWith(CustomerSegment otherSegment)
    {
        if (otherSegment == null)
            return 0m;

        if (!_customerIds.Any() || !otherSegment._customerIds.Any())
            return 0m;

        var intersection = _customerIds.Intersect(otherSegment._customerIds).Count();
        var union = _customerIds.Union(otherSegment._customerIds).Count();

        return union > 0 ? (decimal)intersection / union : 0m;
    }

    public int GetOverlapWith(CustomerSegment otherSegment)
    {
        if (otherSegment == null)
            return 0;

        return _customerIds.Intersect(otherSegment._customerIds).Count();
    }

    // Méthodes statistiques
    public decimal GetGrowthRate(TimeSpan period)
    {
        // Cette méthode nécessiterait un accès à l'historique
        // Implémentation simplifiée pour la démonstration
        return 0m;
    }

    public Dictionary<string, object> GetSegmentStatistics()
    {
        return new Dictionary<string, object>
        {
            { "CustomerCount", CustomerCount },
            { "Type", Type.ToString() },
            { "IsAutomatic", IsAutomatic },
            { "CriteriaCount", _criteria.Count },
            { "LastCalculated", LastCalculationDate },
            { "DaysSinceCreation", (DateTime.UtcNow - CreatedDate).Days },
            { "IsActive", IsActive }
        };
    }

    // Méthodes privées
    private void MarkForRecalculation()
    {
        // Dans une vraie implémentation, cela déclencherait un événement
        // pour signaler qu'un recalcul est nécessaire
        LastUpdatedDate = DateTime.UtcNow;
    }

    public bool ValidateCriteria()
    {
        if (!IsAutomatic)
            return true;

        return _criteria.All(c => c.IsValid());
    }

    public string GetCriteriaDescription()
    {
        if (!_criteria.Any())
            return "Aucun critère défini";

        return string.Join(" ET ", _criteria.Select(c => c.GetDescription()));
    }
}

/// <summary>
/// Critère de segmentation individuel
/// </summary>
public class SegmentCriterion : Entity<Guid>
{
    public SegmentCriteria Criteria { get; private set; }
    public string Operator { get; private set; }
    public string Value { get; private set; }
    public string? ValueType { get; private set; }
    public bool IsNegated { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime LastModifiedDate { get; private set; }

    protected SegmentCriterion() { }

    public SegmentCriterion(
        SegmentCriteria criteria,
        string operator_,
        object value,
        bool isNegated = false)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(operator_))
            throw new ArgumentException("Operator cannot be empty", nameof(operator_));

        if (value == null)
            throw new ArgumentNullException(nameof(value));

        Criteria = criteria;
        Operator = operator_.Trim();
        Value = value.ToString() ?? string.Empty;
        ValueType = value.GetType().Name;
        IsNegated = isNegated;
        CreatedDate = DateTime.UtcNow;
        LastModifiedDate = DateTime.UtcNow;
    }

    public void Update(SegmentCriteria criteria, string operator_, object value)
    {
        if (string.IsNullOrWhiteSpace(operator_))
            throw new ArgumentException("Operator cannot be empty", nameof(operator_));

        if (value == null)
            throw new ArgumentNullException(nameof(value));

        Criteria = criteria;
        Operator = operator_.Trim();
        Value = value.ToString() ?? string.Empty;
        ValueType = value.GetType().Name;
        LastModifiedDate = DateTime.UtcNow;
    }

    public void ToggleNegation()
    {
        IsNegated = !IsNegated;
        LastModifiedDate = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        // Validation basique des critères
        if (string.IsNullOrWhiteSpace(Value))
            return false;

        return Criteria switch
        {
            SegmentCriteria.Age => int.TryParse(Value, out _),
            SegmentCriteria.AverageSpending => decimal.TryParse(Value, out _),
            SegmentCriteria.TotalSpending => decimal.TryParse(Value, out _),
            SegmentCriteria.VisitFrequency => int.TryParse(Value, out _),
            SegmentCriteria.CustomerAge => int.TryParse(Value, out _),
            SegmentCriteria.Satisfaction => decimal.TryParse(Value, out var rating) && rating >= 0 && rating <= 5,
            _ => true
        };
    }

    public string GetDescription()
    {
        var negation = IsNegated ? "PAS " : "";
        var criteriaName = GetCriteriaDisplayName();
        
        return $"{negation}{criteriaName} {Operator} {Value}";
    }

    private string GetCriteriaDisplayName()
    {
        return Criteria switch
        {
            SegmentCriteria.Age => "Âge",
            SegmentCriteria.VisitFrequency => "Fréquence de visite",
            SegmentCriteria.AverageSpending => "Panier moyen",
            SegmentCriteria.TotalSpending => "Total dépensé",
            SegmentCriteria.Location => "Localisation",
            SegmentCriteria.CulinaryPreferences => "Préférences culinaires",
            SegmentCriteria.CustomerAge => "Ancienneté client",
            SegmentCriteria.Satisfaction => "Satisfaction",
            _ => Criteria.ToString()
        };
    }
}