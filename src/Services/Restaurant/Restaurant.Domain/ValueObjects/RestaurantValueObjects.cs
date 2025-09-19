namespace Restaurant.Domain.ValueObjects;

/// <summary>
/// Value Object représentant la capacité d'une table
/// </summary>
public sealed record TableCapacity
{
    public int MinCapacity { get; }
    public int MaxCapacity { get; }
    public int OptimalCapacity { get; }

    public TableCapacity(int minCapacity, int maxCapacity, int? optimalCapacity = null)
    {
        if (minCapacity <= 0)
            throw new ArgumentException("Minimum capacity must be positive", nameof(minCapacity));
        
        if (maxCapacity <= 0)
            throw new ArgumentException("Maximum capacity must be positive", nameof(maxCapacity));
        
        if (minCapacity > maxCapacity)
            throw new ArgumentException("Minimum capacity cannot exceed maximum capacity");

        MinCapacity = minCapacity;
        MaxCapacity = maxCapacity;
        OptimalCapacity = optimalCapacity ?? minCapacity;

        if (OptimalCapacity < minCapacity || OptimalCapacity > maxCapacity)
            throw new ArgumentException("Optimal capacity must be between min and max capacity");
    }

    /// <summary>
    /// Vérifier si la capacité peut accueillir un nombre de personnes
    /// </summary>
    public bool CanAccommodate(int partySize)
    {
        return partySize >= MinCapacity && partySize <= MaxCapacity;
    }

    /// <summary>
    /// Vérifier si c'est une configuration optimale
    /// </summary>
    public bool IsOptimalFor(int partySize)
    {
        return partySize == OptimalCapacity;
    }

    /// <summary>
    /// Calculer l'efficacité pour un nombre de personnes
    /// </summary>
    public decimal CalculateEfficiency(int partySize)
    {
        if (!CanAccommodate(partySize))
            return 0;

        return (decimal)partySize / MaxCapacity;
    }

    public override string ToString() => $"{MinCapacity}-{MaxCapacity} personnes (optimal: {OptimalCapacity})";
}

/// <summary>
/// Value Object représentant le temps de préparation d'un plat
/// </summary>
public sealed record PreparationTime
{
    public int MinutesRequired { get; }
    public int? MaxMinutes { get; }
    public string? Notes { get; }

    public PreparationTime(int minutesRequired, int? maxMinutes = null, string? notes = null)
    {
        if (minutesRequired <= 0)
            throw new ArgumentException("Preparation time must be positive", nameof(minutesRequired));

        if (maxMinutes.HasValue && maxMinutes.Value < minutesRequired)
            throw new ArgumentException("Maximum time cannot be less than required time");

        MinutesRequired = minutesRequired;
        MaxMinutes = maxMinutes;
        Notes = notes?.Trim();
    }

    /// <summary>
    /// Créer un temps de préparation avec une fourchette
    /// </summary>
    public static PreparationTime CreateRange(int minMinutes, int maxMinutes, string? notes = null)
    {
        return new PreparationTime(minMinutes, maxMinutes, notes);
    }

    /// <summary>
    /// Créer un temps fixe
    /// </summary>
    public static PreparationTime CreateFixed(int minutes, string? notes = null)
    {
        return new PreparationTime(minutes, null, notes);
    }

    /// <summary>
    /// Calculer le temps estimé en tenant compte des facteurs
    /// </summary>
    public TimeSpan CalculateEstimatedTime(decimal rushFactor = 1.0m, int kitchenLoad = 0)
    {
        var baseMinutes = MaxMinutes ?? MinutesRequired;
        var adjustedMinutes = baseMinutes * rushFactor + (kitchenLoad * 0.1m);
        return TimeSpan.FromMinutes((double)adjustedMinutes);
    }

    /// <summary>
    /// Vérifier si c'est un plat rapide
    /// </summary>
    public bool IsQuickPrep => MinutesRequired <= 10;

    /// <summary>
    /// Vérifier si c'est un plat long
    /// </summary>
    public bool IsLongPrep => MinutesRequired >= 30;

    public override string ToString()
    {
        if (MaxMinutes.HasValue && MaxMinutes != MinutesRequired)
            return $"{MinutesRequired}-{MaxMinutes} min";
        return $"{MinutesRequired} min";
    }
}

/// <summary>
/// Value Object représentant les informations nutritionnelles
/// </summary>
public sealed record NutritionalInfo
{
    public decimal? Calories { get; }
    public decimal? Protein { get; } // en grammes
    public decimal? Carbohydrates { get; } // en grammes
    public decimal? Fat { get; } // en grammes
    public decimal? Fiber { get; } // en grammes
    public decimal? Sugar { get; } // en grammes
    public decimal? Sodium { get; } // en milligrammes

    public NutritionalInfo(
        decimal? calories = null,
        decimal? protein = null,
        decimal? carbohydrates = null,
        decimal? fat = null,
        decimal? fiber = null,
        decimal? sugar = null,
        decimal? sodium = null)
    {
        ValidateNutritionalValue(calories, nameof(calories));
        ValidateNutritionalValue(protein, nameof(protein));
        ValidateNutritionalValue(carbohydrates, nameof(carbohydrates));
        ValidateNutritionalValue(fat, nameof(fat));
        ValidateNutritionalValue(fiber, nameof(fiber));
        ValidateNutritionalValue(sugar, nameof(sugar));
        ValidateNutritionalValue(sodium, nameof(sodium));

        Calories = calories;
        Protein = protein;
        Carbohydrates = carbohydrates;
        Fat = fat;
        Fiber = fiber;
        Sugar = sugar;
        Sodium = sodium;
    }

    /// <summary>
    /// Calculer les calories à partir des macronutriments
    /// </summary>
    public decimal CalculateCaloriesFromMacros()
    {
        var proteinCals = (Protein ?? 0) * 4;
        var carbCals = (Carbohydrates ?? 0) * 4;
        var fatCals = (Fat ?? 0) * 9;
        return proteinCals + carbCals + fatCals;
    }

    /// <summary>
    /// Vérifier si c'est un plat riche en protéines
    /// </summary>
    public bool IsHighProtein => Protein.HasValue && Protein.Value >= 20;

    /// <summary>
    /// Vérifier si c'est un plat faible en calories
    /// </summary>
    public bool IsLowCalorie => Calories.HasValue && Calories.Value <= 300;

    /// <summary>
    /// Vérifier si c'est un plat riche en fibres
    /// </summary>
    public bool IsHighFiber => Fiber.HasValue && Fiber.Value >= 5;

    private static void ValidateNutritionalValue(decimal? value, string paramName)
    {
        if (value.HasValue && value.Value < 0)
            throw new ArgumentException($"{paramName} cannot be negative", paramName);
    }

    public override string ToString()
    {
        var parts = new List<string>();
        if (Calories.HasValue) parts.Add($"{Calories:F0} cal");
        if (Protein.HasValue) parts.Add($"{Protein:F1}g protéines");
        if (Carbohydrates.HasValue) parts.Add($"{Carbohydrates:F1}g glucides");
        if (Fat.HasValue) parts.Add($"{Fat:F1}g lipides");
        
        return parts.Any() ? string.Join(", ", parts) : "Informations nutritionnelles non disponibles";
    }
}

/// <summary>
/// Value Object représentant les coordonnées d'une table
/// </summary>
public sealed record TablePosition
{
    public decimal X { get; }
    public decimal Y { get; }
    public decimal? Width { get; }
    public decimal? Height { get; }
    public int? Rotation { get; } // en degrés

    public TablePosition(decimal x, decimal y, decimal? width = null, decimal? height = null, int? rotation = null)
    {
        if (width.HasValue && width.Value <= 0)
            throw new ArgumentException("Width must be positive", nameof(width));
        
        if (height.HasValue && height.Value <= 0)
            throw new ArgumentException("Height must be positive", nameof(height));

        if (rotation.HasValue && (rotation.Value < 0 || rotation.Value >= 360))
            throw new ArgumentException("Rotation must be between 0 and 359 degrees", nameof(rotation));

        X = x;
        Y = y;
        Width = width;
        Height = height;
        Rotation = rotation;
    }

    /// <summary>
    /// Calculer la distance vers une autre position
    /// </summary>
    public decimal DistanceTo(TablePosition other)
    {
        var deltaX = X - other.X;
        var deltaY = Y - other.Y;
        return (decimal)Math.Sqrt((double)(deltaX * deltaX + deltaY * deltaY));
    }

    /// <summary>
    /// Vérifier si deux tables sont adjacentes
    /// </summary>
    public bool IsAdjacentTo(TablePosition other, decimal threshold = 2.0m)
    {
        return DistanceTo(other) <= threshold;
    }

    /// <summary>
    /// Obtenir les coordonnées du centre
    /// </summary>
    public (decimal CenterX, decimal CenterY) GetCenter()
    {
        var centerX = Width.HasValue ? X + Width.Value / 2 : X;
        var centerY = Height.HasValue ? Y + Height.Value / 2 : Y;
        return (centerX, centerY);
    }

    public override string ToString() => $"({X:F1}, {Y:F1})";
}

/// <summary>
/// Value Object représentant une note/évaluation
/// </summary>
public sealed record Rating
{
    public decimal Score { get; }
    public decimal MaxScore { get; }
    public string? Comment { get; }
    public DateTime RatedAt { get; }

    public Rating(decimal score, decimal maxScore = 5.0m, string? comment = null, DateTime? ratedAt = null)
    {
        if (score < 0)
            throw new ArgumentException("Score cannot be negative", nameof(score));
        
        if (maxScore <= 0)
            throw new ArgumentException("Max score must be positive", nameof(maxScore));
        
        if (score > maxScore)
            throw new ArgumentException("Score cannot exceed max score", nameof(score));

        Score = score;
        MaxScore = maxScore;
        Comment = comment?.Trim();
        RatedAt = ratedAt ?? DateTime.UtcNow;
    }

    /// <summary>
    /// Calculer le pourcentage
    /// </summary>
    public decimal Percentage => (Score / MaxScore) * 100;

    /// <summary>
    /// Vérifier si c'est une bonne note
    /// </summary>
    public bool IsGoodRating => Percentage >= 80;

    /// <summary>
    /// Vérifier si c'est une mauvaise note
    /// </summary>
    public bool IsPoorRating => Percentage < 50;

    /// <summary>
    /// Obtenir les étoiles (sur 5)
    /// </summary>
    public int GetStars() => (int)Math.Round((Score / MaxScore) * 5);

    public override string ToString() => $"{Score:F1}/{MaxScore:F1} ({Percentage:F0}%)";
}

/// <summary>
/// Value Object représentant un horaire de service
/// </summary>
public sealed record ServiceHours
{
    public TimeOnly OpenTime { get; }
    public TimeOnly CloseTime { get; }
    public DayOfWeek DayOfWeek { get; }
    public bool IsClosed { get; }

    public ServiceHours(DayOfWeek dayOfWeek, TimeOnly? openTime = null, TimeOnly? closeTime = null)
    {
        DayOfWeek = dayOfWeek;
        
        if (openTime == null || closeTime == null)
        {
            IsClosed = true;
            OpenTime = TimeOnly.MinValue;
            CloseTime = TimeOnly.MinValue;
        }
        else
        {
            IsClosed = false;
            OpenTime = openTime.Value;
            CloseTime = closeTime.Value;
            
            // Gérer le cas où la fermeture est le lendemain
            if (closeTime <= openTime)
            {
                // Supposer que la fermeture est le lendemain
                CloseTime = closeTime.Value;
            }
        }
    }

    /// <summary>
    /// Créer des horaires fermés
    /// </summary>
    public static ServiceHours CreateClosed(DayOfWeek dayOfWeek)
    {
        return new ServiceHours(dayOfWeek);
    }

    /// <summary>
    /// Vérifier si c'est ouvert à une heure donnée
    /// </summary>
    public bool IsOpenAt(TimeOnly time)
    {
        if (IsClosed) return false;

        // Si fermeture après minuit
        if (CloseTime <= OpenTime)
        {
            return time >= OpenTime || time <= CloseTime;
        }
        
        return time >= OpenTime && time <= CloseTime;
    }

    /// <summary>
    /// Calculer la durée d'ouverture
    /// </summary>
    public TimeSpan GetOpenDuration()
    {
        if (IsClosed) return TimeSpan.Zero;

        if (CloseTime <= OpenTime)
        {
            // Service sur deux jours
            return (TimeSpan.FromDays(1) - OpenTime.ToTimeSpan()) + CloseTime.ToTimeSpan();
        }

        return CloseTime.ToTimeSpan() - OpenTime.ToTimeSpan();
    }

    public override string ToString()
    {
        if (IsClosed) return $"{DayOfWeek}: Fermé";
        return $"{DayOfWeek}: {OpenTime:HH:mm} - {CloseTime:HH:mm}";
    }
}

/// <summary>
/// Value Object représentant le prix d'un plat
/// </summary>
public sealed record MenuPrice
{
    public decimal Amount { get; }
    public string Currency { get; }
    public decimal? DiscountedAmount { get; }
    public DateTime? DiscountValidUntil { get; }

    public MenuPrice(decimal amount, string currency = "EUR", decimal? discountedAmount = null, DateTime? discountValidUntil = null)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be null or empty", nameof(currency));

        if (discountedAmount.HasValue)
        {
            if (discountedAmount.Value < 0)
                throw new ArgumentException("Discounted amount cannot be negative", nameof(discountedAmount));
            
            if (discountedAmount.Value >= amount)
                throw new ArgumentException("Discounted amount must be less than original amount", nameof(discountedAmount));
        }

        Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency.ToUpperInvariant();
        DiscountedAmount = discountedAmount.HasValue ? Math.Round(discountedAmount.Value, 2, MidpointRounding.AwayFromZero) : null;
        DiscountValidUntil = discountValidUntil;
    }

    /// <summary>
    /// Obtenir le prix effectif (avec remise si applicable)
    /// </summary>
    public decimal GetEffectivePrice()
    {
        if (HasActiveDiscount)
            return DiscountedAmount!.Value;
        
        return Amount;
    }

    /// <summary>
    /// Vérifier si une remise est active
    /// </summary>
    public bool HasActiveDiscount => DiscountedAmount.HasValue && 
                                   (!DiscountValidUntil.HasValue || DateTime.UtcNow <= DiscountValidUntil.Value);

    /// <summary>
    /// Calculer le pourcentage de remise
    /// </summary>
    public decimal? GetDiscountPercentage()
    {
        if (!DiscountedAmount.HasValue) return null;
        
        return ((Amount - DiscountedAmount.Value) / Amount) * 100;
    }

    /// <summary>
    /// Calculer l'économie réalisée
    /// </summary>
    public decimal GetSavings()
    {
        if (!HasActiveDiscount) return 0;
        
        return Amount - DiscountedAmount!.Value;
    }

    public override string ToString()
    {
        var price = $"{Amount:C} {Currency}";
        
        if (HasActiveDiscount)
        {
            price = $"{DiscountedAmount:C} {Currency} (était {Amount:C})";
        }
        
        return price;
    }
}