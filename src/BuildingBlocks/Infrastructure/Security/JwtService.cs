using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace NiesPro.Infrastructure.Security
{
    /// <summary>
    /// JWT Token model
    /// </summary>
    public class JwtToken
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string TokenType { get; set; } = "Bearer";
    }

    /// <summary>
    /// JWT Token payload
    /// </summary>
    public class TokenPayload
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
    }

    /// <summary>
    /// Interface for JWT token service
    /// </summary>
    public interface IJwtService
    {
        JwtToken GenerateToken(TokenPayload payload);
        ClaimsPrincipal? ValidateToken(string token);
        string GenerateRefreshToken();
        bool ValidateRefreshToken(string refreshToken);
    }

    /// <summary>
    /// JWT service implementation
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryMinutes;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
            _secretKey = configuration["Jwt:SecretKey"] ?? "NiesPro2025!SuperSecretKeyForJWTTokenGeneration123456789";
            _issuer = configuration["Jwt:Issuer"] ?? "NiesPro.Auth";
            _audience = configuration["Jwt:Audience"] ?? "NiesPro.Client";
            _expiryMinutes = int.Parse(configuration["Jwt:ExpiryMinutes"] ?? "60");
        }

        public JwtToken GenerateToken(TokenPayload payload)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, payload.UserId.ToString()),
                new(ClaimTypes.Name, payload.Username),
                new(ClaimTypes.Email, payload.Email),
                new("device_id", payload.DeviceId),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Ajouter les rôles
            foreach (var role in payload.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Ajouter les permissions
            foreach (var permission in payload.Permissions)
            {
                claims.Add(new Claim("permission", permission));
            }

            var expiresAt = DateTime.UtcNow.AddMinutes(_expiryMinutes);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var accessToken = tokenHandler.WriteToken(token);
            var refreshToken = GenerateRefreshToken();

            return new JwtToken
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                TokenType = "Bearer"
            };
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
                var tokenHandler = new JwtSecurityTokenHandler();

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var generator = RandomNumberGenerator.Create();
            generator.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public bool ValidateRefreshToken(string refreshToken)
        {
            // TODO: Implement refresh token validation against database
            return !string.IsNullOrEmpty(refreshToken);
        }
    }
}