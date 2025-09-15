using BuildingBlocks.Common.Models;
using MediatR;

namespace Auth.Application.Features.Authentication.Commands.RefreshToken;

public class RefreshTokenCommand : IRequest<ApiResponse<RefreshTokenResponse>>
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
}