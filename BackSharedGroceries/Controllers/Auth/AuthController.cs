using BackSharedGroceries.Data;
using BackSharedGroceries.Data.DTOs;
using Microsoft.AspNetCore.Mvc;
using BackSharedGroceries.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;

namespace BackSharedGroceries.Controllers.Auth
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(AppDbContext context, ILogger<AuthController> logger, IWebHostEnvironment env) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<AuthController> _logger = logger;
        private readonly IWebHostEnvironment _env = env;

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
            // If the model is not valid, which means that required fields are missing or the data is not valid, return a BadRequest with the model state details.
            if (!ModelState.IsValid)
            {
                if (_env.IsDevelopment())
                {
                    return BadRequest(ModelState);
                } else
                {
                    return BadRequest("Invalid registering data.");
                }
            }

            // Declare the user variable, which will be used later IF the user does not exist.
            User? user = await _context.Users.FindAsync(request.Username);

            // If the user already exists, which means that the value of user is not null, return a conflict response.
            if (user != null)
            {
                return Conflict("User already exists.");
            }

            // Start a transaction to ensure data integrity during the user creation.
            using var transaction = await _context.Database.BeginTransactionAsync();

            // At this point, the user is confirmed to not exist, so a new user object is created with the data provided in the request.
            user = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            // Add the new user to the database
            _context.Users.Add(user);

            // Save the changes to the database and commit the transaction.
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Log the successful creation of the user.
            _logger.LogInformation("New user registered: {Username}", user.Username);

            return Ok("User registered successfully.");
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
            // If the model is not valid, which means that required fields are missing or the data is not valid, return a BadRequest.
            // In this case, because it is a login, the model state details are not returned to avoid leaking information. Instead, a general message is provided.
            // BUT, if the environment is development, return the model state for better bug tracking.
            if (!ModelState.IsValid)
            {
                if (_env.IsDevelopment())
                {
                    return BadRequest(ModelState);
                } else
                {
                    return BadRequest("Invalid credentials.");    
                }
            }

            // Declare the user variable that will contain the user data IF the user exists.
            User? user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            // If the user does not exist or the password does not match, return an Unauthorized response.
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid credentials.");
            }

            // Start a transaction to ensure data integrity during the login process.
            using var transaction = await _context.Database.BeginTransactionAsync();

            // Generate a Guid for the device in which the user is loggin in
            user.CurrentDeviceId = Guid.NewGuid();

            // Clean old refresh tokens for the user
            List<RefreshToken> oldTokens = await _context.RefreshTokens.Where(t => t.UserId == user.Id).ToListAsync();
            _context.RefreshTokens.RemoveRange(oldTokens);

            // Generate new refresh token for the user
            RefreshToken newRefreshToken = new()
            {
              Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
              ExpiresAt = DateTime.UtcNow.AddYears(1),
              UserId = user.Id  
            };

            // Add the new refresh token to the database
            _context.RefreshTokens.Add(newRefreshToken);

            // Save the changes to the database and commit the transaction.
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Log the successful login of the user.
            _logger.LogInformation("User logged in: {Username}", user.Username);
            
            // Generate the JWT for the user.
            string token = JWTHandler.GenerateToken(user);

            // Return the token to the user.
            return Ok(new
            {
                Token = token,
                RefreshToken = newRefreshToken.Token,
                user.FamilyId, // Inferred Name = FamilyId
                user.Username // Inferred Name = Username
            });
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

            // Evaluate the model state. If the data was not sent correctly return a BadRequest.
            // As in login endpoint, do not return the model state details unless in development environment.
            if (!ModelState.IsValid)
            {
                if (_env.IsDevelopment())
                {
                    return BadRequest(ModelState);
                } else
                {
                    return BadRequest("Invalid credentials.");    
                }
            }

            // Search for the refresh token in the database
            RefreshToken? existingToken = await _context.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == request.RefreshToken);

            // Validate that the token exists, that is not revoked and not expired
            if (existingToken == null || existingToken.IsRevoked || existingToken.ExpiresAt <= DateTime.UtcNow)
            {
                return Unauthorized("Invalid or expired session. Please log in again.");
            }

            // Other thing that is primordial to check is whether the user has a current device ID assigned.
            if (existingToken.User.CurrentDeviceId == null)
            {
                return Unauthorized("Invalid session state. Please log in again.");
            }

            // At this point, the refresh token is valid, so proceed to generate a new JWT token for the user.
            string newToken = JWTHandler.GenerateToken(existingToken.User);

            // Return the new token to the user
            return Ok(new
            {
                Token = newToken,
                RefreshToken = existingToken.Token
            });
        }
    }
}