namespace Customer.Domain.Enums;

/// <summary>
/// Statut d'un client dans le système CRM
/// </summary>
public enum CustomerStatus
{
    /// <summary>
    /// Client prospect - pas encore de commande
    /// </summary>
    Prospect = 0,
    
    /// <summary>
    /// Client actif avec commandes récentes
    /// </summary>
    Active = 1,
    
    /// <summary>
    /// Client inactif depuis longtemps
    /// </summary>
    Inactive = 2,
    
    /// <summary>
    /// Client VIP avec privilèges spéciaux
    /// </summary>
    VIP = 3,
    
    /// <summary>
    /// Client bloqué temporairement
    /// </summary>
    Suspended = 4,
    
    /// <summary>
    /// Client banni définitivement
    /// </summary>
    Banned = 5,
    
    /// <summary>
    /// Compte supprimé (RGPD)
    /// </summary>
    Deleted = 6
}

/// <summary>
/// Type de client selon la segmentation métier
/// </summary>
public enum CustomerType
{
    /// <summary>
    /// Client particulier individuel
    /// </summary>
    Individual = 0,
    
    /// <summary>
    /// Client professionnel/entreprise
    /// </summary>
    Business = 1,
    
    /// <summary>
    /// Groupe ou famille
    /// </summary>
    Group = 2,
    
    /// <summary>
    /// Organisation ou association
    /// </summary>
    Organization = 3,
    
    /// <summary>
    /// Influenceur ou personnalité
    /// </summary>
    Influencer = 4
}

/// <summary>
/// Genre du client
/// </summary>
public enum Gender
{
    /// <summary>
    /// Non spécifié
    /// </summary>
    Unspecified = 0,
    
    /// <summary>
    /// Masculin
    /// </summary>
    Male = 1,
    
    /// <summary>
    /// Féminin
    /// </summary>
    Female = 2,
    
    /// <summary>
    /// Autre
    /// </summary>
    Other = 3,
    
    /// <summary>
    /// Préfère ne pas dire
    /// </summary>
    PreferNotToSay = 4
}

/// <summary>
/// Préférences de communication du client
/// </summary>
public enum CommunicationPreference
{
    /// <summary>
    /// Email uniquement
    /// </summary>
    Email = 0,
    
    /// <summary>
    /// SMS uniquement
    /// </summary>
    SMS = 1,
    
    /// <summary>
    /// Téléphone uniquement
    /// </summary>
    Phone = 2,
    
    /// <summary>
    /// Notifications push application
    /// </summary>
    Push = 3,
    
    /// <summary>
    /// Courrier postal
    /// </summary>
    Mail = 4,
    
    /// <summary>
    /// Tous les canaux
    /// </summary>
    All = 5,
    
    /// <summary>
    /// Aucune communication
    /// </summary>
    None = 6
}

/// <summary>
/// Fréquence de communication marketing
/// </summary>
public enum CommunicationFrequency
{
    /// <summary>
    /// Communication quotidienne
    /// </summary>
    Daily = 0,
    
    /// <summary>
    /// Communication hebdomadaire
    /// </summary>
    Weekly = 1,
    
    /// <summary>
    /// Communication mensuelle
    /// </summary>
    Monthly = 2,
    
    /// <summary>
    /// Communication trimestrielle
    /// </summary>
    Quarterly = 3,
    
    /// <summary>
    /// Communication occasionnelle
    /// </summary>
    Occasional = 4,
    
    /// <summary>
    /// Aucune communication marketing
    /// </summary>
    Never = 5
}