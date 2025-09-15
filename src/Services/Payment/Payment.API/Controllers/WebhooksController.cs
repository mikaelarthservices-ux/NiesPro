using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Services;
using System.Security.Claims;

namespace Payment.API.Controllers;

/// <summary>
/// Contrôleur pour les webhooks des processeurs de paiement externe
/// </summary>
[ApiController]
[Route("api/v1/webhooks")]
[AllowAnonymous] // Les webhooks utilisent une validation par signature
public class WebhooksController : ControllerBase
{
    private readonly IPaymentProcessorFactory _paymentProcessorFactory;
    private readonly IWebhookService _webhookService;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        IPaymentProcessorFactory paymentProcessorFactory,
        IWebhookService webhookService,
        ILogger<WebhooksController> logger)
    {
        _paymentProcessorFactory = paymentProcessorFactory;
        _webhookService = webhookService;
        _logger = logger;
    }

    /// <summary>
    /// Webhook pour les notifications Stripe
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation de réception</returns>
    [HttpPost("stripe")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StripeWebhook(CancellationToken cancellationToken = default)
    {
        try
        {
            var body = await ReadRequestBodyAsync();
            var signature = Request.Headers["Stripe-Signature"].ToString();

            _logger.LogInformation("Received Stripe webhook with signature {Signature}", signature);

            var result = await _webhookService.ProcessStripeWebhookAsync(body, signature, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("Stripe webhook processed successfully");
                return Ok(new { received = true });
            }
            else
            {
                _logger.LogWarning("Failed to process Stripe webhook: {Error}", result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }
        }
        catch (WebhookSignatureException ex)
        {
            _logger.LogWarning("Invalid Stripe webhook signature: {Error}", ex.Message);
            return BadRequest(new { error = "Invalid signature" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Webhook pour les notifications PayPal
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation de réception</returns>
    [HttpPost("paypal")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PayPalWebhook(CancellationToken cancellationToken = default)
    {
        try
        {
            var body = await ReadRequestBodyAsync();
            var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());

            _logger.LogInformation("Received PayPal webhook");

            var result = await _webhookService.ProcessPayPalWebhookAsync(body, headers, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("PayPal webhook processed successfully");
                return Ok(new { received = true });
            }
            else
            {
                _logger.LogWarning("Failed to process PayPal webhook: {Error}", result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }
        }
        catch (WebhookSignatureException ex)
        {
            _logger.LogWarning("Invalid PayPal webhook signature: {Error}", ex.Message);
            return BadRequest(new { error = "Invalid signature" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PayPal webhook");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Webhook générique pour d'autres processeurs
    /// </summary>
    /// <param name="provider">Nom du processeur</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation de réception</returns>
    [HttpPost("{provider}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenericWebhook(
        string provider,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var body = await ReadRequestBodyAsync();
            var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());

            _logger.LogInformation("Received webhook for provider {Provider}", provider);

            var result = await _webhookService.ProcessGenericWebhookAsync(provider, body, headers, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("Webhook for provider {Provider} processed successfully", provider);
                return Ok(new { received = true });
            }
            else if (result.ErrorMessage?.Contains("Provider not found") == true)
            {
                return NotFound(new { error = "Provider not supported" });
            }
            else
            {
                _logger.LogWarning("Failed to process webhook for provider {Provider}: {Error}", provider, result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }
        }
        catch (WebhookSignatureException ex)
        {
            _logger.LogWarning("Invalid webhook signature for provider {Provider}: {Error}", provider, ex.Message);
            return BadRequest(new { error = "Invalid signature" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook for provider {Provider}", provider);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Test endpoint pour vérifier la connectivité des webhooks
    /// </summary>
    /// <returns>Message de test</returns>
    [HttpGet("test")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult TestWebhook()
    {
        return Ok(new 
        { 
            message = "Webhook endpoint is active",
            timestamp = DateTime.UtcNow,
            server = Environment.MachineName
        });
    }

    private async Task<string> ReadRequestBodyAsync()
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        Request.Body.Position = 0;
        return body;
    }
}