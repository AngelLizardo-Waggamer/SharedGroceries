namespace BackSharedGroceries.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.EntityFrameworkCore;

    [Index(nameof(Username), IsUnique = true)]
    public class User
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        [Column("username")]
        public required string Username { get; set; }

        [Required]
        [Column("password_hash")]
        public required string PasswordHash { get; set; }

        [Column("family_id")]
        public Guid? FamilyId { get; set; }
        [ForeignKey(nameof(FamilyId))]
        public Family? Family { get; set; }

        [Column("current_device_id")]
        public Guid? CurrentDeviceId { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Product> ModifiedProducts { get; set; } = new List<Product>();
    }
}