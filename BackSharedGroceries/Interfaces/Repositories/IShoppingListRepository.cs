using BackSharedGroceries.Models;

namespace BackSharedGroceries.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for ShoppingList entity data access operations.
    /// </summary>
    public interface IShoppingListRepository
    {
        /// <summary>
        /// Creates a new shopping list in the database.
        /// </summary>
        /// <param name="shoppingList">The shopping list entity to create.</param>
        Task CreateShoppingListAsync(ShoppingList shoppingList);

        /// <summary>
        /// Retrieves a shopping list by its ID.
        /// </summary>
        /// <param name="listId">The ID of the shopping list.</param>
        /// <returns>The shopping list entity if found, otherwise null.</returns>
        Task<ShoppingList?> GetShoppingListByIdAsync(Guid listId);

        /// <summary>
        /// Retrieves all shopping lists for a specific family.
        /// </summary>
        /// <param name="familyId">The ID of the family.</param>
        /// <returns>A collection of shopping lists belonging to the family.</returns>
        Task<IEnumerable<ShoppingList>> GetShoppingListsByFamilyIdAsync(Guid familyId);

        /// <summary>
        /// Soft deletes a shopping list by setting IsActive to false.
        /// </summary>
        /// <param name="listId">The ID of the shopping list to soft delete.</param>
        Task SoftDeleteShoppingListAsync(Guid listId);

        /// <summary>
        /// Restores a soft-deleted shopping list by setting IsActive to true.
        /// </summary>
        /// <param name="listId">The ID of the shopping list to restore.</param>
        Task RestoreShoppingListAsync(Guid listId);

        /// <summary>
        /// Updates the active status of a shopping list.
        /// </summary>
        /// <param name="listId">The ID of the shopping list.</param>
        /// <param name="isActive">The new active status.</param>
        Task UpdateShoppingListStatusAsync(Guid listId, bool isActive);

        /// <summary>
        /// Checks if all products in a shopping list are marked as Paid.
        /// </summary>
        /// <param name="listId">The ID of the shopping list.</param>
        /// <returns>True if all products are paid or list has no products, false otherwise.</returns>
        Task<bool> AreAllProductsPaidAsync(Guid listId);
    }
}
