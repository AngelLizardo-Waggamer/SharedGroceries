using System.Security.Claims;
using BackSharedGroceries.Common;
using BackSharedGroceries.DTOs;
using BackSharedGroceries.DTOs.Responses;
using BackSharedGroceries.Enums;
using BackSharedGroceries.Hubs;
using BackSharedGroceries.Interfaces.Repositories;
using BackSharedGroceries.Interfaces.Services;
using BackSharedGroceries.Models;
using Microsoft.AspNetCore.SignalR;

namespace BackSharedGroceries.Services
{
    /// <summary>
    /// Service implementation for shopping list-related operations.
    /// </summary>
    public class ShoppingListService : IShoppingListService
    {
        private readonly IShoppingListRepository _shoppingListRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<ShoppingListHub> _hubContext;

        public ShoppingListService(
            IShoppingListRepository shoppingListRepository,
            IFamilyRepository familyRepository,
            IHttpContextAccessor httpContextAccessor,
            IHubContext<ShoppingListHub> hubContext)
        {
            _shoppingListRepository = shoppingListRepository;
            _familyRepository = familyRepository;
            _httpContextAccessor = httpContextAccessor;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Creates a new shopping list for the authenticated user's family.
        /// </summary>
        /// <param name="request">The request object containing shopping list creation details.</param>
        /// <returns>Service result containing the created shopping list response.</returns>
        public async Task<ServiceResult<ShoppingListResponse>> CreateShoppingListAsync(CreateShoppingListRequest request)
        {
            // Get the user ID from the HTTP context
            // It first gets the user Guid before the operations to ensure it is available. If it is
            // not available it returns a bad request result.
            Guid userId;
            try
            {
                userId = GetUserId();
            }
            catch (Exception ex)
            {
                return ServiceResult<ShoppingListResponse>.BadRequest(ex.Message);
            }

            // Check if user belongs to a family
            var familyId = await _familyRepository.GetUserFamilyIdAsync(userId);
            if (familyId == null)
            {
                return ServiceResult<ShoppingListResponse>.BadRequest("User must belong to a family to create a shopping list.");
            }

            // Create the shopping list entity
            ShoppingList newShoppingList = new()
            {
                Name = request.Name,
                FamilyId = familyId.Value,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Save the shopping list to the database
            await _shoppingListRepository.CreateShoppingListAsync(newShoppingList);

            // Return the created shopping list information
            var response = new ShoppingListResponse
            {
                Id = newShoppingList.Id,
                Name = newShoppingList.Name,
                IsActive = newShoppingList.IsActive,
                CreatedAt = newShoppingList.CreatedAt,
                FamilyId = newShoppingList.FamilyId
            };

            // Notify family members in real-time
            await _hubContext.Clients.Group(familyId.Value.ToString())
                .SendAsync(HubMethod.ListCreated.ToString(), response);

            return ServiceResult<ShoppingListResponse>.Ok(response);
        }

        /// <summary>
        /// Retrieves all shopping lists for the authenticated user's family.
        /// </summary>
        /// <param name="includeInactive">Whether to include inactive (soft-deleted) lists.</param>
        /// <returns>Service result containing a collection of shopping list responses.</returns>
        public async Task<ServiceResult<IEnumerable<ShoppingListResponse>>> GetFamilyShoppingListsAsync(bool includeInactive = false)
        {
            // Get the user ID from the HTTP context
            Guid userId;
            try
            {
                userId = GetUserId();
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<ShoppingListResponse>>.BadRequest(ex.Message);
            }

            // Check if user belongs to a family
            var familyId = await _familyRepository.GetUserFamilyIdAsync(userId);
            if (familyId == null)
            {
                return ServiceResult<IEnumerable<ShoppingListResponse>>.BadRequest("User must belong to a family to view shopping lists.");
            }

            // Retrieve all shopping lists for the family
            var shoppingLists = await _shoppingListRepository.GetShoppingListsByFamilyIdAsync(familyId.Value);

            // Filter by active status if needed
            if (!includeInactive)
            {
                shoppingLists = shoppingLists.Where(sl => sl.IsActive);
            }

            // Map to response DTOs
            var response = shoppingLists.Select(sl => new ShoppingListResponse
            {
                Id = sl.Id,
                Name = sl.Name,
                IsActive = sl.IsActive,
                CreatedAt = sl.CreatedAt,
                FamilyId = sl.FamilyId
            });

            return ServiceResult<IEnumerable<ShoppingListResponse>>.Ok(response);
        }

        /// <summary>
        /// Soft deletes a shopping list by setting IsActive to false.
        /// </summary>
        /// <param name="listId">The ID of the shopping list to soft delete.</param>
        /// <returns>Service result indicating the success or failure of the soft delete operation.</returns>
        public async Task<ServiceResult> SoftDeleteShoppingListAsync(Guid listId)
        {
            // Get the user ID from the HTTP context
            Guid userId;
            try
            {
                userId = GetUserId();
            }
            catch (Exception ex)
            {
                return ServiceResult.BadRequest(ex.Message);
            }

            // Check if user belongs to a family
            var familyId = await _familyRepository.GetUserFamilyIdAsync(userId);
            if (familyId == null)
            {
                return ServiceResult.BadRequest("User must belong to a family to delete a shopping list.");
            }

            // Retrieve the shopping list
            var shoppingList = await _shoppingListRepository.GetShoppingListByIdAsync(listId);
            if (shoppingList == null)
            {
                return ServiceResult.NotFound("Shopping list not found.");
            }

            // Verify the shopping list belongs to the user's family
            if (shoppingList.FamilyId != familyId.Value)
            {
                return ServiceResult.Unauthorized("You do not have permission to delete this shopping list.");
            }

            // Soft delete the shopping list
            await _shoppingListRepository.SoftDeleteShoppingListAsync(listId);

            // Notify family members in real-time
            await _hubContext.Clients.Group(familyId.Value.ToString())
                .SendAsync(HubMethod.ListArchived.ToString(), listId);

            return ServiceResult.Ok();
        }

        /// <summary>
        /// Restores a soft-deleted shopping list by setting IsActive to true.
        /// </summary>
        /// <param name="listId">The ID of the shopping list to restore.</param>
        /// <returns>Service result indicating the success or failure of the restore operation.</returns>
        public async Task<ServiceResult> RestoreShoppingListAsync(Guid listId)
        {
            // Get the user ID from the HTTP context
            Guid userId;
            try
            {
                userId = GetUserId();
            }
            catch (Exception ex)
            {
                return ServiceResult.BadRequest(ex.Message);
            }

            // Check if user belongs to a family
            var familyId = await _familyRepository.GetUserFamilyIdAsync(userId);
            if (familyId == null)
            {
                return ServiceResult.BadRequest("User must belong to a family to restore a shopping list.");
            }

            // Retrieve the shopping list
            var shoppingList = await _shoppingListRepository.GetShoppingListByIdAsync(listId);
            if (shoppingList == null)
            {
                return ServiceResult.NotFound("Shopping list not found.");
            }

            // Verify the shopping list belongs to the user's family
            if (shoppingList.FamilyId != familyId.Value)
            {
                return ServiceResult.Unauthorized("You do not have permission to restore this shopping list.");
            }

            // Restore the shopping list
            await _shoppingListRepository.RestoreShoppingListAsync(listId);

            return ServiceResult.Ok();
        }

        /// <summary>
        /// Updates the active status of a shopping list.
        /// </summary>
        /// <param name="listId">The ID of the shopping list.</param>
        /// <param name="isActive">The new active status.</param>
        /// <returns>Service result indicating the success or failure of the update operation.</returns>
        public async Task<ServiceResult> UpdateShoppingListStatusAsync(Guid listId, bool isActive)
        {
            // Get the user ID from the HTTP context
            Guid userId;
            try
            {
                userId = GetUserId();
            }
            catch (Exception ex)
            {
                return ServiceResult.BadRequest(ex.Message);
            }

            // Check if user belongs to a family
            var familyId = await _familyRepository.GetUserFamilyIdAsync(userId);
            if (familyId == null)
            {
                return ServiceResult.BadRequest("User must belong to a family to update a shopping list.");
            }

            // Retrieve the shopping list
            var shoppingList = await _shoppingListRepository.GetShoppingListByIdAsync(listId);
            if (shoppingList == null)
            {
                return ServiceResult.NotFound("Shopping list not found.");
            }

            // Verify the shopping list belongs to the user's family
            if (shoppingList.FamilyId != familyId.Value)
            {
                return ServiceResult.Unauthorized("You do not have permission to update this shopping list.");
            }

            // Update the status
            await _shoppingListRepository.UpdateShoppingListStatusAsync(listId, isActive);

            // Notify family members in real-time when a list is archived
            if (!isActive)
            {
                await _hubContext.Clients.Group(familyId.Value.ToString())
                    .SendAsync(HubMethod.ListArchived.ToString(), listId);
            }

            return ServiceResult.Ok();
        }

        /// <summary>
        /// Checks if all products in a list are paid and automatically deactivates the list if true.
        /// This method is intended to be called after product updates.
        /// </summary>
        /// <param name="listId">The ID of the shopping list to check.</param>
        /// <returns>Service result indicating whether the list was auto-deactivated.</returns>
        public async Task<ServiceResult<bool>> CheckAndAutoDeactivateListAsync(Guid listId)
        {
            // Check if all products in the list are paid
            var allPaid = await _shoppingListRepository.AreAllProductsPaidAsync(listId);

            if (allPaid)
            {
                // Automatically deactivate the list
                await _shoppingListRepository.UpdateShoppingListStatusAsync(listId, false);
                return ServiceResult<bool>.Ok(true);
            }

            return ServiceResult<bool>.Ok(false);
        }

        /// <summary>
        /// Returns the user Guid parsed from the HttpContext claims.
        /// </summary>
        /// <returns>User Guid object</returns>
        /// <exception cref="InvalidOperationException">Thrown when the HttpContext or User is null.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the NameIdentifier claim is missing.</exception>
        /// <exception cref="FormatException">Thrown when the NameIdentifier claim is not a valid GUID.</exception>
        private Guid GetUserId()
        {
            // Check if the context exists
            var user = (_httpContextAccessor.HttpContext?.User) ?? throw new InvalidOperationException("User context is unavailable.");

            // Try to find the claim
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                throw new UnauthorizedAccessException("User ID claim is missing from the token.");
            }

            // Parse the claim value to a Guid
            if (!Guid.TryParse(userIdClaim.Value, out Guid userGuid))
            {
                throw new FormatException("The User ID claim is not a valid GUID.");
            }

            return userGuid;
        }
    }
}
