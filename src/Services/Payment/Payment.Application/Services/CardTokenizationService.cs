using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Payment.Application.Services
{
    /// <summary>
    /// Interface repository pour les cartes
    /// </summary>
    public interface ICardRepository
    {
        Task<Card?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<Card?> GetByFingerprintAndCustomerAsync(string fingerprint, Guid customerId, CancellationToken cancellationToken = default);
        Task<List<Card>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
        Task<Card?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Card> AddAsync(Card card, CancellationToken cancellationToken = default);
        Task UpdateAsync(Card card, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid cardId, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Interface service de validation des cartes
    /// </summary>
    public interface ICardValidationService
    {
        Task<CardValidationResult> ValidateCardAsync(string cardNumber, int expiryMonth, int expiryYear, string? cvv = null);
        bool ValidateLuhn(string cardNumber);
        bool IsCardExpired(int expiryMonth, int expiryYear);
        bool IsValidCardNumber(string cardNumber);
        bool IsValidCvv(string cvv, CardBrand cardBrand);
        bool IsValidExpiryDate(int month, int year);
        CardBrand DetectCardBrand(string cardNumber);
    }

    /// <summary>
    /// Service de tokenisation des cartes avec conformité PCI
    /// </summary>
    public interface ICardTokenizationService
    {
        Task<CardTokenizationResult> TokenizeCardAsync(CardTokenizationRequest request, CancellationToken cancellationToken = default);
        Task<Card?> GetCardFromTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<bool> RevokeTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<List<Card>> GetCardsByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Implémentation du service de tokenisation des cartes
    /// </summary>
    public class CardTokenizationService : ICardTokenizationService
    {
        private readonly ICardRepository _cardRepository;
        private readonly ICardValidationService _cardValidationService;
        private readonly ILogger<CardTokenizationService> _logger;

        public CardTokenizationService(
            ICardRepository cardRepository,
            ICardValidationService cardValidationService,
            ILogger<CardTokenizationService> logger)
        {
            _cardRepository = cardRepository;
            _cardValidationService = cardValidationService;
            _logger = logger;
        }

        public async Task<CardTokenizationResult> TokenizeCardAsync(CardTokenizationRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting card tokenization for customer {CustomerId}", request.CustomerId);

                // 1. Validation de la carte
                var validationResult = await _cardValidationService.ValidateCardAsync(
                    request.CardNumber, 
                    request.ExpiryMonth, 
                    request.ExpiryYear, 
                    request.Cvv);

                if (!validationResult.IsValid)
                {
                    return new CardTokenizationResult
                    {
                        Success = false,
                        ErrorMessage = validationResult.ErrorMessage
                    };
                }

                // 2. Vérifier si une carte similaire existe déjà
                var fingerprint = GenerateCardFingerprint(request.CardNumber);
                var existingCard = await _cardRepository.GetByFingerprintAndCustomerAsync(fingerprint, request.CustomerId, cancellationToken);

                if (existingCard != null)
                {
                    return new CardTokenizationResult
                    {
                        Success = true,
                        Token = existingCard.Token,
                        Card = existingCard
                    };
                }

                // 3. Créer l'entité Card avec les données chiffrées
                var maskedNumber = MaskCardNumber(request.CardNumber);
                var cardEntity = new Card(
                    token: GenerateSecureToken(),
                    maskedNumber: maskedNumber,
                    last4Digits: request.CardNumber[^4..],
                    cardholderName: request.CardholderName,
                    expiryMonth: request.ExpiryMonth,
                    expiryYear: request.ExpiryYear,
                    brand: DetermineCardBrand(request.CardNumber),
                    cardType: DetermineCardBrand(request.CardNumber).ToString(),
                    customerId: request.CustomerId);

                // 4. Définir l'empreinte
                cardEntity.SetFingerprint(fingerprint);

                // 5. Chiffrer l'adresse de facturation si fournie
                if (!string.IsNullOrWhiteSpace(request.BillingAddress))
                {
                    cardEntity.SetEncryptedBillingAddress(request.BillingAddress); // Simulation
                }

                // 6. Persister en base
                var savedCard = await _cardRepository.AddAsync(cardEntity, cancellationToken);
                await _cardRepository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Card tokenization completed successfully for customer {CustomerId}", request.CustomerId);

                return new CardTokenizationResult
                {
                    Success = true,
                    Token = savedCard.Token,
                    Card = savedCard
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during card tokenization for customer {CustomerId}", request.CustomerId);
                return new CardTokenizationResult
                {
                    Success = false,
                    ErrorMessage = "Une erreur s'est produite lors de la tokenisation de la carte"
                };
            }
        }

        public async Task<Card?> GetCardFromTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _cardRepository.GetByTokenAsync(token, cancellationToken);
        }

        public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var card = await _cardRepository.GetByTokenAsync(token, cancellationToken);
            return card != null && card.IsValidForUse();
        }

        public async Task<bool> RevokeTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            try
            {
                var card = await _cardRepository.GetByTokenAsync(token, cancellationToken);
                if (card == null)
                    return false;

                card.Deactivate();
                await _cardRepository.UpdateAsync(card, cancellationToken);
                await _cardRepository.SaveChangesAsync(cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token {Token}", token);
                return false;
            }
        }

        public async Task<List<Card>> GetCardsByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return await _cardRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        }

        private static CardBrand DetermineCardBrand(string cardNumber)
        {
            var cleanNumber = cardNumber.Replace(" ", "").Replace("-", "");

            return cleanNumber switch
            {
                var num when num.StartsWith("4") => CardBrand.Visa,
                var num when num.StartsWith("5") || (num.StartsWith("2") && num.Length >= 6 && int.Parse(num[..6]) >= 222100 && int.Parse(num[..6]) <= 272099) => CardBrand.Mastercard,
                var num when num.StartsWith("34") || num.StartsWith("37") => CardBrand.AmericanExpress,
                var num when num.StartsWith("6011") || num.StartsWith("65") => CardBrand.Discover,
                var num when num.StartsWith("35") => CardBrand.JCB,
                var num when num.StartsWith("30") || num.StartsWith("36") || num.StartsWith("38") => CardBrand.DinersClub,
                _ => CardBrand.Unknown
            };
        }

        private static string MaskCardNumber(string cardNumber)
        {
            var cleanNumber = cardNumber.Replace(" ", "").Replace("-", "");
            if (cleanNumber.Length <= 8) return new string('*', cleanNumber.Length);
            
            var firstFour = cleanNumber[..4];
            var lastFour = cleanNumber[^4..];
            var middle = new string('*', cleanNumber.Length - 8);
            
            return $"{firstFour}{middle}{lastFour}";
        }

        private static string GenerateCardFingerprint(string cardNumber)
        {
            var cleanNumber = cardNumber.Replace(" ", "").Replace("-", "");
            // Simulation - en réalité utilisation d'un hash cryptographique
            return $"fp_{cleanNumber[..6]}_{cleanNumber[^4..]}";
        }

        private static string GenerateSecureToken()
        {
            // Simulation - en réalité utilisation d'un générateur cryptographique sécurisé
            return $"tok_{Guid.NewGuid():N}";
        }
    }

    public class CardValidationService : ICardValidationService
    {
        private readonly ILogger<CardValidationService> _logger;

        public CardValidationService(ILogger<CardValidationService> logger)
        {
            _logger = logger;
        }

        public Task<CardValidationResult> ValidateCardAsync(string cardNumber, int expiryMonth, int expiryYear, string? cvv = null)
        {
            var result = new CardValidationResult();
            var errors = new List<string>();

            try
            {
                // Validation du numéro de carte
                if (!IsValidCardNumber(cardNumber))
                    errors.Add("Le numéro de carte n'est pas valide");

                // Validation de la date d'expiration
                if (!IsValidExpiryDate(expiryMonth, expiryYear))
                    errors.Add("La date d'expiration n'est pas valide");

                // Validation du CVV si fourni
                if (!string.IsNullOrWhiteSpace(cvv))
                {
                    var cardBrand = DetectCardBrand(cardNumber);
                    if (!IsValidCvv(cvv, cardBrand))
                        errors.Add("Le code CVV n'est pas valide");
                }

                result.IsValid = !errors.Any();
                result.Errors = errors;
                result.ErrorMessage = errors.Any() ? string.Join("; ", errors) : null;

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during card validation");
                return Task.FromResult(new CardValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Erreur lors de la validation de la carte",
                    Errors = new List<string> { "Erreur interne" }
                });
            }
        }

        public bool ValidateLuhn(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return false;

            var cleanNumber = cardNumber.Replace(" ", "").Replace("-", "");
            
            if (!cleanNumber.All(char.IsDigit))
                return false;

            int sum = 0;
            bool isEven = false;

            for (int i = cleanNumber.Length - 1; i >= 0; i--)
            {
                int digit = cleanNumber[i] - '0';

                if (isEven)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit -= 9;
                }

                sum += digit;
                isEven = !isEven;
            }

            return sum % 10 == 0;
        }

        public bool IsCardExpired(int expiryMonth, int expiryYear)
        {
            var now = DateTime.UtcNow;
            var expiryDate = new DateTime(expiryYear, expiryMonth, DateTime.DaysInMonth(expiryYear, expiryMonth));
            
            return expiryDate < now;
        }

        public bool IsValidCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return false;

            var cleanCardNumber = cardNumber.Replace(" ", "").Replace("-", "");
            
            if (!cleanCardNumber.All(char.IsDigit))
                return false;

            if (cleanCardNumber.Length < 13 || cleanCardNumber.Length > 19)
                return false;

            return ValidateLuhn(cleanCardNumber);
        }

        public bool IsValidCvv(string cvv, CardBrand cardBrand)
        {
            if (string.IsNullOrWhiteSpace(cvv))
                return false;

            if (!cvv.All(char.IsDigit))
                return false;

            return cardBrand switch
            {
                CardBrand.AmericanExpress => cvv.Length == 4,
                _ => cvv.Length == 3
            };
        }

        public bool IsValidExpiryDate(int month, int year)
        {
            if (month < 1 || month > 12)
                return false;

            var currentYear = DateTime.UtcNow.Year;
            if (year < currentYear || year > currentYear + 20)
                return false;

            return !IsCardExpired(month, year);
        }

        public CardBrand DetectCardBrand(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return CardBrand.Unknown;

            var cleanNumber = cardNumber.Replace(" ", "").Replace("-", "");

            return cleanNumber switch
            {
                var num when num.StartsWith("4") => CardBrand.Visa,
                var num when num.StartsWith("5") || (num.StartsWith("2") && num.Length >= 6 && int.Parse(num[..6]) >= 222100 && int.Parse(num[..6]) <= 272099) => CardBrand.Mastercard,
                var num when num.StartsWith("34") || num.StartsWith("37") => CardBrand.AmericanExpress,
                var num when num.StartsWith("6011") || num.StartsWith("65") => CardBrand.Discover,
                var num when num.StartsWith("35") => CardBrand.JCB,
                var num when num.StartsWith("30") || num.StartsWith("36") || num.StartsWith("38") => CardBrand.DinersClub,
                _ => CardBrand.Unknown
            };
        }
    }

    // DTOs et interfaces
    public class CardTokenizationRequest
    {
        public Guid CustomerId { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string? Cvv { get; set; }
        public string CardholderName { get; set; } = string.Empty;
        public string? BillingAddress { get; set; }
    }

    public class CardTokenizationResult
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public Card? Card { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class CardValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}