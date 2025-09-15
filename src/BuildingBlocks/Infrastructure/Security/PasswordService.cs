using System.Security.Cryptography;
using System.Text;

namespace NiesPro.Infrastructure.Security
{
    /// <summary>
    /// Interface for password hashing service
    /// </summary>
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
        string GenerateRandomPassword(int length = 12);
        bool ValidatePasswordStrength(string password);
    }

    /// <summary>
    /// Password service using BCrypt for secure hashing
    /// </summary>
    public class PasswordService : IPasswordService
    {
        private const int WorkFactor = 12;

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        public bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            if (string.IsNullOrEmpty(hash))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                return false;
            }
        }

        public string GenerateRandomPassword(int length = 12)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
            var random = new Random();
            var chars = new char[length];

            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[random.Next(validChars.Length)];
            }

            return new string(chars);
        }

        public bool ValidatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            // Au minimum 8 caractères
            if (password.Length < 8)
                return false;

            // Au moins une minuscule
            if (!password.Any(char.IsLower))
                return false;

            // Au moins une majuscule
            if (!password.Any(char.IsUpper))
                return false;

            // Au moins un chiffre
            if (!password.Any(char.IsDigit))
                return false;

            // Au moins un caractère spécial
            if (!password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c)))
                return false;

            return true;
        }
    }
}