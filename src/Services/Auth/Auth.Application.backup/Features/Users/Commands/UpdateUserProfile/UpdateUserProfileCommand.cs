using BuildingBlocks.Common.Models;
using MediatR;

namespace Auth.Application.Features.Users.Commands.UpdateUserProfile;

public class UpdateUserProfileCommand : IRequest<ApiResponse<bool>>
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}