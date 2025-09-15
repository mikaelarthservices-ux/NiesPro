namespace Auth.Application.Features.Users.Commands.RegisterUser
{
    /// <summary>
    /// Response model for user registration
    /// </summary>
    public class RegisterUserResponse
    {
        /// <summary>
        /// User unique identifier
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// User email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User username
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// User creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Email confirmation status
        /// </summary>
        public bool EmailConfirmed { get; set; } = false;

        /// <summary>
        /// Account activation status
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
