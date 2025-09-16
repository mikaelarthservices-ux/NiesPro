using Auth.Application.Contracts.Services;

namespace Auth.Infrastructure.Services
{
    /// <summary>
    /// Password service implementation using BCrypt for Auth.Application
    /// </summary>
    public class PasswordServiceAdapter : IPasswordService
    {
        private const int WorkFactor = 12;

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            if (string.IsNullOrEmpty(hashedPassword))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
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
    }
}