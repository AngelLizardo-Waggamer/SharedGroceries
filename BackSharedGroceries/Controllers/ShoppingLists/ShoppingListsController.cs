using BackSharedGroceries.DTOs;
using BackSharedGroceries.DTOs.Responses;
using BackSharedGroceries.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackSharedGroceries.Controllers.ShoppingLists
{
    /// <summary>
    /// Controller responsible for shopping list management endpoints.
    /// Handles shopping list creation, retrieval, deletion, and status updates.
    /// </summary>
    [ApiController]
    [Route("api/shopping-lists")]
    [Authorize]
    public class ShoppingListsController : ControllerBase
    {
        private readonly IShoppingListService _shoppingListService;
        private readonly IWebHostEnvironment _env;

        /// <summary>
        /// Initializes a new instance of the ShoppingListsController class.
        /// </summary>
        /// <param name="shoppingListService">Service for handling shopping list business logic.</param>
        /// <param name="env">Web host environment to determine if running in development mode.</param>
        public ShoppingListsController(IShoppingListService shoppingListService, IWebHostEnvironment env)
        {
            _shoppingListService = shoppingListService;
            _env = env;
        }

        /// <summary>
        /// Creates a new shopping list for the authenticated user's family.
        /// </summary>
        /// <param name="request">Shopping list creation data (Name)</param>
        /// <returns>Shopping list information including the ID</returns>
        /// <response code="200">The shopping list was created successfully.</response>
        /// <response code="400">The data sent is not valid or user does not belong to a family.</response>
        /// <response code="401">The user is not authenticated.</response>
        [HttpPost("v1/create")]
        public async Task<IActionResult> CreateShoppingList([FromBody] CreateShoppingListRequest request)
        {
            // Validate that the request contains all required fields
            // In development, return detailed validation errors; in production, return generic message for security
            if (!ModelState.IsValid)
            {
                if (_env.IsDevelopment())
                {
                    return BadRequest(ModelState);
                }
                return BadRequest("Invalid shopping list data.");
            }

            // Delegate shopping list creation logic to the service layer
            var result = await _shoppingListService.CreateShoppingListAsync(request);

            // Map ServiceResult to appropriate HTTP status code using pattern matching
            // This ensures proper REST semantics are followed
            return result.ResultType switch
            {
                Common.ServiceResultType.BadRequest => BadRequest(result.ErrorMessage),   // 400 for invalid data
                Common.ServiceResultType.Unauthorized => Unauthorized(result.ErrorMessage), // 401 for auth issues
                _ => Ok(result.Data)                                                       // 200 with shopping list info on success
            };
        }

        /// <summary>
        /// Retrieves all shopping lists for the authenticated user's family.
        /// </summary>
        /// <param name="includeInactive">Whether to include inactive (soft-deleted) lists. Default is false.</param>
        /// <returns>Collection of shopping lists</returns>
        /// <response code="200">Shopping lists retrieved successfully.</response>
        /// <response code="400">User does not belong to a family.</response>
        /// <response code="401">The user is not authenticated.</response>
        [HttpGet("v1/list")]
        public async Task<IActionResult> GetFamilyShoppingLists([FromQuery] bool includeInactive = false)
        {
            // Delegate retrieval logic to the service layer
            var result = await _shoppingListService.GetFamilyShoppingListsAsync(includeInactive);

            // Map ServiceResult to appropriate HTTP status code using pattern matching
            return result.ResultType switch
            {
                Common.ServiceResultType.BadRequest => BadRequest(result.ErrorMessage),     // 400 for invalid data
                Common.ServiceResultType.Unauthorized => Unauthorized(result.ErrorMessage), // 401 for auth issues
                _ => Ok(result.Data)                                                        // 200 with shopping lists on success
            };
        }

        /// <summary>
        /// Soft deletes a shopping list by its ID (sets IsActive to false).
        /// This allows the list to be restored later if needed.
        /// </summary>
        /// <param name="listId">The ID of the shopping list to soft delete</param>
        /// <returns>Confirmation of successful soft delete operation</returns>
        /// <response code="200">The shopping list was soft deleted successfully.</response>
        /// <response code="400">User does not belong to a family.</response>
        /// <response code="401">The user is not authenticated or does not have permission.</response>
        /// <response code="404">Shopping list not found.</response>
        [HttpDelete("v1/{listId}")]
        public async Task<IActionResult> SoftDeleteShoppingList(Guid listId)
        {
            // Delegate soft delete logic to the service layer
            var result = await _shoppingListService.SoftDeleteShoppingListAsync(listId);

            // Map ServiceResult to appropriate HTTP status code using pattern matching
            // This ensures proper REST semantics are followed
            return result.ResultType switch
            {
                Common.ServiceResultType.NotFound => NotFound(result.ErrorMessage),         // 404 if list doesn't exist
                Common.ServiceResultType.BadRequest => BadRequest(result.ErrorMessage),     // 400 for invalid data
                Common.ServiceResultType.Unauthorized => Unauthorized(result.ErrorMessage), // 401 for auth issues
                _ => Ok("Successfully soft deleted the shopping list.")                     // 200 for success
            };
        }

        /// <summary>
        /// Restores a soft-deleted shopping list by setting IsActive to true.
        /// </summary>
        /// <param name="listId">The ID of the shopping list to restore</param>
        /// <returns>Confirmation of successful restore operation</returns>
        /// <response code="200">The shopping list was restored successfully.</response>
        /// <response code="400">User does not belong to a family.</response>
        /// <response code="401">The user is not authenticated or does not have permission.</response>
        /// <response code="404">Shopping list not found.</response>
        [HttpPost("v1/{listId}/restore")]
        public async Task<IActionResult> RestoreShoppingList(Guid listId)
        {
            // Delegate restore logic to the service layer
            var result = await _shoppingListService.RestoreShoppingListAsync(listId);

            // Map ServiceResult to appropriate HTTP status code using pattern matching
            // This ensures proper REST semantics are followed
            return result.ResultType switch
            {
                Common.ServiceResultType.NotFound => NotFound(result.ErrorMessage),         // 404 if list doesn't exist
                Common.ServiceResultType.BadRequest => BadRequest(result.ErrorMessage),     // 400 for invalid data
                Common.ServiceResultType.Unauthorized => Unauthorized(result.ErrorMessage), // 401 for auth issues
                _ => Ok("Successfully restored the shopping list.")                         // 200 for success
            };
        }

        /// <summary>
        /// Updates the active status of a shopping list.
        /// </summary>
        /// <param name="listId">The ID of the shopping list to update</param>
        /// <param name="isActive">The new active status</param>
        /// <returns>Confirmation of successful update operation</returns>
        /// <response code="200">The shopping list status was updated successfully.</response>
        /// <response code="400">User does not belong to a family.</response>
        /// <response code="401">The user is not authenticated or does not have permission.</response>
        /// <response code="404">Shopping list not found.</response>
        [HttpPatch("v1/{listId}/status")]
        public async Task<IActionResult> UpdateShoppingListStatus(Guid listId, [FromQuery] bool isActive)
        {
            // Delegate update logic to the service layer
            var result = await _shoppingListService.UpdateShoppingListStatusAsync(listId, isActive);

            // Map ServiceResult to appropriate HTTP status code using pattern matching
            // This ensures proper REST semantics are followed
            return result.ResultType switch
            {
                Common.ServiceResultType.NotFound => NotFound(result.ErrorMessage),         // 404 if list doesn't exist
                Common.ServiceResultType.BadRequest => BadRequest(result.ErrorMessage),     // 400 for invalid data
                Common.ServiceResultType.Unauthorized => Unauthorized(result.ErrorMessage), // 401 for auth issues
                _ => Ok("Successfully updated the shopping list status.")                   // 200 for success
            };
        }
    }
}
