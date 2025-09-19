using BuildingBlocks.Domain.ValueObjects;

namespace Customer.Domain.ValueObjects;

/// <summary>
/// Informations de contact du client
/// </summary>
public class ContactInfo : ValueObject
{
    public string Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? MobileNumber { get; private set; }
    public Address? Address { get; private set; }
    public string? Website { get; private set; }
    public SocialMediaInfo? SocialMedia { get; private set; }

    protected ContactInfo() { }

    public ContactInfo(
        string email,
        string? phoneNumber = null,
        string? mobileNumber = null,
        Address? address = null,
        string? website = null,
        SocialMediaInfo? socialMedia = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        Email = email.ToLowerInvariant().Trim();
        PhoneNumber = phoneNumber?.Trim();
        MobileNumber = mobileNumber?.Trim();
        Address = address;
        Website = website?.Trim();
        SocialMedia = socialMedia;
    }

    public bool HasPhoneContact => !string.IsNullOrWhiteSpace(PhoneNumber) || !string.IsNullOrWhiteSpace(MobileNumber);
    public bool HasAddress => Address != null;
    public bool HasSocialMedia => SocialMedia != null && SocialMedia.HasAnyProfile;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Email;
        yield return PhoneNumber ?? string.Empty;
        yield return MobileNumber ?? string.Empty;
        yield return Address ?? new Address(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        yield return Website ?? string.Empty;
        yield return SocialMedia ?? new SocialMediaInfo();
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public ContactInfo UpdateEmail(string email)
    {
        return new ContactInfo(email, PhoneNumber, MobileNumber, Address, Website, SocialMedia);
    }

    public ContactInfo UpdatePhoneNumber(string? phoneNumber)
    {
        return new ContactInfo(Email, phoneNumber, MobileNumber, Address, Website, SocialMedia);
    }

    public ContactInfo UpdateMobileNumber(string? mobileNumber)
    {
        return new ContactInfo(Email, PhoneNumber, mobileNumber, Address, Website, SocialMedia);
    }

    public ContactInfo UpdateAddress(Address? address)
    {
        return new ContactInfo(Email, PhoneNumber, MobileNumber, address, Website, SocialMedia);
    }

    public ContactInfo UpdateWebsite(string? website)
    {
        return new ContactInfo(Email, PhoneNumber, MobileNumber, Address, website, SocialMedia);
    }

    public ContactInfo UpdateSocialMedia(SocialMediaInfo? socialMedia)
    {
        return new ContactInfo(Email, PhoneNumber, MobileNumber, Address, Website, socialMedia);
    }
}

/// <summary>
/// Adresse physique
/// </summary>
public class Address : ValueObject
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string PostalCode { get; private set; }
    public string Country { get; private set; }
    public string? State { get; private set; }
    public string? AddressLine2 { get; private set; }

    protected Address() { }

    public Address(
        string street,
        string city,
        string postalCode,
        string country,
        string? state = null,
        string? addressLine2 = null)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty", nameof(street));
        
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty", nameof(city));
        
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code cannot be empty", nameof(postalCode));
        
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty", nameof(country));

        Street = street.Trim();
        City = city.Trim();
        PostalCode = postalCode.Trim();
        Country = country.Trim();
        State = state?.Trim();
        AddressLine2 = addressLine2?.Trim();
    }

    public string FullAddress => 
        $"{Street}{(!string.IsNullOrEmpty(AddressLine2) ? $", {AddressLine2}" : "")}, {City}, {PostalCode}, {Country}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode;
        yield return Country;
        yield return State ?? string.Empty;
        yield return AddressLine2 ?? string.Empty;
    }
}

/// <summary>
/// Informations des réseaux sociaux
/// </summary>
public class SocialMediaInfo : ValueObject
{
    public string? Facebook { get; private set; }
    public string? Twitter { get; private set; }
    public string? Instagram { get; private set; }
    public string? LinkedIn { get; private set; }
    public string? TikTok { get; private set; }

    public SocialMediaInfo(
        string? facebook = null,
        string? twitter = null,
        string? instagram = null,
        string? linkedin = null,
        string? tiktok = null)
    {
        Facebook = facebook?.Trim();
        Twitter = twitter?.Trim();
        Instagram = instagram?.Trim();
        LinkedIn = linkedin?.Trim();
        TikTok = tiktok?.Trim();
    }

    public bool HasAnyProfile => 
        !string.IsNullOrWhiteSpace(Facebook) ||
        !string.IsNullOrWhiteSpace(Twitter) ||
        !string.IsNullOrWhiteSpace(Instagram) ||
        !string.IsNullOrWhiteSpace(LinkedIn) ||
        !string.IsNullOrWhiteSpace(TikTok);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Facebook ?? string.Empty;
        yield return Twitter ?? string.Empty;
        yield return Instagram ?? string.Empty;
        yield return LinkedIn ?? string.Empty;
        yield return TikTok ?? string.Empty;
    }
}