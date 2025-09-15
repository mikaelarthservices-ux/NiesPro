using NiesPro.Contracts.Common;
using Auth.Application.DTOs;
using MediatR;

namespace Auth.Application.Features.Authentication.Commands.Login
{
    public class LoginCommand : IRequest<ApiResponse<LoginResponse>>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DeviceKey { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }
}