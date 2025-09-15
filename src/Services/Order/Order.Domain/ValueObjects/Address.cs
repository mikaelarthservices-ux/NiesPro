using NiesPro.Contracts.Primitives;

namespace Order.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string PostalCode { get; }
    public string Country { get; }
    public string? AddressLine2 { get; }

    private Address(string street, string city, string postalCode, string country, string? addressLine2 = null)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
        Country = country;
        AddressLine2 = addressLine2;
    }

    public static Address Create(string street, string city, string postalCode, string country, string? addressLine2 = null)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be null or empty", nameof(street));
        
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be null or empty", nameof(city));
        
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("PostalCode cannot be null or empty", nameof(postalCode));
        
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be null or empty", nameof(country));

        return new Address(
            street.Trim(),
            city.Trim(),
            postalCode.Trim(),
            country.Trim(),
            addressLine2?.Trim()
        );
    }

    public string GetFullAddress()
    {
        var address = $"{Street}";
        if (!string.IsNullOrWhiteSpace(AddressLine2))
            address += $", {AddressLine2}";
        
        return $"{address}, {PostalCode} {City}, {Country}";
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode;
        yield return Country;
        yield return AddressLine2 ?? string.Empty;
    }

    public override string ToString() => GetFullAddress();
}