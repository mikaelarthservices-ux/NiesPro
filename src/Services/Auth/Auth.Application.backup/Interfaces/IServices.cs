namespace Auth.Application.Interfaces
{
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        string GenerateRandomPassword(int length = 12);
    }

    public interface IJwtService
    {
        string GenerateToken(Guid userId, string email, List<string> roles);
        string GenerateRefreshToken();
        bool ValidateToken(string token);
        Guid? GetUserIdFromToken(string token);
        DateTime GetTokenExpiration(string token);
    }
}