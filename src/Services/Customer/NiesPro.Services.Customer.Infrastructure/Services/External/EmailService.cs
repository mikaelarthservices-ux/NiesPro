using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using NiesPro.Services.Customer.Infrastructure.Configuration;

namespace NiesPro.Services.Customer.Infrastructure.Services.External;

/// <summary>
/// Service email sophistiqué avec support SendGrid/SMTP et patterns de résilience
/// </summary>
public interface IEmailService
{
    Task<EmailResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
    Task<EmailResult> SendTemplateAsync(TemplateEmailMessage message, CancellationToken cancellationToken = default);
    Task<BulkEmailResult> SendBulkAsync(List<EmailMessage> messages, CancellationToken cancellationToken = default);
    Task<EmailResult> SendWelcomeEmailAsync(string email, string firstName, string customerNumber, CancellationToken cancellationToken = default);
    Task<EmailResult> SendBirthdayEmailAsync(string email, string firstName, string specialOffer, CancellationToken cancellationToken = default);
    Task<EmailResult> SendLoyaltyPointsEmailAsync(string email, string firstName, int pointsEarned, int totalPoints, CancellationToken cancellationToken = default);
    Task<EmailResult> SendChurnPreventionEmailAsync(string email, string firstName, string specialOffer, CancellationToken cancellationToken = default);
}

public class EmailService : IEmailService
{
    private readonly EmailConfiguration _config;
    private readonly ILogger<EmailService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;

    public EmailService(
        IOptions<EmailConfiguration> config,
        ILogger<EmailService> logger,
        HttpClient httpClient)
    {
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        ConfigureHttpClient();
        _retryPolicy = CreateRetryPolicy();
    }

    public async Task<EmailResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            ValidateMessage(message);

            _logger.LogInformation("Sending email to {Email} with subject {Subject}",
                message.To, message.Subject);

            var result = _config.Provider.ToLowerInvariant() switch
            {
                "sendgrid" => await SendViaSendGridAsync(message, cancellationToken),
                "smtp" => await SendViaSmtpAsync(message, cancellationToken),
                _ => throw new NotSupportedException($"Email provider {_config.Provider} not supported")
            };

            _logger.LogInformation("Email sent successfully to {Email}. MessageId: {MessageId}",
                message.To, result.MessageId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", message.To);
            return EmailResult.Failed(ex.Message, ex);
        }
    }

    public async Task<EmailResult> SendTemplateAsync(TemplateEmailMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            ValidateTemplateMessage(message);

            _logger.LogInformation("Sending template email {TemplateId} to {Email}",
                message.TemplateId, message.To);

            var result = _config.Provider.ToLowerInvariant() switch
            {
                "sendgrid" => await SendTemplateViaSendGridAsync(message, cancellationToken),
                "smtp" => await SendTemplateViaSmtpAsync(message, cancellationToken),
                _ => throw new NotSupportedException($"Email provider {_config.Provider} not supported")
            };

            _logger.LogInformation("Template email sent successfully to {Email}. MessageId: {MessageId}",
                message.To, result.MessageId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send template email to {Email}", message.To);
            return EmailResult.Failed(ex.Message, ex);
        }
    }

    public async Task<BulkEmailResult> SendBulkAsync(List<EmailMessage> messages, CancellationToken cancellationToken = default)
    {
        if (!messages?.Any() == true)
            return BulkEmailResult.Success(0, 0);

        var results = new List<EmailResult>();
        var batchSize = _config.BulkBatchSize;
        var successCount = 0;
        var failureCount = 0;

        _logger.LogInformation("Sending bulk email to {Count} recipients in batches of {BatchSize}",
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

        _logger.LogInformation("Bulk email completed. Success: {Success}, Failures: {Failures}",
            successCount, failureCount);

        return new BulkEmailResult
        {
            IsSuccess = failureCount == 0,
            SuccessCount = successCount,
            FailureCount = failureCount,
            Results = results
        };
    }

    // ===== BUSINESS EMAIL TEMPLATES =====

    public async Task<EmailResult> SendWelcomeEmailAsync(
        string email, 
        string firstName, 
        string customerNumber, 
        CancellationToken cancellationToken = default)
    {
        var message = new TemplateEmailMessage
        {
            To = email,
            TemplateId = _config.Templates.WelcomeTemplate,
            TemplateData = new
            {
                firstName = firstName,
                customerNumber = customerNumber,
                loginUrl = $"{_config.BaseUrl}/login",
                supportEmail = _config.SupportEmail
            }
        };

        return await SendTemplateAsync(message, cancellationToken);
    }

    public async Task<EmailResult> SendBirthdayEmailAsync(
        string email, 
        string firstName, 
        string specialOffer, 
        CancellationToken cancellationToken = default)
    {
        var message = new TemplateEmailMessage
        {
            To = email,
            TemplateId = _config.Templates.BirthdayTemplate,
            TemplateData = new
            {
                firstName = firstName,
                specialOffer = specialOffer,
                offerUrl = $"{_config.BaseUrl}/offers/birthday",
                expiryDate = DateTime.UtcNow.AddDays(30).ToString("dd/MM/yyyy")
            }
        };

        return await SendTemplateAsync(message, cancellationToken);
    }

    public async Task<EmailResult> SendLoyaltyPointsEmailAsync(
        string email, 
        string firstName, 
        int pointsEarned, 
        int totalPoints, 
        CancellationToken cancellationToken = default)
    {
        var message = new TemplateEmailMessage
        {
            To = email,
            TemplateId = _config.Templates.LoyaltyPointsTemplate,
            TemplateData = new
            {
                firstName = firstName,
                pointsEarned = pointsEarned,
                totalPoints = totalPoints,
                rewardsUrl = $"{_config.BaseUrl}/rewards",
                nextTierPoints = CalculateNextTierPoints(totalPoints)
            }
        };

        return await SendTemplateAsync(message, cancellationToken);
    }

    public async Task<EmailResult> SendChurnPreventionEmailAsync(
        string email, 
        string firstName, 
        string specialOffer, 
        CancellationToken cancellationToken = default)
    {
        var message = new TemplateEmailMessage
        {
            To = email,
            TemplateId = _config.Templates.ChurnPreventionTemplate,
            TemplateData = new
            {
                firstName = firstName,
                specialOffer = specialOffer,
                offerUrl = $"{_config.BaseUrl}/offers/comeback",
                contactUrl = $"{_config.BaseUrl}/contact"
            }
        };

        return await SendTemplateAsync(message, cancellationToken);
    }

    // ===== PRIVATE METHODS =====

    private void ConfigureHttpClient()
    {
        _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
        
        if (_config.Provider.Equals("sendgrid", StringComparison.OrdinalIgnoreCase))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.SendGrid.ApiKey}");
            _httpClient.BaseAddress = new Uri("https://api.sendgrid.com/");
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
                    _logger.LogWarning("Email delivery retry {RetryCount} after {Delay}ms",
                        retryCount, timespan.TotalMilliseconds);
                })
            .WrapAsync(Policy
                .Handle<HttpRequestException>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: _config.CircuitBreakerThreshold,
                    durationOfBreak: TimeSpan.FromMinutes(_config.CircuitBreakerDurationMinutes),
                    onBreak: (ex, duration) => _logger.LogWarning("Email service circuit breaker opened for {Duration}", duration),
                    onReset: () => _logger.LogInformation("Email service circuit breaker reset")));
    }

    private async Task<EmailResult> SendViaSendGridAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        var payload = new
        {
            personalizations = new[]
            {
                new
                {
                    to = new[] { new { email = message.To, name = message.ToName } },
                    subject = message.Subject
                }
            },
            from = new { email = _config.FromEmail, name = _config.FromName },
            content = new[]
            {
                new { type = "text/html", value = message.HtmlBody },
                new { type = "text/plain", value = message.TextBody }
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _retryPolicy.ExecuteAsync(async () =>
            await _httpClient.PostAsync("v3/mail/send", content, cancellationToken));

        if (response.IsSuccessStatusCode)
        {
            var messageId = response.Headers.FirstOrDefault(h => h.Key == "X-Message-Id").Value?.FirstOrDefault();
            return EmailResult.Success(messageId ?? Guid.NewGuid().ToString());
        }

        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return EmailResult.Failed($"SendGrid API error: {response.StatusCode} - {errorContent}");
    }

    private async Task<EmailResult> SendTemplateViaSendGridAsync(TemplateEmailMessage message, CancellationToken cancellationToken)
    {
        var payload = new
        {
            personalizations = new[]
            {
                new
                {
                    to = new[] { new { email = message.To, name = message.ToName } },
                    dynamic_template_data = message.TemplateData
                }
            },
            from = new { email = _config.FromEmail, name = _config.FromName },
            template_id = message.TemplateId
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _retryPolicy.ExecuteAsync(async () =>
            await _httpClient.PostAsync("v3/mail/send", content, cancellationToken));

        if (response.IsSuccessStatusCode)
        {
            var messageId = response.Headers.FirstOrDefault(h => h.Key == "X-Message-Id").Value?.FirstOrDefault();
            return EmailResult.Success(messageId ?? Guid.NewGuid().ToString());
        }

        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return EmailResult.Failed($"SendGrid API error: {response.StatusCode} - {errorContent}");
    }

    private async Task<EmailResult> SendViaSmtpAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        // Implementation for SMTP would go here
        // Using System.Net.Mail.SmtpClient or MailKit
        throw new NotImplementedException("SMTP implementation not yet available");
    }

    private async Task<EmailResult> SendTemplateViaSmtpAsync(TemplateEmailMessage message, CancellationToken cancellationToken)
    {
        // Implementation for SMTP templates would go here
        throw new NotImplementedException("SMTP template implementation not yet available");
    }

    private void ValidateMessage(EmailMessage message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        if (string.IsNullOrWhiteSpace(message.To))
            throw new ArgumentException("To email is required", nameof(message));

        if (string.IsNullOrWhiteSpace(message.Subject))
            throw new ArgumentException("Subject is required", nameof(message));

        if (string.IsNullOrWhiteSpace(message.HtmlBody) && string.IsNullOrWhiteSpace(message.TextBody))
            throw new ArgumentException("Email body is required", nameof(message));
    }

    private void ValidateTemplateMessage(TemplateEmailMessage message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        if (string.IsNullOrWhiteSpace(message.To))
            throw new ArgumentException("To email is required", nameof(message));

        if (string.IsNullOrWhiteSpace(message.TemplateId))
            throw new ArgumentException("Template ID is required", nameof(message));
    }

    private int CalculateNextTierPoints(int totalPoints)
    {
        // Business logic for tier thresholds
        return totalPoints switch
        {
            < 1000 => 1000 - totalPoints,
            < 5000 => 5000 - totalPoints,
            < 15000 => 15000 - totalPoints,
            < 50000 => 50000 - totalPoints,
            _ => 0
        };
    }
}

// ===== MODEL CLASSES =====

public class EmailMessage
{
    public string To { get; set; } = string.Empty;
    public string? ToName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public string? TextBody { get; set; }
    public List<EmailAttachment>? Attachments { get; set; }
    public Dictionary<string, string>? CustomHeaders { get; set; }
    public string? ReplyTo { get; set; }
    public List<string>? Tags { get; set; }
    public DateTime? ScheduledTime { get; set; }
}

public class TemplateEmailMessage
{
    public string To { get; set; } = string.Empty;
    public string? ToName { get; set; }
    public string TemplateId { get; set; } = string.Empty;
    public object? TemplateData { get; set; }
    public List<EmailAttachment>? Attachments { get; set; }
    public Dictionary<string, string>? CustomHeaders { get; set; }
    public string? ReplyTo { get; set; }
    public List<string>? Tags { get; set; }
    public DateTime? ScheduledTime { get; set; }
}

public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
    public string? ContentId { get; set; }
}

public class EmailResult
{
    public bool IsSuccess { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public static EmailResult Success(string messageId) => new()
    {
        IsSuccess = true,
        MessageId = messageId
    };

    public static EmailResult Failed(string errorMessage, Exception? exception = null) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage,
        Exception = exception
    };
}

public class BulkEmailResult
{
    public bool IsSuccess { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<EmailResult> Results { get; set; } = new();

    public static BulkEmailResult Success(int successCount, int failureCount) => new()
    {
        IsSuccess = failureCount == 0,
        SuccessCount = successCount,
        FailureCount = failureCount
    };
}