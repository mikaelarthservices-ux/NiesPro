namespace Auth.Application.DTOs
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public UserDto User { get; set; } = new();
    }

    public class RegisterUserResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class DeviceDto
    {
        public Guid Id { get; set; }
        public string DeviceKey { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime LastAccessAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}