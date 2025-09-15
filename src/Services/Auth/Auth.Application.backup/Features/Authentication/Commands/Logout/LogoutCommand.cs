using BuildingBlocks.Common.Models;
using MediatR;

namespace Auth.Application.Features.Authentication.Commands.Logout;

public class LogoutCommand : IRequest<ApiResponse<bool>>
{
    public string RefreshToken { get; set; } = string.Empty;
    public Guid? DeviceId { get; set; }
}