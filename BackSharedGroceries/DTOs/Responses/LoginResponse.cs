namespace BackSharedGroceries.DTOs.Responses
{
    /// <summary>
    /// Response DTO for successful login or token refresh operations.
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// JWT access token for API authentication.
        /// </summary>
        public required string Token { get; init; }

        /// <summary>
        /// Refresh token for obtaining new access tokens.
        /// </summary>
        public required string RefreshToken { get; init; }

        /// <summary>
        /// Username of the authenticated user.
        /// </summary>
        public required string Username { get; init; }

        /// <summary>
        /// Family ID if the user belongs to a family.
        /// </summary>
        public Guid? FamilyId { get; init; }
    }
}
