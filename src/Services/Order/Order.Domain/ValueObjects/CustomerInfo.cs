using NiesPro.Contracts.Primitives;

namespace Order.Domain.ValueObjects;

public sealed class CustomerInfo : ValueObject
{
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public string? PhoneNumber { get; }

    private CustomerInfo(string firstName, string lastName, string email, string? phoneNumber = null)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
    }

    public static CustomerInfo Create(string firstName, string lastName, string email, string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("FirstName cannot be null or empty", nameof(firstName));
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("LastName cannot be null or empty", nameof(lastName));
        
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        if (!string.IsNullOrWhiteSpace(phoneNumber) && !IsValidPhoneNumber(phoneNumber))
            throw new ArgumentException("Invalid phone number format", nameof(phoneNumber));

        return new CustomerInfo(
            firstName.Trim(),
            lastName.Trim(),
            email.Trim().ToLowerInvariant(),
            phoneNumber?.Trim()
        );
    }

    public string GetFullName() => $"{FirstName} {LastName}";

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

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        // Validation basique pour les numéros de téléphone français/internationaux
        return System.Text.RegularExpressions.Regex.IsMatch(
            phoneNumber, 
            @"^(\+\d{1,3}[-.\s]?)?\(?\d{1,4}\)?[-.\s]?\d{1,4}[-.\s]?\d{1,9}$"
        );
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
        yield return Email;
        yield return PhoneNumber ?? string.Empty;
    }

    public override string ToString() => $"{GetFullName()} ({Email})";
}