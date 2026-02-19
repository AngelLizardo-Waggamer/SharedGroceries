using BackSharedGroceries.DTOs;
using BackSharedGroceries.DTOs.Responses;
using BackSharedGroceries.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackSharedGroceries.Controllers.Products
{
    /// <summary>
    /// Controller responsible for product management endpoints.
    /// Handles CRUD operations and batch synchronization for offline-first mobile clients.
    /// </summary>
    [ApiController]
    [Route("api/products")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IWebHostEnvironment _env;

        /// <summary>
        /// Initializes a new instance of the ProductsController class.
        /// </summary>
        /// <param name="productService">Service for handling product business logic.</param>
        /// <param name="env">Web host environment to determine if running in development mode.</param>
        public ProductsController(IProductService productService, IWebHostEnvironment env)
        {
            _productService = productService;
            _env = env;
        }

        /// <summary>
        /// Retrieves product name suggestions for autocomplete functionality.
        /// </summary>
        /// <returns>List of the top 50 most frequently used product names in the user's family</returns>
        /// <response code="200">Successfully retrieved suggestions.</response>
        /// <response code="401">The user is not authenticated.</response>
        [HttpGet("v1/suggestions")]
        public async Task<IActionResult> GetSuggestions()
        {
            var result = await _productService.GetProductSuggestionsAsync();

            return result.ResultType switch
            {
                Common.ServiceResultType.Unauthorized => Unauthorized(result.ErrorMessage),
                Common.ServiceResultType.BadRequest => BadRequest(result.ErrorMessage),
                _ => Ok(result.Data)
            };
        }

        /// <summary>
        /// Creates a new product in a shopping list.
        /// </summary>
        /// <param name="dto">Product data to create</param>
        /// <returns>Created product information</returns>
        /// <response code="200">The product was created successfully.</response>
        /// <response code="400">The data sent is not valid.</response>
        /// <response code="401">The user is not authenticated or list doesn't belong to user's family.</response>
        [HttpPost("v1/create")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductUpsertDto dto)
        {
            if (!ModelState.IsValid)
            {
                if (_env.IsDevelopment())
                {
                    return BadRequest(ModelState);
                }
                return BadRequest("Invalid product data.");
            }

            var result = await _productService.AddProductAsync(dto);

            return result.ResultType switch
            {
                Common.ServiceResultType.BadRequest => BadRequest(result.ErrorMessage),
                Common.ServiceResultType.Unauthorized => Unauthorized(result.ErrorMessage),
                _ => Ok(result.Data)
            };
        }

        /// <summary>
        /// Updates an existing product.
        /// </summary>
        /// <param name="id">Product ID to update</param>
        /// <param name="dto">Updated product data</param>
        /// <returns>Updated product information</returns>
        /// <response code="200">The product was updated successfully.</response>
        /// <response code="400">The data sent is not valid.</response>
        /// <response code="401">The user is not authenticated or doesn't have permission.</response>
        /// <response code="404">Product not found.</response>
        /// <response code="409">Product has been modified by another user (stale timestamp).</response>
        [HttpPatch("v1/update/{id}")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductUpsertDto dto)
        {
            if (!ModelState.IsValid)
            {
                if (_env.IsDevelopment())
                {
                    return BadRequest(ModelState);
                }
                return BadRequest("Invalid product data.");
            }

            var result = await _productService.UpdateProductAsync(id, dto);

            return result.ResultType switch
            {
                Common.ServiceResultType.NotFound => NotFound(result.ErrorMessage),
                Common.ServiceResultType.BadRequest => BadRequest(result.ErrorMessage),
                Common.ServiceResultType.Unauthorized => Unauthorized(result.ErrorMessage),
                Common.ServiceResultType.Conflict => Conflict(result.ErrorMessage),
                _ => Ok(result.Data)
            };
        }

        /// <summary>
        /// Deletes a product from a shopping list.
        /// </summary>
        /// <param name="id">Product ID to delete</param>
        /// <returns>Success confirmation</returns>
        /// <response code="200">The product was deleted successfully.</response>
        /// <response code="401">The user is not authenticated or doesn't have permission.</response>
        /// <response code="404">Product not found.</response>
        [HttpDelete("v1/delete/{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var result = await _productService.DeleteProductAsync(id);

            return result.ResultType switch
            {
                Common.ServiceResultType.NotFound => NotFound(result.ErrorMessage),
                Common.ServiceResultType.Unauthorized => Unauthorized(result.ErrorMessage),
                Common.ServiceResultType.BadRequest => BadRequest(result.ErrorMessage),
                _ => Ok()
            };
        }

        /// <summary>
        /// Synchronizes a batch of products from an offline client.
        /// </summary>
        /// <param name="batch">Batch of products to synchronize</param>
        /// <returns>Detailed sync results indicating which products were synced or ignored</returns>
        /// <response code="200">Batch synchronization completed.</response>
        /// <response code="400">The data sent is not valid.</response>
        /// <response code="401">The user is not authenticated.</response>
        [HttpPost("v1/sync")]
        public async Task<IActionResult> SyncBatch([FromBody] SyncBatchDto batch)
        {
            if (!ModelState.IsValid)
            {
                if (_env.IsDevelopment())
                {
                    return BadRequest(ModelState);
                }
                return BadRequest("Invalid sync batch data.");
            }

            var result = await _productService.SyncBatchAsync(batch);

            return result.ResultType switch
            {
                Common.ServiceResultType.BadRequest => BadRequest(result.ErrorMessage),
                Common.ServiceResultType.Unauthorized => Unauthorized(result.ErrorMessage),
                _ => Ok(result.Data)
            };
        }
    }
}
