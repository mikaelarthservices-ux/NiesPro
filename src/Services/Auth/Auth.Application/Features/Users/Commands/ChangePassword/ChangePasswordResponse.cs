namespace Auth.Application.Features.Users.Commands.ChangePassword
{
    /// <summary>
    /// Response model for password change operation
    /// </summary>
    public class ChangePasswordResponse
    {
        /// <summary>
        /// User unique identifier
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Password change timestamp
        /// </summary>
        public DateTime ChangedAt { get; set; }

        /// <summary>
        /// Indicates if user sessions were invalidated
        /// </summary>
        public bool SessionsInvalidated { get; set; }

        /// <summary>
        /// Number of devices that were logged out
        /// </summary>
        public int DevicesLoggedOut { get; set; }

        /// <summary>
        /// Password change operation success message
        /// </summary>
        public string Message { get; set; } = "Password changed successfully";
    }
}
