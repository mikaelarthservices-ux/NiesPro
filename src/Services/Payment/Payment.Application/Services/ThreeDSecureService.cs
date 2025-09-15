using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Microsoft.Extensions.Logging;
using Payment.Application.Services;

namespace Payment.Application.Services;

/// <summary>
/// Service 3D Secure pour l'authentification forte des paiements
/// </summary>
public interface IThreeDSecureService
{
    Task<ThreeDSecureInitiationResult> InitiateAuthenticationAsync(ThreeDSecureRequest request, CancellationToken cancellationToken = default);
    Task<ThreeDSecureCompletionResult> CompleteAuthenticationAsync(string transactionId, string paRes, CancellationToken cancellationToken = default);
    Task<ThreeDSecureStatus> GetAuthenticationStatusAsync(string transactionId, CancellationToken cancellationToken = default);
    Task<bool> IsCardEligibleFor3DSecureAsync(string cardToken, CancellationToken cancellationToken = default);
    Task<ThreeDSecureValidationResult> ValidatePaResAsync(string paRes, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implémentation du service 3D Secure avec support multi-provider
/// </summary>
public class ThreeDSecureService : IThreeDSecureService
{
    private readonly IThreeDSecureProviderFactory _providerFactory;
    private readonly IThreeDSecureRepository _threeDSecureRepository;
    private readonly ICardTokenizationService _cardTokenizationService;
    private readonly ILogger<ThreeDSecureService> _logger;

    public ThreeDSecureService(
        IThreeDSecureProviderFactory providerFactory,
        IThreeDSecureRepository threeDSecureRepository,
        ICardTokenizationService cardTokenizationService,
        ILogger<ThreeDSecureService> logger)
    {
        _providerFactory = providerFactory;
        _threeDSecureRepository = threeDSecureRepository;
        _cardTokenizationService = cardTokenizationService;
        _logger = logger;
    }

    public async Task<ThreeDSecureInitiationResult> InitiateAuthenticationAsync(ThreeDSecureRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Initiating 3D Secure authentication for transaction {TransactionId}", request.TransactionId);

            // 1. Validation des prérequis
            var validationResult = await ValidateRequestAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return new ThreeDSecureInitiationResult
                {
                    Success = false,
                    ErrorMessage = validationResult.ErrorMessage,
                    ValidationErrors = validationResult.Errors
                };
            }

            // 2. Vérifier l'éligibilité de la carte
            var isEligible = await IsCardEligibleFor3DSecureAsync(request.CardToken, cancellationToken);
            if (!isEligible)
            {
                _logger.LogInformation("Card not eligible for 3D Secure for transaction {TransactionId}", request.TransactionId);
                return new ThreeDSecureInitiationResult
                {
                    Success = true,
                    AuthenticationRequired = false,
                    Status = ThreeDSecureStatus.NotRequired
                };
            }

            // 3. Obtenir les informations de la carte
            var card = await _cardTokenizationService.GetCardFromTokenAsync(request.CardToken, cancellationToken);
            if (card == null)
            {
                return new ThreeDSecureInitiationResult
                {
                    Success = false,
                    ErrorMessage = "Carte non trouvée ou token invalide"
                };
            }

            // 4. Déterminer le provider 3D Secure approprié
            var provider = _providerFactory.GetProvider(card.Brand);

            // 5. Préparer la demande d'authentification
            var providerRequest = new ProviderThreeDSecureRequest
            {
                TransactionId = request.TransactionId,
                CardToken = request.CardToken,
                Amount = request.Amount,
                Currency = request.Currency,
                MerchantId = request.MerchantId,
                ReturnUrl = request.ReturnUrl,
                CardholderName = card.CardholderName,
                BillingAddress = request.BillingAddress,
                DeviceData = request.DeviceData,
                BrowserInfo = request.BrowserInfo
            };

            // 6. Initier l'authentification via le provider
            var providerResult = await provider.InitiateAuthenticationAsync(providerRequest, cancellationToken);

            // 7. Sauvegarder l'état de l'authentification
            var threeDSecureAuth = new ThreeDSecureAuthentication(
                transactionId: request.TransactionId,
                cardId: card.Id,
                provider: provider.ProviderName,
                authenticationUrl: providerResult.AcsUrl,
                authenticationToken: providerResult.PaReq
            );

            if (providerResult.Cavv != null)
                threeDSecureAuth.SetCavv(providerResult.Cavv);

            if (providerResult.Eci != null)
                threeDSecureAuth.SetEci(providerResult.Eci);

            if (providerResult.Xid != null)
                threeDSecureAuth.SetXid(providerResult.Xid);

            await _threeDSecureRepository.AddAsync(threeDSecureAuth, cancellationToken);

            _logger.LogInformation("3D Secure authentication initiated for transaction {TransactionId}, status: {Status}", 
                request.TransactionId, threeDSecureAuth.Status);

            return new ThreeDSecureInitiationResult
            {
                Success = true,
                AuthenticationRequired = providerResult.AuthenticationRequired,
                Status = MapProviderStatus(providerResult.Status),
                AcsUrl = providerResult.AcsUrl,
                PaReq = providerResult.PaReq,
                Md = providerResult.Md,
                AuthenticationId = threeDSecureAuth.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating 3D Secure authentication for transaction {TransactionId}", request.TransactionId);
            return new ThreeDSecureInitiationResult
            {
                Success = false,
                ErrorMessage = "Erreur lors de l'initiation de l'authentification 3D Secure"
            };
        }
    }

    public async Task<ThreeDSecureCompletionResult> CompleteAuthenticationAsync(string transactionId, string paRes, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Completing 3D Secure authentication for transaction {TransactionId}", transactionId);

            // 1. Récupérer l'authentification en cours
            var authentication = await _threeDSecureRepository.GetByTransactionIdAsync(transactionId, cancellationToken);
            if (authentication == null)
            {
                return new ThreeDSecureCompletionResult
                {
                    Success = false,
                    ErrorMessage = "Authentification 3D Secure non trouvée"
                };
            }

            if (authentication.Status != ThreeDSecureStatus.Pending)
            {
                return new ThreeDSecureCompletionResult
                {
                    Success = false,
                    ErrorMessage = $"L'authentification n'est pas dans un état valide pour être complétée (statut: {authentication.Status})"
                };
            }

            // 2. Valider le PaRes
            var paResValidation = await ValidatePaResAsync(paRes, cancellationToken);
            if (!paResValidation.IsValid)
            {
                authentication.MarkAsFailed("PaRes invalide");
                await _threeDSecureRepository.UpdateAsync(authentication, cancellationToken);

                return new ThreeDSecureCompletionResult
                {
                    Success = false,
                    ErrorMessage = paResValidation.ErrorMessage
                };
            }

            // 3. Obtenir le provider approprié
            var provider = _providerFactory.GetProvider(authentication.Provider);

            // 4. Compléter l'authentification via le provider
            var providerResult = await provider.CompleteAuthenticationAsync(transactionId, paRes, cancellationToken);

            // 5. Mettre à jour l'authentification
            if (providerResult.Success)
            {
                authentication.MarkAsSuccessful(
                    authenticationResult: "SUCCESS",
                    cavv: providerResult.Cavv,
                    eci: providerResult.Eci,
                    xid: providerResult.Xid,
                    directoryServerTransactionId: providerResult.DirectoryServerTransactionId
                );
            }
            else
            {
                authentication.MarkAsFailed(providerResult.ErrorMessage ?? "Échec de l'authentification");
            }

            await _threeDSecureRepository.UpdateAsync(authentication, cancellationToken);

            _logger.LogInformation("3D Secure authentication completed for transaction {TransactionId}, success: {Success}", 
                transactionId, providerResult.Success);

            return new ThreeDSecureCompletionResult
            {
                Success = providerResult.Success,
                Status = authentication.Status,
                Cavv = authentication.Cavv,
                Eci = authentication.Eci,
                Xid = authentication.Xid,
                DirectoryServerTransactionId = authentication.DirectoryServerTransactionId,
                ErrorMessage = providerResult.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing 3D Secure authentication for transaction {TransactionId}", transactionId);
            return new ThreeDSecureCompletionResult
            {
                Success = false,
                ErrorMessage = "Erreur lors de la finalisation de l'authentification 3D Secure"
            };
        }
    }

    public async Task<ThreeDSecureStatus> GetAuthenticationStatusAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var authentication = await _threeDSecureRepository.GetByTransactionIdAsync(transactionId, cancellationToken);
            return authentication?.Status ?? ThreeDSecureStatus.NotFound;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting 3D Secure status for transaction {TransactionId}", transactionId);
            return ThreeDSecureStatus.Error;
        }
    }

    public async Task<bool> IsCardEligibleFor3DSecureAsync(string cardToken, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cardToken))
                return false;

            var card = await _cardTokenizationService.GetCardFromTokenAsync(cardToken, cancellationToken);
            if (card == null)
                return false;

            // Vérifier si la marque de carte supporte 3D Secure
            var supports3DS = card.Brand switch
            {
                CardBrand.Visa => true,
                CardBrand.Mastercard => true,
                CardBrand.AmericanExpress => true,
                CardBrand.JCB => true,
                CardBrand.Discover => true,
                CardBrand.DinersClub => false, // Généralement non supporté
                _ => false
            };

            if (!supports3DS)
            {
                _logger.LogInformation("Card brand {CardBrand} does not support 3D Secure", card.Brand);
                return false;
            }

            // Vérifier si un provider est disponible pour cette carte
            try
            {
                var provider = _providerFactory.GetProvider(card.Brand);
                return provider != null;
            }
            catch
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking 3D Secure eligibility for card token {CardToken}", cardToken);
            return false;
        }
    }

    public async Task<ThreeDSecureValidationResult> ValidatePaResAsync(string paRes, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(paRes))
            {
                return new ThreeDSecureValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "PaRes est requis"
                };
            }

            // Validation basique du format Base64
            try
            {
                Convert.FromBase64String(paRes);
            }
            catch
            {
                return new ThreeDSecureValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "PaRes n'est pas un Base64 valide"
                };
            }

            // Validation de la taille (généralement entre 100 et 8000 caractères)
            if (paRes.Length < 100 || paRes.Length > 8000)
            {
                return new ThreeDSecureValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "PaRes a une taille invalide"
                };
            }

            return new ThreeDSecureValidationResult
            {
                IsValid = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating PaRes");
            return new ThreeDSecureValidationResult
            {
                IsValid = false,
                ErrorMessage = "Erreur lors de la validation du PaRes"
            };
        }
    }

    private async Task<ValidationResult> ValidateRequestAsync(ThreeDSecureRequest request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.TransactionId))
            errors.Add("Transaction ID est requis");

        if (string.IsNullOrWhiteSpace(request.CardToken))
            errors.Add("Token de carte est requis");

        if (request.Amount <= 0)
            errors.Add("Le montant doit être supérieur à 0");

        if (string.IsNullOrWhiteSpace(request.Currency))
            errors.Add("La devise est requise");

        if (string.IsNullOrWhiteSpace(request.ReturnUrl))
            errors.Add("URL de retour est requise");
        else if (!Uri.TryCreate(request.ReturnUrl, UriKind.Absolute, out _))
            errors.Add("URL de retour invalide");

        // Validation du token de carte
        var isValidToken = await _cardTokenizationService.ValidateTokenAsync(request.CardToken, cancellationToken);
        if (!isValidToken)
            errors.Add("Token de carte invalide");

        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors,
            ErrorMessage = errors.Any() ? string.Join("; ", errors) : null
        };
    }

    private static ThreeDSecureStatus MapProviderStatus(ProviderThreeDSecureStatus providerStatus)
    {
        return providerStatus switch
        {
            ProviderThreeDSecureStatus.NotRequired => ThreeDSecureStatus.NotRequired,
            ProviderThreeDSecureStatus.Pending => ThreeDSecureStatus.Pending,
            ProviderThreeDSecureStatus.Successful => ThreeDSecureStatus.Successful,
            ProviderThreeDSecureStatus.Failed => ThreeDSecureStatus.Failed,
            ProviderThreeDSecureStatus.Abandoned => ThreeDSecureStatus.Abandoned,
            _ => ThreeDSecureStatus.Error
        };
    }
}

// Factory pour les providers 3D Secure
public interface IThreeDSecureProviderFactory
{
    IThreeDSecureProvider GetProvider(CardBrand cardBrand);
    IThreeDSecureProvider GetProvider(string providerName);
}

public class ThreeDSecureProviderFactory : IThreeDSecureProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<CardBrand, string> _providerMappings;
    private readonly Dictionary<string, Type> _providerTypes;

    public ThreeDSecureProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        _providerMappings = new Dictionary<CardBrand, string>
        {
            { CardBrand.Visa, "VisaSecure" },
            { CardBrand.Mastercard, "MastercardIdentityCheck" },
            { CardBrand.AmericanExpress, "AmexSafeKey" },
            { CardBrand.JCB, "JSecure" },
            { CardBrand.Discover, "DiscoverProtectBuy" }
        };

        _providerTypes = new Dictionary<string, Type>
        {
            { "VisaSecure", typeof(VisaSecureProvider) },
            { "MastercardIdentityCheck", typeof(MastercardIdentityCheckProvider) },
            { "AmexSafeKey", typeof(AmexSafeKeyProvider) },
            { "JSecure", typeof(JSecureProvider) },
            { "DiscoverProtectBuy", typeof(DiscoverProtectBuyProvider) }
        };
    }

    public IThreeDSecureProvider GetProvider(CardBrand cardBrand)
    {
        if (!_providerMappings.TryGetValue(cardBrand, out var providerName))
            throw new NotSupportedException($"3D Secure not supported for card brand {cardBrand}");

        return GetProvider(providerName);
    }

    public IThreeDSecureProvider GetProvider(string providerName)
    {
        if (!_providerTypes.TryGetValue(providerName, out var providerType))
            throw new NotSupportedException($"3D Secure provider {providerName} not supported");

        var provider = _serviceProvider.GetService(providerType) as IThreeDSecureProvider;
        if (provider == null)
            throw new InvalidOperationException($"Failed to resolve 3D Secure provider {providerName}");

        return provider;
    }
}

// Interface commune pour les providers 3D Secure
public interface IThreeDSecureProvider
{
    string ProviderName { get; }
    CardBrand[] SupportedCardBrands { get; }

    Task<ProviderThreeDSecureInitiationResult> InitiateAuthenticationAsync(ProviderThreeDSecureRequest request, CancellationToken cancellationToken = default);
    Task<ProviderThreeDSecureCompletionResult> CompleteAuthenticationAsync(string transactionId, string paRes, CancellationToken cancellationToken = default);
}

// Implémentation exemple pour Visa Secure
public class VisaSecureProvider : IThreeDSecureProvider
{
    public string ProviderName => "VisaSecure";
    public CardBrand[] SupportedCardBrands => new[] { CardBrand.Visa };

    private readonly ILogger<VisaSecureProvider> _logger;

    public VisaSecureProvider(ILogger<VisaSecureProvider> logger)
    {
        _logger = logger;
    }

    public Task<ProviderThreeDSecureInitiationResult> InitiateAuthenticationAsync(ProviderThreeDSecureRequest request, CancellationToken cancellationToken = default)
    {
        // Implémentation simulée - dans la réalité, ici on communiquerait avec l'API Visa
        _logger.LogInformation("Initiating Visa Secure authentication for transaction {TransactionId}", request.TransactionId);

        var result = new ProviderThreeDSecureInitiationResult
        {
            Success = true,
            AuthenticationRequired = true,
            Status = ProviderThreeDSecureStatus.Pending,
            AcsUrl = "https://centinelapi.cardinalcommerce.com/V1/Cruise/Collect",
            PaReq = GenerateMockPaReq(),
            Md = Guid.NewGuid().ToString()
        };

        return Task.FromResult(result);
    }

    public Task<ProviderThreeDSecureCompletionResult> CompleteAuthenticationAsync(string transactionId, string paRes, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Completing Visa Secure authentication for transaction {TransactionId}", transactionId);

        // Simulation d'une validation réussie
        var result = new ProviderThreeDSecureCompletionResult
        {
            Success = true,
            Status = ProviderThreeDSecureStatus.Successful,
            Cavv = GenerateMockCavv(),
            Eci = "05", // Fully authenticated
            Xid = Guid.NewGuid().ToString("N"),
            DirectoryServerTransactionId = Guid.NewGuid().ToString()
        };

        return Task.FromResult(result);
    }

    private static string GenerateMockPaReq()
    {
        var mockPaReq = $"<PAReq><Message id=\"{Guid.NewGuid()}\">{DateTime.UtcNow:yyyyMMddHHmmss}</Message></PAReq>";
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(mockPaReq));
    }

    private static string GenerateMockCavv()
    {
        var random = new Random();
        var cavvBytes = new byte[20];
        random.NextBytes(cavvBytes);
        return Convert.ToBase64String(cavvBytes);
    }
}

// Stubs pour les autres providers
public class MastercardIdentityCheckProvider : IThreeDSecureProvider
{
    public string ProviderName => "MastercardIdentityCheck";
    public CardBrand[] SupportedCardBrands => new[] { CardBrand.Mastercard };

    public Task<ProviderThreeDSecureInitiationResult> InitiateAuthenticationAsync(ProviderThreeDSecureRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Mastercard Identity Check not yet implemented");
    }

    public Task<ProviderThreeDSecureCompletionResult> CompleteAuthenticationAsync(string transactionId, string paRes, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

public class AmexSafeKeyProvider : IThreeDSecureProvider
{
    public string ProviderName => "AmexSafeKey";
    public CardBrand[] SupportedCardBrands => new[] { CardBrand.AmericanExpress };

    public Task<ProviderThreeDSecureInitiationResult> InitiateAuthenticationAsync(ProviderThreeDSecureRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Amex SafeKey not yet implemented");
    }

    public Task<ProviderThreeDSecureCompletionResult> CompleteAuthenticationAsync(string transactionId, string paRes, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

public class JSecureProvider : IThreeDSecureProvider
{
    public string ProviderName => "JSecure";
    public CardBrand[] SupportedCardBrands => new[] { CardBrand.JCB };

    public Task<ProviderThreeDSecureInitiationResult> InitiateAuthenticationAsync(ProviderThreeDSecureRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("J/Secure not yet implemented");
    }

    public Task<ProviderThreeDSecureCompletionResult> CompleteAuthenticationAsync(string transactionId, string paRes, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

public class DiscoverProtectBuyProvider : IThreeDSecureProvider
{
    public string ProviderName => "DiscoverProtectBuy";
    public CardBrand[] SupportedCardBrands => new[] { CardBrand.Discover };

    public Task<ProviderThreeDSecureInitiationResult> InitiateAuthenticationAsync(ProviderThreeDSecureRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Discover ProtectBuy not yet implemented");
    }

    public Task<ProviderThreeDSecureCompletionResult> CompleteAuthenticationAsync(string transactionId, string paRes, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

// DTOs pour les requêtes et résultats
public class ThreeDSecureRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public string CardToken { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string MerchantId { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string? BillingAddress { get; set; }
    public string? DeviceData { get; set; }
    public BrowserInfo? BrowserInfo { get; set; }
}

public class BrowserInfo
{
    public string UserAgent { get; set; } = string.Empty;
    public string AcceptHeader { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public int ScreenHeight { get; set; }
    public int ScreenWidth { get; set; }
    public int ColorDepth { get; set; }
    public int TimeZoneOffset { get; set; }
    public bool JavaEnabled { get; set; }
    public bool JavascriptEnabled { get; set; }
}

public class ThreeDSecureInitiationResult
{
    public bool Success { get; set; }
    public bool AuthenticationRequired { get; set; }
    public ThreeDSecureStatus Status { get; set; }
    public string? AcsUrl { get; set; }
    public string? PaReq { get; set; }
    public string? Md { get; set; }
    public Guid? AuthenticationId { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
}

public class ThreeDSecureCompletionResult
{
    public bool Success { get; set; }
    public ThreeDSecureStatus Status { get; set; }
    public string? Cavv { get; set; }
    public string? Eci { get; set; }
    public string? Xid { get; set; }
    public string? DirectoryServerTransactionId { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ThreeDSecureValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Errors { get; set; } = new();
}

// DTOs pour les providers
public class ProviderThreeDSecureRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public string CardToken { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string MerchantId { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string? CardholderName { get; set; }
    public string? BillingAddress { get; set; }
    public string? DeviceData { get; set; }
    public BrowserInfo? BrowserInfo { get; set; }
}

public class ProviderThreeDSecureInitiationResult
{
    public bool Success { get; set; }
    public bool AuthenticationRequired { get; set; }
    public ProviderThreeDSecureStatus Status { get; set; }
    public string? AcsUrl { get; set; }
    public string? PaReq { get; set; }
    public string? Md { get; set; }
    public string? Cavv { get; set; }
    public string? Eci { get; set; }
    public string? Xid { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ProviderThreeDSecureCompletionResult
{
    public bool Success { get; set; }
    public ProviderThreeDSecureStatus Status { get; set; }
    public string? Cavv { get; set; }
    public string? Eci { get; set; }
    public string? Xid { get; set; }
    public string? DirectoryServerTransactionId { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum ProviderThreeDSecureStatus
{
    NotRequired,
    Pending,
    Successful,
    Failed,
    Abandoned
}

// Interface pour le repository 3D Secure
public interface IThreeDSecureRepository
{
    Task<ThreeDSecureAuthentication?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default);
    Task AddAsync(ThreeDSecureAuthentication authentication, CancellationToken cancellationToken = default);
    Task UpdateAsync(ThreeDSecureAuthentication authentication, CancellationToken cancellationToken = default);
}