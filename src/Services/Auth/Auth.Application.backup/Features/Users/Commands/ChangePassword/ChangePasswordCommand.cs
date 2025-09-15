using BuildingBlocks.Common.Models;
using MediatR;

namespace Auth.Application.Features.Users.Commands.ChangePassword;

public class ChangePasswordCommand : IRequest<ApiResponse<bool>>
{
    public Guid UserId { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}