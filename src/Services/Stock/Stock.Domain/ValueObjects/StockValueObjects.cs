namespace Stock.Domain.ValueObjects;

/// <summary>
/// Value Object représentant une quantité de stock avec unité
/// </summary>
public sealed record StockQuantity
{
    public decimal Value { get; }
    public string Unit { get; }

    public StockQuantity(decimal value, string unit = "UNIT")
    {
        if (value < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(value));
        
        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Unit cannot be null or empty", nameof(unit));

        Value = value;
        Unit = unit.ToUpperInvariant();
    }

    /// <summary>
    /// Créer une quantité zéro
    /// </summary>
    public static StockQuantity Zero(string unit = "UNIT") => new(0, unit);

    /// <summary>
    /// Additionner deux quantités (même unité)
    /// </summary>
    public StockQuantity Add(StockQuantity other)
    {
        if (Unit != other.Unit)
            throw new InvalidOperationException($"Cannot add different units: {Unit} and {other.Unit}");

        return new StockQuantity(Value + other.Value, Unit);
    }

    /// <summary>
    /// Soustraire deux quantités (même unité)
    /// </summary>
    public StockQuantity Subtract(StockQuantity other)
    {
        if (Unit != other.Unit)
            throw new InvalidOperationException($"Cannot subtract different units: {Unit} and {other.Unit}");

        var result = Value - other.Value;
        if (result < 0)
            throw new InvalidOperationException("Result cannot be negative");

        return new StockQuantity(result, Unit);
    }

    /// <summary>
    /// Multiplier par un facteur
    /// </summary>
    public StockQuantity Multiply(decimal factor)
    {
        if (factor < 0)
            throw new ArgumentException("Factor cannot be negative", nameof(factor));

        return new StockQuantity(Value * factor, Unit);
    }

    /// <summary>
    /// Vérifier si la quantité est suffisante
    /// </summary>
    public bool IsSufficientFor(StockQuantity required)
    {
        if (Unit != required.Unit)
            throw new InvalidOperationException($"Cannot compare different units: {Unit} and {required.Unit}");

        return Value >= required.Value;
    }

    /// <summary>
    /// Vérifier si c'est zéro
    /// </summary>
    public bool IsZero => Value == 0;

    /// <summary>
    /// Vérifier si c'est positif
    /// </summary>
    public bool IsPositive => Value > 0;

    public override string ToString() => $"{Value:F2} {Unit}";

    // Opérateurs
    public static StockQuantity operator +(StockQuantity left, StockQuantity right) => left.Add(right);
    public static StockQuantity operator -(StockQuantity left, StockQuantity right) => left.Subtract(right);
    public static StockQuantity operator *(StockQuantity quantity, decimal factor) => quantity.Multiply(factor);
    public static bool operator >(StockQuantity left, StockQuantity right) => left.Value > right.Value && left.Unit == right.Unit;
    public static bool operator <(StockQuantity left, StockQuantity right) => left.Value < right.Value && left.Unit == right.Unit;
    public static bool operator >=(StockQuantity left, StockQuantity right) => left.Value >= right.Value && left.Unit == right.Unit;
    public static bool operator <=(StockQuantity left, StockQuantity right) => left.Value <= right.Value && left.Unit == right.Unit;
}

/// <summary>
/// Value Object représentant un coût unitaire avec devise
/// </summary>
public sealed record UnitCost
{
    public decimal Amount { get; }
    public string Currency { get; }

    public UnitCost(decimal amount, string currency = "EUR")
    {
        if (amount < 0)
            throw new ArgumentException("Unit cost cannot be negative", nameof(amount));
        
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be null or empty", nameof(currency));

        Amount = Math.Round(amount, 4, MidpointRounding.AwayFromZero);
        Currency = currency.ToUpperInvariant();
    }

    /// <summary>
    /// Créer un coût zéro
    /// </summary>
    public static UnitCost Zero(string currency = "EUR") => new(0, currency);

    /// <summary>
    /// Calculer le coût total pour une quantité
    /// </summary>
    public Money CalculateTotal(StockQuantity quantity)
    {
        return new Money(Amount * quantity.Value, Currency);
    }

    /// <summary>
    /// Additionner deux coûts (même devise)
    /// </summary>
    public UnitCost Add(UnitCost other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add different currencies: {Currency} and {other.Currency}");

        return new UnitCost(Amount + other.Amount, Currency);
    }

    public override string ToString() => $"{Amount:C} {Currency}";
}

/// <summary>
/// Value Object représentant un montant monétaire
/// </summary>
public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = "EUR")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be null or empty", nameof(currency));

        Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency.ToUpperInvariant();
    }

    /// <summary>
    /// Créer un montant zéro
    /// </summary>
    public static Money Zero(string currency = "EUR") => new(0, currency);

    /// <summary>
    /// Additionner deux montants (même devise)
    /// </summary>
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add different currencies: {Currency} and {other.Currency}");

        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Soustraire deux montants (même devise)
    /// </summary>
    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot subtract different currencies: {Currency} and {other.Currency}");

        var result = Amount - other.Amount;
        if (result < 0)
            throw new InvalidOperationException("Result cannot be negative");

        return new Money(result, Currency);
    }

    public override string ToString() => $"{Amount:C} {Currency}";

    // Opérateurs
    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
}

/// <summary>
/// Value Object représentant une adresse d'emplacement
/// </summary>
public sealed record LocationAddress
{
    public string Street { get; }
    public string? AdditionalInfo { get; }
    public string City { get; }
    public string PostalCode { get; }
    public string Country { get; }

    public LocationAddress(string street, string city, string postalCode, string country, string? additionalInfo = null)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be null or empty", nameof(street));
        
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be null or empty", nameof(city));
        
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code cannot be null or empty", nameof(postalCode));
        
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be null or empty", nameof(country));

        Street = street.Trim();
        City = city.Trim();
        PostalCode = postalCode.Trim();
        Country = country.Trim();
        AdditionalInfo = additionalInfo?.Trim();
    }

    /// <summary>
    /// Formatage complet de l'adresse
    /// </summary>
    public string GetFullAddress()
    {
        var parts = new List<string> { Street };
        
        if (!string.IsNullOrEmpty(AdditionalInfo))
            parts.Add(AdditionalInfo);
            
        parts.AddRange(new[] { City, PostalCode, Country });
        
        return string.Join(", ", parts);
    }

    public override string ToString() => GetFullAddress();
}

/// <summary>
/// Value Object représentant une période de validité
/// </summary>
public sealed record ValidityPeriod
{
    public DateTime StartDate { get; }
    public DateTime? EndDate { get; }

    public ValidityPeriod(DateTime startDate, DateTime? endDate = null)
    {
        if (endDate.HasValue && endDate.Value <= startDate)
            throw new ArgumentException("End date must be after start date", nameof(endDate));

        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>
    /// Vérifier si la période est active à une date donnée
    /// </summary>
    public bool IsActiveAt(DateTime date)
    {
        return date >= StartDate && (!EndDate.HasValue || date <= EndDate.Value);
    }

    /// <summary>
    /// Vérifier si la période est actuellement active
    /// </summary>
    public bool IsCurrentlyActive => IsActiveAt(DateTime.UtcNow);

    /// <summary>
    /// Vérifier si la période est expirée
    /// </summary>
    public bool IsExpired => EndDate.HasValue && DateTime.UtcNow > EndDate.Value;

    /// <summary>
    /// Calculer la durée de la période
    /// </summary>
    public TimeSpan? Duration => EndDate.HasValue ? EndDate.Value - StartDate : null;

    public override string ToString()
    {
        return EndDate.HasValue 
            ? $"{StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}"
            : $"From {StartDate:yyyy-MM-dd}";
    }
}