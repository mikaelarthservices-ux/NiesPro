using BuildingBlocks.Domain.Entities;
using Customer.Domain.Enums;
using Customer.Domain.ValueObjects;
using Customer.Domain.Events;

namespace Customer.Domain.Entities;

/// <summary>
/// Entité racine représentant un client dans le système CRM
/// </summary>
public class Customer : AggregateRoot<Guid>
{
    private readonly List<CustomerInteraction> _interactions = new();
    private readonly List<CustomerPreference> _preferences = new();

    public string CustomerNumber { get; private set; }
    public PersonalInfo PersonalInfo { get; private set; }
    public ContactInfo ContactInfo { get; private set; }
    public CustomerType Type { get; private set; }
    public CustomerStatus Status { get; private set; }
    public DateTime RegistrationDate { get; private set; }
    public DateTime? LastActivityDate { get; private set; }
    public CommunicationSettings CommunicationSettings { get; private set; }
    public LoyaltyStats LoyaltyStats { get; private set; }
    public BehavioralMetrics BehavioralMetrics { get; private set; }
    public SatisfactionMetrics SatisfactionMetrics { get; private set; }
    public string? Notes { get; private set; }
    public string? ReferralSource { get; private set; }
    public Guid? ReferredById { get; private set; }
    public bool IsVIP { get; private set; }
    public DateTime? VIPSince { get; private set; }
    public string? VIPReason { get; private set; }

    // Navigation properties
    public IReadOnlyCollection<CustomerInteraction> Interactions => _interactions.AsReadOnly();
    public IReadOnlyCollection<CustomerPreference> Preferences => _preferences.AsReadOnly();

    protected Customer() { }

    public Customer(
        PersonalInfo personalInfo,
        ContactInfo contactInfo,
        CustomerType type = CustomerType.Individual,
        string? referralSource = null,
        Guid? referredById = null)
        : base(Guid.NewGuid())
    {
        PersonalInfo = personalInfo ?? throw new ArgumentNullException(nameof(personalInfo));
        ContactInfo = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo));
        Type = type;
        Status = CustomerStatus.Prospect;
        RegistrationDate = DateTime.UtcNow;
        CustomerNumber = GenerateCustomerNumber();
        CommunicationSettings = new CommunicationSettings(CommunicationPreference.Email);
        LoyaltyStats = new LoyaltyStats();
        BehavioralMetrics = new BehavioralMetrics();
        SatisfactionMetrics = new SatisfactionMetrics();
        ReferralSource = referralSource;
        ReferredById = referredById;

        // Événement de domaine
        AddDomainEvent(new CustomerRegisteredEvent(
            Id, CustomerNumber, personalInfo.FullName, contactInfo.Email, type, RegistrationDate));
    }

    // Méthodes métier pour la gestion du profil
    public void UpdatePersonalInfo(PersonalInfo personalInfo)
    {
        if (personalInfo == null)
            throw new ArgumentNullException(nameof(personalInfo));

        var oldInfo = PersonalInfo;
        PersonalInfo = personalInfo;
        LastActivityDate = DateTime.UtcNow;

        AddDomainEvent(new CustomerPersonalInfoUpdatedEvent(
            Id, CustomerNumber, oldInfo, personalInfo, DateTime.UtcNow));
    }

    public void UpdateContactInfo(ContactInfo contactInfo)
    {
        if (contactInfo == null)
            throw new ArgumentNullException(nameof(contactInfo));

        var oldEmail = ContactInfo.Email;
        ContactInfo = contactInfo;
        LastActivityDate = DateTime.UtcNow;

        if (oldEmail != contactInfo.Email)
        {
            AddDomainEvent(new CustomerEmailChangedEvent(
                Id, CustomerNumber, oldEmail, contactInfo.Email, DateTime.UtcNow));
        }

        AddDomainEvent(new CustomerContactInfoUpdatedEvent(
            Id, CustomerNumber, contactInfo, DateTime.UtcNow));
    }

    public void UpdateCommunicationSettings(CommunicationSettings settings)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        CommunicationSettings = settings;
        LastActivityDate = DateTime.UtcNow;

        AddDomainEvent(new CustomerCommunicationSettingsUpdatedEvent(
            Id, CustomerNumber, settings, DateTime.UtcNow));
    }

    // Méthodes métier pour la gestion du statut
    public void Activate()
    {
        if (Status == CustomerStatus.Active)
            return;

        var oldStatus = Status;
        Status = CustomerStatus.Active;
        LastActivityDate = DateTime.UtcNow;

        AddDomainEvent(new CustomerStatusChangedEvent(
            Id, CustomerNumber, oldStatus, Status, DateTime.UtcNow));
    }

    public void Deactivate(string reason)
    {
        if (Status == CustomerStatus.Inactive)
            return;

        var oldStatus = Status;
        Status = CustomerStatus.Inactive;
        LastActivityDate = DateTime.UtcNow;
        Notes = $"{Notes}\n[{DateTime.UtcNow:yyyy-MM-dd}] Désactivé: {reason}".Trim();

        AddDomainEvent(new CustomerStatusChangedEvent(
            Id, CustomerNumber, oldStatus, Status, DateTime.UtcNow, reason));
    }

    public void Suspend(string reason)
    {
        if (Status == CustomerStatus.Suspended)
            return;

        var oldStatus = Status;
        Status = CustomerStatus.Suspended;
        LastActivityDate = DateTime.UtcNow;
        Notes = $"{Notes}\n[{DateTime.UtcNow:yyyy-MM-dd}] Suspendu: {reason}".Trim();

        AddDomainEvent(new CustomerStatusChangedEvent(
            Id, CustomerNumber, oldStatus, Status, DateTime.UtcNow, reason));
    }

    public void Ban(string reason)
    {
        var oldStatus = Status;
        Status = CustomerStatus.Banned;
        LastActivityDate = DateTime.UtcNow;
        Notes = $"{Notes}\n[{DateTime.UtcNow:yyyy-MM-dd}] Banni: {reason}".Trim();

        AddDomainEvent(new CustomerStatusChangedEvent(
            Id, CustomerNumber, oldStatus, Status, DateTime.UtcNow, reason));

        AddDomainEvent(new CustomerBannedEvent(
            Id, CustomerNumber, reason, DateTime.UtcNow));
    }

    // Méthodes métier pour le statut VIP
    public void PromoteToVIP(string reason)
    {
        if (IsVIP)
            return;

        IsVIP = true;
        VIPSince = DateTime.UtcNow;
        VIPReason = reason;
        LastActivityDate = DateTime.UtcNow;

        AddDomainEvent(new CustomerPromotedToVIPEvent(
            Id, CustomerNumber, reason, DateTime.UtcNow));
    }

    public void RemoveVIPStatus(string reason)
    {
        if (!IsVIP)
            return;

        IsVIP = false;
        VIPSince = null;
        VIPReason = null;
        LastActivityDate = DateTime.UtcNow;
        Notes = $"{Notes}\n[{DateTime.UtcNow:yyyy-MM-dd}] VIP retiré: {reason}".Trim();

        AddDomainEvent(new CustomerVIPStatusRemovedEvent(
            Id, CustomerNumber, reason, DateTime.UtcNow));
    }

    // Méthodes métier pour la fidélité
    public void AddLoyaltyPoints(int points, string reason)
    {
        if (points <= 0)
            throw new ArgumentException("Points must be positive", nameof(points));

        var oldPoints = LoyaltyStats.TotalPoints;
        LoyaltyStats = LoyaltyStats.AddPoints(points);
        LastActivityDate = DateTime.UtcNow;

        AddDomainEvent(new CustomerLoyaltyPointsEarnedEvent(
            Id, CustomerNumber, points, reason, oldPoints, LoyaltyStats.TotalPoints, DateTime.UtcNow));

        // Vérifier l'upgrade de tier
        CheckAndUpgradeLoyaltyTier();
    }

    public void RedeemLoyaltyPoints(int points, string reason)
    {
        if (points <= 0)
            throw new ArgumentException("Points must be positive", nameof(points));

        if (points > LoyaltyStats.TotalPoints)
            throw new InvalidOperationException("Insufficient loyalty points");

        var oldPoints = LoyaltyStats.TotalPoints;
        LoyaltyStats = LoyaltyStats.RedeemPoints(points);
        LastActivityDate = DateTime.UtcNow;

        AddDomainEvent(new CustomerLoyaltyPointsRedeemedEvent(
            Id, CustomerNumber, points, reason, oldPoints, LoyaltyStats.TotalPoints, DateTime.UtcNow));
    }

    public void RecordVisit(decimal amountSpent)
    {
        if (amountSpent < 0)
            throw new ArgumentException("Amount spent cannot be negative", nameof(amountSpent));

        LoyaltyStats = LoyaltyStats.RecordVisit(amountSpent);
        LastActivityDate = DateTime.UtcNow;

        // Auto-activation lors de la première commande
        if (Status == CustomerStatus.Prospect)
        {
            Activate();
        }

        AddDomainEvent(new CustomerVisitRecordedEvent(
            Id, CustomerNumber, amountSpent, DateTime.UtcNow));
    }

    // Méthodes métier pour les interactions
    public void AddInteraction(CustomerInteraction interaction)
    {
        if (interaction == null)
            throw new ArgumentNullException(nameof(interaction));

        _interactions.Add(interaction);
        LastActivityDate = DateTime.UtcNow;
    }

    // Méthodes métier pour les préférences
    public void AddPreference(CustomerPreference preference)
    {
        if (preference == null)
            throw new ArgumentNullException(nameof(preference));

        _preferences.Add(preference);
        LastActivityDate = DateTime.UtcNow;

        AddDomainEvent(new CustomerPreferenceAddedEvent(
            Id, CustomerNumber, preference.Type, preference.Value, DateTime.UtcNow));
    }

    public void RemovePreference(Guid preferenceId)
    {
        var preference = _preferences.FirstOrDefault(p => p.Id == preferenceId);
        if (preference != null)
        {
            _preferences.Remove(preference);
            LastActivityDate = DateTime.UtcNow;

            AddDomainEvent(new CustomerPreferenceRemovedEvent(
                Id, CustomerNumber, preference.Type, preference.Value, DateTime.UtcNow));
        }
    }

    // Méthodes métier pour les notes
    public void AddNote(string note)
    {
        if (string.IsNullOrWhiteSpace(note))
            throw new ArgumentException("Note cannot be empty", nameof(note));

        Notes = $"{Notes}\n[{DateTime.UtcNow:yyyy-MM-dd}] {note}".Trim();
        LastActivityDate = DateTime.UtcNow;

        AddDomainEvent(new CustomerNoteAddedEvent(
            Id, CustomerNumber, note, DateTime.UtcNow));
    }

    // Méthodes d'analyse métier
    public bool IsActive => Status == CustomerStatus.Active && 
                           LastActivityDate.HasValue && 
                           LastActivityDate.Value >= DateTime.UtcNow.AddDays(-90);

    public bool IsFrequentCustomer => LoyaltyStats.TotalVisits >= 10 && 
                                     LoyaltyStats.DaysSinceLastVisit <= 30;

    public bool IsHighValueCustomer => LoyaltyStats.TotalSpent >= 1000m || 
                                      BehavioralMetrics.AverageOrderValue >= 100m;

    public int DaysSinceRegistration => (DateTime.UtcNow - RegistrationDate).Days;

    // Méthodes utilitaires privées
    private void CheckAndUpgradeLoyaltyTier()
    {
        var currentTier = LoyaltyStats.CurrentTier;
        var newTier = CalculateTierFromPoints(LoyaltyStats.LifetimePoints);

        if (newTier > currentTier)
        {
            LoyaltyStats = LoyaltyStats.UpgradeTier(newTier);

            AddDomainEvent(new CustomerLoyaltyTierUpgradedEvent(
                Id, CustomerNumber, currentTier, newTier, LoyaltyStats.LifetimePoints, DateTime.UtcNow));
        }
    }

    private static LoyaltyTier CalculateTierFromPoints(int lifetimePoints)
    {
        return lifetimePoints switch
        {
            >= 10000 => LoyaltyTier.Diamond,
            >= 5000 => LoyaltyTier.Platinum,
            >= 2000 => LoyaltyTier.Gold,
            >= 500 => LoyaltyTier.Silver,
            _ => LoyaltyTier.Bronze
        };
    }

    private static string GenerateCustomerNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(1000, 9999);
        return $"CUST{timestamp}{random}";
    }
}