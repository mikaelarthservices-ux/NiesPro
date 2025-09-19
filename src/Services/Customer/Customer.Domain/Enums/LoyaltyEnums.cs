namespace Customer.Domain.Enums;

/// <summary>
/// Niveau de fidélité d'un client
/// </summary>
public enum LoyaltyTier
{
    /// <summary>
    /// Niveau bronze - nouveau client
    /// </summary>
    Bronze = 0,
    
    /// <summary>
    /// Niveau argent - client régulier
    /// </summary>
    Silver = 1,
    
    /// <summary>
    /// Niveau or - client fidèle
    /// </summary>
    Gold = 2,
    
    /// <summary>
    /// Niveau platine - client premium
    /// </summary>
    Platinum = 3,
    
    /// <summary>
    /// Niveau diamant - client VIP
    /// </summary>
    Diamond = 4
}

/// <summary>
/// Type de récompense dans le programme de fidélité
/// </summary>
public enum RewardType
{
    /// <summary>
    /// Points de fidélité
    /// </summary>
    Points = 0,
    
    /// <summary>
    /// Réduction en pourcentage
    /// </summary>
    PercentageDiscount = 1,
    
    /// <summary>
    /// Réduction en montant fixe
    /// </summary>
    FixedAmountDiscount = 2,
    
    /// <summary>
    /// Produit gratuit
    /// </summary>
    FreeProduct = 3,
    
    /// <summary>
    /// Service gratuit
    /// </summary>
    FreeService = 4,
    
    /// <summary>
    /// Accès VIP
    /// </summary>
    VIPAccess = 5,
    
    /// <summary>
    /// Cadeau personnalisé
    /// </summary>
    CustomGift = 6
}

/// <summary>
/// Statut d'une récompense
/// </summary>
public enum RewardStatus
{
    /// <summary>
    /// Récompense disponible
    /// </summary>
    Available = 0,
    
    /// <summary>
    /// Récompense utilisée
    /// </summary>
    Redeemed = 1,
    
    /// <summary>
    /// Récompense expirée
    /// </summary>
    Expired = 2,
    
    /// <summary>
    /// Récompense annulée
    /// </summary>
    Cancelled = 3,
    
    /// <summary>
    /// Récompense en attente
    /// </summary>
    Pending = 4
}

/// <summary>
/// Type de transaction de points
/// </summary>
public enum PointTransactionType
{
    /// <summary>
    /// Points gagnés par achat
    /// </summary>
    Earned = 0,
    
    /// <summary>
    /// Points utilisés pour récompense
    /// </summary>
    Redeemed = 1,
    
    /// <summary>
    /// Points expirés automatiquement
    /// </summary>
    Expired = 2,
    
    /// <summary>
    /// Points ajustés manuellement
    /// </summary>
    Adjustment = 3,
    
    /// <summary>
    /// Points bonus promotionnel
    /// </summary>
    Bonus = 4,
    
    /// <summary>
    /// Points de parrainage
    /// </summary>
    Referral = 5
}