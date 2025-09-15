using MediatR;
using NiesPro.Contracts.Common;
using Auth.Application.Common.Models;

namespace Auth.Application.Features.Users.Commands.RegisterUser
{
    /// <summary>
    /// Register user command request
    /// </summary>
    public class RegisterUserCommand : IRequest<ApiResponse<RegisterUserResponse>>
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string DeviceKey { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }
}
