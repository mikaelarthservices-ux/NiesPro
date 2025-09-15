using BuildingBlocks.Common.Models;
using MediatR;

namespace Auth.Application.Features.Users.Queries.GetUserProfile;

public class GetUserProfileQuery : IRequest<ApiResponse<UserProfileDto>>
{
    public Guid UserId { get; set; }
}

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}