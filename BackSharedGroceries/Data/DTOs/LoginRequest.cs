using System.ComponentModel.DataAnnotations;

namespace BackSharedGroceries.Data.DTOs
{
    /// <summary>
    /// DTO for user login request.
    /// </summary>
    /// <param name="Username"></param>
    /// <param name="Password"></param>
    public record LoginRequest(
        [Required]
        string Username,
        [Required]
        string Password
    );
}