using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Payment.Application.Services;

/// <summary>
/// Factory pour les processeurs de paiement
/// </summary>
public interface IPaymentProcessorFactory
{
    IPaymentProcessor GetProcessor(PaymentMethodType paymentMethodType);
    IPaymentProcessor GetProcessor(string processorName);
    Task<List<string>> GetAvailableProcessorsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface commune pour tous les processeurs de paiement
/// </summary>
public interface IPaymentProcessor
{
    string ProcessorName { get; }
    PaymentMethodType[] SupportedPaymentMethods { get; }
    bool SupportsRefunds { get; }
    bool SupportsPartialCapture { get; }
    bool Supports3DSecure { get; }

    Task<PaymentProcessorResult> CreatePaymentAsync(PaymentProcessorRequest request, CancellationToken cancellationToken = default);
    Task<PaymentProcessorResult> CapturePaymentAsync(string transactionId, Money amount, CancellationToken cancellationToken = default);
    Task<PaymentProcessorResult> RefundPaymentAsync(string transactionId, Money amount, string? reason, CancellationToken cancellationToken = default);
    Task<PaymentProcessorResult> VoidPaymentAsync(string transactionId, CancellationToken cancellationToken = default);
    Task<PaymentProcessorStatus> GetPaymentStatusAsync(string transactionId, CancellationToken cancellationToken = default);
    Task<PaymentProcessorResult> Process3DSecureAsync(string transactionId, string paRes, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implémentation de la factory de processeurs de paiement
/// </summary>
public class PaymentProcessorFactory : IPaymentProcessorFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<PaymentMethodType, string> _processorMappings;
    private readonly Dictionary<string, Type> _processorTypes;
    private readonly ILogger<PaymentProcessorFactory> _logger;

    public PaymentProcessorFactory(
        IServiceProvider serviceProvider,
        ILogger<PaymentProcessorFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        // Configuration des mappings processeur -> type de paiement
        _processorMappings = new Dictionary<PaymentMethodType, string>
        {
            { PaymentMethodType.CreditCard, "Stripe" },
            { PaymentMethodType.ContactlessCard, "Stripe" },
            { PaymentMethodType.BankTransfer, "Plaid" },
            { PaymentMethodType.MobilePayment, "PayPal" },
            { PaymentMethodType.DigitalWallet, "PayPal" },
            { PaymentMethodType.Cryptocurrency, "Coinbase" },
            { PaymentMethodType.GiftCard, "Internal" },
            { PaymentMethodType.Cash, "Internal" }
        };

        // Mapping des processeurs vers leurs types d'implémentation
        _processorTypes = new Dictionary<string, Type>
        {
            { "Stripe", typeof(StripePaymentProcessor) },
            { "PayPal", typeof(PayPalPaymentProcessor) },
            { "Plaid", typeof(PlaidPaymentProcessor) },
            { "Coinbase", typeof(CoinbasePaymentProcessor) },
            { "Internal", typeof(InternalPaymentProcessor) }
        };
    }

    public IPaymentProcessor GetProcessor(PaymentMethodType paymentMethodType)
    {
        if (!_processorMappings.TryGetValue(paymentMethodType, out var processorName))
        {
            _logger.LogWarning("No processor configured for payment method type {PaymentMethodType}", paymentMethodType);
            throw new NotSupportedException($"Payment method type {paymentMethodType} is not supported");
        }

        return GetProcessor(processorName);
    }

    public IPaymentProcessor GetProcessor(string processorName)
    {
        if (!_processorTypes.TryGetValue(processorName, out var processorType))
        {
            _logger.LogWarning("Processor {ProcessorName} not found", processorName);
            throw new NotSupportedException($"Payment processor {processorName} is not supported");
        }

        var processor = _serviceProvider.GetService(processorType) as IPaymentProcessor;
        if (processor == null)
        {
            _logger.LogError("Failed to resolve processor {ProcessorName} of type {ProcessorType}", processorName, processorType);
            throw new InvalidOperationException($"Failed to resolve payment processor {processorName}");
        }

        return processor;
    }

    public Task<List<string>> GetAvailableProcessorsAsync(CancellationToken cancellationToken = default)
    {
        var processors = _processorTypes.Keys.ToList();
        return Task.FromResult(processors);
    }
}

/// <summary>
/// Processeur Stripe pour cartes de crédit
/// </summary>
public class StripePaymentProcessor : IPaymentProcessor
{
    private readonly IStripeService _stripeService;
    private readonly ILogger<StripePaymentProcessor> _logger;

    public string ProcessorName => "Stripe";
    public PaymentMethodType[] SupportedPaymentMethods => new[] 
    { 
        PaymentMethodType.CreditCard, 
        PaymentMethodType.ContactlessCard 
    };
    public bool SupportsRefunds => true;
    public bool SupportsPartialCapture => true;
    public bool Supports3DSecure => true;

    public StripePaymentProcessor(
        IStripeService stripeService,
        ILogger<StripePaymentProcessor> logger)
    {
        _stripeService = stripeService;
        _logger = logger;
    }

    public async Task<PaymentProcessorResult> CreatePaymentAsync(PaymentProcessorRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating Stripe payment for amount {Amount}", request.Amount);

            var stripeRequest = new StripePaymentRequest
            {
                Amount = request.Amount.Amount,
                Currency = request.Amount.Currency.Code,
                PaymentMethodId = request.PaymentMethodToken,
                Description = request.Description,
                Metadata = request.Metadata,
                CustomerId = request.CustomerId?.ToString(),
                CaptureMethod = request.CaptureMethod == PaymentCaptureMethod.Automatic ? "automatic" : "manual"
            };

            var stripeResult = await _stripeService.CreatePaymentIntentAsync(stripeRequest, cancellationToken);

            return new PaymentProcessorResult
            {
                Success = stripeResult.Status != "failed",
                TransactionId = stripeResult.Id,
                Status = MapStripeStatus(stripeResult.Status),
                Amount = new Money(stripeResult.Amount / 100m, stripeResult.Currency.ToUpper()),
                ProcessorResponse = stripeResult.RawResponse,
                RequiresAction = stripeResult.Status == "requires_action",
                ActionUrl = stripeResult.NextAction?.RedirectToUrl?.Url,
                ErrorMessage = stripeResult.LastPaymentError?.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Stripe payment");
            return new PaymentProcessorResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<PaymentProcessorResult> CapturePaymentAsync(string transactionId, Money amount, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Capturing Stripe payment {TransactionId} for amount {Amount}", transactionId, amount);

            var stripeResult = await _stripeService.CapturePaymentIntentAsync(transactionId, 
                (long)(amount.Amount * 100), cancellationToken);

            return new PaymentProcessorResult
            {
                Success = stripeResult.Status == "succeeded",
                TransactionId = stripeResult.Id,
                Status = MapStripeStatus(stripeResult.Status),
                Amount = new Money(stripeResult.AmountReceived / 100m, stripeResult.Currency.ToUpper()),
                ProcessorResponse = stripeResult.RawResponse
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing Stripe payment {TransactionId}", transactionId);
            return new PaymentProcessorResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<PaymentProcessorResult> RefundPaymentAsync(string transactionId, Money amount, string? reason, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Refunding Stripe payment {TransactionId} for amount {Amount}", transactionId, amount);

            var stripeResult = await _stripeService.CreateRefundAsync(transactionId, 
                (long)(amount.Amount * 100), reason, cancellationToken);

            return new PaymentProcessorResult
            {
                Success = stripeResult.Status == "succeeded",
                TransactionId = stripeResult.Id,
                Status = PaymentProcessorStatus.Succeeded,
                Amount = new Money(stripeResult.Amount / 100m, stripeResult.Currency.ToUpper()),
                ProcessorResponse = stripeResult.RawResponse
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding Stripe payment {TransactionId}", transactionId);
            return new PaymentProcessorResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<PaymentProcessorResult> VoidPaymentAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Voiding Stripe payment {TransactionId}", transactionId);

            var stripeResult = await _stripeService.CancelPaymentIntentAsync(transactionId, cancellationToken);

            return new PaymentProcessorResult
            {
                Success = stripeResult.Status == "canceled",
                TransactionId = stripeResult.Id,
                Status = MapStripeStatus(stripeResult.Status),
                ProcessorResponse = stripeResult.RawResponse
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voiding Stripe payment {TransactionId}", transactionId);
            return new PaymentProcessorResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<PaymentProcessorStatus> GetPaymentStatusAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var stripeResult = await _stripeService.GetPaymentIntentAsync(transactionId, cancellationToken);
            return MapStripeStatus(stripeResult.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Stripe payment status for {TransactionId}", transactionId);
            return PaymentProcessorStatus.Failed;
        }
    }

    public async Task<PaymentProcessorResult> Process3DSecureAsync(string transactionId, string paRes, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing 3D Secure for Stripe payment {TransactionId}", transactionId);

            var stripeResult = await _stripeService.Confirm3DSecureAsync(transactionId, paRes, cancellationToken);

            return new PaymentProcessorResult
            {
                Success = stripeResult.Status != "failed",
                TransactionId = stripeResult.Id,
                Status = MapStripeStatus(stripeResult.Status),
                ProcessorResponse = stripeResult.RawResponse,
                RequiresAction = stripeResult.Status == "requires_action"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing 3D Secure for Stripe payment {TransactionId}", transactionId);
            return new PaymentProcessorResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private static PaymentProcessorStatus MapStripeStatus(string stripeStatus)
    {
        return stripeStatus switch
        {
            "requires_payment_method" => PaymentProcessorStatus.Pending,
            "requires_confirmation" => PaymentProcessorStatus.Pending,
            "requires_action" => PaymentProcessorStatus.RequiresAction,
            "processing" => PaymentProcessorStatus.Processing,
            "requires_capture" => PaymentProcessorStatus.Authorized,
            "succeeded" => PaymentProcessorStatus.Succeeded,
            "canceled" => PaymentProcessorStatus.Canceled,
            _ => PaymentProcessorStatus.Failed
        };
    }
}

/// <summary>
/// Processeur PayPal pour portefeuilles numériques
/// </summary>
public class PayPalPaymentProcessor : IPaymentProcessor
{
    private readonly IPayPalService _paypalService;
    private readonly ILogger<PayPalPaymentProcessor> _logger;

    public string ProcessorName => "PayPal";
    public PaymentMethodType[] SupportedPaymentMethods => new[] 
    { 
        PaymentMethodType.MobilePayment, 
        PaymentMethodType.DigitalWallet 
    };
    public bool SupportsRefunds => true;
    public bool SupportsPartialCapture => true;
    public bool Supports3DSecure => false;

    public PayPalPaymentProcessor(
        IPayPalService paypalService,
        ILogger<PayPalPaymentProcessor> logger)
    {
        _paypalService = paypalService;
        _logger = logger;
    }

    public async Task<PaymentProcessorResult> CreatePaymentAsync(PaymentProcessorRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating PayPal payment for amount {Amount}", request.Amount);

            var paypalRequest = new PayPalPaymentRequest
            {
                Amount = request.Amount.Amount,
                Currency = request.Amount.Currency.Code,
                Description = request.Description,
                ReturnUrl = request.ReturnUrl,
                CancelUrl = request.CancelUrl
            };

            var paypalResult = await _paypalService.CreatePaymentAsync(paypalRequest, cancellationToken);

            return new PaymentProcessorResult
            {
                Success = !string.IsNullOrEmpty(paypalResult.Id),
                TransactionId = paypalResult.Id,
                Status = PaymentProcessorStatus.Pending,
                Amount = request.Amount,
                ProcessorResponse = paypalResult.RawResponse,
                RequiresAction = true,
                ActionUrl = paypalResult.ApprovalUrl
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PayPal payment");
            return new PaymentProcessorResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<PaymentProcessorResult> CapturePaymentAsync(string transactionId, Money amount, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Capturing PayPal payment {TransactionId}", transactionId);

            var paypalResult = await _paypalService.CapturePaymentAsync(transactionId, cancellationToken);

            return new PaymentProcessorResult
            {
                Success = paypalResult.State == "completed",
                TransactionId = paypalResult.Id,
                Status = paypalResult.State == "completed" ? PaymentProcessorStatus.Succeeded : PaymentProcessorStatus.Failed,
                Amount = amount,
                ProcessorResponse = paypalResult.RawResponse
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing PayPal payment {TransactionId}", transactionId);
            return new PaymentProcessorResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<PaymentProcessorResult> RefundPaymentAsync(string transactionId, Money amount, string? reason, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Refunding PayPal payment {TransactionId}", transactionId);

            var paypalResult = await _paypalService.RefundPaymentAsync(transactionId, amount.Amount, cancellationToken);

            return new PaymentProcessorResult
            {
                Success = paypalResult.State == "completed",
                TransactionId = paypalResult.Id,
                Status = PaymentProcessorStatus.Succeeded,
                Amount = amount,
                ProcessorResponse = paypalResult.RawResponse
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding PayPal payment {TransactionId}", transactionId);
            return new PaymentProcessorResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public Task<PaymentProcessorResult> VoidPaymentAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        // PayPal ne supporte pas l'annulation, uniquement le remboursement
        throw new NotSupportedException("PayPal does not support void operations. Use refund instead.");
    }

    public async Task<PaymentProcessorStatus> GetPaymentStatusAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var paypalResult = await _paypalService.GetPaymentAsync(transactionId, cancellationToken);
            return paypalResult.State switch
            {
                "created" => PaymentProcessorStatus.Pending,
                "approved" => PaymentProcessorStatus.Authorized,
                "completed" => PaymentProcessorStatus.Succeeded,
                "cancelled" => PaymentProcessorStatus.Canceled,
                "failed" => PaymentProcessorStatus.Failed,
                _ => PaymentProcessorStatus.Failed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PayPal payment status for {TransactionId}", transactionId);
            return PaymentProcessorStatus.Failed;
        }
    }

    public Task<PaymentProcessorResult> Process3DSecureAsync(string transactionId, string paRes, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("PayPal does not support 3D Secure");
    }
}

/// <summary>
/// Processeur interne pour cash et gift cards
/// </summary>
public class InternalPaymentProcessor : IPaymentProcessor
{
    private readonly ILogger<InternalPaymentProcessor> _logger;

    public string ProcessorName => "Internal";
    public PaymentMethodType[] SupportedPaymentMethods => new[] 
    { 
        PaymentMethodType.Cash, 
        PaymentMethodType.GiftCard 
    };
    public bool SupportsRefunds => true;
    public bool SupportsPartialCapture => true;
    public bool Supports3DSecure => false;

    public InternalPaymentProcessor(ILogger<InternalPaymentProcessor> logger)
    {
        _logger = logger;
    }

    public Task<PaymentProcessorResult> CreatePaymentAsync(PaymentProcessorRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating internal payment for amount {Amount}", request.Amount);

        // Pour les paiements internes, on confirme immédiatement
        var result = new PaymentProcessorResult
        {
            Success = true,
            TransactionId = Guid.NewGuid().ToString(),
            Status = PaymentProcessorStatus.Succeeded,
            Amount = request.Amount,
            ProcessorResponse = "Internal payment processed successfully"
        };

        return Task.FromResult(result);
    }

    public Task<PaymentProcessorResult> CapturePaymentAsync(string transactionId, Money amount, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Capturing internal payment {TransactionId}", transactionId);

        // Les paiements internes sont déjà capturés
        var result = new PaymentProcessorResult
        {
            Success = true,
            TransactionId = transactionId,
            Status = PaymentProcessorStatus.Succeeded,
            Amount = amount,
            ProcessorResponse = "Internal payment already captured"
        };

        return Task.FromResult(result);
    }

    public Task<PaymentProcessorResult> RefundPaymentAsync(string transactionId, Money amount, string? reason, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Refunding internal payment {TransactionId}", transactionId);

        var result = new PaymentProcessorResult
        {
            Success = true,
            TransactionId = Guid.NewGuid().ToString(),
            Status = PaymentProcessorStatus.Succeeded,
            Amount = amount,
            ProcessorResponse = $"Internal refund processed: {reason}"
        };

        return Task.FromResult(result);
    }

    public Task<PaymentProcessorResult> VoidPaymentAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Voiding internal payment {TransactionId}", transactionId);

        var result = new PaymentProcessorResult
        {
            Success = true,
            TransactionId = transactionId,
            Status = PaymentProcessorStatus.Canceled,
            ProcessorResponse = "Internal payment voided"
        };

        return Task.FromResult(result);
    }

    public Task<PaymentProcessorStatus> GetPaymentStatusAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        // Pour les paiements internes, on retourne toujours succeeded
        return Task.FromResult(PaymentProcessorStatus.Succeeded);
    }

    public Task<PaymentProcessorResult> Process3DSecureAsync(string transactionId, string paRes, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Internal processor does not support 3D Secure");
    }
}

// Processeurs additionnels (stubs pour Plaid et Coinbase)
public class PlaidPaymentProcessor : IPaymentProcessor
{
    public string ProcessorName => "Plaid";
    public PaymentMethodType[] SupportedPaymentMethods => new[] { PaymentMethodType.BankTransfer };
    public bool SupportsRefunds => true;
    public bool SupportsPartialCapture => false;
    public bool Supports3DSecure => false;

    public Task<PaymentProcessorResult> CreatePaymentAsync(PaymentProcessorRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Plaid integration not yet implemented");
    }

    public Task<PaymentProcessorResult> CapturePaymentAsync(string transactionId, Money amount, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PaymentProcessorResult> RefundPaymentAsync(string transactionId, Money amount, string? reason, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PaymentProcessorResult> VoidPaymentAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PaymentProcessorStatus> GetPaymentStatusAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PaymentProcessorResult> Process3DSecureAsync(string transactionId, string paRes, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}

public class CoinbasePaymentProcessor : IPaymentProcessor
{
    public string ProcessorName => "Coinbase";
    public PaymentMethodType[] SupportedPaymentMethods => new[] { PaymentMethodType.Cryptocurrency };
    public bool SupportsRefunds => false;
    public bool SupportsPartialCapture => false;
    public bool Supports3DSecure => false;

    public Task<PaymentProcessorResult> CreatePaymentAsync(PaymentProcessorRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Coinbase integration not yet implemented");
    }

    public Task<PaymentProcessorResult> CapturePaymentAsync(string transactionId, Money amount, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PaymentProcessorResult> RefundPaymentAsync(string transactionId, Money amount, string? reason, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Cryptocurrency payments cannot be refunded");
    }

    public Task<PaymentProcessorResult> VoidPaymentAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PaymentProcessorStatus> GetPaymentStatusAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PaymentProcessorResult> Process3DSecureAsync(string transactionId, string paRes, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}

// DTOs pour les résultats des processeurs
public class PaymentProcessorResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public PaymentProcessorStatus Status { get; set; }
    public Money? Amount { get; set; }
    public string? ProcessorResponse { get; set; }
    public string? ErrorMessage { get; set; }
    public bool RequiresAction { get; set; }
    public string? ActionUrl { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class PaymentProcessorRequest
{
    public Money Amount { get; set; } = null!;
    public string? PaymentMethodToken { get; set; }
    public Guid? CustomerId { get; set; }
    public string? Description { get; set; }
    public PaymentCaptureMethod CaptureMethod { get; set; } = PaymentCaptureMethod.Automatic;
    public string? ReturnUrl { get; set; }
    public string? CancelUrl { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public enum PaymentProcessorStatus
{
    Pending,
    Processing,
    RequiresAction,
    Authorized,
    Succeeded,
    Failed,
    Canceled
}

public enum PaymentCaptureMethod
{
    Automatic,
    Manual
}

// Interfaces pour les services externes
public interface IStripeService
{
    Task<StripePaymentResult> CreatePaymentIntentAsync(StripePaymentRequest request, CancellationToken cancellationToken = default);
    Task<StripePaymentResult> CapturePaymentIntentAsync(string paymentIntentId, long amount, CancellationToken cancellationToken = default);
    Task<StripeRefundResult> CreateRefundAsync(string paymentIntentId, long amount, string? reason, CancellationToken cancellationToken = default);
    Task<StripePaymentResult> CancelPaymentIntentAsync(string paymentIntentId, CancellationToken cancellationToken = default);
    Task<StripePaymentResult> GetPaymentIntentAsync(string paymentIntentId, CancellationToken cancellationToken = default);
    Task<StripePaymentResult> Confirm3DSecureAsync(string paymentIntentId, string paRes, CancellationToken cancellationToken = default);
}

public interface IPayPalService
{
    Task<PayPalPaymentResult> CreatePaymentAsync(PayPalPaymentRequest request, CancellationToken cancellationToken = default);
    Task<PayPalPaymentResult> CapturePaymentAsync(string paymentId, CancellationToken cancellationToken = default);
    Task<PayPalRefundResult> RefundPaymentAsync(string paymentId, decimal amount, CancellationToken cancellationToken = default);
    Task<PayPalPaymentResult> GetPaymentAsync(string paymentId, CancellationToken cancellationToken = default);
}

// DTOs pour les services externes
public class StripePaymentRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? PaymentMethodId { get; set; }
    public string? Description { get; set; }
    public string? CustomerId { get; set; }
    public string CaptureMethod { get; set; } = "automatic";
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class StripePaymentResult
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long Amount { get; set; }
    public long AmountReceived { get; set; }
    public string Currency { get; set; } = string.Empty;
    public StripeNextAction? NextAction { get; set; }
    public StripeError? LastPaymentError { get; set; }
    public string RawResponse { get; set; } = string.Empty;
}

public class StripeNextAction
{
    public StripeRedirectToUrl? RedirectToUrl { get; set; }
}

public class StripeRedirectToUrl
{
    public string? Url { get; set; }
}

public class StripeError
{
    public string? Message { get; set; }
}

public class StripeRefundResult
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string RawResponse { get; set; } = string.Empty;
}

public class PayPalPaymentRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ReturnUrl { get; set; }
    public string? CancelUrl { get; set; }
}

public class PayPalPaymentResult
{
    public string Id { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? ApprovalUrl { get; set; }
    public string RawResponse { get; set; } = string.Empty;
}

public class PayPalRefundResult
{
    public string Id { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string RawResponse { get; set; } = string.Empty;
}