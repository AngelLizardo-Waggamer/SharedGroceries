using BackSharedGroceries.Models;

namespace BackSharedGroceries.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for Product entity data access operations.
    /// </summary>
    public interface IProductRepository
    {
        /// <summary>
        /// Retrieves a product by its unique identifier.
        /// </summary>
        /// <param name="id">The product ID.</param>
        /// <returns>The product if found, otherwise null.</returns>
        Task<Product?> GetProductByIdAsync(Guid id);

        /// <summary>
        /// Performs an upsert operation (insert if new, update if exists).
        /// </summary>
        /// <param name="product">The product to upsert.</param>
        /// <returns>True if the product was inserted/updated, false if ignored due to timestamp.</returns>
        Task<bool> UpsertProductAsync(Product product);

        /// <summary>
        /// Deletes a product from the database.
        /// </summary>
        /// <param name="id">The product ID to delete.</param>
        /// <returns>True if deleted, false if not found.</returns>
        Task<bool> DeleteProductAsync(Guid id);

        /// <summary>
        /// Retrieves all shopping list IDs that belong to a specific family.
        /// </summary>
        /// <param name="familyId">The family ID.</param>
        /// <returns>List of shopping list IDs belonging to the family.</returns>
        Task<List<Guid>> GetFamilyListIdsAsync(Guid familyId);

        /// <summary>
        /// Retrieves the top 50 most frequently used product names for autocomplete.
        /// </summary>
        /// <param name="familyId">The family ID.</param>
        /// <returns>List of product names ordered by frequency.</returns>
        Task<IEnumerable<string>> GetProductSuggestionsAsync(Guid familyId);

        /// <summary>
        /// Retrieves all products for a specific shopping list.
        /// </summary>
        /// <param name="listId">The shopping list ID.</param>
        /// <returns>Collection of products that belong to the shopping list.</returns>
        Task<IEnumerable<Product>> GetProductsByListIdAsync(Guid listId);
    }
}
