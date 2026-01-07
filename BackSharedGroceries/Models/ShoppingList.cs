namespace BackSharedGroceries.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ShoppingList
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required, MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    
    [Column("is_active")]
    public bool IsActive { get; set; } = true;
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("family_id")]
    public Guid FamilyId { get; set; }
    [ForeignKey("FamilyId")]
    public Family Family { get; set; } = null!;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
}