using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Auth.Application.Features.Authentication.Commands.Login;
using Auth.Application.Features.Users.Commands.RegisterUser;
using Auth.Application.Features.Users.Commands.ChangePassword;
using Auth.API.Models.Requests;
using Auth.API.Models.Responses;
using BuildingBlocks.Common.DTOs;

namespace Auth.API.Controllers.V1
{
    /// <summary>
    /// Authentication controller for login, registration and password management
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IMediator mediator, ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate user and generate access token
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>JWT token and user information</returns>
        /// <response code="200">Login successful</response>
        /// <response code="400">Invalid credentials</response>
        /// <response code="401">Authentication failed</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Login attempt for user: {Email}", request.Email);

                var command = new LoginCommand
                {
                    Email = request.Email,
                    Password = request.Password,
                    DeviceKey = request.DeviceKey,
                    IpAddress = GetClientIpAddress(),
                    UserAgent = Request.Headers.UserAgent.ToString()
                };

                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                {
                    var response = new AuthResponse
                    {
                        AccessToken = result.Data!.AccessToken,
                        RefreshToken = result.Data.RefreshToken,
                        TokenType = "Bearer",
                        ExpiresIn = result.Data.ExpiresIn,
                        User = new UserInfo
                        {
                            Id = result.Data.User.Id,
                            Username = result.Data.User.Username,
                            Email = result.Data.User.Email,
                            FirstName = result.Data.User.FirstName,
                            LastName = result.Data.User.LastName
                        }
                    };

                    _logger.LogInformation("Login successful for user: {Email}", request.Email);
                    return Ok(ApiResponse<AuthResponse>.CreateSuccess(response, "Login successful"));
                }

                _logger.LogWarning("Login failed for user: {Email}", request.Email);
                return Unauthorized(ApiResponse<object>.CreateError("Invalid credentials"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Email}", request.Email);
                return StatusCode(500, ApiResponse<object>.CreateError("An error occurred during login"));
            }
        }

        /// <summary>
        /// Register new user account
        /// </summary>
        /// <param name="request">User registration information</param>
        /// <returns>Registration confirmation</returns>
        /// <response code="201">Registration successful</response>
        /// <response code="400">Invalid registration data</response>
        /// <response code="409">User already exists</response>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<RegisterResponse>>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

                var command = new RegisterUserCommand
                {
                    Username = request.Username,
                    Email = request.Email,
                    Password = request.Password,
                    ConfirmPassword = request.ConfirmPassword,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    DeviceKey = request.DeviceKey,
                    DeviceName = request.DeviceName,
                    IpAddress = GetClientIpAddress(),
                    UserAgent = Request.Headers.UserAgent.ToString()
                };

                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                {
                    var response = new RegisterResponse
                    {
                        UserId = result.Data!.UserId,
                        Username = result.Data.Username,
                        Email = result.Data.Email,
                        IsActive = result.Data.IsActive,
                        EmailConfirmed = result.Data.EmailConfirmed,
                        CreatedAt = result.Data.CreatedAt
                    };

                    _logger.LogInformation("Registration successful for email: {Email}", request.Email);
                    return CreatedAtAction(nameof(Register), ApiResponse<RegisterResponse>.CreateSuccess(response, "Registration successful"));
                }

                _logger.LogWarning("Registration failed for email: {Email}. Error: {Error}", request.Email, result.Message);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
                return StatusCode(500, ApiResponse<object>.CreateError("An error occurred during registration"));
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="request">Password change information</param>
        /// <returns>Password change confirmation</returns>
        /// <response code="200">Password changed successfully</response>
        /// <response code="400">Invalid password data</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<Models.Responses.ChangePasswordResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<Models.Responses.ChangePasswordResponse>>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("Password change attempt for user: {UserId}", userId);

                var command = new ChangePasswordCommand
                {
                    UserId = userId,
                    CurrentPassword = request.CurrentPassword,
                    NewPassword = request.NewPassword,
                    ConfirmPassword = request.ConfirmPassword,
                    DeviceKey = request.DeviceKey,
                    DeviceName = request.DeviceName,
                    IpAddress = GetClientIpAddress(),
                    UserAgent = Request.Headers.UserAgent.ToString()
                };

                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                {
                    var response = new Models.Responses.ChangePasswordResponse
                    {
                        UserId = result.Data!.UserId,
                        ChangedAt = result.Data.ChangedAt,
                        SessionsInvalidated = result.Data.SessionsInvalidated,
                        DevicesLoggedOut = result.Data.DevicesLoggedOut,
                        Message = result.Data.Message
                    };

                    _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
                    return Ok(ApiResponse<Models.Responses.ChangePasswordResponse>.CreateSuccess(response, "Password changed successfully"));
                }

                _logger.LogWarning("Password change failed for user: {UserId}. Error: {Error}", userId, result.Message);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change for user: {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponse<object>.CreateError("An error occurred during password change"));
            }
        }

        /// <summary>
        /// Refresh access token
        /// </summary>
        /// <param name="request">Refresh token information</param>
        /// <returns>New access token</returns>
        /// <response code="200">Token refreshed successfully</response>
        /// <response code="400">Invalid refresh token</response>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                _logger.LogInformation("Token refresh attempt");

                // TODO: Implement RefreshTokenCommand
                // var command = new RefreshTokenCommand
                // {
                //     RefreshToken = request.RefreshToken,
                //     DeviceKey = request.DeviceKey
                // };
                // var result = await _mediator.Send(command);

                return BadRequest(ApiResponse<object>.CreateError("Refresh token functionality not implemented yet"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, ApiResponse<object>.CreateError("An error occurred during token refresh"));
            }
        }

        /// <summary>
        /// Logout user and invalidate tokens
        /// </summary>
        /// <returns>Logout confirmation</returns>
        /// <response code="200">Logout successful</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> Logout()
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("Logout attempt for user: {UserId}", userId);

                // TODO: Implement LogoutCommand
                // var command = new LogoutCommand
                // {
                //     UserId = userId,
                //     DeviceKey = GetDeviceKey()
                // };
                // await _mediator.Send(command);

                _logger.LogInformation("Logout successful for user: {UserId}", userId);
                return Ok(ApiResponse<object>.CreateSuccess(null, "Logout successful"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user: {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponse<object>.CreateError("An error occurred during logout"));
            }
        }

        #region Helper Methods

        /// <summary>
        /// Get current user ID from JWT claims
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("userId");
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID not found in token");
        }

        /// <summary>
        /// Get client IP address
        /// </summary>
        private string GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        #endregion
    }
}