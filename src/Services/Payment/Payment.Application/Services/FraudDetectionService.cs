using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Payment.Application.DTOs;

namespace Payment.Application.Services;

/// <summary>
/// Service de détection de fraude avancé
/// </summary>
public interface IFraudDetectionService
{
    Task<int> AnalyzeTransactionAsync(Transaction transaction, string? ipAddress, string? geoLocation, CancellationToken cancellationToken = default);
    Task<FraudAnalysisResult> GetDetailedAnalysisAsync(Transaction transaction, string? ipAddress, string? geoLocation, CancellationToken cancellationToken = default);
    Task<bool> IsHighRiskCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<bool> IsBlacklistedIpAsync(string ipAddress, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implémentation du service de détection de fraude
/// </summary>
public class FraudDetectionService : IFraudDetectionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IGeoLocationService _geoLocationService;
    private readonly IBlacklistService _blacklistService;
    private readonly ILogger<FraudDetectionService> _logger;

    public FraudDetectionService(
        ITransactionRepository transactionRepository,
        IPaymentRepository paymentRepository,
        IGeoLocationService geoLocationService,
        IBlacklistService blacklistService,
        ILogger<FraudDetectionService> logger)
    {
        _transactionRepository = transactionRepository;
        _paymentRepository = paymentRepository;
        _geoLocationService = geoLocationService;
        _blacklistService = blacklistService;
        _logger = logger;
    }

    public async Task<int> AnalyzeTransactionAsync(Transaction transaction, string? ipAddress, string? geoLocation, CancellationToken cancellationToken = default)
    {
        var analysis = await GetDetailedAnalysisAsync(transaction, ipAddress, geoLocation, cancellationToken);
        return analysis.FraudScore;
    }

    public async Task<FraudAnalysisResult> GetDetailedAnalysisAsync(Transaction transaction, string? ipAddress, string? geoLocation, CancellationToken cancellationToken = default)
    {
        var result = new FraudAnalysisResult();
        var riskFactors = new List<FraudRiskFactor>();

        try
        {
            // 1. Analyse de la vélocité (fréquence des transactions)
            var velocityScore = await AnalyzeVelocityAsync(transaction, cancellationToken);
            riskFactors.Add(new FraudRiskFactor
            {
                Type = FraudRiskType.Velocity,
                Score = velocityScore,
                Description = $"Score de vélocité basé sur la fréquence des transactions"
            });

            // 2. Analyse géographique
            var geoScore = await AnalyzeGeolocationAsync(transaction, ipAddress, geoLocation, cancellationToken);
            riskFactors.Add(new FraudRiskFactor
            {
                Type = FraudRiskType.Geography,
                Score = geoScore,
                Description = "Analyse de localisation géographique"
            });

            // 3. Analyse des patterns de montant
            var amountScore = await AnalyzeAmountPatternsAsync(transaction, cancellationToken);
            riskFactors.Add(new FraudRiskFactor
            {
                Type = FraudRiskType.AmountPattern,
                Score = amountScore,
                Description = "Analyse des patterns de montant de transaction"
            });

            // 4. Analyse de l'historique client
            var customerScore = await AnalyzeCustomerHistoryAsync(transaction, cancellationToken);
            riskFactors.Add(new FraudRiskFactor
            {
                Type = FraudRiskType.CustomerHistory,
                Score = customerScore,
                Description = "Analyse de l'historique du client"
            });

            // 5. Vérification des listes noires
            var blacklistScore = await AnalyzeBlacklistsAsync(transaction, ipAddress, cancellationToken);
            riskFactors.Add(new FraudRiskFactor
            {
                Type = FraudRiskType.Blacklist,
                Score = blacklistScore,
                Description = "Vérification contre les listes noires"
            });

            // 6. Analyse du moyen de paiement
            var paymentMethodScore = await AnalyzePaymentMethodAsync(transaction, cancellationToken);
            riskFactors.Add(new FraudRiskFactor
            {
                Type = FraudRiskType.PaymentMethod,
                Score = paymentMethodScore,
                Description = "Analyse du moyen de paiement utilisé"
            });

            // Calcul du score final (moyenne pondérée)
            var finalScore = CalculateWeightedScore(riskFactors);

            result.FraudScore = Math.Min(100, Math.Max(0, finalScore));
            result.RiskLevel = GetRiskLevel(result.FraudScore);
            result.RiskFactors = riskFactors;
            result.Recommendation = GetRecommendation(result.FraudScore, riskFactors);

            _logger.LogInformation("Fraud analysis completed for transaction {TransactionId}, score: {Score}", 
                transaction.Id, result.FraudScore);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during fraud analysis for transaction {TransactionId}", transaction.Id);
            return new FraudAnalysisResult
            {
                FraudScore = 50, // Score neutre en cas d'erreur
                RiskLevel = FraudRiskLevel.Medium,
                RiskFactors = riskFactors,
                Recommendation = FraudRecommendation.ManualReview
            };
        }
    }

    public async Task<bool> IsHighRiskCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Récupérer l'historique récent du client (30 derniers jours)
            var recentPayments = await _paymentRepository.GetRecentByCustomerIdAsync(
                customerId, TimeSpan.FromDays(30), cancellationToken);

            // Calculer le taux d'échec
            var totalPayments = recentPayments.Count;
            if (totalPayments == 0) return false;

            var failedPayments = recentPayments.Count(p => p.Status == PaymentStatus.Failed);
            var failureRate = (decimal)failedPayments / totalPayments;

            // Client à haut risque si taux d'échec > 50% ou plus de 10 échecs récents
            return failureRate > 0.5m || failedPayments > 10;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking high risk customer {CustomerId}", customerId);
            return false;
        }
    }

    public async Task<bool> IsBlacklistedIpAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return false;

        return await _blacklistService.IsIpBlacklistedAsync(ipAddress, cancellationToken);
    }

    private async Task<int> AnalyzeVelocityAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        // Analyser la fréquence des transactions dans les dernières heures
        var cutoffTime = DateTime.UtcNow.AddHours(-1);
        var recentTransactions = await _transactionRepository.GetRecentByCustomerAsync(
            transaction.CustomerId, cutoffTime, cancellationToken);

        var transactionCount = recentTransactions.Count;

        return transactionCount switch
        {
            > 20 => 90, // Très suspect
            > 10 => 70, // Suspect
            > 5 => 40,  // Modéré
            > 2 => 20,  // Léger
            _ => 0      // Normal
        };
    }

    private async Task<int> AnalyzeGeolocationAsync(Transaction transaction, string? ipAddress, string? geoLocation, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return 30; // Score modéré si pas d'IP

        try
        {
            // Obtenir la localisation de l'IP
            var currentLocation = await _geoLocationService.GetLocationAsync(ipAddress, cancellationToken);
            if (currentLocation == null)
                return 20;

            // Récupérer les transactions récentes du client pour analyser les patterns géographiques
            var recentTransactions = await _transactionRepository.GetRecentByCustomerAsync(
                transaction.CustomerId, DateTime.UtcNow.AddDays(-30), cancellationToken);

            if (!recentTransactions.Any())
                return 0; // Nouveau client, pas de référence

            // Analyser les pays d'origine des transactions récentes
            var recentCountries = recentTransactions
                .Where(t => !string.IsNullOrEmpty(t.GeoLocation))
                .Select(t => ExtractCountryFromGeoLocation(t.GeoLocation!))
                .Where(c => !string.IsNullOrEmpty(c))
                .GroupBy(c => c)
                .OrderByDescending(g => g.Count())
                .ToList();

            if (!recentCountries.Any())
                return 10;

            var mostCommonCountry = recentCountries.First().Key;

            // Score élevé si le pays actuel est différent du pays habituel
            if (!string.Equals(currentLocation.Country, mostCommonCountry, StringComparison.OrdinalIgnoreCase))
            {
                // Vérifier si c'est un pays à haut risque
                if (IsHighRiskCountry(currentLocation.Country))
                    return 80;
                
                return 50; // Changement de pays
            }

            return 0; // Même pays, normal
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error analyzing geolocation for transaction {TransactionId}", transaction.Id);
            return 20; // Score modéré en cas d'erreur
        }
    }

    private async Task<int> AnalyzeAmountPatternsAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        try
        {
            // Récupérer les transactions récentes du client
            var recentTransactions = await _transactionRepository.GetRecentByCustomerAsync(
                transaction.CustomerId, DateTime.UtcNow.AddDays(-30), cancellationToken);

            if (recentTransactions.Count < 3)
                return 0; // Pas assez d'historique

            var amounts = recentTransactions.Select(t => t.Amount.Amount).ToList();
            var currentAmount = transaction.Amount.Amount;

            // Calculer les statistiques des montants
            var avgAmount = amounts.Average();
            var maxAmount = amounts.Max();
            var minAmount = amounts.Min();

            // Score élevé si le montant actuel est très différent des habitudes
            if (currentAmount > maxAmount * 3) // 3x le montant max habituel
                return 70;

            if (currentAmount > avgAmount * 5) // 5x le montant moyen
                return 50;

            // Détecter des montants ronds suspects (ex: 100, 500, 1000 exactement)
            if (IsRoundAmount(currentAmount) && currentAmount >= 100)
                return 30;

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error analyzing amount patterns for transaction {TransactionId}", transaction.Id);
            return 0;
        }
    }

    private async Task<int> AnalyzeCustomerHistoryAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        try
        {
            var isHighRisk = await IsHighRiskCustomerAsync(transaction.CustomerId, cancellationToken);
            if (isHighRisk)
                return 60;

            // Vérifier si c'est un nouveau client
            var allCustomerPayments = await _paymentRepository.GetByCustomerIdAsync(transaction.CustomerId, cancellationToken);
            if (allCustomerPayments.Count == 0) // Premier paiement
                return 30;

            if (allCustomerPayments.Count == 1) // Deuxième paiement
                return 20;

            return 0; // Client établi
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error analyzing customer history for transaction {TransactionId}", transaction.Id);
            return 0;
        }
    }

    private async Task<int> AnalyzeBlacklistsAsync(Transaction transaction, string? ipAddress, CancellationToken cancellationToken)
    {
        var score = 0;

        try
        {
            // Vérifier l'IP
            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                var isBlacklistedIp = await _blacklistService.IsIpBlacklistedAsync(ipAddress, cancellationToken);
                if (isBlacklistedIp)
                    score += 90;
            }

            // Vérifier l'email du client (si disponible)
            var isBlacklistedCustomer = await _blacklistService.IsCustomerBlacklistedAsync(transaction.CustomerId, cancellationToken);
            if (isBlacklistedCustomer)
                score += 95;

            // Vérifier le moyen de paiement
            var isBlacklistedPaymentMethod = await _blacklistService.IsPaymentMethodBlacklistedAsync(transaction.PaymentMethodId, cancellationToken);
            if (isBlacklistedPaymentMethod)
                score += 85;

            return Math.Min(100, score);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error analyzing blacklists for transaction {TransactionId}", transaction.Id);
            return 0;
        }
    }

    private async Task<int> AnalyzePaymentMethodAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        try
        {
            var paymentMethod = transaction.PaymentMethod;

            // Score basé sur le type de moyen de paiement
            var score = paymentMethod.Type switch
            {
                PaymentMethodType.Cash => 0,                    // Très sûr
                PaymentMethodType.CreditCard => 10,             // Risque faible
                PaymentMethodType.ContactlessCard => 15,        // Risque faible
                PaymentMethodType.BankTransfer => 5,            // Très sûr
                PaymentMethodType.MobilePayment => 20,          // Risque modéré
                PaymentMethodType.DigitalWallet => 25,          // Risque modéré
                PaymentMethodType.Cryptocurrency => 60,         // Risque élevé
                PaymentMethodType.GiftCard => 40,               // Risque modéré-élevé
                _ => 30
            };

            // Vérifier si le moyen de paiement est nouveau
            var paymentMethodTransactions = await _transactionRepository.GetByPaymentMethodIdAsync(paymentMethod.Id, cancellationToken);
            if (paymentMethodTransactions.Count <= 1) // Premier ou deuxième usage
                score += 20;

            // Vérifier l'âge du moyen de paiement
            var daysSinceCreation = (DateTime.UtcNow - paymentMethod.CreatedAt).Days;
            if (daysSinceCreation < 1) // Créé aujourd'hui
                score += 30;
            else if (daysSinceCreation < 7) // Créé cette semaine
                score += 15;

            return Math.Min(100, score);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error analyzing payment method for transaction {TransactionId}", transaction.Id);
            return 0;
        }
    }

    private static int CalculateWeightedScore(List<FraudRiskFactor> riskFactors)
    {
        // Poids pour chaque type de risque
        var weights = new Dictionary<FraudRiskType, decimal>
        {
            { FraudRiskType.Blacklist, 0.3m },        // 30% - Le plus important
            { FraudRiskType.Velocity, 0.25m },        // 25%
            { FraudRiskType.Geography, 0.2m },        // 20%
            { FraudRiskType.CustomerHistory, 0.15m }, // 15%
            { FraudRiskType.AmountPattern, 0.07m },   // 7%
            { FraudRiskType.PaymentMethod, 0.03m }    // 3%
        };

        decimal weightedSum = 0;
        decimal totalWeight = 0;

        foreach (var factor in riskFactors)
        {
            if (weights.TryGetValue(factor.Type, out var weight))
            {
                weightedSum += factor.Score * weight;
                totalWeight += weight;
            }
        }

        return totalWeight > 0 ? (int)(weightedSum / totalWeight) : 0;
    }

    private static FraudRiskLevel GetRiskLevel(int score)
    {
        return score switch
        {
            >= 80 => FraudRiskLevel.Critical,
            >= 60 => FraudRiskLevel.High,
            >= 40 => FraudRiskLevel.Medium,
            >= 20 => FraudRiskLevel.Low,
            _ => FraudRiskLevel.VeryLow
        };
    }

    private static FraudRecommendation GetRecommendation(int score, List<FraudRiskFactor> riskFactors)
    {
        // Bloquer automatiquement si blacklisté
        if (riskFactors.Any(rf => rf.Type == FraudRiskType.Blacklist && rf.Score >= 80))
            return FraudRecommendation.Block;

        return score switch
        {
            >= 90 => FraudRecommendation.Block,
            >= 70 => FraudRecommendation.RequireAdditionalVerification,
            >= 50 => FraudRecommendation.ManualReview,
            >= 30 => FraudRecommendation.Monitor,
            _ => FraudRecommendation.Allow
        };
    }

    private static string? ExtractCountryFromGeoLocation(string geoLocation)
    {
        // Parser basique pour extraire le pays de la géolocalisation
        // Format attendu: "City, Country" ou "Country"
        var parts = geoLocation.Split(',');
        return parts.Length > 1 ? parts[^1].Trim() : parts[0].Trim();
    }

    private static bool IsHighRiskCountry(string country)
    {
        // Liste simplifiée de pays à haut risque pour la fraude
        var highRiskCountries = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Nigeria", "Ghana", "Romania", "Bulgaria", "Ukraine", "Pakistan", "Bangladesh"
        };

        return highRiskCountries.Contains(country);
    }

    private static bool IsRoundAmount(decimal amount)
    {
        return amount % 1 == 0 && (amount % 10 == 0 || amount % 25 == 0 || amount % 50 == 0 || amount % 100 == 0);
    }
}

// DTOs et enums pour l'analyse de fraude
public class FraudAnalysisResult
{
    public int FraudScore { get; set; }
    public FraudRiskLevel RiskLevel { get; set; }
    public List<FraudRiskFactor> RiskFactors { get; set; } = new();
    public FraudRecommendation Recommendation { get; set; }
}

public class FraudRiskFactor
{
    public FraudRiskType Type { get; set; }
    public int Score { get; set; }
    public string Description { get; set; } = string.Empty;
}

public enum FraudRiskType
{
    Velocity,
    Geography,
    AmountPattern,
    CustomerHistory,
    Blacklist,
    PaymentMethod
}

public enum FraudRiskLevel
{
    VeryLow,
    Low,
    Medium,
    High,
    Critical
}

public enum FraudRecommendation
{
    Allow,
    Monitor,
    ManualReview,
    RequireAdditionalVerification,
    Block
}

// Interfaces des services externes
public interface IGeoLocationService
{
    Task<GeoLocation?> GetLocationAsync(string ipAddress, CancellationToken cancellationToken = default);
}

public interface IBlacklistService
{
    Task<bool> IsIpBlacklistedAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task<bool> IsCustomerBlacklistedAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<bool> IsPaymentMethodBlacklistedAsync(Guid paymentMethodId, CancellationToken cancellationToken = default);
}

public class GeoLocation
{
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

// Extensions pour les repositories
public interface ITransactionRepository
{
    Task<List<Transaction>> GetRecentByCustomerAsync(Guid customerId, DateTime since, CancellationToken cancellationToken = default);
    Task<List<Transaction>> GetByPaymentMethodIdAsync(Guid paymentMethodId, CancellationToken cancellationToken = default);
    // ... autres méthodes existantes
}

public interface IPaymentRepository
{
    Task<List<Domain.Entities.Payment>> GetRecentByCustomerIdAsync(Guid customerId, TimeSpan timeSpan, CancellationToken cancellationToken = default);
    Task<List<Domain.Entities.Payment>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    // ... autres méthodes existantes
}