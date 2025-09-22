using MediatR;
using NiesPro.Contracts.Common;
using Auth.Application.Common.Models;

namespace Auth.Application.Features.Authentication.Commands.Login
{
    /// <summary>
    /// Login command request
    /// </summary>
    public class LoginCommand : IRequest<ApiResponse<LoginResponse>>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DeviceKey { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }
}
