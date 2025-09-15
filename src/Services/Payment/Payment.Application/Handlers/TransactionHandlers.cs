using MediatR;
using AutoMapper;
using Payment.Application.Commands;
using Payment.Application.DTOs;
using Payment.Application.Services;
using Payment.Domain.Entities;
using Payment.Domain.ValueObjects;
using Payment.Domain.Enums;
using Payment.Domain.Events;
using Microsoft.Extensions.Logging;
using AdvancedPaymentProcessor = Payment.Application.Services.IPaymentProcessor;
using AdvancedPaymentProcessorFactory = Payment.Application.Services.IPaymentProcessorFactory;

namespace Payment.Application.Handlers;

/// <summary>
/// Handler pour la capture de transaction
/// </summary>
public class CaptureTransactionHandler : IRequestHandler<CaptureTransactionCommand, CaptureTransactionResult>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly AdvancedPaymentProcessorFactory _paymentProcessorFactory;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly ILogger<CaptureTransactionHandler> _logger;

    public CaptureTransactionHandler(
        ITransactionRepository transactionRepository,
        IPaymentRepository paymentRepository,
        AdvancedPaymentProcessorFactory paymentProcessorFactory,
        IMapper mapper,
        IMediator mediator,
        ILogger<CaptureTransactionHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _paymentRepository = paymentRepository;
        _paymentProcessorFactory = paymentProcessorFactory;
        _mapper = mapper;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<CaptureTransactionResult> Handle(CaptureTransactionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Capturing transaction {TransactionId}", request.TransactionId);

            // 1. Récupérer la transaction
            var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId, cancellationToken);
            if (transaction == null)
            {
                return new CaptureTransactionResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Transaction introuvable"
                };
            }

            // 2. Vérifier que la transaction peut être capturée
            if (transaction.Status != PaymentStatus.Authorized)
            {
                return new CaptureTransactionResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"La transaction ne peut pas être capturée dans l'état {transaction.Status}"
                };
            }

            // 3. Déterminer le montant à capturer
            Money? captureAmount = null;
            if (request.Amount.HasValue && !string.IsNullOrEmpty(request.Currency))
            {
                captureAmount = new Money(request.Amount.Value, request.Currency);
                
                if (captureAmount > transaction.Amount)
                {
                    return new CaptureTransactionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Le montant de capture ne peut pas dépasser le montant autorisé"
                    };
                }
            }

            // 4. Appeler le processeur de paiement pour la capture
            var processor = _paymentProcessorFactory.GetProcessor(transaction.PaymentMethod.Type);
            var captureResult = await processor.CapturePaymentAsync(
                transaction.Id.ToString(), captureAmount, cancellationToken);

            if (!captureResult.Success)
            {
                return new CaptureTransactionResult
                {
                    TransactionId = transaction.Id,
                    TransactionNumber = transaction.TransactionNumber,
                    IsSuccess = false,
                    ErrorMessage = captureResult.ErrorMessage ?? "Erreur lors de la capture"
                };
            }

            // 5. Mettre à jour la transaction
            transaction.Capture(captureAmount, null); // Pas de frais dans le résultat actuel

            // 6. Mettre à jour le paiement parent si complètement payé
            var payment = await _paymentRepository.GetByIdAsync(transaction.PaymentMethodId, cancellationToken);
            if (payment != null && payment.IsFullyPaid())
            {
                payment.MarkAsSuccessful();
                await _paymentRepository.UpdateAsync(payment, cancellationToken);
            }

            // 7. Sauvegarder les changements
            await _transactionRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Transaction {TransactionId} captured successfully, amount: {Amount}", 
                transaction.Id, transaction.Amount);

            // 8. Publier les événements
            foreach (var domainEvent in transaction.DomainEvents)
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }
            transaction.ClearDomainEvents();

            return new CaptureTransactionResult
            {
                TransactionId = transaction.Id,
                TransactionNumber = transaction.TransactionNumber,
                Status = transaction.Status,
                CapturedAmount = transaction.Amount.Amount,
                Currency = transaction.Amount.Currency.Code,
                Fees = transaction.Fees?.Amount,
                ProcessedAt = transaction.ProcessedAt ?? DateTime.UtcNow,
                IsSuccess = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing transaction {TransactionId}", request.TransactionId);
            return new CaptureTransactionResult
            {
                IsSuccess = false,
                ErrorMessage = "Erreur lors de la capture de la transaction"
            };
        }
    }
}

/// <summary>
/// Handler pour le remboursement de transaction
/// </summary>
public class RefundTransactionHandler : IRequestHandler<RefundTransactionCommand, RefundTransactionResult>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly AdvancedPaymentProcessorFactory _paymentProcessorFactory;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly ILogger<RefundTransactionHandler> _logger;

    public RefundTransactionHandler(
        ITransactionRepository transactionRepository,
        AdvancedPaymentProcessorFactory paymentProcessorFactory,
        IMapper mapper,
        IMediator mediator,
        ILogger<RefundTransactionHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _paymentProcessorFactory = paymentProcessorFactory;
        _mapper = mapper;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<RefundTransactionResult> Handle(RefundTransactionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Refunding transaction {TransactionId}, amount: {Amount} {Currency}", 
                request.TransactionId, request.Amount, request.Currency);

            // 1. Récupérer la transaction
            var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId, cancellationToken);
            if (transaction == null)
            {
                return new RefundTransactionResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Transaction introuvable"
                };
            }

            // 2. Vérifier que la transaction peut être remboursée
            if (!transaction.CanBeRefunded())
            {
                return new RefundTransactionResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Cette transaction ne peut pas être remboursée"
                };
            }

            // 3. Valider le montant de remboursement
            var refundAmount = new Money(request.Amount, request.Currency);
            if (refundAmount > transaction.GetRefundableAmount())
            {
                return new RefundTransactionResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Le montant de remboursement dépasse le montant disponible"
                };
            }

            // 4. Appeler le processeur de paiement pour le remboursement
            var processor = _paymentProcessorFactory.GetProcessor(transaction.PaymentMethod.Type);
            var refundResult = await processor.RefundPaymentAsync(
                transaction.Id.ToString(), refundAmount, request.Reason, cancellationToken);

            if (!refundResult.Success)
            {
                return new RefundTransactionResult
                {
                    OriginalTransactionId = transaction.Id,
                    IsSuccess = false,
                    ErrorMessage = refundResult.ErrorMessage ?? "Erreur lors du remboursement"
                };
            }

            // 5. Créer la transaction de remboursement
            var refundTransaction = transaction.Refund(refundAmount, request.Reason);
            if (!string.IsNullOrEmpty(refundResult.TransactionId))
            {
                refundTransaction.SetExternalReference(refundResult.TransactionId);
            }

            // 6. Sauvegarder les changements
            await _transactionRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Transaction {TransactionId} refunded successfully, refund amount: {Amount}, refund transaction: {RefundTransactionId}", 
                transaction.Id, refundAmount, refundTransaction.Id);

            // 7. Publier les événements
            foreach (var domainEvent in transaction.DomainEvents)
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }
            transaction.ClearDomainEvents();

            return new RefundTransactionResult
            {
                RefundTransactionId = refundTransaction.Id,
                RefundTransactionNumber = refundTransaction.TransactionNumber,
                OriginalTransactionId = transaction.Id,
                RefundedAmount = refundAmount.Amount,
                Currency = refundAmount.Currency,
                Status = refundTransaction.Status,
                ProcessedAt = refundTransaction.ProcessedAt ?? DateTime.UtcNow,
                IsSuccess = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding transaction {TransactionId}", request.TransactionId);
            return new RefundTransactionResult
            {
                IsSuccess = false,
                ErrorMessage = "Erreur lors du remboursement"
            };
        }
    }
}

/// <summary>
/// Handler pour la création de moyen de paiement
/// </summary>
public class CreatePaymentMethodHandler : IRequestHandler<CreatePaymentMethodCommand, CreatePaymentMethodResult>
{
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly ICardTokenizationService _cardTokenizationService;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly ILogger<CreatePaymentMethodHandler> _logger;

    public CreatePaymentMethodHandler(
        IPaymentMethodRepository paymentMethodRepository,
        ICardTokenizationService cardTokenizationService,
        IMapper mapper,
        IMediator mediator,
        ILogger<CreatePaymentMethodHandler> logger)
    {
        _paymentMethodRepository = paymentMethodRepository;
        _cardTokenizationService = cardTokenizationService;
        _mapper = mapper;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<CreatePaymentMethodResult> Handle(CreatePaymentMethodCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating payment method of type {Type} for customer {CustomerId}", 
                request.Type, request.CustomerId);

            CreditCard? creditCard = null;

            // 1. Traitement spécial pour les cartes de crédit
            if (request.Type.RequiresOnlineValidation() && request.CreditCard != null)
            {
                // Tokeniser la carte de crédit
                var tokenizationResult = await _cardTokenizationService.TokenizeCardAsync(
                    request.CreditCard.Number, 
                    request.CreditCard.ExpiryMonth, 
                    request.CreditCard.ExpiryYear,
                    request.CreditCard.HolderName,
                    cancellationToken);

                if (!tokenizationResult.IsSuccess)
                {
                    return new CreatePaymentMethodResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Erreur lors de la tokenisation de la carte"
                    };
                }

                creditCard = CreditCard.Create(
                    request.CreditCard.Number,
                    request.CreditCard.HolderName,
                    request.CreditCard.ExpiryMonth,
                    request.CreditCard.ExpiryYear,
                    request.CreditCard.Cvv ?? string.Empty);
            }

            // 2. Créer les limites si spécifiées
            Money? dailyLimit = null;
            Money? transactionLimit = null;

            if (request.DailyLimit.HasValue && !string.IsNullOrEmpty(request.DailyLimitCurrency))
            {
                dailyLimit = new Money(request.DailyLimit.Value, request.DailyLimitCurrency);
            }

            if (request.TransactionLimit.HasValue && !string.IsNullOrEmpty(request.TransactionLimitCurrency))
            {
                transactionLimit = new Money(request.TransactionLimit.Value, request.TransactionLimitCurrency);
            }

            // 3. Créer le moyen de paiement
            var paymentMethod = new PaymentMethod(
                request.Type,
                request.DisplayName,
                request.CustomerId,
                creditCard,
                dailyLimit,
                transactionLimit,
                request.ExpiryDate,
                request.ExternalToken);

            // 4. Ajouter les métadonnées
            if (request.Metadata != null)
            {
                foreach (var metadata in request.Metadata)
                {
                    paymentMethod.SetMetadata(metadata.Key, metadata.Value);
                }
            }

            // 5. Gérer le statut par défaut
            if (request.SetAsDefault)
            {
                // Désactiver l'ancien moyen par défaut
                var existingMethods = await _paymentMethodRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
                var currentDefault = existingMethods.FirstOrDefault(pm => pm.IsDefault);
                if (currentDefault != null)
                {
                    currentDefault.RemoveDefault();
                    await _paymentMethodRepository.UpdateAsync(currentDefault, cancellationToken);
                }

                paymentMethod.SetAsDefault();
            }

            // 6. Sauvegarder le moyen de paiement
            await _paymentMethodRepository.AddAsync(paymentMethod, cancellationToken);
            await _paymentMethodRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment method {PaymentMethodId} created successfully for customer {CustomerId}", 
                paymentMethod.Id, request.CustomerId);

            // 7. Publier l'événement
            await _mediator.Publish(new PaymentMethodCreatedEvent(
                paymentMethod.Id,
                paymentMethod.Type,
                paymentMethod.CustomerId,
                paymentMethod.DisplayName), cancellationToken);

            return new CreatePaymentMethodResult
            {
                PaymentMethodId = paymentMethod.Id,
                Type = paymentMethod.Type,
                SecureDisplayName = paymentMethod.GetSecureDisplayName(),
                IsDefault = paymentMethod.IsDefault,
                CardToken = creditCard?.Token,
                LastFourDigits = creditCard?.GetMaskedNumber().Substring(creditCard.GetMaskedNumber().Length - 4),
                CardBrand = creditCard?.Brand.ToString(),
                CardExpiryDate = creditCard?.ExpiryDate,
                IsSuccess = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment method for customer {CustomerId}", request.CustomerId);
            return new CreatePaymentMethodResult
            {
                IsSuccess = false,
                ErrorMessage = "Erreur lors de la création du moyen de paiement"
            };
        }
    }
}

/// <summary>
/// Handler pour définir un moyen de paiement par défaut
/// </summary>
public class SetDefaultPaymentMethodHandler : IRequestHandler<SetDefaultPaymentMethodCommand, SetDefaultPaymentMethodResult>
{
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<SetDefaultPaymentMethodHandler> _logger;

    public SetDefaultPaymentMethodHandler(
        IPaymentMethodRepository paymentMethodRepository,
        IMediator mediator,
        ILogger<SetDefaultPaymentMethodHandler> logger)
    {
        _paymentMethodRepository = paymentMethodRepository;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<SetDefaultPaymentMethodResult> Handle(SetDefaultPaymentMethodCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Setting payment method {PaymentMethodId} as default for customer {CustomerId}", 
                request.PaymentMethodId, request.CustomerId);

            // 1. Récupérer tous les moyens de paiement du client
            var customerPaymentMethods = await _paymentMethodRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);

            // 2. Trouver le moyen de paiement à définir par défaut
            var newDefaultMethod = customerPaymentMethods.FirstOrDefault(pm => pm.Id == request.PaymentMethodId);
            if (newDefaultMethod == null)
            {
                return new SetDefaultPaymentMethodResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Moyen de paiement introuvable"
                };
            }

            if (newDefaultMethod.CustomerId != request.CustomerId)
            {
                return new SetDefaultPaymentMethodResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Ce moyen de paiement n'appartient pas à ce client"
                };
            }

            if (!newDefaultMethod.CanBeUsed())
            {
                return new SetDefaultPaymentMethodResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Ce moyen de paiement ne peut pas être utilisé"
                };
            }

            // 3. Trouver l'ancien moyen par défaut
            var currentDefault = customerPaymentMethods.FirstOrDefault(pm => pm.IsDefault);
            Guid? previousDefaultId = currentDefault?.Id;

            // 4. Mettre à jour les statuts
            if (currentDefault != null && currentDefault.Id != request.PaymentMethodId)
            {
                currentDefault.RemoveDefault();
                await _paymentMethodRepository.UpdateAsync(currentDefault, cancellationToken);
            }

            newDefaultMethod.SetAsDefault();
            await _paymentMethodRepository.UpdateAsync(newDefaultMethod, cancellationToken);

            // 5. Sauvegarder les changements
            await _paymentMethodRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment method {PaymentMethodId} set as default for customer {CustomerId}", 
                request.PaymentMethodId, request.CustomerId);

            // 6. Publier l'événement
            await _mediator.Publish(new DefaultPaymentMethodChangedEvent(
                request.CustomerId,
                request.PaymentMethodId,
                previousDefaultId), cancellationToken);

            return new SetDefaultPaymentMethodResult
            {
                CustomerId = request.CustomerId,
                PaymentMethodId = request.PaymentMethodId,
                PreviousDefaultPaymentMethodId = previousDefaultId,
                IsSuccess = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default payment method {PaymentMethodId} for customer {CustomerId}", 
                request.PaymentMethodId, request.CustomerId);
            return new SetDefaultPaymentMethodResult
            {
                IsSuccess = false,
                ErrorMessage = "Erreur lors de la définition du moyen de paiement par défaut"
            };
        }
    }
}

// Interfaces additionnelles
public interface ICardTokenizationService
{
    Task<CardTokenizationResult> TokenizeCardAsync(string cardNumber, int expiryMonth, int expiryYear, string holderName, CancellationToken cancellationToken = default);
}

// DTOs additionnels
public class CardTokenizationResult
{
    public bool IsSuccess { get; set; }
    public string? Token { get; set; }
    public string? ErrorMessage { get; set; }
}

public class CaptureResult
{
    public bool IsSuccess { get; set; }
    public Money? Fees { get; set; }
    public string? ExternalReference { get; set; }
    public string? ErrorMessage { get; set; }
}

public class RefundResult
{
    public bool IsSuccess { get; set; }
    public string? ExternalReference { get; set; }
    public string? ErrorMessage { get; set; }
}