using BackSharedGroceries.Data;
using BackSharedGroceries.Data.DTOs;
using Microsoft.AspNetCore.Mvc;
using BackSharedGroceries.Models;
using Microsoft.EntityFrameworkCore;

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
        /// Endpoint to register a new user in the system. Requires an object within the body containing the information of RegisterRequest.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // If the model is not valid, which means that required fields are missing or the data is not valid, return a BadRequest with the model state details.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
                user.FamilyId, // Inferred Name = FamilyId
                user.Username // Inferred Name = Username
            });
        }
    }
}