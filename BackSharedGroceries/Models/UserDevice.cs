using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackSharedGroceries.Models
{
    public class UserDevice
    {
        [Key]
        [Column("user_id")]
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required]
        [Column("fcm_token")]
        public string FcmToken { get; set; } = string.Empty;
        
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}