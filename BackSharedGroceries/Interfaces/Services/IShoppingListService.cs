using BackSharedGroceries.Common;
using BackSharedGroceries.DTOs;
using BackSharedGroceries.DTOs.Responses;

namespace BackSharedGroceries.Interfaces.Services
{
    /// <summary>
    /// Service interface for shopping list-related operations.
    /// </summary>
    public interface IShoppingListService
    {
        /// <summary>
        /// Creates a new shopping list for the authenticated user's family.
        /// </summary>
        /// <param name="request">The request object containing shopping list creation details.</param>
        /// <returns>Service result containing the created shopping list response.</returns>
        Task<ServiceResult<ShoppingListResponse>> CreateShoppingListAsync(CreateShoppingListRequest request);

        /// <summary>
        /// Retrieves all shopping lists for the authenticated user's family.
        /// </summary>
        /// <param name="includeInactive">Whether to include inactive (soft-deleted) lists.</param>
        /// <returns>Service result containing a collection of shopping list responses.</returns>
        Task<ServiceResult<IEnumerable<ShoppingListResponse>>> GetFamilyShoppingListsAsync(bool includeInactive = false);

        /// <summary>
        /// Soft deletes a shopping list by setting IsActive to false.
        /// </summary>
        /// <param name="listId">The ID of the shopping list to soft delete.</param>
        /// <returns>Service result indicating the success or failure of the soft delete operation.</returns>
        Task<ServiceResult> SoftDeleteShoppingListAsync(Guid listId);

        /// <summary>
        /// Restores a soft-deleted shopping list by setting IsActive to true.
        /// </summary>
        /// <param name="listId">The ID of the shopping list to restore.</param>
        /// <returns>Service result indicating the success or failure of the restore operation.</returns>
        Task<ServiceResult> RestoreShoppingListAsync(Guid listId);

        /// <summary>
        /// Updates the active status of a shopping list.
        /// </summary>
        /// <param name="listId">The ID of the shopping list.</param>
        /// <param name="isActive">The new active status.</param>
        /// <returns>Service result indicating the success or failure of the update operation.</returns>
        Task<ServiceResult> UpdateShoppingListStatusAsync(Guid listId, bool isActive);

        /// <summary>
        /// Checks if all products in a list are paid and automatically deactivates the list if true.
        /// </summary>
        /// <param name="listId">The ID of the shopping list to check.</param>
        /// <returns>Service result indicating whether the list was auto-deactivated.</returns>
        Task<ServiceResult<bool>> CheckAndAutoDeactivateListAsync(Guid listId);
    }
}
