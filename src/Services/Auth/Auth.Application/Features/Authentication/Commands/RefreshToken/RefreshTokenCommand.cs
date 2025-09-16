using MediatR;
using Auth.Application.Common.Models;
using BuildingBlocks.Common.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Auth.Application.Features.Authentication.Commands.RefreshToken
{
    /// <summary>
    /// Command for refreshing JWT access token using refresh token
    /// </summary>
    public class RefreshTokenCommand : IRequest<ApiResponse<RefreshTokenResponse>>
    {
        /// <summary>
        /// Refresh token for generating new access token
        /// </summary>
        [Required]
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Client IP address for security logging
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent information for device tracking
        /// </summary>
        public string? UserAgent { get; set; }
    }
}