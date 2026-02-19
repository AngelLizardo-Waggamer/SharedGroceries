using System.ComponentModel.DataAnnotations;

namespace BackSharedGroceries.DTOs
{
    /// <summary>
    /// DTO for batch synchronization of multiple products.
    /// </summary>
    public class SyncBatchDto
    {
        /// <summary>
        /// List of products to synchronize.
        /// </summary>
        [Required]
        public required List<ProductUpsertDto> Products { get; set; }
    }
}
