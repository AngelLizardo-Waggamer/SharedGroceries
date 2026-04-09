using BackSharedGroceries.Data;
using BackSharedGroceries.Interfaces.Repositories;
using BackSharedGroceries.Models;
using Microsoft.EntityFrameworkCore;

namespace BackSharedGroceries.Repositories
{
    /// <summary>
    /// Repository implementation for Product entity data access operations.
    /// Provides methods for CRUD operations, family-scoped queries, and offline-first sync support.
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a product by its unique identifier.
        /// </summary>
        /// <param name="id">The product ID.</param>
        /// <returns>The product if found, otherwise null.</returns>
        public async Task<Product?> GetProductByIdAsync(Guid id)
        {
            return await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Performs an upsert operation (insert if new, update if exists).
        /// Uses client timestamps for Last-Write-Wins conflict resolution in offline-first scenarios.
        /// </summary>
        /// <param name="product">The product to upsert.</param>
        /// <returns>True if the product was inserted/updated, false if ignored due to stale client timestamp.</returns>
        public async Task<bool> UpsertProductAsync(Product product)
        {
            // Check if product already exists in the database
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            if (existingProduct == null)
            {
                // New product - insert it
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                // Product exists - apply Last-Write-Wins logic
                // Only update if incoming client timestamp is newer or equal
                if (product.ClientTimestamp >= existingProduct.ClientTimestamp)
                {
                    // Update all fields except the Id
                    existingProduct.Name = product.Name;
                    existingProduct.Quantity = product.Quantity;
                    existingProduct.Status = product.Status;
                    existingProduct.ListId = product.ListId;
                    existingProduct.LastModifiedByUserId = product.LastModifiedByUserId;
                    existingProduct.ClientTimestamp = product.ClientTimestamp;
                    // UpdatedAt will be automatically set by DbContext

                    await _context.SaveChangesAsync();
                    return true;
                }
                else
                {
                    // Incoming data is stale - ignore the update
                    return false;
                }
            }
        }

        /// <summary>
        /// Deletes a product from the database.
        /// </summary>
        /// <param name="id">The product ID to delete.</param>
        /// <returns>True if deleted, false if not found.</returns>
        public async Task<bool> DeleteProductAsync(Guid id)
        {
            var rowsAffected = await _context.Products
                .Where(p => p.Id == id)
                .ExecuteDeleteAsync();

            return rowsAffected > 0;
        }

        /// <summary>
        /// Retrieves all shopping list IDs that belong to a specific family.
        /// </summary>
        /// <param name="familyId">The family ID.</param>
        /// <returns>List of shopping list IDs belonging to the family.</returns>
        public async Task<List<Guid>> GetFamilyListIdsAsync(Guid familyId)
        {
            return await _context.ShoppingLists
                .Where(sl => sl.FamilyId == familyId)
                .Select(sl => sl.Id)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves the top 50 most frequently used product names for autocomplete.
        /// </summary>
        /// <param name="familyId">The family ID.</param>
        /// <returns>List of product names ordered by frequency (most common first).</returns>
        public async Task<IEnumerable<string>> GetProductSuggestionsAsync(Guid familyId)
        {
            // Join Products with ShoppingLists to filter by family
            // Group by product name and count occurrences
            // Return top 50 most frequent names
            return await _context.Products
                .Where(p => p.List.FamilyId == familyId)
                .GroupBy(p => p.Name)
                .OrderByDescending(g => g.Count())
                .Take(50)
                .Select(g => g.Key)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all products for a specific shopping list.
        /// </summary>
        /// <param name="listId">The shopping list ID.</param>
        /// <returns>Collection of products that belong to the shopping list.</returns>
        public async Task<IEnumerable<Product>> GetProductsByListIdAsync(Guid listId)
        {
            return await _context.Products
                .AsNoTracking()
                .Where(p => p.ListId == listId)
                .OrderByDescending(p => p.UpdatedAt)
                .ToListAsync();
        }
    }
}
