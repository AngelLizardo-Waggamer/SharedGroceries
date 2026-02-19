using System.ComponentModel.DataAnnotations;
using BackSharedGroceries.Enums;

namespace BackSharedGroceries.DTOs
{
    /// <summary>
    /// DTO for creating or updating a product.
    /// Uses client-generated UUIDs for offline-first sync capability.
    /// </summary>
    public class ProductUpsertDto
    {
        /// <summary>
        /// Client-generated unique identifier. Required for both create and update operations.
        /// </summary>
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        [Required]
        public Guid Id { get; set; }

        /// <summary>
        /// Product name.
        /// </summary>
        /// <example>Milk</example>
        [Required]
        [MaxLength(255)]
        public required string Name { get; set; }

        /// <summary>
        /// Optional quantity specification.
        /// </summary>
        /// <example>2L</example>
        [MaxLength(50)]
        public string? Quantity { get; set; }

        /// <summary>
        /// Current product status in the shopping workflow.
        /// </summary>
        /// <example>Pending</example>
        [Required]
        public ProductStatus Status { get; set; }

        /// <summary>
        /// The shopping list this product belongs to. Must belong to the user's family.
        /// </summary>
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        [Required]
        public Guid ListId { get; set; }

        /// <summary>
        /// Client-side timestamp of when this change occurred. Used for conflict resolution.
        /// </summary>
        /// <example>2026-02-17T10:30:00Z</example>
        [Required]
        public DateTime ClientTimestamp { get; set; }
    }
}
