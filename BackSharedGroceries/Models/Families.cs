namespace BackSharedGroceries.Models
{
    using Microsoft.EntityFrameworkCore;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    [Index(nameof(FamilyInviteCode), IsUnique = true)]
    public class Family
    {
        [Key]
        [Column("id")]
        public Guid FamilyId { get; set; } = Guid.NewGuid();
        
        [Required, MaxLength(100)]
        [Column("name")]
        public required string FamilyName { get; set; }
        
        [StringLength(8, MinimumLength = 6)]
        [Column("invite_code")]
        public string FamilyInviteCode { get; set; } = string.Empty;
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<ShoppingList> Lists { get; set; } = new List<ShoppingList>();
    }
}
