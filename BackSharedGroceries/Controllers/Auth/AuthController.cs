using BackSharedGroceries.DTOs;
using BackSharedGroceries.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace BackSharedGroceries.Controllers.Auth
{
    /// <summary>
    /// Controller responsible for authentication endpoints.
    /// Handles user registration, login, and token refresh operations.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IWebHostEnvironment _env;

        /// <summary>
        /// Initializes a new instance of the AuthController class.
        /// </summary>
        /// <param name="authService">Service for handling authentication business logic.</param>
        /// <param name="env">Web host environment to determine if running in development mode.</param>
        public AuthController(IAuthService authService, IWebHostEnvironment env)
        {
            _authService = authService;
            _env = env;
        }

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="request">Registry data (Username and Password)</param>
        /// <returns>Confirmation of registry creation</returns>
        /// <response code="200">The user was registered successfully.</response>
        /// <response code="400">The data sent is not valid</response>
        /// <response code="409">The username is already in use.</response>
        [HttpPost("v1/register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Validate that the request contains all required fields
            // In development, return detailed validation errors; in production, return generic message for security
            if (!ModelState.IsValid)
            {
                if (_env.IsDevelopment())
                {
                    return BadRequest(ModelState);
                }
                return BadRequest("Invalid registering data.");
            }

            // Delegate registration logic to the service layer
            var result = await _authService.RegisterAsync(request);

            // Map ServiceResult to appropriate HTTP status code using pattern matching
            // This ensures proper REST semantics are followed
            return result.ResultType switch
            {
                Common.ServiceResultType.Conflict => Conflict(result.ErrorMessage),      // 409 if username exists
                Common.ServiceResultType.BadRequest => BadRequest(result.ErrorMessage),   // 400 for invalid data
                _ => Ok("User registered successfully.")                                   // 200 for success
            };
        }

        /// <summary>
        /// Authenticates a user and generates a JWT token along with a refresh token.
        /// </summary>
        /// <param name="request">Login data (Username and Password)</param>
        /// <returns>JWT token and refresh token</returns>
        /// <response code="200">The user was authenticated successfully.</response>
        /// <response code="400">The data sent is not valid</response>
        /// <response code="401">The credentials provided are invalid.</response>
        [HttpPost("v1/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Validate request data
            // In production, return generic "Invalid credentials" to avoid information leakage about what's wrong
            if (!ModelState.IsValid)
            {
                if (_env.IsDevelopment())
                {
                    return BadRequest(ModelState);
                }
                return BadRequest("Invalid credentials.");
            }

            // Delegate authentication logic to the service layer
            var result = await _authService.LoginAsync(request);

            // Map ServiceResult to appropriate HTTP status code
            // Returns JWT and refresh token on success, or appropriate error code on failure
            return result.ResultType switch
            {
                Common.ServiceResultType.Unauthorized => Unauthorized(result.ErrorMessage), // 401 for invalid credentials
                Common.ServiceResultType.BadRequest => BadRequest(result.ErrorMessage),      // 400 for malformed request
                Common.ServiceResultType.NotFound => NotFound(result.ErrorMessage),          // 404 if needed
                _ => Ok(result.Data)                                                         // 200 with tokens on success
            };
        }

        /// <summary>
        /// Refreshes a JWT token using a valid refresh token.
        /// </summary>
        /// <param name="request">Refresh token data</param>
        /// <returns>New JWT token and refresh token</returns>
        /// <response code="200">The token was refreshed successfully.</response>
        /// <response code="400">The data sent is not valid</response>
        /// <response code="401">The refresh token is invalid or expired.</response>
        [HttpPost("v1/refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            // Validate that the refresh token was provided in the request
            if (!ModelState.IsValid)
            {
                if (_env.IsDevelopment())
                {
                    return BadRequest(ModelState);
                }
                return BadRequest("Invalid credentials.");
            }

            // Delegate token refresh logic to the service layer
            var result = await _authService.RefreshTokenAsync(request);
            
            // Map ServiceResult to appropriate HTTP status code
            // Returns new JWT token on success, or error if refresh token is invalid/expired
            return result.ResultType switch
            {
                Common.ServiceResultType.Unauthorized => Unauthorized(result.ErrorMessage), // 401 for invalid/expired token
                Common.ServiceResultType.BadRequest => BadRequest(result.ErrorMessage),      // 400 for malformed request
                Common.ServiceResultType.NotFound => NotFound(result.ErrorMessage),          // 404 if needed
                _ => Ok(result.Data)                                                         // 200 with new JWT on success
            };
        }
    }
}