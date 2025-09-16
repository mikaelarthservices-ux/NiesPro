using AutoMapper;
using Payment.Application.DTOs;
using Payment.Application.Utilities;
using Payment.Domain.Entities;
using Payment.Domain.ValueObjects;
using Payment.Domain.Enums;

namespace Payment.Application.Mappings;

/// <summary>
/// Profil de mapping AutoMapper pour les entités Payment
/// </summary>
public class PaymentMappingProfile : Profile
{
    public PaymentMappingProfile()
    {
        // Mapping Payment vers DTOs
        CreateMap<Domain.Entities.Payment, PaymentDetailDto>()
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Amount.Currency.Code))
            .ForMember(dest => dest.PaidAmount, opt => opt.MapFrom(src => src.GetPaidAmount().Amount))
            .ForMember(dest => dest.RefundedAmount, opt => opt.MapFrom(src => src.GetRefundedAmount().Amount))
            .ForMember(dest => dest.NetAmount, opt => opt.MapFrom(src => src.GetNetAmount().Amount))
            .ForMember(dest => dest.ProcessingFees, opt => opt.MapFrom(src => src.ProcessingFees != null ? src.ProcessingFees.Amount : (decimal?)null))
            .ForMember(dest => dest.FeeMode, opt => opt.MapFrom(src => src.FeeMode.ToString()))
            .ForMember(dest => dest.MinimumPartialAmount, opt => opt.MapFrom(src => src.MinimumPartialAmount != null ? src.MinimumPartialAmount.Amount : (decimal?)null))
            .ForMember(dest => dest.LastPaymentMethod, opt => opt.MapFrom(src => src.LastPaymentMethod))
            .ForMember(dest => dest.Transactions, opt => opt.MapFrom(src => src.Transactions))
            .ForMember(dest => dest.SessionData, opt => opt.MapFrom(src => src.SessionData));

        CreateMap<Domain.Entities.Payment, PaymentSummaryDto>()
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Amount.Currency.Code))
            .ForMember(dest => dest.PaidAmount, opt => opt.MapFrom(src => src.GetPaidAmount().Amount))
            .ForMember(dest => dest.LastPaymentMethodType, opt => opt.MapFrom(src => src.LastPaymentMethod != null ? src.LastPaymentMethod.Type : (PaymentMethodType?)null));

        // Mapping Transaction vers DTOs
        CreateMap<Transaction, TransactionDetailDto>()
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Amount.Currency.Code))
            .ForMember(dest => dest.Fees, opt => opt.MapFrom(src => src.Fees != null ? src.Fees.Amount : (decimal?)null))
            .ForMember(dest => dest.PaymentId, opt => opt.MapFrom(src => src.PaymentMethod != null ? (Guid?)null : null)) // À ajuster selon la relation
            .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod))
            .ForMember(dest => dest.ChildTransactions, opt => opt.MapFrom(src => src.ChildTransactions))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.Metadata));

        CreateMap<Transaction, TransactionSummaryDto>()
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Amount.Currency.Code))
            .ForMember(dest => dest.PaymentMethodType, opt => opt.MapFrom(src => src.PaymentMethod != null ? src.PaymentMethod.Type : (PaymentMethodType?)null));

        // Mapping PaymentMethod vers DTOs
        CreateMap<PaymentMethod, PaymentMethodDetailDto>()
            .ForMember(dest => dest.SecureDisplayName, opt => opt.MapFrom(src => src.GetSecureDisplayName()))
            .ForMember(dest => dest.DailyLimit, opt => opt.MapFrom(src => src.DailyLimit != null ? src.DailyLimit.Amount : (decimal?)null))
            .ForMember(dest => dest.TransactionLimit, opt => opt.MapFrom(src => src.TransactionLimit != null ? src.TransactionLimit.Amount : (decimal?)null))
            .ForMember(dest => dest.CreditCard, opt => opt.MapFrom(src => src.CreditCard))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.Metadata))
            .ForMember(dest => dest.UsageStats, opt => opt.Ignore()); // Calculé séparément

        CreateMap<PaymentMethod, PaymentMethodDto>()
            .ForMember(dest => dest.SecureDisplayName, opt => opt.MapFrom(src => src.GetSecureDisplayName()))
            .ForMember(dest => dest.LastFourDigits, opt => opt.Ignore()) // À calculer manuellement après mapping
            .ForMember(dest => dest.CardBrand, opt => opt.MapFrom(src => src.CreditCard != null ? src.CreditCard.Brand.ToString() : null));

        CreateMap<PaymentMethod, PaymentMethodSummaryDto>()
            .ForMember(dest => dest.SecureDisplayName, opt => opt.MapFrom(src => src.GetSecureDisplayName()));

        // Mapping CreditCard vers DTO
        CreateMap<CreditCard, CreditCardInfoDto>()
            .ForMember(dest => dest.LastFourDigits, opt => opt.Ignore()) // À calculer manuellement après mapping
            .ForMember(dest => dest.Token, opt => opt.MapFrom(src => src.Token));

        // Mapping Money vers decimal (utilisé implicitement)
        CreateMap<Money, decimal>()
            .ConvertUsing(src => src.Amount);
    }
}

/// <summary>
/// Extensions pour faciliter les mappings personnalisés
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// Mapper un Money vers un montant/devise séparés (TODO: Corriger le mapping AutoMapper)
    /// </summary>
    /*
    public static void MapMoney<TSource, TDest>(this IMappingExpression<TSource, TDest> mapping,
        System.Linq.Expressions.Expression<System.Func<TDest, decimal>> amountProperty,
        System.Linq.Expressions.Expression<System.Func<TDest, string>> currencyProperty,
        System.Linq.Expressions.Expression<System.Func<TSource, Money?>> sourceProperty)
    {
        mapping.ForMember(amountProperty, opt => opt.MapFrom(src => 
            sourceProperty.Compile()(src) != null ? sourceProperty.Compile()(src)!.Amount : 0));
        mapping.ForMember(currencyProperty, opt => opt.MapFrom(src => 
            sourceProperty.Compile()(src) != null ? sourceProperty.Compile()(src)!.Currency : string.Empty));
    }
    */

    /// <summary>
    /// Créer un Money à partir de montant et devise
    /// </summary>
    public static Money CreateMoney(decimal amount, string currency)
    {
        return new Money(amount, currency);
    }

    /// <summary>
    /// Calculer les statistiques d'utilisation d'un moyen de paiement
    /// </summary>
    public static PaymentMethodUsageStatsDto CalculateUsageStats(PaymentMethod paymentMethod, List<Transaction> transactions)
    {
        var relevantTransactions = transactions.Where(t => t.PaymentMethodId == paymentMethod.Id).ToList();
        var successfulTransactions = relevantTransactions.Where(t => t.Status.IsEquivalentTo(PaymentStatus.Captured) || t.Status.IsEquivalentTo(PaymentStatus.Settled)).ToList();

        return new PaymentMethodUsageStatsDto
        {
            TotalTransactions = relevantTransactions.Count,
            SuccessfulTransactions = successfulTransactions.Count,
            SuccessRate = relevantTransactions.Count > 0 ? (decimal)successfulTransactions.Count / relevantTransactions.Count * 100 : 0,
            TotalAmount = successfulTransactions.Sum(t => t.Amount.Amount),
            AverageTransactionAmount = successfulTransactions.Count > 0 ? successfulTransactions.Average(t => t.Amount.Amount) : 0,
            LastUsedAt = relevantTransactions.Max(t => (DateTime?)t.CreatedAt)
        };
    }

    /// <summary>
    /// Créer des statistiques par période
    /// </summary>
    public static List<PeriodStatsDto> CreatePeriodStats(List<Domain.Entities.Payment> payments, DateTime fromDate, DateTime toDate, StatsPeriod period)
    {
        var stats = new List<PeriodStatsDto>();
        var current = fromDate;

        while (current <= toDate)
        {
            var nextPeriod = period switch
            {
                StatsPeriod.Today => current.AddDays(1),
                StatsPeriod.ThisWeek => current.AddDays(7),
                StatsPeriod.ThisMonth => current.AddMonths(1),
                StatsPeriod.ThisYear => current.AddYears(1),
                StatsPeriod.Last7Days => current.AddDays(1),
                _ => current.AddDays(1)
            };

            var periodPayments = payments.Where(p => p.CreatedAt >= current && p.CreatedAt < nextPeriod).ToList();
            var successfulPayments = periodPayments.Where(p => p.Status == PaymentStatus.Captured || p.Status == PaymentStatus.Settled).ToList();

            stats.Add(new PeriodStatsDto
            {
                Date = current,
                PaymentCount = periodPayments.Count,
                Amount = successfulPayments.Sum(p => p.Amount.Amount),
                SuccessRate = periodPayments.Count > 0 ? (decimal)successfulPayments.Count / periodPayments.Count * 100 : 0
            });

            current = nextPeriod;
        }

        return stats;
    }

    /// <summary>
    /// Créer des statistiques par type de moyen de paiement
    /// </summary>
    public static List<PaymentMethodStatsDto> CreatePaymentMethodStats(List<Domain.Entities.Payment> payments)
    {
        var stats = new List<PaymentMethodStatsDto>();
        var totalAmount = payments.Where(p => p.Status == PaymentStatus.Captured || p.Status == PaymentStatus.Settled)
                                 .Sum(p => p.Amount.Amount);

        var groupedByMethod = payments
            .Where(p => p.LastPaymentMethod != null)
            .GroupBy(p => p.LastPaymentMethod!.Type)
            .ToList();

        foreach (var group in groupedByMethod)
        {
            var groupPayments = group.ToList();
            var successfulPayments = groupPayments.Where(p => p.Status == PaymentStatus.Captured || p.Status == PaymentStatus.Settled).ToList();
            var methodAmount = successfulPayments.Sum(p => p.Amount.Amount);

            stats.Add(new PaymentMethodStatsDto
            {
                Type = group.Key,
                Count = groupPayments.Count,
                Amount = methodAmount,
                Percentage = totalAmount > 0 ? methodAmount / totalAmount * 100 : 0,
                SuccessRate = groupPayments.Count > 0 ? (decimal)successfulPayments.Count / groupPayments.Count * 100 : 0
            });
        }

        return stats.OrderByDescending(s => s.Amount).ToList();
    }

    /// <summary>
    /// Obtenir les 4 derniers chiffres d'une méthode de paiement
    /// </summary>
    public static string? GetLastFourDigitsForPaymentMethod(PaymentMethod paymentMethod)
    {
        if (paymentMethod.CreditCard == null)
            return null;

        var maskedNumber = paymentMethod.CreditCard.GetMaskedNumber();
        if (string.IsNullOrEmpty(maskedNumber))
            return null;

        return maskedNumber.Length >= 4 ? maskedNumber.Substring(maskedNumber.Length - 4) : maskedNumber;
    }

    /// <summary>
    /// Obtenir les 4 derniers chiffres d'une carte de crédit
    /// </summary>
    public static string? GetLastFourDigitsForCreditCard(CreditCard creditCard)
    {
        var maskedNumber = creditCard.GetMaskedNumber();
        if (string.IsNullOrEmpty(maskedNumber))
            return null;

        return maskedNumber.Length >= 4 ? maskedNumber.Substring(maskedNumber.Length - 4) : maskedNumber;
    }
}