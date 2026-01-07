namespace BackSharedGroceries.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using BackSharedGroceries.Enums;

    public class Product
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(255)]
        [Column("name")]
        public required string Name { get; set; }

        [MaxLength(50)]
        [Column("quantity")]
        public string? Quantity { get; set; }

        [Column("status")]
        public ProductStatus Status { get; set; } = ProductStatus.Pending;

        [Column("last_modified_by_user_id")]
        public Guid? LastModifiedByUserId { get; set; }
        [ForeignKey(nameof(LastModifiedByUserId))]
        public User? LastModifiedByUser { get; set; }
        
        [ConcurrencyCheck]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("list_id")]
        public Guid ListId { get; set; }
        [ForeignKey(nameof(ListId))]
        public ShoppingList List { get; set; } = null!;
    }
}