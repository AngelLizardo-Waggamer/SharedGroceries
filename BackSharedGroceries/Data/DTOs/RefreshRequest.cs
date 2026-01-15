using System.ComponentModel.DataAnnotations;

namespace BackSharedGroceries.Data.DTOs
{
    /// <summary>
    /// DTO for refresh token request.
    /// </summary>
    /// <param name="RefreshToken"></param>
    public record RefreshRequest(
        [Required]
        string RefreshToken
    );
}