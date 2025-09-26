using MediatR;
using NiesPro.Contracts.Common;
using Auth.Application.Common.Models;
using NiesPro.Contracts.Application.CQRS;

namespace Auth.Application.Features.Authentication.Commands.Login
{
    /// <summary>
    /// Login command request - NiesPro Enterprise Standard
    /// </summary>
    public class LoginCommand : BaseCommand<ApiResponse<LoginResponse>>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DeviceKey { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }
}
