using BuildingBlocks.Domain.ValueObjects;
using Customer.Domain.Enums;

namespace Customer.Domain.ValueObjects;

/// <summary>
/// Informations personnelles du client
/// </summary>
public class PersonalInfo : ValueObject
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Gender Gender { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string? Nationality { get; private set; }
    public string? Profession { get; private set; }

    protected PersonalInfo() { }

    public PersonalInfo(
        string firstName, 
        string lastName, 
        Gender gender = Gender.Unspecified,
        DateTime? dateOfBirth = null,
        string? nationality = null,
        string? profession = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        if (dateOfBirth.HasValue && dateOfBirth.Value > DateTime.UtcNow)
            throw new ArgumentException("Date of birth cannot be in the future", nameof(dateOfBirth));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Gender = gender;
        DateOfBirth = dateOfBirth;
        Nationality = nationality?.Trim();
        Profession = profession?.Trim();
    }

    public string FullName => $"{FirstName} {LastName}";
    
    public int? Age
    {
        get
        {
            if (!DateOfBirth.HasValue) return null;
            
            var today = DateTime.UtcNow;
            var age = today.Year - DateOfBirth.Value.Year;
            
            if (DateOfBirth.Value.Date > today.AddYears(-age))
                age--;
                
            return age;
        }
    }

    public bool IsMinor => Age.HasValue && Age.Value < 18;
    public bool IsSenior => Age.HasValue && Age.Value >= 65;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
        yield return Gender;
        yield return DateOfBirth ?? DateTime.MinValue;
        yield return Nationality ?? string.Empty;
        yield return Profession ?? string.Empty;
    }

    public PersonalInfo UpdateName(string firstName, string lastName)
    {
        return new PersonalInfo(firstName, lastName, Gender, DateOfBirth, Nationality, Profession);
    }

    public PersonalInfo UpdateGender(Gender gender)
    {
        return new PersonalInfo(FirstName, LastName, gender, DateOfBirth, Nationality, Profession);
    }

    public PersonalInfo UpdateDateOfBirth(DateTime? dateOfBirth)
    {
        return new PersonalInfo(FirstName, LastName, Gender, dateOfBirth, Nationality, Profession);
    }

    public PersonalInfo UpdateNationality(string? nationality)
    {
        return new PersonalInfo(FirstName, LastName, Gender, DateOfBirth, nationality, Profession);
    }

    public PersonalInfo UpdateProfession(string? profession)
    {
        return new PersonalInfo(FirstName, LastName, Gender, DateOfBirth, Nationality, profession);
    }
}