namespace Payment.Application.Services;

// Interface temporaire pour la compilation
public interface IWebhookService
{
    Task<WebhookProcessingResult> ProcessStripeWebhookAsync(string body, string signature, CancellationToken cancellationToken = default);
    Task<WebhookProcessingResult> ProcessPayPalWebhookAsync(string body, Dictionary<string, string> headers, CancellationToken cancellationToken = default);
    Task<WebhookProcessingResult> ProcessGenericWebhookAsync(string provider, string body, Dictionary<string, string> headers, CancellationToken cancellationToken = default);
}

public class WebhookProcessingResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
}

public class WebhookSignatureException : Exception
{
    public WebhookSignatureException(string message) : base(message)
    {
    }
}