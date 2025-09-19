using BuildingBlocks.Domain.Entities;
using Customer.Domain.Enums;
using Customer.Domain.ValueObjects;
using Customer.Domain.Events;

namespace Customer.Domain.Entities;

/// <summary>
/// Profil détaillé et préférences avancées d'un client
/// </summary>
public class CustomerProfile : Entity<Guid>
{
    private readonly List<string> _allergies = new();
    private readonly List<DietaryRestriction> _dietaryRestrictions = new();
    private readonly List<string> _favoriteDishes = new();
    private readonly List<string> _dislikedDishes = new();

    public Guid CustomerId { get; private set; }
    public string? PhotoUrl { get; private set; }
    public string? Biography { get; private set; }
    public AmbiancePreference PreferredAmbiance { get; private set; }
    public PreferredTimeSlot PreferredTimeSlot { get; private set; }
    public int? PreferredTableSize { get; private set; }
    public string? PreferredTableLocation { get; private set; }
    public string? SpecialOccasions { get; private set; }
    public decimal? BudgetRange { get; private set; }
    public string? CulinaryStyle { get; private set; }
    public string? SpicePreference { get; private set; }
    public bool IsWineEnthusiast { get; private set; }
    public string? PreferredWineType { get; private set; }
    public bool HasChildrenAccommodations { get; private set; }
    public bool RequiresAccessibility { get; private set; }
    public string? AccessibilityNeeds { get; private set; }
    public string? MusicPreference { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime LastUpdatedDate { get; private set; }

    // Collections en lecture seule
    public IReadOnlyCollection<string> Allergies => _allergies.AsReadOnly();
    public IReadOnlyCollection<DietaryRestriction> DietaryRestrictions => _dietaryRestrictions.AsReadOnly();
    public IReadOnlyCollection<string> FavoriteDishes => _favoriteDishes.AsReadOnly();
    public IReadOnlyCollection<string> DislikedDishes => _dislikedDishes.AsReadOnly();

    protected CustomerProfile() { }

    public CustomerProfile(Guid customerId)
        : base(Guid.NewGuid())
    {
        CustomerId = customerId;
        PreferredAmbiance = AmbiancePreference.Casual;
        PreferredTimeSlot = PreferredTimeSlot.Flexible;
        CreatedDate = DateTime.UtcNow;
        LastUpdatedDate = DateTime.UtcNow;
    }

    // Méthodes métier pour la gestion du profil
    public void UpdateBasicInfo(
        string? biography = null,
        string? photoUrl = null,
        AmbiancePreference? ambiance = null,
        PreferredTimeSlot? timeSlot = null)
    {
        Biography = biography;
        PhotoUrl = photoUrl;
        
        if (ambiance.HasValue)
            PreferredAmbiance = ambiance.Value;
            
        if (timeSlot.HasValue)
            PreferredTimeSlot = timeSlot.Value;

        LastUpdatedDate = DateTime.UtcNow;
    }

    public void UpdateTablePreferences(
        int? tableSize = null,
        string? tableLocation = null)
    {
        if (tableSize.HasValue && tableSize.Value > 0)
            PreferredTableSize = tableSize.Value;

        PreferredTableLocation = tableLocation;
        LastUpdatedDate = DateTime.UtcNow;
    }

    public void UpdateCulinaryPreferences(
        string? culinaryStyle = null,
        string? spicePreference = null,
        decimal? budgetRange = null)
    {
        CulinaryStyle = culinaryStyle;
        SpicePreference = spicePreference;
        
        if (budgetRange.HasValue && budgetRange.Value >= 0)
            BudgetRange = budgetRange.Value;

        LastUpdatedDate = DateTime.UtcNow;
    }

    public void UpdateWinePreferences(bool isEnthusiast, string? preferredType = null)
    {
        IsWineEnthusiast = isEnthusiast;
        PreferredWineType = isEnthusiast ? preferredType : null;
        LastUpdatedDate = DateTime.UtcNow;
    }

    public void UpdateAccessibilityInfo(bool requiresAccessibility, string? needs = null)
    {
        RequiresAccessibility = requiresAccessibility;
        AccessibilityNeeds = requiresAccessibility ? needs : null;
        LastUpdatedDate = DateTime.UtcNow;
    }

    public void UpdateSpecialNeeds(
        bool hasChildrenAccommodations,
        string? musicPreference = null,
        string? specialOccasions = null)
    {
        HasChildrenAccommodations = hasChildrenAccommodations;
        MusicPreference = musicPreference;
        SpecialOccasions = specialOccasions;
        LastUpdatedDate = DateTime.UtcNow;
    }

    // Méthodes métier pour les allergies
    public void AddAllergy(string allergy)
    {
        if (string.IsNullOrWhiteSpace(allergy))
            throw new ArgumentException("Allergy cannot be empty", nameof(allergy));

        var allergyName = allergy.Trim();
        if (!_allergies.Contains(allergyName, StringComparer.OrdinalIgnoreCase))
        {
            _allergies.Add(allergyName);
            LastUpdatedDate = DateTime.UtcNow;
        }
    }

    public void RemoveAllergy(string allergy)
    {
        if (string.IsNullOrWhiteSpace(allergy))
            return;

        var allergyToRemove = _allergies.FirstOrDefault(a => 
            string.Equals(a, allergy.Trim(), StringComparison.OrdinalIgnoreCase));

        if (allergyToRemove != null)
        {
            _allergies.Remove(allergyToRemove);
            LastUpdatedDate = DateTime.UtcNow;
        }
    }

    public void SetAllergies(IEnumerable<string> allergies)
    {
        _allergies.Clear();
        
        if (allergies != null)
        {
            foreach (var allergy in allergies.Where(a => !string.IsNullOrWhiteSpace(a)))
            {
                _allergies.Add(allergy.Trim());
            }
        }
        
        LastUpdatedDate = DateTime.UtcNow;
    }

    // Méthodes métier pour les restrictions alimentaires
    public void AddDietaryRestriction(DietaryRestriction restriction)
    {
        if (!_dietaryRestrictions.Contains(restriction))
        {
            _dietaryRestrictions.Add(restriction);
            LastUpdatedDate = DateTime.UtcNow;
        }
    }

    public void RemoveDietaryRestriction(DietaryRestriction restriction)
    {
        if (_dietaryRestrictions.Remove(restriction))
        {
            LastUpdatedDate = DateTime.UtcNow;
        }
    }

    public void SetDietaryRestrictions(IEnumerable<DietaryRestriction> restrictions)
    {
        _dietaryRestrictions.Clear();
        
        if (restrictions != null)
        {
            _dietaryRestrictions.AddRange(restrictions.Distinct());
        }
        
        LastUpdatedDate = DateTime.UtcNow;
    }

    // Méthodes métier pour les plats favoris
    public void AddFavoriteDish(string dish)
    {
        if (string.IsNullOrWhiteSpace(dish))
            throw new ArgumentException("Dish cannot be empty", nameof(dish));

        var dishName = dish.Trim();
        if (!_favoriteDishes.Contains(dishName, StringComparer.OrdinalIgnoreCase))
        {
            _favoriteDishes.Add(dishName);
            
            // Retirer des plats non aimés si présent
            var dislikedToRemove = _dislikedDishes.FirstOrDefault(d => 
                string.Equals(d, dishName, StringComparison.OrdinalIgnoreCase));
            if (dislikedToRemove != null)
            {
                _dislikedDishes.Remove(dislikedToRemove);
            }
            
            LastUpdatedDate = DateTime.UtcNow;
        }
    }

    public void RemoveFavoriteDish(string dish)
    {
        if (string.IsNullOrWhiteSpace(dish))
            return;

        var dishToRemove = _favoriteDishes.FirstOrDefault(d => 
            string.Equals(d, dish.Trim(), StringComparison.OrdinalIgnoreCase));

        if (dishToRemove != null)
        {
            _favoriteDishes.Remove(dishToRemove);
            LastUpdatedDate = DateTime.UtcNow;
        }
    }

    public void AddDislikedDish(string dish)
    {
        if (string.IsNullOrWhiteSpace(dish))
            throw new ArgumentException("Dish cannot be empty", nameof(dish));

        var dishName = dish.Trim();
        if (!_dislikedDishes.Contains(dishName, StringComparer.OrdinalIgnoreCase))
        {
            _dislikedDishes.Add(dishName);
            
            // Retirer des plats favoris si présent
            var favoriteToRemove = _favoriteDishes.FirstOrDefault(d => 
                string.Equals(d, dishName, StringComparison.OrdinalIgnoreCase));
            if (favoriteToRemove != null)
            {
                _favoriteDishes.Remove(favoriteToRemove);
            }
            
            LastUpdatedDate = DateTime.UtcNow;
        }
    }

    public void RemoveDislikedDish(string dish)
    {
        if (string.IsNullOrWhiteSpace(dish))
            return;

        var dishToRemove = _dislikedDishes.FirstOrDefault(d => 
            string.Equals(d, dish.Trim(), StringComparison.OrdinalIgnoreCase));

        if (dishToRemove != null)
        {
            _dislikedDishes.Remove(dishToRemove);
            LastUpdatedDate = DateTime.UtcNow;
        }
    }

    // Méthodes d'analyse du profil
    public bool HasAllergies => _allergies.Any();
    public bool HasDietaryRestrictions => _dietaryRestrictions.Any();
    public bool HasFoodPreferences => _favoriteDishes.Any() || _dislikedDishes.Any();
    public bool IsDetailedProfile => HasAllergies || HasDietaryRestrictions || HasFoodPreferences || 
                                   !string.IsNullOrWhiteSpace(Biography);

    public bool IsVegetarian => _dietaryRestrictions.Contains(DietaryRestriction.Vegetarian);
    public bool IsVegan => _dietaryRestrictions.Contains(DietaryRestriction.Vegan);
    public bool IsGlutenFree => _dietaryRestrictions.Contains(DietaryRestriction.GlutenFree);
    public bool IsLactoseFree => _dietaryRestrictions.Contains(DietaryRestriction.LactoseFree);

    public bool HasSpecialDietary => IsVegetarian || IsVegan || IsGlutenFree || IsLactoseFree;
    
    public bool PrefersFormalDining => PreferredAmbiance == AmbiancePreference.Upscale || 
                                      PreferredAmbiance == AmbiancePreference.Business;
    
    public bool IsFamilyOriented => HasChildrenAccommodations || 
                                   PreferredAmbiance == AmbiancePreference.Family;

    public int ProfileCompletionPercentage
    {
        get
        {
            var completedFields = 0;
            var totalFields = 15; // Nombre total de champs principaux

            if (!string.IsNullOrWhiteSpace(Biography)) completedFields++;
            if (!string.IsNullOrWhiteSpace(PhotoUrl)) completedFields++;
            if (PreferredAmbiance != AmbiancePreference.Casual) completedFields++;
            if (PreferredTimeSlot != PreferredTimeSlot.Flexible) completedFields++;
            if (PreferredTableSize.HasValue) completedFields++;
            if (!string.IsNullOrWhiteSpace(PreferredTableLocation)) completedFields++;
            if (BudgetRange.HasValue) completedFields++;
            if (!string.IsNullOrWhiteSpace(CulinaryStyle)) completedFields++;
            if (!string.IsNullOrWhiteSpace(SpicePreference)) completedFields++;
            if (IsWineEnthusiast) completedFields++;
            if (HasAllergies) completedFields++;
            if (HasDietaryRestrictions) completedFields++;
            if (HasFoodPreferences) completedFields++;
            if (RequiresAccessibility) completedFields++;
            if (!string.IsNullOrWhiteSpace(SpecialOccasions)) completedFields++;

            return (completedFields * 100) / totalFields;
        }
    }
}