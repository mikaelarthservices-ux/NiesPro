namespace Customer.Domain.Enums;

/// <summary>
/// Type de préférence client
/// </summary>
public enum PreferenceType
{
    /// <summary>
    /// Préférence culinaire
    /// </summary>
    Culinary = 0,
    
    /// <summary>
    /// Préférence d'ambiance
    /// </summary>
    Ambiance = 1,
    
    /// <summary>
    /// Préférence de service
    /// </summary>
    Service = 2,
    
    /// <summary>
    /// Préférence de table
    /// </summary>
    Table = 3,
    
    /// <summary>
    /// Préférence horaire
    /// </summary>
    Timing = 4,
    
    /// <summary>
    /// Préférence de communication
    /// </summary>
    Communication = 5,
    
    /// <summary>
    /// Préférence de paiement
    /// </summary>
    Payment = 6
}

/// <summary>
/// Niveau d'allergie ou restriction alimentaire
/// </summary>
public enum AllergyLevel
{
    /// <summary>
    /// Allergie légère - inconfort
    /// </summary>
    Mild = 0,
    
    /// <summary>
    /// Allergie modérée - symptômes notables
    /// </summary>
    Moderate = 1,
    
    /// <summary>
    /// Allergie sévère - réaction importante
    /// </summary>
    Severe = 2,
    
    /// <summary>
    /// Allergie critique - risque vital
    /// </summary>
    Critical = 3
}

/// <summary>
/// Type de restriction alimentaire
/// </summary>
public enum DietaryRestriction
{
    /// <summary>
    /// Aucune restriction
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Végétarien
    /// </summary>
    Vegetarian = 1,
    
    /// <summary>
    /// Végan
    /// </summary>
    Vegan = 2,
    
    /// <summary>
    /// Sans gluten
    /// </summary>
    GlutenFree = 3,
    
    /// <summary>
    /// Sans lactose
    /// </summary>
    LactoseFree = 4,
    
    /// <summary>
    /// Halal
    /// </summary>
    Halal = 5,
    
    /// <summary>
    /// Casher
    /// </summary>
    Kosher = 6,
    
    /// <summary>
    /// Régime cétogène
    /// </summary>
    Keto = 7,
    
    /// <summary>
    /// Régime paléo
    /// </summary>
    Paleo = 8,
    
    /// <summary>
    /// Diabétique
    /// </summary>
    Diabetic = 9
}

/// <summary>
/// Moment préféré de la journée
/// </summary>
public enum PreferredTimeSlot
{
    /// <summary>
    /// Petit-déjeuner (6h-11h)
    /// </summary>
    Breakfast = 0,
    
    /// <summary>
    /// Déjeuner (11h-15h)
    /// </summary>
    Lunch = 1,
    
    /// <summary>
    /// Goûter (15h-18h)
    /// </summary>
    Afternoon = 2,
    
    /// <summary>
    /// Dîner (18h-22h)
    /// </summary>
    Dinner = 3,
    
    /// <summary>
    /// Tard le soir (22h+)
    /// </summary>
    LateNight = 4,
    
    /// <summary>
    /// Flexible
    /// </summary>
    Flexible = 5
}

/// <summary>
/// Type d'ambiance préférée
/// </summary>
public enum AmbiancePreference
{
    /// <summary>
    /// Calme et tranquille
    /// </summary>
    Quiet = 0,
    
    /// <summary>
    /// Animé et vivant
    /// </summary>
    Lively = 1,
    
    /// <summary>
    /// Romantique
    /// </summary>
    Romantic = 2,
    
    /// <summary>
    /// Familial
    /// </summary>
    Family = 3,
    
    /// <summary>
    /// Professionnel
    /// </summary>
    Business = 4,
    
    /// <summary>
    /// Décontracté
    /// </summary>
    Casual = 5,
    
    /// <summary>
    /// Haut de gamme
    /// </summary>
    Upscale = 6
}