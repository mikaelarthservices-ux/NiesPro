namespace Customer.Domain.Enums;

/// <summary>
/// Type de segment client
/// </summary>
public enum SegmentType
{
    /// <summary>
    /// Segmentation démographique
    /// </summary>
    Demographic = 0,
    
    /// <summary>
    /// Segmentation comportementale
    /// </summary>
    Behavioral = 1,
    
    /// <summary>
    /// Segmentation géographique
    /// </summary>
    Geographic = 2,
    
    /// <summary>
    /// Segmentation psychographique
    /// </summary>
    Psychographic = 3,
    
    /// <summary>
    /// Segmentation par valeur client
    /// </summary>
    ValueBased = 4,
    
    /// <summary>
    /// Segmentation par cycle de vie
    /// </summary>
    Lifecycle = 5
}

/// <summary>
/// Critères de segmentation
/// </summary>
public enum SegmentCriteria
{
    /// <summary>
    /// Age du client
    /// </summary>
    Age = 0,
    
    /// <summary>
    /// Fréquence de visite
    /// </summary>
    VisitFrequency = 1,
    
    /// <summary>
    /// Montant moyen dépensé
    /// </summary>
    AverageSpending = 2,
    
    /// <summary>
    /// Montant total dépensé
    /// </summary>
    TotalSpending = 3,
    
    /// <summary>
    /// Localisation géographique
    /// </summary>
    Location = 4,
    
    /// <summary>
    /// Préférences culinaires
    /// </summary>
    CulinaryPreferences = 5,
    
    /// <summary>
    /// Ancienneté client
    /// </summary>
    CustomerAge = 6,
    
    /// <summary>
    /// Satisfaction client
    /// </summary>
    Satisfaction = 7
}

/// <summary>
/// Type d'interaction avec le client
/// </summary>
public enum InteractionType
{
    /// <summary>
    /// Réservation de table
    /// </summary>
    Reservation = 0,
    
    /// <summary>
    /// Commande en ligne
    /// </summary>
    OnlineOrder = 1,
    
    /// <summary>
    /// Visite en restaurant
    /// </summary>
    InPersonVisit = 2,
    
    /// <summary>
    /// Appel téléphonique
    /// </summary>
    PhoneCall = 3,
    
    /// <summary>
    /// Email envoyé
    /// </summary>
    EmailSent = 4,
    
    /// <summary>
    /// SMS envoyé
    /// </summary>
    SMSSent = 5,
    
    /// <summary>
    /// Feedback reçu
    /// </summary>
    FeedbackReceived = 6,
    
    /// <summary>
    /// Réclamation
    /// </summary>
    Complaint = 7,
    
    /// <summary>
    /// Compliment
    /// </summary>
    Compliment = 8,
    
    /// <summary>
    /// Support client
    /// </summary>
    Support = 9
}

/// <summary>
/// Canal d'interaction
/// </summary>
public enum InteractionChannel
{
    /// <summary>
    /// Site web
    /// </summary>
    Website = 0,
    
    /// <summary>
    /// Application mobile
    /// </summary>
    MobileApp = 1,
    
    /// <summary>
    /// Téléphone
    /// </summary>
    Phone = 2,
    
    /// <summary>
    /// Email
    /// </summary>
    Email = 3,
    
    /// <summary>
    /// SMS
    /// </summary>
    SMS = 4,
    
    /// <summary>
    /// En personne
    /// </summary>
    InPerson = 5,
    
    /// <summary>
    /// Réseaux sociaux
    /// </summary>
    SocialMedia = 6,
    
    /// <summary>
    /// Chat en ligne
    /// </summary>
    LiveChat = 7,
    
    /// <summary>
    /// Courrier postal
    /// </summary>
    Mail = 8
}

/// <summary>
/// Résultat d'une interaction
/// </summary>
public enum InteractionOutcome
{
    /// <summary>
    /// Succès - objectif atteint
    /// </summary>
    Success = 0,
    
    /// <summary>
    /// Échec - objectif non atteint
    /// </summary>
    Failure = 1,
    
    /// <summary>
    /// En cours - interaction non terminée
    /// </summary>
    Pending = 2,
    
    /// <summary>
    /// Annulé par le client
    /// </summary>
    Cancelled = 3,
    
    /// <summary>
    /// Reporté à plus tard
    /// </summary>
    Postponed = 4,
    
    /// <summary>
    /// Transféré à un autre service
    /// </summary>
    Transferred = 5
}