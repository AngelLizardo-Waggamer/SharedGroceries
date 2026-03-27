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
    /// Service implementation for product-related operations.
    /// Handles business logic, security validation, and conflict resolution for product management.
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly IShoppingListRepository _shoppingListRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ProductService> _logger;
        private readonly IHubContext<ShoppingListHub> _hubContext;

        public ProductService(
            IProductRepository productRepository,
            IFamilyRepository familyRepository,
            IShoppingListRepository shoppingListRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ProductService> logger,
            IHubContext<ShoppingListHub> hubContext)
        {
            _productRepository = productRepository;
            _familyRepository = familyRepository;
            _shoppingListRepository = shoppingListRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Adds a new product to a shopping list.
        /// Validates that the target list belongs to the user's family before creating the product.
        /// </summary>
        /// <param name="dto">Product data transfer object.</param>
        /// <returns>Service result containing the created product response.</returns>
        public async Task<ServiceResult<ProductResponse>> AddProductAsync(ProductUpsertDto dto)
        {
            // Get authenticated user ID
            Guid userId;
            try
            {
                userId = GetUserId();
            }
            catch (Exception ex)
            {
                return ServiceResult<ProductResponse>.Unauthorized(ex.Message);
            }

            // Get user's family ID
            var familyId = await _familyRepository.GetUserFamilyIdAsync(userId);
            if (familyId == null)
            {
                return ServiceResult<ProductResponse>.BadRequest("User does not belong to a family.");
            }

            // Validate that the target list belongs to the user's family
            var familyListIds = await _productRepository.GetFamilyListIdsAsync(familyId.Value);
            if (!familyListIds.Contains(dto.ListId))
            {
                return ServiceResult<ProductResponse>.Unauthorized("The specified list does not belong to your family.");
            }

            // Create the product entity
            var product = new Product
            {
                Id = dto.Id,
                Name = dto.Name,
                Quantity = dto.Quantity,
                Status = dto.Status,
                ListId = dto.ListId,
                ClientTimestamp = dto.ClientTimestamp,
                LastModifiedByUserId = userId
            };

            // Upsert the product
            var success = await _productRepository.UpsertProductAsync(product);

            if (!success)
            {
                // This shouldn't happen for a create operation, but handle it
                _logger.LogWarning("Product upsert failed for ID {ProductId}", dto.Id);
                return ServiceResult<ProductResponse>.BadRequest("Failed to create product.");
            }

            // Check if all products in the list are paid and auto-deactivate if necessary
            await CheckAndAutoDeactivateListIfAllPaidAsync(product.ListId, familyId.Value);

            // Map to response DTO
            var response = MapToResponse(product);

            // Notify family members in real-time
            await _hubContext.Clients.Group(familyId.Value.ToString())
                .SendAsync(HubMethod.ProductAdded.ToString(), response);

            return ServiceResult<ProductResponse>.Ok(response);
        }

        /// <summary>
        /// Updates an existing product with conflict resolution.
        /// Implements Last-Write-Wins strategy using client timestamps to handle offline sync scenarios.
        /// </summary>
        /// <param name="dto">Updated product data (Id is read from the DTO).</param>
        /// <returns>Service result containing the updated product response.</returns>
        public async Task<ServiceResult<ProductResponse>> UpdateProductAsync(ProductUpsertDto dto)
        {
            // Get authenticated user ID
            Guid userId;
            try
            {
                userId = GetUserId();
            }
            catch (Exception ex)
            {
                return ServiceResult<ProductResponse>.Unauthorized(ex.Message);
            }

            // Get user's family ID
            var familyId = await _familyRepository.GetUserFamilyIdAsync(userId);
            if (familyId == null)
            {
                return ServiceResult<ProductResponse>.BadRequest("User does not belong to a family.");
            }

            // Validate that the target list belongs to the user's family
            var familyListIds = await _productRepository.GetFamilyListIdsAsync(familyId.Value);
            if (!familyListIds.Contains(dto.ListId))
            {
                return ServiceResult<ProductResponse>.Unauthorized("The specified list does not belong to your family.");
            }

            // Check if product exists
            var existingProduct = await _productRepository.GetProductByIdAsync(dto.Id);
            if (existingProduct == null)
            {
                return ServiceResult<ProductResponse>.NotFound("Product not found.");
            }

            // Validate that the existing product's list also belongs to the family (security check)
            if (!familyListIds.Contains(existingProduct.ListId))
            {
                return ServiceResult<ProductResponse>.Unauthorized("You do not have permission to update this product.");
            }

            // Create updated product entity
            var product = new Product
            {
                Id = dto.Id,
                Name = dto.Name,
                Quantity = dto.Quantity,
                Status = dto.Status,
                ListId = dto.ListId,
                ClientTimestamp = dto.ClientTimestamp,
                LastModifiedByUserId = userId
            };

            // Upsert with timestamp check
            var success = await _productRepository.UpsertProductAsync(product);

            if (!success)
            {
                // Update was ignored because the database has a newer client timestamp
                // This can happen when multiple users edit the same product while offline
                return ServiceResult<ProductResponse>.Conflict("Product has been modified by another user. Please refresh and try again.");
            }

            // Check if all products in the list are paid and auto-deactivate if necessary
            await CheckAndAutoDeactivateListIfAllPaidAsync(product.ListId, familyId.Value);

            // Map to response DTO
            var response = MapToResponse(product);

            // Notify family members in real-time
            await _hubContext.Clients.Group(familyId.Value.ToString())
                .SendAsync(HubMethod.ProductUpdated.ToString(), response);

            return ServiceResult<ProductResponse>.Ok(response);
        }

        /// <summary>
        /// Deletes a product from a shopping list.
        /// Validates that the product belongs to a list in the user's family before deletion.
        /// </summary>
        /// <param name="id">The product ID to delete.</param>
        /// <returns>Service result indicating success or failure.</returns>
        public async Task<ServiceResult> DeleteProductAsync(Guid id)
        {
            // Get authenticated user ID
            Guid userId;
            try
            {
                userId = GetUserId();
            }
            catch (Exception ex)
            {
                return ServiceResult.Unauthorized(ex.Message);
            }

            // Get user's family ID
            var familyId = await _familyRepository.GetUserFamilyIdAsync(userId);
            if (familyId == null)
            {
                return ServiceResult.BadRequest("User does not belong to a family.");
            }

            // Check if product exists
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            {
                return ServiceResult.NotFound("Product not found.");
            }

            // Validate that the product's list belongs to the user's family
            var familyListIds = await _productRepository.GetFamilyListIdsAsync(familyId.Value);
            if (!familyListIds.Contains(product.ListId))
            {
                return ServiceResult.Unauthorized("You do not have permission to delete this product.");
            }

            // Delete the product
            var success = await _productRepository.DeleteProductAsync(id);

            if (!success)
            {
                _logger.LogWarning("Failed to delete product {ProductId}", id);
                return ServiceResult.BadRequest("Failed to delete product.");
            }

            // Notify family members in real-time
            await _hubContext.Clients.Group(familyId.Value.ToString())
                .SendAsync(HubMethod.ProductDeleted.ToString(), id);

            return ServiceResult.Ok();
        }

        /// <summary>
        /// Synchronizes a batch of products from an offline client.
        /// Applies Last-Write-Wins conflict resolution for each product using client timestamps.
        /// </summary>
        /// <param name="batch">Batch of products to synchronize.</param>
        /// <returns>Service result containing detailed sync results.</returns>
        public async Task<ServiceResult<SyncResultDto>> SyncBatchAsync(SyncBatchDto batch)
        {
            // Get authenticated user ID
            Guid userId;
            try
            {
                userId = GetUserId();
            }
            catch (Exception ex)
            {
                return ServiceResult<SyncResultDto>.Unauthorized(ex.Message);
            }

            // Get user's family ID
            var familyId = await _familyRepository.GetUserFamilyIdAsync(userId);
            if (familyId == null)
            {
                return ServiceResult<SyncResultDto>.BadRequest("User does not belong to a family.");
            }

            // Get all valid list IDs for the family
            var familyListIds = await _productRepository.GetFamilyListIdsAsync(familyId.Value);

            var syncResult = new SyncResultDto
            {
                TotalProcessed = batch.Products.Count
            };

            // Process each product in the batch
            foreach (var dto in batch.Products)
            {
                // Validate that the product's list belongs to the family
                if (!familyListIds.Contains(dto.ListId))
                {
                    _logger.LogWarning("Skipping product {ProductId} - list {ListId} does not belong to family {FamilyId}",
                        dto.Id, dto.ListId, familyId);
                    syncResult.Ignored.Add(dto.Id);
                    continue;
                }

                // Create product entity
                var product = new Product
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Quantity = dto.Quantity,
                    Status = dto.Status,
                    ListId = dto.ListId,
                    ClientTimestamp = dto.ClientTimestamp,
                    LastModifiedByUserId = userId
                };

                // Attempt upsert with timestamp check
                var success = await _productRepository.UpsertProductAsync(product);

                if (success)
                {
                    syncResult.Synced.Add(dto.Id);
                    // Check if all products in the list are paid and auto-deactivate if necessary
                    await CheckAndAutoDeactivateListIfAllPaidAsync(product.ListId, familyId.Value);
                    // Notify family members in real-time
                    await _hubContext.Clients.Group(familyId.Value.ToString())
                        .SendAsync(HubMethod.ProductUpdated.ToString(), MapToResponse(product));
                }
                else
                {
                    syncResult.Ignored.Add(dto.Id);
                }
            }

            _logger.LogInformation("Batch sync completed: {Synced} synced, {Ignored} ignored",
                syncResult.Synced.Count, syncResult.Ignored.Count);

            return ServiceResult<SyncResultDto>.Ok(syncResult);
        }

        /// <summary>
        /// Retrieves product name suggestions for autocomplete.
        /// Returns the top 50 most frequently used product names in the user's family history.
        /// </summary>
        /// <returns>Service result containing list of product name suggestions.</returns>
        public async Task<ServiceResult<IEnumerable<string>>> GetProductSuggestionsAsync()
        {
            // Get authenticated user ID
            Guid userId;
            try
            {
                userId = GetUserId();
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<string>>.Unauthorized(ex.Message);
            }

            // Get user's family ID
            var familyId = await _familyRepository.GetUserFamilyIdAsync(userId);
            if (familyId == null)
            {
                return ServiceResult<IEnumerable<string>>.BadRequest("User does not belong to a family.");
            }

            // Get product suggestions from repository
            var suggestions = await _productRepository.GetProductSuggestionsAsync(familyId.Value);

            return ServiceResult<IEnumerable<string>>.Ok(suggestions);
        }

        /// <summary>
        /// Maps a Product entity to a ProductResponse DTO.
        /// </summary>
        /// <param name="product">The product entity.</param>
        /// <returns>Product response DTO.</returns>
        private ProductResponse MapToResponse(Product product)
        {
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Quantity = product.Quantity,
                Status = product.Status,
                ListId = product.ListId,
                ClientTimestamp = product.ClientTimestamp,
                UpdatedAt = product.UpdatedAt
            };
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

        /// <summary>
        /// Checks if all products in a shopping list are paid and deactivates it if so.
        /// Also fires a ListArchived hub event to the family group.
        /// </summary>
        /// <param name="listId">The shopping list to check.</param>
        /// <param name="familyId">Used to send the hub event to the right family group.</param>
        private async Task CheckAndAutoDeactivateListIfAllPaidAsync(Guid listId, Guid familyId)
        {
            try
            {
                var allPaid = await _shoppingListRepository.AreAllProductsPaidAsync(listId);
                if (allPaid)
                {
                    await _shoppingListRepository.UpdateShoppingListStatusAsync(listId, false);
                    _logger.LogInformation("Shopping list {ListId} was automatically deactivated because all products are paid.", listId);

                    // Notify family members that the list was archived
                    await _hubContext.Clients.Group(familyId.ToString())
                        .SendAsync(HubMethod.ListArchived.ToString(), listId);
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the product update operation
                _logger.LogError(ex, "Failed to auto-deactivate shopping list {ListId}", listId);
            }
        }
    }
}
