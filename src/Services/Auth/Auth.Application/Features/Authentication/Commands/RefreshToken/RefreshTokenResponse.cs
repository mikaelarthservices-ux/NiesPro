namespace Auth.Application.Features.Authentication.Commands.RefreshToken
{
    /// <summary>
    /// Response model for token refresh operation
    /// </summary>
    public class RefreshTokenResponse
    {
        /// <summary>
        /// New JWT access token
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// New refresh token for future token renewals
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Token expiration date and time (UTC)
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Token expiration time in seconds from now
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Token type (always "Bearer")
        /// </summary>
        public string TokenType { get; set; } = "Bearer";
    }
}