using System.ComponentModel.DataAnnotations;

namespace BackSharedGroceries.Data.DTOs
{
    /// <summary>
    /// DTO for user registration request. 
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// Username of the new user. Must be unique.
        /// </summary>
        /// <example>demo_user</example>
        [Required]
        public required string Username { get; init; }

        /// <summary>
        /// Password of the new user.
        /// </summary>
        /// <example>Admin.12345</example>
        [Required]
        public required string Password { get; init; }
    }
}