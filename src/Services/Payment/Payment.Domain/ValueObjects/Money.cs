namespace Payment.Domain.ValueObjects;

/// <summary>
/// Value Object représentant un montant monétaire sécurisé
/// Utilisé pour les transactions financières avec validation stricte
/// </summary>
public sealed record Money
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public Money(decimal amount, Currency currency)
    {
        if (amount < 0)
            throw new ArgumentException("Le montant ne peut pas être négatif", nameof(amount));
        
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
        Amount = Math.Round(amount, currency.DecimalPlaces, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Constructeur avec code de devise
    /// </summary>
    public Money(decimal amount, string currencyCode)
        : this(amount, Currency.FromCode(currencyCode))
    {
    }

    /// <summary>
    /// Créer un montant zéro dans la devise spécifiée
    /// </summary>
    public static Money Zero(string currencyCode) => new(0, currencyCode);

    /// <summary>
    /// Créer un montant zéro dans la devise spécifiée
    /// </summary>
    public static Money Zero(Currency currency) => new(0, currency);

    /// <summary>
    /// Additionner deux montants de même devise
    /// </summary>
    public Money Add(Money other)
    {
        ValidateSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Soustraire un montant
    /// </summary>
    public Money Subtract(Money other)
    {
        ValidateSameCurrency(other);
        var result = Amount - other.Amount;
        
        if (result < 0)
            throw new InvalidOperationException("Le résultat de la soustraction ne peut pas être négatif");
            
        return new Money(result, Currency);
    }

    /// <summary>
    /// Multiplier par un coefficient
    /// </summary>
    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new ArgumentException("Le facteur ne peut pas être négatif", nameof(factor));
            
        return new Money(Amount * factor, Currency);
    }

    /// <summary>
    /// Appliquer un pourcentage de réduction
    /// </summary>
    public Money ApplyDiscount(decimal discountPercent)
    {
        if (discountPercent < 0 || discountPercent > 100)
            throw new ArgumentException("Le pourcentage doit être entre 0 et 100", nameof(discountPercent));
            
        var discountAmount = Amount * (discountPercent / 100);
        return new Money(Amount - discountAmount, Currency);
    }

    /// <summary>
    /// Diviser le montant en parts égales
    /// </summary>
    public IEnumerable<Money> Split(int parts)
    {
        if (parts <= 0)
            throw new ArgumentException("Le nombre de parts doit être positif", nameof(parts));

        var baseAmount = Amount / parts;
        var remainder = Amount % parts;

        for (int i = 0; i < parts; i++)
        {
            var amount = i < remainder ? baseAmount + 0.01m : baseAmount;
            yield return new Money(amount, Currency);
        }
    }

    /// <summary>
    /// Vérifier si le montant est supérieur à un autre
    /// </summary>
    public bool IsGreaterThan(Money other)
    {
        ValidateSameCurrency(other);
        return Amount > other.Amount;
    }

    /// <summary>
    /// Vérifier si le montant est suffisant pour un paiement
    /// </summary>
    public bool IsSufficientFor(Money required)
    {
        ValidateSameCurrency(required);
        return Amount >= required.Amount;
    }

    private void ValidateSameCurrency(Money other)
    {
        if (Currency.Code != other.Currency.Code)
            throw new InvalidOperationException($"Impossible d'opérer sur des devises différentes: {Currency.Code} vs {other.Currency.Code}");
    }

    public override string ToString() => $"{Amount.ToString($"F{Currency.DecimalPlaces}")} {Currency.Code}";

    // Opérateurs pour faciliter les calculs
    public static Money operator +(Money left, Money right)
    {
        left.ValidateSameCurrency(right);
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        left.ValidateSameCurrency(right);
        return new Money(left.Amount - right.Amount, left.Currency);
    }

    public static bool operator >(Money left, Money right)
    {
        left.ValidateSameCurrency(right);
        return left.Amount > right.Amount;
    }

    public static bool operator <(Money left, Money right)
    {
        left.ValidateSameCurrency(right);
        return left.Amount < right.Amount;
    }

    public static bool operator >=(Money left, Money right)
    {
        left.ValidateSameCurrency(right);
        return left.Amount >= right.Amount;
    }

    public static bool operator <=(Money left, Money right)
    {
        left.ValidateSameCurrency(right);
        return left.Amount <= right.Amount;
    }

    /// <summary>
    /// Convertir vers une devise différente (nécessite un taux de change)
    /// </summary>
    public Money ConvertTo(string targetCurrency, decimal exchangeRate)
    {
        if (string.IsNullOrWhiteSpace(targetCurrency))
            throw new ArgumentException("La devise cible ne peut pas être vide", nameof(targetCurrency));
            
        if (exchangeRate <= 0)
            throw new ArgumentException("Le taux de change doit être positif", nameof(exchangeRate));

        if (Currency.Code == targetCurrency.ToUpperInvariant())
            return this;

        return new Money(Amount * exchangeRate, targetCurrency);
    }
}