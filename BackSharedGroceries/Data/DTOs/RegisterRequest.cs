using System.ComponentModel.DataAnnotations;

namespace BackSharedGroceries.Data.DTOs
{
    /// <summary>
    /// DTO for user registration request. 
    /// When a new user is created, it will not have a family associated, so the only required fields are the username and the password.
    /// </summary>
    /// <param name="Username"></param>
    /// <param name="Password"></param>
    public record RegisterRequest(
        [Required]
        string Username,
        [Required]
        string Password
    );
}