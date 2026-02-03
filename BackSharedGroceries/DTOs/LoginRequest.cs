using System.ComponentModel.DataAnnotations;

namespace BackSharedGroceries.DTOs
{
    /// <summary>
    /// DTO for user login request.
    /// </summary>
    public class LoginRequest{

        /// <summary>
        /// Username of the user trying to log in.
        /// </summary>
        /// <example>demo_user</example>
        [Required]
        public required string Username { get; init; }

        /// <summary>
        /// Password of the user trying to log in.
        /// </summary>
        /// <example>Admin.12345</example>
        [Required]
        public required string Password { get; init; }
    }
}