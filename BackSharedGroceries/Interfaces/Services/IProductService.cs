using BackSharedGroceries.Common;
using BackSharedGroceries.DTOs;
using BackSharedGroceries.DTOs.Responses;

namespace BackSharedGroceries.Interfaces.Services
{
    /// <summary>
    /// Service interface for product-related operations.
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Adds a new product to a shopping list.
        /// </summary>
        /// <param name="dto">Product data transfer object.</param>
        /// <returns>Service result containing the created product response.</returns>
        Task<ServiceResult<ProductResponse>> AddProductAsync(ProductUpsertDto dto);

        /// <summary>
        /// Updates an existing product.
        /// </summary>
        /// <param name="dto">Updated product data (Id is read from the DTO).</param>
        /// <returns>Service result containing the updated product response.</returns>
        Task<ServiceResult<ProductResponse>> UpdateProductAsync(ProductUpsertDto dto);

        /// <summary>
        /// Deletes a product.
        /// </summary>
        /// <param name="id">The product ID to delete.</param>
        /// <returns>Service result indicating success or failure.</returns>
        Task<ServiceResult> DeleteProductAsync(Guid id);

        /// <summary>
        /// Synchronizes a batch of products from an offline client.
        /// </summary>
        /// <param name="batch">Batch of products to synchronize.</param>
        /// <returns>Service result containing detailed sync results.</returns>
        Task<ServiceResult<SyncResultDto>> SyncBatchAsync(SyncBatchDto batch);

        /// <summary>
        /// Retrieves product name suggestions for autocomplete.
        /// </summary>
        /// <returns>Service result containing list of product name suggestions.</returns>
        Task<ServiceResult<IEnumerable<string>>> GetProductSuggestionsAsync();
    }
}
