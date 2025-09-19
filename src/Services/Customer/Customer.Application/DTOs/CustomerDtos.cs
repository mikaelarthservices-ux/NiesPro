using Customer.Domain.Enums;

namespace Customer.Application.DTOs;

/// <summary>
/// DTO pour la création d'un nouveau client
/// </summary>
public class CreateCustomerDto
{
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Phone { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public Gender? Gender { get; init; }
    public string? RegistrationSource { get; init; }
    public string? ReferralCode { get; init; }
    public bool AcceptMarketing { get; init; } = false;
    public bool AcceptTerms { get; init; } = true;
    
    // Informations de profil optionnelles
    public List<DietaryRestriction>? DietaryRestrictions { get; init; }
    public List<string>? Allergies { get; init; }
    public AmbiancePreference? PreferredAmbiance { get; init; }
    public PreferredTimeSlot? PreferredTimeSlot { get; init; }
}

/// <summary>
/// DTO pour la mise à jour d'un client
/// </summary>
public class UpdateCustomerDto
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Phone { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public Gender? Gender { get; init; }
    public bool? AcceptMarketing { get; init; }
}

/// <summary>
/// DTO complet pour l'affichage d'un client
/// </summary>
public class CustomerDto
{
    public Guid Id { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Phone { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public Gender? Gender { get; init; }
    public CustomerStatus Status { get; init; }
    public bool IsEmailVerified { get; init; }
    public bool AcceptMarketing { get; init; }
    public DateTime RegistrationDate { get; init; }
    public DateTime? LastLoginDate { get; init; }
    public DateTime? LastVisitDate { get; init; }
    public string? RegistrationSource { get; init; }
    public string? ReferralCode { get; init; }
    public Guid? ReferredByCustomerId { get; init; }
    
    // Informations calculées
    public string FullName => $"{FirstName} {LastName}";
    public int? Age => DateOfBirth?.CalculateAge();
    public int DaysSinceRegistration => (DateTime.UtcNow - RegistrationDate).Days;
    public bool IsActive => Status == CustomerStatus.Active;
    
    // Profil associé
    public CustomerProfileDto? Profile { get; init; }
    
    // Statistiques de fidélité
    public List<CustomerLoyaltyStatsDto>? LoyaltyStats { get; init; }
    
    // Segments
    public List<CustomerSegmentSummaryDto>? Segments { get; init; }
}

/// <summary>
/// DTO résumé pour les listes de clients
/// </summary>
public class CustomerSummaryDto
{
    public Guid Id { get; init; }
    public required string Email { get; init; }
    public required string FullName { get; init; }
    public string? Phone { get; init; }
    public CustomerStatus Status { get; init; }
    public DateTime RegistrationDate { get; init; }
    public DateTime? LastVisitDate { get; init; }
    public int TotalLoyaltyPoints { get; init; }
    public string? PrimarySegment { get; init; }
}

/// <summary>
/// DTO pour la recherche de clients
/// </summary>
public class CustomerSearchDto
{
    public string? SearchTerm { get; init; }
    public CustomerStatus? Status { get; init; }
    public DateTime? RegisteredAfter { get; init; }
    public DateTime? RegisteredBefore { get; init; }
    public bool? HasPhone { get; init; }
    public bool? IsEmailVerified { get; init; }
    public List<Guid>? SegmentIds { get; init; }
    public int? MinLoyaltyPoints { get; init; }
    public int? MaxLoyaltyPoints { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

/// <summary>
/// DTO pour les statistiques clients
/// </summary>
public class CustomerStatsDto
{
    public int TotalCustomers { get; init; }
    public int ActiveCustomers { get; init; }
    public int InactiveCustomers { get; init; }
    public int NewRegistrationsThisMonth { get; init; }
    public Dictionary<CustomerStatus, int> CustomersByStatus { get; init; } = new();
    public Dictionary<string, int> RegistrationsBySource { get; init; } = new();
    public double AverageCustomerAge { get; init; }
    public int EmailVerificationRate { get; init; }
}

/// <summary>
/// DTO pour le profil client
/// </summary>
public class CustomerProfileDto
{
    public Guid CustomerId { get; init; }
    public List<DietaryRestriction> DietaryRestrictions { get; init; } = new();
    public List<string> Allergies { get; init; } = new();
    public AmbiancePreference? PreferredAmbiance { get; init; }
    public PreferredTimeSlot? PreferredTimeSlot { get; init; }
    public int? PreferredTableSize { get; init; }
    public string? PreferredTableLocation { get; init; }
    public string? SpecialRequests { get; init; }
    public string? Notes { get; init; }
    public decimal CompletionPercentage { get; init; }
    public DateTime? LastUpdated { get; init; }
    
    // Préférences de communication
    public CommunicationPreference PreferredCommunication { get; init; }
    public CommunicationFrequency CommunicationFrequency { get; init; }
    public List<CommunicationPreference> AllowedChannels { get; init; } = new();
}

/// <summary>
/// DTO pour la mise à jour du profil
/// </summary>
public class UpdateCustomerProfileDto
{
    public List<DietaryRestriction>? DietaryRestrictions { get; init; }
    public List<string>? Allergies { get; init; }
    public AmbiancePreference? PreferredAmbiance { get; init; }
    public PreferredTimeSlot? PreferredTimeSlot { get; init; }
    public int? PreferredTableSize { get; init; }
    public string? PreferredTableLocation { get; init; }
    public string? SpecialRequests { get; init; }
    public string? Notes { get; init; }
    public CommunicationPreference? PreferredCommunication { get; init; }
    public CommunicationFrequency? CommunicationFrequency { get; init; }
    public List<CommunicationPreference>? AllowedChannels { get; init; }
}

/// <summary>
/// DTO pour les statistiques de fidélité d'un client
/// </summary>
public class CustomerLoyaltyStatsDto
{
    public Guid ProgramId { get; init; }
    public required string ProgramName { get; init; }
    public int TotalPoints { get; init; }
    public int AvailablePoints { get; init; }
    public int PendingPoints { get; init; }
    public required string CurrentTier { get; init; }
    public int PointsToNextTier { get; init; }
    public DateTime? TierExpirationDate { get; init; }
    public int LifetimePointsEarned { get; init; }
    public int LifetimePointsRedeemed { get; init; }
    public DateTime? LastPointsEarned { get; init; }
    public DateTime? LastPointsRedeemed { get; init; }
}

/// <summary>
/// DTO résumé pour les segments clients
/// </summary>
public class CustomerSegmentSummaryDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public SegmentType Type { get; init; }
    public decimal MatchScore { get; init; }
    public DateTime AssignedDate { get; init; }
    public bool IsAutomatic { get; init; }
}

// Extensions pour les calculs
public static class CustomerDtoExtensions
{
    public static int CalculateAge(this DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }
}