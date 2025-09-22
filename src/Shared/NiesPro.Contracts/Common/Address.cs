using NiesPro.Contracts.Primitives;

namespace NiesPro.Contracts.Common;

/// <summary>
/// Value Object représentant une adresse complète
/// </summary>
public sealed class Address : ValueObject
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string PostalCode { get; private set; }
    public string Country { get; private set; }
    public string? State { get; private set; }
    public string? Region { get; private set; }
    public string? Apartment { get; private set; }
    public string? BuildingName { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }

    private Address(
        string street,
        string city,
        string postalCode,
        string country,
        string? state = null,
        string? region = null,
        string? apartment = null,
        string? buildingName = null,
        double? latitude = null,
        double? longitude = null)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
        Country = country;
        State = state;
        Region = region;
        Apartment = apartment;
        BuildingName = buildingName;
        Latitude = latitude;
        Longitude = longitude;
    }

    public static Address Create(
        string street,
        string city,
        string postalCode,
        string country,
        string? state = null,
        string? region = null,
        string? apartment = null,
        string? buildingName = null,
        double? latitude = null,
        double? longitude = null)
    {
        ValidateAddress(street, city, postalCode, country);

        return new Address(
            street.Trim(),
            city.Trim(),
            postalCode.Trim(),
            country.Trim(),
            state?.Trim(),
            region?.Trim(),
            apartment?.Trim(),
            buildingName?.Trim(),
            latitude,
            longitude);
    }

    private static void ValidateAddress(string street, string city, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street is required", nameof(street));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required", nameof(city));

        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code is required", nameof(postalCode));

        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country is required", nameof(country));

        if (street.Length > 200)
            throw new ArgumentException("Street cannot exceed 200 characters", nameof(street));

        if (city.Length > 100)
            throw new ArgumentException("City cannot exceed 100 characters", nameof(city));

        if (postalCode.Length > 20)
            throw new ArgumentException("Postal code cannot exceed 20 characters", nameof(postalCode));

        if (country.Length > 100)
            throw new ArgumentException("Country cannot exceed 100 characters", nameof(country));
    }

    /// <summary>
    /// Obtient l'adresse formatée complète
    /// </summary>
    public string GetFormattedAddress()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(BuildingName))
            parts.Add(BuildingName);

        parts.Add(Street);

        if (!string.IsNullOrWhiteSpace(Apartment))
            parts.Add($"Apt {Apartment}");

        parts.Add($"{PostalCode} {City}");

        if (!string.IsNullOrWhiteSpace(State))
            parts.Add(State);

        if (!string.IsNullOrWhiteSpace(Region))
            parts.Add(Region);

        parts.Add(Country);

        return string.Join(", ", parts);
    }

    /// <summary>
    /// Vérifie si l'adresse a des coordonnées GPS
    /// </summary>
    public bool HasCoordinates => Latitude.HasValue && Longitude.HasValue;

    /// <summary>
    /// Calcule la distance approximative avec une autre adresse (en km)
    /// </summary>
    public double? CalculateDistanceTo(Address other)
    {
        if (!HasCoordinates || !other.HasCoordinates)
            return null;

        const double earthRadius = 6371; // Rayon de la Terre en km

        var lat1Rad = DegreesToRadians(Latitude!.Value);
        var lat2Rad = DegreesToRadians(other.Latitude!.Value);
        var deltaLatRad = DegreesToRadians(other.Latitude!.Value - Latitude!.Value);
        var deltaLonRad = DegreesToRadians(other.Longitude!.Value - Longitude!.Value);

        var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadius * c;
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode;
        yield return Country;
        yield return State ?? string.Empty;
        yield return Region ?? string.Empty;
        yield return Apartment ?? string.Empty;
        yield return BuildingName ?? string.Empty;
    }

    public override string ToString()
    {
        return GetFormattedAddress();
    }
}