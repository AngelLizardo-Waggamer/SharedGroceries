using BackSharedGroceries.Enums;

namespace BackSharedGroceries.DTOs.Responses
{
    /// <summary>
    /// Response DTO for product data.
    /// </summary>
    public class ProductResponse
    {
        /// <summary>
        /// Product unique identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Product name.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Optional quantity specification.
        /// </summary>
        public string? Quantity { get; set; }

        /// <summary>
        /// Current product status.
        /// </summary>
        public ProductStatus Status { get; set; }

        /// <summary>
        /// The shopping list this product belongs to.
        /// </summary>
        public Guid ListId { get; set; }

        /// <summary>
        /// Client-side timestamp of when this change occurred.
        /// </summary>
        public DateTime ClientTimestamp { get; set; }

        /// <summary>
        /// Server-side timestamp of when this record was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
