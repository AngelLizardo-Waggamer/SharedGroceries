using BackSharedGroceries.Data;
using BackSharedGroceries.Interfaces.Repositories;
using BackSharedGroceries.Models;
using Microsoft.EntityFrameworkCore;

namespace BackSharedGroceries.Repositories
{
    /// <summary>
    /// Repository implementation for ShoppingList entity data access operations.
    /// Implements the Repository pattern to abstract database access and provide a clean API for ShoppingList-related queries.
    /// </summary>
    public class ShoppingListRepository : IShoppingListRepository
    {
        private readonly AppDbContext _context;

        public ShoppingListRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new shopping list in the database.
        /// </summary>
        /// <param name="shoppingList">The shopping list entity to create.</param>
        public async Task CreateShoppingListAsync(ShoppingList shoppingList)
        {
            await _context.ShoppingLists.AddAsync(shoppingList);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a shopping list by its ID.
        /// </summary>
        /// <param name="listId">The ID of the shopping list.</param>
        /// <returns>The shopping list entity if found, otherwise null.</returns>
        public async Task<ShoppingList?> GetShoppingListByIdAsync(Guid listId)
        {
            return await _context.ShoppingLists
                .FirstOrDefaultAsync(sl => sl.Id == listId);
        }

        /// <summary>
        /// Retrieves all shopping lists for a specific family.
        /// </summary>
        /// <param name="familyId">The ID of the family.</param>
        /// <returns>A collection of shopping lists belonging to the family.</returns>
        public async Task<IEnumerable<ShoppingList>> GetShoppingListsByFamilyIdAsync(Guid familyId)
        {
            return await _context.ShoppingLists
                .Where(sl => sl.FamilyId == familyId)
                .OrderByDescending(sl => sl.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Soft deletes a shopping list by setting IsActive to false.
        /// </summary>
        /// <param name="listId">The ID of the shopping list to soft delete.</param>
        public async Task SoftDeleteShoppingListAsync(Guid listId)
        {
            var shoppingList = await _context.ShoppingLists.FindAsync(listId);
            if (shoppingList != null)
            {
                shoppingList.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Restores a soft-deleted shopping list by setting IsActive to true.
        /// </summary>
        /// <param name="listId">The ID of the shopping list to restore.</param>
        public async Task RestoreShoppingListAsync(Guid listId)
        {
            var shoppingList = await _context.ShoppingLists.FindAsync(listId);
            if (shoppingList != null)
            {
                shoppingList.IsActive = true;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Updates the active status of a shopping list.
        /// </summary>
        /// <param name="listId">The ID of the shopping list.</param>
        /// <param name="isActive">The new active status.</param>
        public async Task UpdateShoppingListStatusAsync(Guid listId, bool isActive)
        {
            var shoppingList = await _context.ShoppingLists.FindAsync(listId);
            if (shoppingList != null)
            {
                shoppingList.IsActive = isActive;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Checks if all products in a shopping list are marked as Paid.
        /// </summary>
        /// <param name="listId">The ID of the shopping list.</param>
        /// <returns>True if all products are paid or list has no products, false otherwise.</returns>
        public async Task<bool> AreAllProductsPaidAsync(Guid listId)
        {
            var products = await _context.Products
                .Where(p => p.ListId == listId)
                .ToListAsync();

            // If there are no products, consider it as "all paid"
            if (products.Count == 0)
            {
                return true;
            }

            // Check if all products have Paid status
            return products.All(p => p.Status == Enums.ProductStatus.Paid);
        }
    }
}
