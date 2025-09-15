using Microsoft.Extensions.DependencyInjection;
using Payment.Application.Services;

namespace Payment.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Enregistrement MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
            
            // Services temporaires pour la compilation
            services.AddScoped<IWebhookService, WebhookService>();
            
            return services;
        }
    }
}

// Implémentation temporaire des services pour la compilation
namespace Payment.Application.Services
{
    public class WebhookService : IWebhookService
    {
        public Task<WebhookProcessingResult> ProcessStripeWebhookAsync(string body, string signature, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new WebhookProcessingResult { Success = true });
        }

        public Task<WebhookProcessingResult> ProcessPayPalWebhookAsync(string body, Dictionary<string, string> headers, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new WebhookProcessingResult { Success = true });
        }

        public Task<WebhookProcessingResult> ProcessGenericWebhookAsync(string provider, string body, Dictionary<string, string> headers, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new WebhookProcessingResult { Success = true });
        }
    }
}