using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Application.DTOs;
using Payment.Application.Services;
using Payment.Domain.Entities;
using Payment.Domain.Interfaces;
using Payment.Domain.ValueObjects;
using Payment.Domain.Enums;
using NiesPro.Contracts.Application.CQRS;
using NiesPro.Contracts.Common;
using NiesPro.Logging.Client;

namespace Payment.Application.Queries.V2;

/// <summary>
/// Requête pour récupérer un paiement par ID - NiesPro Enterprise Standard
/// </summary>
public class GetPaymentByIdQuery : BaseQuery<ApiResponse<PaymentDetailDto>>
{
    /// <summary>
    /// Identifiant du paiement
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Inclure les transactions associées
    /// </summary>
    public bool IncludeTransactions { get; set; } = false;

    /// <summary>
    /// Inclure les remboursements
    /// </summary>
    public bool IncludeRefunds { get; set; } = false;

    public GetPaymentByIdQuery(Guid paymentId)
    {
        PaymentId = paymentId;
    }
}

/// <summary>
/// Requête pour récupérer les paiements d'un client - NiesPro Enterprise Standard
/// </summary>
public class GetPaymentsByCustomerQuery : BaseQuery<ApiResponse<PagedResult<PaymentSummaryDto>>>
{
    /// <summary>
    /// Identifiant du client
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Numéro de page (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Taille de page (maximum 100)
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Filtre par statut
    /// </summary>
    public PaymentStatus? Status { get; set; }

    /// <summary>
    /// Date de début (optionnelle)
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Date de fin (optionnelle)
    /// </summary>
    public DateTime? EndDate { get; set; }

    public GetPaymentsByCustomerQuery(Guid customerId)
    {
        CustomerId = customerId;
    }
}

/// <summary>
/// Handler pour récupérer un paiement par ID - NiesPro Enterprise Standard
/// </summary>
public class GetPaymentByIdQueryHandler : BaseQueryHandler<GetPaymentByIdQuery, ApiResponse<PaymentDetailDto>>,
    IRequestHandler<GetPaymentByIdQuery, ApiResponse<PaymentDetailDto>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogsServiceClient _logsService;

    public GetPaymentByIdQueryHandler(
        IPaymentRepository paymentRepository,
        ITransactionRepository transactionRepository,
        ILogsServiceClient logsService,
        ILogger<GetPaymentByIdQueryHandler> logger) : base(logger)
    {
        _paymentRepository = paymentRepository;
        _transactionRepository = transactionRepository;
        _logsService = logsService;
    }

    /// <summary>
    /// MediatR Handle method - délègue vers BaseQueryHandler
    /// </summary>
    public async Task<ApiResponse<PaymentDetailDto>> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
        => await HandleAsync(request, cancellationToken);

    /// <summary>
    /// Exécute la logique de récupération du paiement - NiesPro Enterprise Implementation
    /// </summary>
    protected override async Task<ApiResponse<PaymentDetailDto>> ExecuteAsync(GetPaymentByIdQuery query, CancellationToken cancellationToken)
    {
        try
        {
            Logger.LogInformation("Retrieving payment {PaymentId}", query.PaymentId);

            await _logsService.LogInformationAsync($"Retrieving payment: {query.PaymentId}", new Dictionary<string, object>
            {
                ["QueryId"] = query.QueryId,
                ["PaymentId"] = query.PaymentId,
                ["IncludeTransactions"] = query.IncludeTransactions,
                ["IncludeRefunds"] = query.IncludeRefunds
            });

            // Récupération du paiement
            var payment = await _paymentRepository.GetByIdAsync(query.PaymentId);
            if (payment == null)
            {
                await _logsService.LogWarningAsync($"Payment not found: {query.PaymentId}", new Dictionary<string, object>
                {
                    ["QueryId"] = query.QueryId,
                    ["PaymentId"] = query.PaymentId
                });

                return ApiResponse<PaymentDetailDto>.CreateError(
                    "Payment not found",
                    404
                );
            }

            // Construction du DTO
            var paymentDto = new PaymentDetailDto
            {
                Id = payment.Id,
                PaymentNumber = payment.PaymentNumber,
                OrderId = payment.OrderId,
                CustomerId = payment.CustomerId,
                MerchantId = payment.MerchantId,
                Amount = payment.Amount.Amount,
                Currency = payment.Amount.Currency.Code,
                Status = payment.Status,
                PaymentMethod = payment.Method,
                Description = payment.Description,
                ReturnUrl = payment.ReturnUrl,
                NotificationUrl = payment.WebhookUrl,
                Metadata = payment.Metadata.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value),
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt ?? DateTime.UtcNow
            };

            // Transactions associées (si demandées)
            if (query.IncludeTransactions)
            {
                var transactions = await _transactionRepository.GetByPaymentIdAsync(payment.Id);
                paymentDto.Transactions = transactions.Select(t => new TransactionDto
                {
                    Id = t.Id,
                    Type = t.Type,
                    Status = t.Status,
                    Amount = t.Amount.Amount,
                    Currency = t.Amount.Currency.Code,
                    ExternalReference = t.ExternalReference,
                    ProcessedAt = t.ProcessedAt ?? DateTime.UtcNow
                }).ToList();
            }

            // Remboursements (si demandés)
            if (query.IncludeRefunds)
            {
                var refunds = await _paymentRepository.GetRefundsByPaymentIdAsync(payment.Id);
                paymentDto.Refunds = refunds.Select(r => new RefundDto
                {
                    Id = r.Id,
                    Amount = r.Amount.Amount,
                    Currency = r.Amount.Currency.Code,
                    Reason = r.Reason,
                    Status = r.Status,
                    RefundDate = r.ProcessedAt ?? DateTime.UtcNow
                }).ToList();
            }

            await _logsService.LogInformationAsync($"Payment retrieved successfully: {payment.PaymentNumber}", new Dictionary<string, object>
            {
                ["QueryId"] = query.QueryId,
                ["PaymentId"] = payment.Id,
                ["PaymentNumber"] = payment.PaymentNumber,
                ["Status"] = payment.Status.ToString()
            });

            return ApiResponse<PaymentDetailDto>.CreateSuccess(
                paymentDto,
                "Payment retrieved successfully"
            );
        }
        catch (Exception ex)
        {
            await _logsService.LogErrorAsync(ex, $"Error retrieving payment: {query.PaymentId}", new Dictionary<string, object>
            {
                ["QueryId"] = query.QueryId,
                ["PaymentId"] = query.PaymentId,
                ["ErrorMessage"] = ex.Message
            });

            Logger.LogError(ex, "Error retrieving payment {PaymentId}", query.PaymentId);
            
            return ApiResponse<PaymentDetailDto>.CreateError(
                "Internal server error while retrieving payment",
                new[] { ex.Message }
            );
        }
    }
}

/// <summary>
/// Handler pour récupérer les paiements d'un client - NiesPro Enterprise Standard  
/// </summary>
public class GetPaymentsByCustomerQueryHandler : BaseQueryHandler<GetPaymentsByCustomerQuery, ApiResponse<PagedResult<PaymentSummaryDto>>>,
    IRequestHandler<GetPaymentsByCustomerQuery, ApiResponse<PagedResult<PaymentSummaryDto>>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogsServiceClient _logsService;

    public GetPaymentsByCustomerQueryHandler(
        IPaymentRepository paymentRepository,
        ILogsServiceClient logsService,
        ILogger<GetPaymentsByCustomerQueryHandler> logger) : base(logger)
    {
        _paymentRepository = paymentRepository;
        _logsService = logsService;
    }

    public async Task<ApiResponse<PagedResult<PaymentSummaryDto>>> Handle(GetPaymentsByCustomerQuery request, CancellationToken cancellationToken)
        => await HandleAsync(request, cancellationToken);

    protected override async Task<ApiResponse<PagedResult<PaymentSummaryDto>>> ExecuteAsync(GetPaymentsByCustomerQuery query, CancellationToken cancellationToken)
    {
        try
        {
            Logger.LogInformation("Retrieving payments for customer {CustomerId}", query.CustomerId);

            await _logsService.LogInformationAsync($"Retrieving payments for customer: {query.CustomerId}", new Dictionary<string, object>
            {
                ["QueryId"] = query.QueryId,
                ["CustomerId"] = query.CustomerId,
                ["Page"] = query.Page,
                ["PageSize"] = query.PageSize,
                ["Status"] = query.Status?.ToString() ?? "All",
                ["StartDate"] = query.StartDate?.ToString("yyyy-MM-dd") ?? "None",
                ["EndDate"] = query.EndDate?.ToString("yyyy-MM-dd") ?? "None"
            });

            // Validation des paramètres de pagination
            if (query.Page < 1) query.Page = 1;
            if (query.PageSize < 1 || query.PageSize > 100) query.PageSize = 20;

            // Récupération des paiements avec pagination
            var payments = await _paymentRepository.GetByCustomerIdAsync(
                query.CustomerId,
                query.Page,
                query.PageSize,
                query.Status,
                query.StartDate,
                query.EndDate
            );

            var totalCount = await _paymentRepository.GetCountByCustomerIdAsync(
                query.CustomerId,
                query.Status,
                query.StartDate,
                query.EndDate
            );

            // Conversion vers DTOs
            var paymentDtos = payments.Select(p => new PaymentSummaryDto
            {
                Id = p.Id,
                PaymentNumber = p.PaymentNumber,
                OrderId = p.OrderId,
                Amount = p.Amount.Amount,
                Currency = p.Amount.Currency.Code,
                Status = p.Status,
                PaymentMethod = p.Method,
                Description = p.Description,
                CreatedAt = p.CreatedAt
            }).ToList();

            // Résultat paginé
            var pagedResult = new PagedResult<PaymentSummaryDto>
            {
                Items = paymentDtos,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
            };

            await _logsService.LogInformationAsync($"Retrieved {paymentDtos.Count} payments for customer: {query.CustomerId}", new Dictionary<string, object>
            {
                ["QueryId"] = query.QueryId,
                ["CustomerId"] = query.CustomerId,
                ["ItemsCount"] = paymentDtos.Count,
                ["TotalCount"] = totalCount,
                ["Page"] = query.Page,
                ["TotalPages"] = pagedResult.TotalPages
            });

            return ApiResponse<PagedResult<PaymentSummaryDto>>.CreateSuccess(
                pagedResult,
                $"Retrieved {paymentDtos.Count} payments successfully"
            );
        }
        catch (Exception ex)
        {
            await _logsService.LogErrorAsync(ex, $"Error retrieving payments for customer: {query.CustomerId}", new Dictionary<string, object>
            {
                ["QueryId"] = query.QueryId,
                ["CustomerId"] = query.CustomerId,
                ["ErrorMessage"] = ex.Message
            });

            Logger.LogError(ex, "Error retrieving payments for customer {CustomerId}", query.CustomerId);
            
            return ApiResponse<PagedResult<PaymentSummaryDto>>.CreateError(
                "Internal server error while retrieving payments",
                new[] { ex.Message }
            );
        }
    }
}

/// <summary>
/// DTO pour les détails complets d'un paiement
/// </summary>
public class PaymentDetailDto
{
    public Guid Id { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid MerchantId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";
    public PaymentStatus Status { get; set; }
    public PaymentMethodType PaymentMethod { get; set; }
    public string? Description { get; set; }
    public string? ReturnUrl { get; set; }
    public string? NotificationUrl { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Relations optionnelles
    public List<TransactionDto>? Transactions { get; set; }
    public List<RefundDto>? Refunds { get; set; }
}

/// <summary>
/// DTO pour le résumé d'un paiement
/// </summary>
public class PaymentSummaryDto
{
    public Guid Id { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";
    public PaymentStatus Status { get; set; }
    public PaymentMethodType PaymentMethod { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO pour une transaction
/// </summary>
public class TransactionDto
{
    public Guid Id { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";
    public string? ExternalReference { get; set; }
    public DateTime ProcessedAt { get; set; }
}

/// <summary>
/// DTO pour un remboursement
/// </summary>
public class RefundDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";
    public string Reason { get; set; } = string.Empty;
    public RefundStatus Status { get; set; }
    public DateTime RefundDate { get; set; }
}

/// <summary>
/// Résultat paginé générique
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

/// <summary>
/// Query pour rechercher des paiements avec critères - NiesPro Enterprise Standard
/// </summary>
public class SearchPaymentsQuery : BaseQuery<ApiResponse<PagedResult<PaymentSummaryDto>>>
{
    public string? SearchTerm { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? MerchantId { get; set; }
    public PaymentStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public PaymentMethodType? PaymentMethod { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Query pour rechercher des transactions avec critères - NiesPro Enterprise Standard
/// </summary>
public class SearchTransactionsQuery : BaseQuery<ApiResponse<PagedResult<TransactionSummaryDto>>>
{
    public string? SearchTerm { get; set; }
    public Guid? PaymentId { get; set; }
    public Guid? CustomerId { get; set; }
    public TransactionStatus? Status { get; set; }
    public TransactionType? Type { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Handler pour la recherche de paiements - NiesPro Enterprise Standard
/// </summary>
public class SearchPaymentsQueryHandler : IRequestHandler<SearchPaymentsQuery, ApiResponse<PagedResult<PaymentSummaryDto>>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogsServiceClient _logsService;

    public SearchPaymentsQueryHandler(
        IPaymentRepository paymentRepository,
        ILogsServiceClient logsService)
    {
        _paymentRepository = paymentRepository;
        _logsService = logsService;
    }

    public async Task<ApiResponse<PagedResult<PaymentSummaryDto>>> Handle(SearchPaymentsQuery request, CancellationToken cancellationToken)
    {
        var searchResult = await _paymentRepository.SearchAsync(
            searchTerm: request.SearchTerm,
            customerId: request.CustomerId,
            merchantId: request.MerchantId,
            fromDate: request.StartDate,
            toDate: request.EndDate,
            pageNumber: request.Page,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        var paymentDtos = searchResult.payments.Select(p => new PaymentSummaryDto
        {
            Id = p.Id,
            PaymentNumber = p.PaymentNumber,
            Amount = p.Amount.Amount,
            Currency = p.Amount.Currency.Code,
            Status = p.Status,
            PaymentMethod = p.Method,
            Description = p.Description,
            CreatedAt = p.CreatedAt
        }).ToList();

        var result = new PagedResult<PaymentSummaryDto>
        {
            Items = paymentDtos,
            TotalCount = searchResult.totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)searchResult.totalCount / request.PageSize)
        };

        return ApiResponse<PagedResult<PaymentSummaryDto>>.CreateSuccess(result, "Payments search completed successfully");
    }
}

/// <summary>
/// Handler pour la recherche de transactions - NiesPro Enterprise Standard
/// </summary>
public class SearchTransactionsQueryHandler : IRequestHandler<SearchTransactionsQuery, ApiResponse<PagedResult<TransactionSummaryDto>>>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogsServiceClient _logsService;

    public SearchTransactionsQueryHandler(
        ITransactionRepository transactionRepository,
        ILogsServiceClient logsService)
    {
        _transactionRepository = transactionRepository;
        _logsService = logsService;
    }

    public async Task<ApiResponse<PagedResult<TransactionSummaryDto>>> Handle(SearchTransactionsQuery request, CancellationToken cancellationToken)
    {
        // TODO: Implémenter la recherche de transactions dans le repository
        var transactions = new List<TransactionSummaryDto>();
        var totalCount = 0;

        var result = new PagedResult<TransactionSummaryDto>
        {
            Items = transactions,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };

        return ApiResponse<PagedResult<TransactionSummaryDto>>.CreateSuccess(result, "Transactions search completed successfully");
    }
}

/// <summary>
/// DTO pour résumé de transaction
/// </summary>
public class TransactionSummaryDto
{
    public Guid Id { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public Guid PaymentId { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";
    public string? ExternalReference { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}