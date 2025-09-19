using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Polly;
using Polly.CircuitBreaker;
using NiesPro.Services.Customer.Infrastructure.Configuration;

namespace NiesPro.Services.Customer.Infrastructure.Services.External;

/// <summary>
/// Service SMS sophistiqué avec support Twilio et patterns de résilience
/// </summary>
public interface ISmsService
{
    Task<SmsResult> SendAsync(SmsMessage message, CancellationToken cancellationToken = default);
    Task<BulkSmsResult> SendBulkAsync(List<SmsMessage> messages, CancellationToken cancellationToken = default);
    Task<SmsResult> SendOtpAsync(string phoneNumber, string otpCode, CancellationToken cancellationToken = default);
    Task<SmsResult> SendWelcomeSmsAsync(string phoneNumber, string firstName, string customerNumber, CancellationToken cancellationToken = default);
    Task<SmsResult> SendOrderConfirmationAsync(string phoneNumber, string firstName, string orderNumber, decimal amount, CancellationToken cancellationToken = default);
    Task<SmsResult> SendLoyaltyNotificationAsync(string phoneNumber, string firstName, int pointsEarned, CancellationToken cancellationToken = default);
    Task<SmsResult> SendAppointmentReminderAsync(string phoneNumber, string firstName, DateTime appointmentDate, string serviceName, CancellationToken cancellationToken = default);
    Task<SmsResult> SendPromotionalAsync(string phoneNumber, string firstName, string offerText, string promoCode, CancellationToken cancellationToken = default);
    Task<SmsStatusResult> GetDeliveryStatusAsync(string messageId, CancellationToken cancellationToken = default);
}

public class SmsService : ISmsService
{
    private readonly SmsConfiguration _config;
    private readonly ILogger<SmsService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;

    public SmsService(
        IOptions<SmsConfiguration> config,
        ILogger<SmsService> logger,
        HttpClient httpClient)
    {
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        ConfigureHttpClient();
        _retryPolicy = CreateRetryPolicy();
    }

    public async Task<SmsResult> SendAsync(SmsMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            ValidateMessage(message);

            _logger.LogInformation("Sending SMS to {PhoneNumber} with {Length} characters",
                message.To, message.Body.Length);

            var result = _config.Provider.ToLowerInvariant() switch
            {
                "twilio" => await SendViaTwilioAsync(message, cancellationToken),
                "nexmo" => await SendViaNexmoAsync(message, cancellationToken),
                "mock" => await SendViaMockAsync(message, cancellationToken),
                _ => throw new NotSupportedException($"SMS provider {_config.Provider} not supported")
            };

            _logger.LogInformation("SMS sent successfully to {PhoneNumber}. MessageId: {MessageId}",
                message.To, result.MessageId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", message.To);
            return SmsResult.Failed(ex.Message, ex);
        }
    }

    public async Task<BulkSmsResult> SendBulkAsync(List<SmsMessage> messages, CancellationToken cancellationToken = default)
    {
        if (!messages?.Any() == true)
            return BulkSmsResult.Success(0, 0);

        var results = new List<SmsResult>();
        var batchSize = _config.BulkBatchSize;
        var successCount = 0;
        var failureCount = 0;

        _logger.LogInformation("Sending bulk SMS to {Count} recipients in batches of {BatchSize}",
            messages.Count, batchSize);

        for (int i = 0; i < messages.Count; i += batchSize)
        {
            var batch = messages.Skip(i).Take(batchSize);
            var batchTasks = batch.Select(msg => SendAsync(msg, cancellationToken));

            var batchResults = await Task.WhenAll(batchTasks);
            results.AddRange(batchResults);

            successCount += batchResults.Count(r => r.IsSuccess);
            failureCount += batchResults.Count(r => !r.IsSuccess);

            // Delay between batches to avoid rate limiting
            if (i + batchSize < messages.Count)
            {
                await Task.Delay(_config.BulkDelayMs, cancellationToken);
            }
        }

        _logger.LogInformation("Bulk SMS completed. Success: {Success}, Failures: {Failures}",
            successCount, failureCount);

        return new BulkSmsResult
        {
            IsSuccess = failureCount == 0,
            SuccessCount = successCount,
            FailureCount = failureCount,
            Results = results
        };
    }

    // ===== BUSINESS SMS TEMPLATES =====

    public async Task<SmsResult> SendOtpAsync(
        string phoneNumber, 
        string otpCode, 
        CancellationToken cancellationToken = default)
    {
        var message = new SmsMessage
        {
            To = phoneNumber,
            Body = $"Votre code de vérification: {otpCode}. Valide 10 minutes. Ne le partagez pas.",
            MessageType = SmsMessageType.Transactional
        };

        return await SendAsync(message, cancellationToken);
    }

    public async Task<SmsResult> SendWelcomeSmsAsync(
        string phoneNumber, 
        string firstName, 
        string customerNumber, 
        CancellationToken cancellationToken = default)
    {
        var message = new SmsMessage
        {
            To = phoneNumber,
            Body = $"Bienvenue {firstName}! Votre numéro client: {customerNumber}. " +
                   $"Profitez de nos services premium!",
            MessageType = SmsMessageType.Transactional
        };

        return await SendAsync(message, cancellationToken);
    }

    public async Task<SmsResult> SendOrderConfirmationAsync(
        string phoneNumber, 
        string firstName, 
        string orderNumber, 
        decimal amount, 
        CancellationToken cancellationToken = default)
    {
        var message = new SmsMessage
        {
            To = phoneNumber,
            Body = $"Commande confirmée {firstName}! N°{orderNumber} - {amount:C}. " +
                   $"Suivi: {_config.TrackingUrl}/{orderNumber}",
            MessageType = SmsMessageType.Transactional
        };

        return await SendAsync(message, cancellationToken);
    }

    public async Task<SmsResult> SendLoyaltyNotificationAsync(
        string phoneNumber, 
        string firstName, 
        int pointsEarned, 
        CancellationToken cancellationToken = default)
    {
        var message = new SmsMessage
        {
            To = phoneNumber,
            Body = $"Félicitations {firstName}! +{pointsEarned} points fidélité. " +
                   $"Consultez vos récompenses: {_config.LoyaltyUrl}",
            MessageType = SmsMessageType.Marketing
        };

        return await SendAsync(message, cancellationToken);
    }

    public async Task<SmsResult> SendAppointmentReminderAsync(
        string phoneNumber, 
        string firstName, 
        DateTime appointmentDate, 
        string serviceName, 
        CancellationToken cancellationToken = default)
    {
        var dateStr = appointmentDate.ToString("dd/MM à HH:mm");
        var message = new SmsMessage
        {
            To = phoneNumber,
            Body = $"Rappel RDV {firstName}: {serviceName} le {dateStr}. " +
                   $"Modifier: {_config.AppointmentUrl}",
            MessageType = SmsMessageType.Transactional
        };

        return await SendAsync(message, cancellationToken);
    }

    public async Task<SmsResult> SendPromotionalAsync(
        string phoneNumber, 
        string firstName, 
        string offerText, 
        string promoCode, 
        CancellationToken cancellationToken = default)
    {
        var message = new SmsMessage
        {
            To = phoneNumber,
            Body = $"Offre spéciale {firstName}: {offerText}. Code: {promoCode}. " +
                   $"Valable jusqu'au {DateTime.UtcNow.AddDays(7):dd/MM}. STOP=STOP",
            MessageType = SmsMessageType.Marketing
        };

        return await SendAsync(message, cancellationToken);
    }

    public async Task<SmsStatusResult> GetDeliveryStatusAsync(string messageId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting delivery status for message {MessageId}", messageId);

            var result = _config.Provider.ToLowerInvariant() switch
            {
                "twilio" => await GetTwilioStatusAsync(messageId, cancellationToken),
                "nexmo" => await GetNexmoStatusAsync(messageId, cancellationToken),
                "mock" => await GetMockStatusAsync(messageId, cancellationToken),
                _ => throw new NotSupportedException($"SMS provider {_config.Provider} not supported")
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get delivery status for {MessageId}", messageId);
            return SmsStatusResult.Failed(ex.Message, ex);
        }
    }

    // ===== PRIVATE METHODS =====

    private void ConfigureHttpClient()
    {
        _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
        
        if (_config.Provider.Equals("twilio", StringComparison.OrdinalIgnoreCase))
        {
            var credentials = Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes($"{_config.Twilio.AccountSid}:{_config.Twilio.AuthToken}"));
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");
            _httpClient.BaseAddress = new Uri($"https://api.twilio.com/2010-04-01/Accounts/{_config.Twilio.AccountSid}/");
        }
    }

    private IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy()
    {
        return Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: _config.RetryCount,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("SMS delivery retry {RetryCount} after {Delay}ms",
                        retryCount, timespan.TotalMilliseconds);
                })
            .WrapAsync(Policy
                .Handle<HttpRequestException>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: _config.CircuitBreakerThreshold,
                    durationOfBreak: TimeSpan.FromMinutes(_config.CircuitBreakerDurationMinutes),
                    onBreak: (ex, duration) => _logger.LogWarning("SMS service circuit breaker opened for {Duration}", duration),
                    onReset: () => _logger.LogInformation("SMS service circuit breaker reset")));
    }

    private async Task<SmsResult> SendViaTwilioAsync(SmsMessage message, CancellationToken cancellationToken)
    {
        var parameters = new List<KeyValuePair<string, string>>
        {
            new("To", NormalizePhoneNumber(message.To)),
            new("From", _config.Twilio.FromNumber),
            new("Body", message.Body)
        };

        if (!string.IsNullOrEmpty(message.StatusCallback))
        {
            parameters.Add(new("StatusCallback", message.StatusCallback));
        }

        var content = new FormUrlEncodedContent(parameters);

        var response = await _retryPolicy.ExecuteAsync(async () =>
            await _httpClient.PostAsync("Messages.json", content, cancellationToken));

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var twilioResponse = JsonSerializer.Deserialize<TwilioSmsResponse>(responseContent);
            
            return SmsResult.Success(
                twilioResponse?.Sid ?? Guid.NewGuid().ToString(),
                CalculateSmsSegments(message.Body),
                GetSmsPrice(message.Body.Length));
        }

        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return SmsResult.Failed($"Twilio API error: {response.StatusCode} - {errorContent}");
    }

    private async Task<SmsResult> SendViaNexmoAsync(SmsMessage message, CancellationToken cancellationToken)
    {
        // Implementation for Nexmo/Vonage would go here
        throw new NotImplementedException("Nexmo implementation not yet available");
    }

    private async Task<SmsResult> SendViaMockAsync(SmsMessage message, CancellationToken cancellationToken)
    {
        // Mock implementation for testing
        await Task.Delay(100, cancellationToken);
        
        var messageId = $"mock_{Guid.NewGuid():N}";
        var segments = CalculateSmsSegments(message.Body);
        var price = GetSmsPrice(message.Body.Length);

        _logger.LogInformation("Mock SMS sent - ID: {MessageId}, Segments: {Segments}, Price: {Price}",
            messageId, segments, price);

        return SmsResult.Success(messageId, segments, price);
    }

    private async Task<SmsStatusResult> GetTwilioStatusAsync(string messageId, CancellationToken cancellationToken)
    {
        var response = await _retryPolicy.ExecuteAsync(async () =>
            await _httpClient.GetAsync($"Messages/{messageId}.json", cancellationToken));

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var twilioResponse = JsonSerializer.Deserialize<TwilioSmsResponse>(responseContent);

            return new SmsStatusResult
            {
                IsSuccess = true,
                MessageId = messageId,
                Status = MapTwilioStatus(twilioResponse?.Status),
                DeliveredAt = twilioResponse?.DateSent,
                ErrorCode = twilioResponse?.ErrorCode?.ToString(),
                ErrorMessage = twilioResponse?.ErrorMessage
            };
        }

        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return SmsStatusResult.Failed($"Failed to get status: {errorContent}");
    }

    private async Task<SmsStatusResult> GetNexmoStatusAsync(string messageId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Nexmo status check not yet available");
    }

    private async Task<SmsStatusResult> GetMockStatusAsync(string messageId, CancellationToken cancellationToken)
    {
        await Task.Delay(50, cancellationToken);
        
        return new SmsStatusResult
        {
            IsSuccess = true,
            MessageId = messageId,
            Status = SmsDeliveryStatus.Delivered,
            DeliveredAt = DateTime.UtcNow.AddSeconds(-30)
        };
    }

    private void ValidateMessage(SmsMessage message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        if (string.IsNullOrWhiteSpace(message.To))
            throw new ArgumentException("To phone number is required", nameof(message));

        if (string.IsNullOrWhiteSpace(message.Body))
            throw new ArgumentException("Message body is required", nameof(message));

        if (message.Body.Length > _config.MaxMessageLength)
            throw new ArgumentException($"Message body exceeds maximum length of {_config.MaxMessageLength}", nameof(message));
    }

    private string NormalizePhoneNumber(string phoneNumber)
    {
        // Remove all non-digit characters except +
        var normalized = new string(phoneNumber.Where(c => char.IsDigit(c) || c == '+').ToArray());
        
        // Add + if not present and starts with country code
        if (!normalized.StartsWith("+") && normalized.Length >= 10)
        {
            normalized = "+" + normalized;
        }

        return normalized;
    }

    private int CalculateSmsSegments(string body)
    {
        // Basic SMS segmentation logic
        const int singleSmsLength = 160;
        const int concatenatedSmsLength = 153;

        if (body.Length <= singleSmsLength)
            return 1;

        return (int)Math.Ceiling((double)body.Length / concatenatedSmsLength);
    }

    private decimal GetSmsPrice(int bodyLength)
    {
        var segments = CalculateSmsSegments(new string('x', bodyLength));
        return segments * _config.PricePerSegment;
    }

    private SmsDeliveryStatus MapTwilioStatus(string? twilioStatus)
    {
        return twilioStatus?.ToLowerInvariant() switch
        {
            "queued" => SmsDeliveryStatus.Queued,
            "sending" => SmsDeliveryStatus.Sending,
            "sent" => SmsDeliveryStatus.Sent,
            "delivered" => SmsDeliveryStatus.Delivered,
            "failed" => SmsDeliveryStatus.Failed,
            "undelivered" => SmsDeliveryStatus.Failed,
            _ => SmsDeliveryStatus.Unknown
        };
    }
}

// ===== MODEL CLASSES =====

public class SmsMessage
{
    public string To { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public SmsMessageType MessageType { get; set; } = SmsMessageType.Transactional;
    public string? StatusCallback { get; set; }
    public DateTime? ScheduledTime { get; set; }
    public int? ValidityPeriod { get; set; }
    public Dictionary<string, string>? CustomData { get; set; }
}

public enum SmsMessageType
{
    Transactional,
    Marketing,
    Otp,
    Alert
}

public enum SmsDeliveryStatus
{
    Unknown,
    Queued,
    Sending,
    Sent,
    Delivered,
    Failed
}

public class SmsResult
{
    public bool IsSuccess { get; set; }
    public string? MessageId { get; set; }
    public int Segments { get; set; }
    public decimal Cost { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public static SmsResult Success(string messageId, int segments = 1, decimal cost = 0) => new()
    {
        IsSuccess = true,
        MessageId = messageId,
        Segments = segments,
        Cost = cost
    };

    public static SmsResult Failed(string errorMessage, Exception? exception = null) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage,
        Exception = exception
    };
}

public class BulkSmsResult
{
    public bool IsSuccess { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public decimal TotalCost { get; set; }
    public List<SmsResult> Results { get; set; } = new();

    public static BulkSmsResult Success(int successCount, int failureCount) => new()
    {
        IsSuccess = failureCount == 0,
        SuccessCount = successCount,
        FailureCount = failureCount
    };
}

public class SmsStatusResult
{
    public bool IsSuccess { get; set; }
    public string? MessageId { get; set; }
    public SmsDeliveryStatus Status { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }

    public static SmsStatusResult Failed(string errorMessage, Exception? exception = null) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage,
        Exception = exception
    };
}

// ===== TWILIO RESPONSE MODELS =====

internal class TwilioSmsResponse
{
    public string? Sid { get; set; }
    public string? Status { get; set; }
    public DateTime? DateSent { get; set; }
    public int? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}