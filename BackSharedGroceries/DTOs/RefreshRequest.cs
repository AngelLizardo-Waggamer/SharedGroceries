using System.ComponentModel.DataAnnotations;

namespace BackSharedGroceries.DTOs
{
    /// <summary>
    /// DTO for refresh token request.
    /// </summary>
    public class RefreshRequest
    {
        /// <summary>
        /// The current stored refresh token in the device.
        /// </summary>
        /// <example>token...</example>
        [Required]
        public required string RefreshToken {get; init;}
    }
}