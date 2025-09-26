using FluentValidation;
using Payment.Application.Commands;
using Payment.Domain.Enums;

namespace Payment.Application.Validators;

/// <summary>
/// Validateur pour la commande de création de paiement
/// </summary>
public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Le montant doit être positif")
            .LessThanOrEqualTo(1000000)
            .WithMessage("Le montant ne peut pas dépasser 1 000 000");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("La devise est obligatoire")
            .Length(3)
            .WithMessage("La devise doit faire 3 caractères")
            .Must(BeValidCurrency)
            .WithMessage("Devise non supportée");

        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("L'identifiant de commande est obligatoire");

        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant client est obligatoire");

        RuleFor(x => x.MerchantId)
            .NotEmpty()
            .WithMessage("L'identifiant commerçant est obligatoire");

        RuleFor(x => x.TimeoutMinutes)
            .GreaterThan(0)
            .WithMessage("Le délai d'expiration doit être positif")
            .LessThanOrEqualTo(10080) // 7 jours
            .WithMessage("Le délai d'expiration ne peut pas dépasser 7 jours");

        RuleFor(x => x.MaxAttempts)
            .GreaterThan(0)
            .WithMessage("Le nombre maximum de tentatives doit être positif")
            .LessThanOrEqualTo(10)
            .WithMessage("Le nombre maximum de tentatives ne peut pas dépasser 10");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("La description ne peut pas dépasser 500 caractères");

        When(x => x.AllowPartialPayments, () =>
        {
            RuleFor(x => x.MinimumPartialAmount)
                .NotNull()
                .WithMessage("Le montant minimum partiel est obligatoire quand les paiements partiels sont autorisés")
                .GreaterThan(0)
                .WithMessage("Le montant minimum partiel doit être positif")
                .LessThan(x => x.Amount)
                .WithMessage("Le montant minimum partiel doit être inférieur au montant total");

            RuleFor(x => x.MinimumPartialCurrency)
                .NotEmpty()
                .WithMessage("La devise du montant minimum partiel est obligatoire")
                .Equal(x => x.Currency)
                .WithMessage("La devise du montant minimum partiel doit être identique à celle du paiement");
        });

        RuleFor(x => x.SuccessUrl)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrEmpty(x.SuccessUrl))
            .WithMessage("L'URL de succès n'est pas valide");

        RuleFor(x => x.FailureUrl)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrEmpty(x.FailureUrl))
            .WithMessage("L'URL d'échec n'est pas valide");

        RuleFor(x => x.WebhookUrl)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrEmpty(x.WebhookUrl))
            .WithMessage("L'URL de webhook n'est pas valide");
    }

    private static bool BeValidCurrency(string currency)
    {
        var supportedCurrencies = new[] { "EUR", "USD", "GBP", "CHF", "CAD", "JPY", "AUD" };
        return supportedCurrencies.Contains(currency.ToUpper());
    }

    private static bool BeValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result) && 
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}

/// <summary>
/// Validateur pour la commande de traitement de paiement
/// </summary>
public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage("L'identifiant de paiement est obligatoire");

        RuleFor(x => x.PaymentMethodId)
            .NotEmpty()
            .WithMessage("L'identifiant du moyen de paiement est obligatoire");

        When(x => x.Amount.HasValue, () =>
        {
            RuleFor(x => x.Amount!.Value)
                .GreaterThan(0)
                .WithMessage("Le montant doit être positif");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("La devise est obligatoire quand un montant est spécifié")
                .Length(3)
                .WithMessage("La devise doit faire 3 caractères")
                .Must(BeValidCurrency)
                .WithMessage("Devise non supportée");
        });

        RuleFor(x => x.VerificationCode)
            .Length(3, 4)
            .When(x => !string.IsNullOrEmpty(x.VerificationCode))
            .WithMessage("Le code de vérification doit faire 3 ou 4 caractères")
            .Matches(@"^\d+$")
            .When(x => !string.IsNullOrEmpty(x.VerificationCode))
            .WithMessage("Le code de vérification ne doit contenir que des chiffres");

        RuleFor(x => x.ClientIpAddress)
            .Must(BeValidIpAddress)
            .When(x => !string.IsNullOrEmpty(x.ClientIpAddress))
            .WithMessage("L'adresse IP n'est pas valide");
    }

    private static bool BeValidIpAddress(string? ipAddress)
    {
        return System.Net.IPAddress.TryParse(ipAddress, out _);
    }

    private static bool BeValidCurrency(string currency)
    {
        var supportedCurrencies = new[] { "EUR", "USD", "GBP", "CHF", "CAD", "JPY", "AUD" };
        return supportedCurrencies.Contains(currency.ToUpper());
    }
}

/// <summary>
/// Validateur pour la commande de capture de transaction
/// </summary>
public class CaptureTransactionCommandValidator : AbstractValidator<CaptureTransactionCommand>
{
    public CaptureTransactionCommandValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty()
            .WithMessage("L'identifiant de transaction est obligatoire");

        When(x => x.Amount.HasValue, () =>
        {
            RuleFor(x => x.Amount!.Value)
                .GreaterThan(0)
                .WithMessage("Le montant de capture doit être positif");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("La devise est obligatoire quand un montant est spécifié")
                .Length(3)
                .WithMessage("La devise doit faire 3 caractères");
        });

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .WithMessage("La raison ne peut pas dépasser 500 caractères");
    }
}

/// <summary>
/// Validateur pour la commande de remboursement
/// </summary>
public class RefundTransactionCommandValidator : AbstractValidator<RefundTransactionCommand>
{
    public RefundTransactionCommandValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty()
            .WithMessage("L'identifiant de transaction est obligatoire");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Le montant de remboursement doit être positif")
            .LessThanOrEqualTo(1000000)
            .WithMessage("Le montant de remboursement ne peut pas dépasser 1 000 000");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("La devise est obligatoire")
            .Length(3)
            .WithMessage("La devise doit faire 3 caractères")
            .Must(BeValidCurrency)
            .WithMessage("Devise non supportée");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .WithMessage("La raison ne peut pas dépasser 500 caractères");
    }

    private static bool BeValidCurrency(string currency)
    {
        var supportedCurrencies = new[] { "EUR", "USD", "GBP", "CHF", "CAD", "JPY", "AUD" };
        return supportedCurrencies.Contains(currency.ToUpper());
    }
}

/// <summary>
/// Validateur pour la commande de création de moyen de paiement
/// </summary>
public class CreatePaymentMethodCommandValidator : AbstractValidator<CreatePaymentMethodCommand>
{
    public CreatePaymentMethodCommandValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Type de moyen de paiement invalide");

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("Le nom d'affichage est obligatoire")
            .MaximumLength(100)
            .WithMessage("Le nom d'affichage ne peut pas dépasser 100 caractères");

        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("L'identifiant client est obligatoire");

        // Validation spécifique pour les cartes de crédit
        When(x => x.Type == PaymentMethodType.CreditCard || x.Type == PaymentMethodType.ContactlessCard, () =>
        {
            RuleFor(x => x.CreditCard)
                .NotNull()
                .WithMessage("Les informations de carte sont obligatoires pour ce type de moyen de paiement");

            When(x => x.CreditCard != null, () =>
            {
                RuleFor(x => x.CreditCard!.Number)
                    .NotEmpty()
                    .WithMessage("Le numéro de carte est obligatoire")
                    .Must(BeValidCreditCardNumber)
                    .WithMessage("Le numéro de carte n'est pas valide");

                RuleFor(x => x.CreditCard!.HolderName)
                    .NotEmpty()
                    .WithMessage("Le nom du porteur est obligatoire")
                    .MaximumLength(100)
                    .WithMessage("Le nom du porteur ne peut pas dépasser 100 caractères");

                RuleFor(x => x.CreditCard!.ExpiryMonth)
                    .InclusiveBetween(1, 12)
                    .WithMessage("Le mois d'expiration doit être entre 1 et 12");

                RuleFor(x => x.CreditCard!.ExpiryYear)
                    .GreaterThanOrEqualTo(DateTime.Now.Year)
                    .WithMessage("L'année d'expiration ne peut pas être dans le passé")
                    .LessThanOrEqualTo(DateTime.Now.Year + 20)
                    .WithMessage("L'année d'expiration ne peut pas être si lointaine");

                RuleFor(x => x.CreditCard!.Cvv)
                    .Length(3, 4)
                    .When(x => !string.IsNullOrEmpty(x.CreditCard?.Cvv))
                    .WithMessage("Le CVV doit faire 3 ou 4 caractères")
                    .Matches(@"^\d+$")
                    .When(x => !string.IsNullOrEmpty(x.CreditCard?.Cvv))
                    .WithMessage("Le CVV ne doit contenir que des chiffres");
            });
        });

        // Validation des limites
        When(x => x.DailyLimit.HasValue, () =>
        {
            RuleFor(x => x.DailyLimit!.Value)
                .GreaterThan(0)
                .WithMessage("La limite quotidienne doit être positive");

            RuleFor(x => x.DailyLimitCurrency)
                .NotEmpty()
                .WithMessage("La devise de la limite quotidienne est obligatoire")
                .Length(3)
                .WithMessage("La devise doit faire 3 caractères");
        });

        When(x => x.TransactionLimit.HasValue, () =>
        {
            RuleFor(x => x.TransactionLimit!.Value)
                .GreaterThan(0)
                .WithMessage("La limite de transaction doit être positive");

            RuleFor(x => x.TransactionLimitCurrency)
                .NotEmpty()
                .WithMessage("La devise de la limite de transaction est obligatoire")
                .Length(3)
                .WithMessage("La devise doit faire 3 caractères");
        });

        // Validation que la limite quotidienne >= limite de transaction
        When(x => x.DailyLimit.HasValue && x.TransactionLimit.HasValue && 
                  x.DailyLimitCurrency == x.TransactionLimitCurrency, () =>
        {
            RuleFor(x => x.DailyLimit!.Value)
                .GreaterThanOrEqualTo(x => x.TransactionLimit!.Value)
                .WithMessage("La limite quotidienne doit être supérieure ou égale à la limite de transaction");
        });
    }

    private static bool BeValidCreditCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
            return false;

        // Supprimer les espaces et tirets
        cardNumber = cardNumber.Replace(" ", "").Replace("-", "");

        // Vérifier que ce ne sont que des chiffres
        if (!cardNumber.All(char.IsDigit))
            return false;

        // Vérifier la longueur (13-19 chiffres pour la plupart des cartes)
        if (cardNumber.Length < 13 || cardNumber.Length > 19)
            return false;

        // Algorithme de Luhn
        return IsValidLuhn(cardNumber);
    }

    private static bool IsValidLuhn(string cardNumber)
    {
        int sum = 0;
        bool isEven = false;

        for (int i = cardNumber.Length - 1; i >= 0; i--)
        {
            int digit = cardNumber[i] - '0';

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
}

/// <summary>
/// Validateur pour la commande de mise à jour de moyen de paiement
/// </summary>
public class UpdatePaymentMethodCommandValidator : AbstractValidator<UpdatePaymentMethodCommand>
{
    public UpdatePaymentMethodCommandValidator()
    {
        RuleFor(x => x.PaymentMethodId)
            .NotEmpty()
            .WithMessage("L'identifiant du moyen de paiement est obligatoire");

        RuleFor(x => x.DisplayName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.DisplayName))
            .WithMessage("Le nom d'affichage ne peut pas dépasser 100 caractères");

        When(x => x.DailyLimit.HasValue, () =>
        {
            RuleFor(x => x.DailyLimit!.Value)
                .GreaterThan(0)
                .WithMessage("La limite quotidienne doit être positive");

            RuleFor(x => x.DailyLimitCurrency)
                .NotEmpty()
                .WithMessage("La devise de la limite quotidienne est obligatoire")
                .Length(3)
                .WithMessage("La devise doit faire 3 caractères");
        });

        When(x => x.TransactionLimit.HasValue, () =>
        {
            RuleFor(x => x.TransactionLimit!.Value)
                .GreaterThan(0)
                .WithMessage("La limite de transaction doit être positive");

            RuleFor(x => x.TransactionLimitCurrency)
                .NotEmpty()
                .WithMessage("La devise de la limite de transaction est obligatoire")
                .Length(3)
                .WithMessage("La devise doit faire 3 caractères");
        });
    }
}

/// <summary>
/// Validateur pour la commande de validation de moyen de paiement
/// </summary>
public class ValidatePaymentMethodCommandValidator : AbstractValidator<ValidatePaymentMethodCommand>
{
    public ValidatePaymentMethodCommandValidator()
    {
        RuleFor(x => x.PaymentMethodId)
            .NotEmpty()
            .WithMessage("L'identifiant du moyen de paiement est obligatoire");

        RuleFor(x => x.ValidationAmount)
            .GreaterThan(0)
            .WithMessage("Le montant de validation doit être positif")
            .LessThanOrEqualTo(10)
            .WithMessage("Le montant de validation ne peut pas dépasser 10 unités");

        RuleFor(x => x.ValidationCurrency)
            .NotEmpty()
            .WithMessage("La devise de validation est obligatoire")
            .Length(3)
            .WithMessage("La devise doit faire 3 caractères");

        RuleFor(x => x.VerificationCode)
            .Length(3, 4)
            .When(x => !string.IsNullOrEmpty(x.VerificationCode))
            .WithMessage("Le code de vérification doit faire 3 ou 4 caractères")
            .Matches(@"^\d+$")
            .When(x => !string.IsNullOrEmpty(x.VerificationCode))
            .WithMessage("Le code de vérification ne doit contenir que des chiffres");
    }
}