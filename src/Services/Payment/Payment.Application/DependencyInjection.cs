using Microsoft.Extensions.DependencyInjection;
using Payment.Application.Services;
using Payment.Application.Handlers;
using Payment.Domain.Enums;
using Payment.Domain.Entities;
using Payment.Domain.ValueObjects;
using AutoMapper;
using System.Linq.Expressions;

namespace Payment.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Enregistrement MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
            
            // Enregistrement d'un mapper minimal pour éviter les erreurs
            services.AddScoped<IMapper, MockMapper>();
            
            // Services temporaires pour les handlers - implementations minimales
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<Services.IFraudDetectionService, Services.MockFraudDetectionService>();
            services.AddScoped<Services.IPaymentProcessorFactory, Services.MockPaymentProcessorFactory>();
            services.AddScoped<Services.ICardTokenizationService, Services.MockCardTokenizationService>();
            services.AddScoped<Services.IThreeDSecureService, Services.MockThreeDSecureService>();
            
            return services;
        }
    }
}

// DTO temporaire pour Order
public class OrderDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";
    public string Status { get; set; } = string.Empty;
}

// Interface pour le service Order
public interface IOrderService
{
    Task<OrderValidationResult> ValidateOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}

// Result pour validation Order
public class OrderValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

// Services temporaires pour les handlers
namespace Payment.Application.Services
{
    using Payment.Application.Handlers;

    public class OrderService : IOrderService
    {
        public Task<OrderValidationResult> ValidateOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            // Implementation temporaire - toujours valide pour le demo
            return Task.FromResult(new OrderValidationResult 
            {
                IsValid = true,
                Errors = new List<string>()
            });
        }
    }

    // Service de détection de fraude temporaire - implémentation minimale
    public class MockFraudDetectionService : IFraudDetectionService
    {
        public Task<int> AnalyzeTransactionAsync(Transaction transaction, string? ipAddress, string? geoLocation, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(10); // Score faible
        }

        public Task<FraudAnalysisResult> GetDetailedAnalysisAsync(Transaction transaction, string? ipAddress, string? geoLocation, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new FraudAnalysisResult()); // Instance par défaut
        }

        public Task<bool> IsHighRiskCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task<bool> IsBlacklistedIpAsync(string ipAddress, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }
    }

    // Factory pour les processeurs de paiement temporaire - implémentation minimale
    public class MockPaymentProcessorFactory : IPaymentProcessorFactory
    {
        public IPaymentProcessor GetProcessor(PaymentMethodType methodType)
        {
            return new MockPaymentProcessor();
        }

        public IPaymentProcessor GetProcessor(string processorName)
        {
            return new MockPaymentProcessor();
        }

        public IPaymentProcessor GetDefaultProcessor()
        {
            return new MockPaymentProcessor();
        }

        public Task<List<string>> GetAvailableProcessorsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new List<string> { "MockProcessor" });
        }
    }

    // Processeur de paiement temporaire - implémentation très simple
    public class MockPaymentProcessor : IPaymentProcessor
    {
        public string Name => "MockProcessor";
        public bool IsEnabled => true;
        public PaymentMethodType[] SupportedMethods => new[] { PaymentMethodType.CreditCard };
        public string ProcessorName => "MockProcessor";
        public PaymentMethodType[] SupportedPaymentMethods => new[] { PaymentMethodType.CreditCard };
        public bool SupportsRefunds => true;
        public bool SupportsPartialCapture => true;
        public bool Supports3DSecure => false;

        public Task<PaymentProcessorResult> ProcessPaymentAsync(PaymentProcessorRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PaymentProcessorResult()); // Instance par défaut
        }

        public Task<PaymentProcessorResult> CreatePaymentAsync(PaymentProcessorRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PaymentProcessorResult()); // Instance par défaut
        }

        public Task<PaymentProcessorResult> CapturePaymentAsync(string transactionId, Money amount, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PaymentProcessorResult()); // Instance par défaut
        }

        public Task<PaymentProcessorResult> RefundPaymentAsync(string transactionId, Money amount, string? reason, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PaymentProcessorResult()); // Instance par défaut
        }

        public Task<PaymentProcessorResult> VoidPaymentAsync(string transactionId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PaymentProcessorResult()); // Instance par défaut
        }

        public Task<PaymentProcessorStatus> GetPaymentStatusAsync(string transactionId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PaymentProcessorStatus()); // Instance par défaut
        }

        public Task<PaymentProcessorResult> Process3DSecureAsync(string transactionId, string authenticationData, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PaymentProcessorResult()); // Instance par défaut
        }
    }

    // Service de tokenisation de cartes temporaire - implémentation minimale
    public class MockCardTokenizationService : ICardTokenizationService
    {
        public Task<CardTokenizationResult> TokenizeCardAsync(CardTokenizationRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CardTokenizationResult()); // Instance par défaut
        }

        public Task<Domain.Entities.Card?> GetCardFromTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Domain.Entities.Card?>(null); // Null pour simplicité
        }

        public Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true); // Toujours valide pour les tests
        }

        public Task<bool> RevokeTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true); // Toujours réussi pour les tests
        }

        public Task<List<Domain.Entities.Card>> GetCardsByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new List<Domain.Entities.Card>()); // Liste vide
        }
    }

    // Service 3D Secure temporaire - implémentation minimale
    public class MockThreeDSecureService : IThreeDSecureService
    {
        public Task<ThreeDSecureInitiationResult> InitiateAuthenticationAsync(ThreeDSecureRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ThreeDSecureInitiationResult()); // Instance par défaut
        }

        public Task<ThreeDSecureCompletionResult> CompleteAuthenticationAsync(string transactionId, string paRes, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ThreeDSecureCompletionResult()); // Instance par défaut
        }

        public Task<ThreeDSecureStatus> GetAuthenticationStatusAsync(string transactionId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ThreeDSecureStatus()); // Instance par défaut
        }

        public Task<bool> IsCardEligibleFor3DSecureAsync(string cardToken, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false); // Jamais requis pour les tests
        }

        public Task<ThreeDSecureValidationResult> ValidatePaResAsync(string paRes, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ThreeDSecureValidationResult()); // Instance par défaut
        }
    }

    // Implémentation minimale d'AutoMapper pour les tests
    public class MockMapper : IMapper
    {
        public TDestination Map<TDestination>(object source) 
        {
            // Retourne une instance par défaut pour les tests
            return Activator.CreateInstance<TDestination>();
        }

        public TDestination Map<TSource, TDestination>(TSource source) 
        {
            return Activator.CreateInstance<TDestination>();
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination) 
        {
            return destination;
        }

        public object Map(object source, Type sourceType, Type destinationType) 
        {
            return Activator.CreateInstance(destinationType) ?? new object();
        }

        public object Map(object source, object destination, Type sourceType, Type destinationType) 
        {
            return destination;
        }

        public TDestination Map<TDestination>(object source, Action<IMappingOperationOptions<object, TDestination>> opts) 
        {
            return Activator.CreateInstance<TDestination>();
        }

        public TDestination Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions<TSource, TDestination>> opts) 
        {
            return Activator.CreateInstance<TDestination>();
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions<TSource, TDestination>> opts) 
        {
            return destination;
        }

        public object Map(object source, Type sourceType, Type destinationType, Action<IMappingOperationOptions<object, object>> opts) 
        {
            return Activator.CreateInstance(destinationType) ?? new object();
        }

        public object Map(object source, object destination, Type sourceType, Type destinationType, Action<IMappingOperationOptions<object, object>> opts) 
        {
            return destination;
        }

        public IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source, object parameters = null, params Expression<Func<TDestination, object>>[] membersToExpand) 
        {
            return new List<TDestination>().AsQueryable();
        }

        public IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source, IDictionary<string, object> parameters, params string[] membersToExpand) 
        {
            return new List<TDestination>().AsQueryable();
        }

        public IQueryable ProjectTo(IQueryable source, Type destinationType, IDictionary<string, object> parameters = null, params string[] membersToExpand) 
        {
            return source;
        }

        public IConfigurationProvider ConfigurationProvider => throw new NotImplementedException();
    }
}