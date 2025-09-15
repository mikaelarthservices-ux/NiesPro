using MediatR;
using AutoMapper;
using Payment.Application.Queries;
using Payment.Application.DTOs;
using Payment.Application.Mappings;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Payment.Application.Handlers;

/// <summary>
/// Handler pour récupérer un paiement par ID
/// </summary>
public class GetPaymentByIdHandler : IRequestHandler<GetPaymentByIdQuery, PaymentDetailDto?>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPaymentByIdHandler> _logger;

    public GetPaymentByIdHandler(
        IPaymentRepository paymentRepository,
        ITransactionRepository transactionRepository,
        IMapper mapper,
        ILogger<GetPaymentByIdHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _transactionRepository = transactionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaymentDetailDto?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting payment {PaymentId}", request.PaymentId);

            var payment = await _paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken);
            if (payment == null)
            {
                return null;
            }

            var dto = _mapper.Map<PaymentDetailDto>(payment);

            // Charger les transactions si demandé
            if (request.IncludeTransactions)
            {
                var transactions = await _transactionRepository.GetByPaymentIdAsync(payment.Id, cancellationToken);
                dto.Transactions = _mapper.Map<List<TransactionSummaryDto>>(transactions);
            }

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment {PaymentId}", request.PaymentId);
            return null;
        }
    }
}

/// <summary>
/// Handler pour récupérer un paiement par numéro
/// </summary>
public class GetPaymentByNumberHandler : IRequestHandler<GetPaymentByNumberQuery, PaymentDetailDto?>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPaymentByNumberHandler> _logger;

    public GetPaymentByNumberHandler(
        IPaymentRepository paymentRepository,
        ITransactionRepository transactionRepository,
        IMapper mapper,
        ILogger<GetPaymentByNumberHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _transactionRepository = transactionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaymentDetailDto?> Handle(GetPaymentByNumberQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting payment by number {PaymentNumber}", request.PaymentNumber);

            var payment = await _paymentRepository.GetByNumberAsync(request.PaymentNumber, cancellationToken);
            if (payment == null)
            {
                return null;
            }

            var dto = _mapper.Map<PaymentDetailDto>(payment);

            if (request.IncludeTransactions)
            {
                var transactions = await _transactionRepository.GetByPaymentIdAsync(payment.Id, cancellationToken);
                dto.Transactions = _mapper.Map<List<TransactionSummaryDto>>(transactions);
            }

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment by number {PaymentNumber}", request.PaymentNumber);
            return null;
        }
    }
}

/// <summary>
/// Handler pour récupérer les paiements d'un client
/// </summary>
public class GetPaymentsByCustomerHandler : IRequestHandler<GetPaymentsByCustomerQuery, Payment.Application.DTOs.PagedResult<PaymentSummaryDto>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPaymentsByCustomerHandler> _logger;

    public GetPaymentsByCustomerHandler(
        IPaymentRepository paymentRepository,
        IMapper mapper,
        ILogger<GetPaymentsByCustomerHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Payment.Application.DTOs.PagedResult<PaymentSummaryDto>> Handle(GetPaymentsByCustomerQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting payments for customer {CustomerId}, page {Page}", 
                request.CustomerId, request.Page);

            var (payments, totalCount) = await _paymentRepository.GetByCustomerIdPagedAsync(
                request.CustomerId,
                request.Page,
                request.PageSize,
                request.Status,
                request.FromDate,
                request.ToDate,
                request.MinAmount,
                request.MaxAmount,
                request.Currency,
                cancellationToken);

            var dtos = _mapper.Map<List<PaymentSummaryDto>>(payments);

            return new Payment.Application.DTOs.PagedResult<PaymentSummaryDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = request.Page,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payments for customer {CustomerId}", request.CustomerId);
            return new Payment.Application.DTOs.PagedResult<PaymentSummaryDto>
            {
                Items = new List<PaymentSummaryDto>(),
                TotalCount = 0,
                PageNumber = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}

/// <summary>
/// Handler pour récupérer les statistiques de paiement d'un commerçant
/// </summary>
public class GetMerchantPaymentStatsHandler : IRequestHandler<GetMerchantPaymentStatsQuery, MerchantPaymentStatsDto>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMerchantPaymentStatsHandler> _logger;

    public GetMerchantPaymentStatsHandler(
        IPaymentRepository paymentRepository,
        ITransactionRepository transactionRepository,
        IMapper mapper,
        ILogger<GetMerchantPaymentStatsHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _transactionRepository = transactionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<MerchantPaymentStatsDto> Handle(GetMerchantPaymentStatsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting payment stats for merchant {MerchantId} from {FromDate} to {ToDate}", 
                request.MerchantId, request.FromDate, request.ToDate);

            // Récupérer tous les paiements de la période
            var payments = await _paymentRepository.GetByMerchantIdAndDateRangeAsync(
                request.MerchantId,
                request.FromDate,
                request.ToDate,
                cancellationToken);

            // Calculer les statistiques globales
            var totalPayments = payments.Count;
            var successfulPayments = payments.Count(p => p.Status == PaymentStatus.Captured || p.Status == PaymentStatus.Settled);
            var failedPayments = payments.Count(p => p.Status == PaymentStatus.Failed);
            var cancelledPayments = payments.Count(p => p.Status == PaymentStatus.Cancelled);

            var successfulAmount = payments
                .Where(p => p.Status == PaymentStatus.Captured || p.Status == PaymentStatus.Settled)
                .Sum(p => p.Amount.Amount);

            var refundedAmount = payments.Sum(p => p.GetRefundedAmount().Amount);
            var netAmount = successfulAmount - refundedAmount;

            // Calculer les frais totaux
            var totalFees = payments
                .Where(p => p.ProcessingFees != null)
                .Sum(p => p.ProcessingFees!.Amount);

            // Statistiques par type de moyen de paiement
            var paymentMethodStats = MappingExtensions.CreatePaymentMethodStats(payments);

            // Statistiques par période
            var periodStats = MappingExtensions.CreatePeriodStats(payments, request.FromDate, request.ToDate, request.GroupBy);

            return new MerchantPaymentStatsDto
            {
                MerchantId = request.MerchantId,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Currency = request.Currency,
                TotalPayments = totalPayments,
                SuccessfulPayments = successfulPayments,
                FailedPayments = failedPayments,
                CancelledPayments = cancelledPayments,
                SuccessRate = totalPayments > 0 ? (decimal)successfulPayments / totalPayments * 100 : 0,
                TotalAmount = payments.Sum(p => p.Amount.Amount),
                SuccessfulAmount = successfulAmount,
                RefundedAmount = refundedAmount,
                NetAmount = netAmount,
                AveragePaymentAmount = successfulPayments > 0 ? successfulAmount / successfulPayments : 0,
                TotalFees = totalFees,
                PaymentMethodStats = paymentMethodStats,
                PeriodStats = periodStats
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment stats for merchant {MerchantId}", request.MerchantId);
            return new MerchantPaymentStatsDto
            {
                MerchantId = request.MerchantId,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Currency = request.Currency
            };
        }
    }
}

/// <summary>
/// Handler pour rechercher des paiements
/// </summary>
public class SearchPaymentsHandler : IRequestHandler<SearchPaymentsQuery, Payment.Application.DTOs.PagedResult<PaymentSummaryDto>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SearchPaymentsHandler> _logger;

    public SearchPaymentsHandler(
        IPaymentRepository paymentRepository,
        IMapper mapper,
        ILogger<SearchPaymentsHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Payment.Application.DTOs.PagedResult<PaymentSummaryDto>> Handle(SearchPaymentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Searching payments with term '{SearchTerm}', page {Page}", 
                request.SearchTerm, request.Page);

            var (payments, totalCount) = await _paymentRepository.SearchAsync(
                request.SearchTerm,
                request.MerchantId,
                request.CustomerId,
                request.Statuses,
                request.PaymentMethodTypes,
                request.FromDate,
                request.ToDate,
                request.MinAmount,
                request.MaxAmount,
                request.Currency,
                request.Page,
                request.PageSize,
                request.SortBy,
                request.SortOrder,
                cancellationToken);

            var dtos = _mapper.Map<List<PaymentSummaryDto>>(payments);

            return new Payment.Application.DTOs.PagedResult<PaymentSummaryDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = request.Page,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching payments");
            return new Payment.Application.DTOs.PagedResult<PaymentSummaryDto>
            {
                Items = new List<PaymentSummaryDto>(),
                TotalCount = 0,
                PageNumber = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}

/// <summary>
/// Handler pour récupérer une transaction par ID
/// </summary>
public class GetTransactionByIdHandler : IRequestHandler<GetTransactionByIdQuery, TransactionDetailDto?>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTransactionByIdHandler> _logger;

    public GetTransactionByIdHandler(
        ITransactionRepository transactionRepository,
        IMapper mapper,
        ILogger<GetTransactionByIdHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TransactionDetailDto?> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting transaction {TransactionId}", request.TransactionId);

            var transaction = await _transactionRepository.GetByIdWithDetailsAsync(
                request.TransactionId, 
                cancellationToken);

            if (transaction == null)
            {
                return null;
            }

            return _mapper.Map<TransactionDetailDto>(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction {TransactionId}", request.TransactionId);
            return null;
        }
    }
}

/// <summary>
/// Handler pour récupérer les moyens de paiement d'un client
/// </summary>
public class GetPaymentMethodsByCustomerHandler : IRequestHandler<GetPaymentMethodsByCustomerQuery, List<PaymentMethodDto>>
{
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPaymentMethodsByCustomerHandler> _logger;

    public GetPaymentMethodsByCustomerHandler(
        IPaymentMethodRepository paymentMethodRepository,
        ITransactionRepository transactionRepository,
        IMapper mapper,
        ILogger<GetPaymentMethodsByCustomerHandler> logger)
    {
        _paymentMethodRepository = paymentMethodRepository;
        _transactionRepository = transactionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<PaymentMethodDto>> Handle(GetPaymentMethodsByCustomerQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting payment methods for customer {CustomerId}", request.CustomerId);

            var paymentMethods = await _paymentMethodRepository.GetByCustomerIdAsync(
                request.CustomerId, 
                cancellationToken);

            return _mapper.Map<List<PaymentMethodDto>>(paymentMethods);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment methods for customer {CustomerId}", request.CustomerId);
            return new List<PaymentMethodDto>();
        }
    }
}

/// <summary>
/// Handler pour récupérer un moyen de paiement par ID
/// </summary>
public class GetPaymentMethodByIdHandler : IRequestHandler<GetPaymentMethodByIdQuery, PaymentMethodDetailDto?>
{
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPaymentMethodByIdHandler> _logger;

    public GetPaymentMethodByIdHandler(
        IPaymentMethodRepository paymentMethodRepository,
        ITransactionRepository transactionRepository,
        IMapper mapper,
        ILogger<GetPaymentMethodByIdHandler> logger)
    {
        _paymentMethodRepository = paymentMethodRepository;
        _transactionRepository = transactionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaymentMethodDetailDto?> Handle(GetPaymentMethodByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting payment method {PaymentMethodId}", request.PaymentMethodId);

            var paymentMethod = await _paymentMethodRepository.GetByIdAsync(request.PaymentMethodId, cancellationToken);
            if (paymentMethod == null)
            {
                return null;
            }

            var dto = _mapper.Map<PaymentMethodDetailDto>(paymentMethod);

            // Calculer les statistiques d'utilisation si demandé
            if (request.IncludeUsageStats)
            {
                var transactions = await _transactionRepository.GetByPaymentMethodIdAsync(paymentMethod.Id, cancellationToken);
                dto.UsageStats = MappingExtensions.CalculateUsageStats(paymentMethod, transactions);
            }

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment method {PaymentMethodId}", request.PaymentMethodId);
            return null;
        }
    }
}