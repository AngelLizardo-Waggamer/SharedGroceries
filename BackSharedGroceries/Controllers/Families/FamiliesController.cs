using BackSharedGroceries.DTOs;
using BackSharedGroceries.DTOs.Responses;
using BackSharedGroceries.Interfaces.Repositories;
using BackSharedGroceries.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackSharedGroceries.Controllers.Families
{
    /// <summary>
    /// Controller responsible for family management endpoints.
    /// Handles family creation and join operations.
    /// </summary>
    [ApiController]
    [Route("api/families")]
    [Authorize]
    public class FamiliesController : ControllerBase
    {
        private readonly IFamilyService _familyService;
        private readonly IWebHostEnvironment _env;

        /// <summary>
        /// Initializes a new instance of the FamiliesController class.
        /// </summary>
        /// <param name="familyService">Service for handling family business logic.</param>
        /// <param name="env">Web host environment to determine if running in development mode.</param>
        public FamiliesController(IFamilyService familyService, IWebHostEnvironment env)
        {
            _familyService = familyService;
            _env = env;
        }

        /// <summary>
        /// Creates a new family and associates the authenticated user as a member.
        /// </summary>
        /// <param name="request">Family creation data (Family Name)</param>
        /// <returns>Family information including the invite code</returns>
        /// <response code="200">The family was created successfully.</response>
        /// <response code="400">The data sent is not valid</response>
        /// <response code="401">The user is not authenticated.</response>
        [HttpPost("v1/create")]
        public async Task<IActionResult> CreateFamily([FromBody] CreateFamilyRequest request)
        {
            // Validate that the request contains all required fields
            // In development, return detailed validation errors; in production, return generic message for security
            if (!ModelState.IsValid)
            {
                if (_env.IsDevelopment())
                {
                    return BadRequest(ModelState);
                }
                return BadRequest(new { message = "Invalid family data." });
            }

            // Delegate family creation logic to the service layer
            var result = await _familyService.CreateFamilyAsync(request);

            // Map ServiceResult to appropriate HTTP status code using pattern matching
            // This ensures proper REST semantics are followed
            return result.ResultType switch
            {
                Common.ServiceResultType.BadRequest => BadRequest(new { message = result.ErrorMessage }),   // 400 for invalid data
                Common.ServiceResultType.Unauthorized => Unauthorized(new { message = result.ErrorMessage }), // 401 for auth issues
                _ => Ok(result.Data)                                                                         // 200 with family info on success
            };
        }

        /// <summary>
        /// Allows an authenticated user to join an existing family using an invite code.
        /// </summary>
        /// <param name="request">Join family data (Invite Code)</param>
        /// <returns>Confirmation of successful join operation</returns>
        /// <response code="200">The user joined the family successfully.</response>
        /// <response code="400">The data sent is not valid</response>
        /// <response code="401">The user is not authenticated.</response>
        /// <response code="404">Family with the provided invite code does not exist.</response>
        [HttpPost("v1/join")]
        public async Task<IActionResult> JoinFamily([FromBody] JoinFamilyRequest request)
        {
            // Validate that the request contains all required fields
            // In development, return detailed validation errors; in production, return generic message for security
            if (!ModelState.IsValid)
            {
                if (_env.IsDevelopment())
                {
                    return BadRequest(ModelState);
                }
                return BadRequest(new { message = "Invalid invite code." });
            }

            // Delegate join family logic to the service layer
            var result = await _familyService.JoinFamilyAsync(request);

            // Map ServiceResult to appropriate HTTP status code using pattern matching
            // This ensures proper REST semantics are followed
            return result.ResultType switch
            {
                Common.ServiceResultType.NotFound => NotFound(new { message = result.ErrorMessage }),         // 404 if family doesn't exist
                Common.ServiceResultType.BadRequest => BadRequest(new { message = result.ErrorMessage }),     // 400 for invalid data
                Common.ServiceResultType.Unauthorized => Unauthorized(new { message = result.ErrorMessage }), // 401 for auth issues
                _ => Ok(new { message = "Successfully joined the family." })                                  // 200 for success
            };
        }

        /// <summary>
        /// Allows an authenticated user to leave their current family.
        /// </summary>
        /// <returns>Confirmation of successful leave operation</returns>
        /// <response code="200">The user left the family successfully.</response>
        /// <response code="400">The user is not currently part of any family.</response>
        /// <response code="401">The user is not authenticated.</response>
        [HttpPost("v1/leave")]
        public async Task<IActionResult> LeaveFamily()
        {
            // Delegate leave family logic to the service layer
            // No request body needed as the user ID is obtained from the authentication token
            var result = await _familyService.LeaveFamilyAsync();

            // Map ServiceResult to appropriate HTTP status code using pattern matching
            // This ensures proper REST semantics are followed
            return result.ResultType switch
            {
                Common.ServiceResultType.BadRequest => BadRequest(new { message = result.ErrorMessage }),     // 400 if user has no family
                Common.ServiceResultType.Unauthorized => Unauthorized(new { message = result.ErrorMessage }), // 401 for auth issues
                _ => Ok(new { message = "Successfully left the family." })                                    // 200 for success
            };
        }
    }
}