using BackSharedGroceries.Common;
using BackSharedGroceries.DTOs;
using BackSharedGroceries.DTOs.Responses;
using BackSharedGroceries.Helpers.JWT;
using BackSharedGroceries.Interfaces.Repositories;
using BackSharedGroceries.Interfaces.Services;
using BackSharedGroceries.Models;
using System.Security.Cryptography;

namespace BackSharedGroceries.Services
{
    /// <summary>
    /// Service implementation for authentication operations.
    /// Handles the business logic for user registration, login, and token refresh.
    /// This service acts as an intermediary between controllers and repositories, implementing core authentication logic.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ILogger<AuthService> _logger;

        /// <summary>
        /// Initializes a new instance of the AuthService class.
        /// </summary>
        /// <param name="userRepository">Repository for user data access operations.</param>
        /// <param name="refreshTokenRepository">Repository for refresh token data access operations.</param>
        /// <param name="logger">Logger for recording authentication events and errors.</param>
        public AuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user in the system.
        /// Validates that the username is unique, hashes the password using BCrypt, and creates the user record.
        /// </summary>
        /// <param name="request">Registration request containing username and password.</param>
        /// <returns>ServiceResult indicating success or conflict if username already exists.</returns>
        public async Task<ServiceResult> RegisterAsync(RegisterRequest request)
        {
            // Check if a user with this username already exists in the database
            if (await _userRepository.ExistsAsync(request.Username))
            {
                return ServiceResult.Conflict("User already exists.");
            }

            // Create new user entity with hashed password for security
            // BCrypt is used for password hashing as it includes salt and is resistant to rainbow table attacks
            var user = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            // Persist the new user to the database
            await _userRepository.CreateAsync(user);

            // Log successful registration for audit purposes
            _logger.LogInformation("New user registered: {Username}", user.Username);

            return ServiceResult.Ok();
        }

        /// <summary>
        /// Authenticates a user and generates JWT and refresh tokens.
        /// Validates credentials, creates a new device session, invalidates old tokens, and returns authentication tokens.
        /// </summary>
        /// <param name="request">Login request containing username and password.</param>
        /// <returns>ServiceResult with LoginResponse containing tokens and user data, or Unauthorized if credentials are invalid.</returns>
        public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request)
        {
            // Retrieve user from database by username
            var user = await _userRepository.GetByUsernameAsync(request.Username);

            // Validate that user exists and password is correct using BCrypt verification
            // BCrypt.Verify compares the plain text password with the stored hash
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return ServiceResult<LoginResponse>.Unauthorized("Invalid credentials.");
            }

            // Generate a new unique device ID for this login session
            // This is used to track which device the user is currently using and enforce single-device sessions
            user.CurrentDeviceId = Guid.NewGuid();

            // Delete all existing refresh tokens for this user to ensure only one active session
            // This is a security measure to prevent token accumulation and enforce session limits
            await _refreshTokenRepository.DeleteAllForUserAsync(user.Id);

            // Generate a new cryptographically secure refresh token
            // The token is a 64-byte random value encoded as Base64 for safe transmission
            var newRefreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpiresAt = DateTime.UtcNow.AddYears(1), // Long expiration for convenience
                UserId = user.Id
            };

            // Save the new refresh token to the database
            await _refreshTokenRepository.CreateAsync(newRefreshToken);

            // Update user record with the new device ID
            await _userRepository.UpdateAsync(user);

            // Log successful login for security monitoring and audit trail
            _logger.LogInformation("User logged in: {Username}", user.Username);

            // Generate JWT access token containing user claims (ID, username, device ID, family ID)
            var jwtToken = JWTHandler.GenerateToken(user);

            // Return successful result with all authentication data
            return ServiceResult<LoginResponse>.Ok(new LoginResponse
            {
                Token = jwtToken,
                RefreshToken = newRefreshToken.Token,
                Username = user.Username,
                FamilyId = user.FamilyId
            });
        }

        /// <summary>
        /// Refreshes an expired JWT access token using a valid refresh token.
        /// Validates the refresh token and generates a new JWT without requiring the user to log in again.
        /// </summary>
        /// <param name="request">Refresh request containing the refresh token.</param>
        /// <returns>ServiceResult with LoginResponse containing new JWT token, or Unauthorized if refresh token is invalid.</returns>
        public async Task<ServiceResult<LoginResponse>> RefreshTokenAsync(RefreshRequest request)
        {
            // Retrieve the refresh token from database along with associated user data
            var existingToken = await _refreshTokenRepository.GetByTokenWithUserAsync(request.RefreshToken);

            // Validate that the token exists, has not been revoked, and has not expired
            // Multiple conditions are checked to ensure token validity
            if (existingToken == null || existingToken.IsRevoked || existingToken.ExpiresAt <= DateTime.UtcNow)
            {
                return ServiceResult<LoginResponse>.Unauthorized("Invalid or expired session. Please log in again.");
            }

            // Verify that the user has an active device session
            // A null CurrentDeviceId indicates the user's session was terminated or they logged out
            if (existingToken.User.CurrentDeviceId == null)
            {
                return ServiceResult<LoginResponse>.Unauthorized("Invalid session state. Please log in again.");
            }

            // Generate a new JWT access token with refreshed expiration time
            // The refresh token remains the same and can be reused until it expires
            var newToken = JWTHandler.GenerateToken(existingToken.User);

            // Return successful result with the new JWT and existing refresh token
            return ServiceResult<LoginResponse>.Ok(new LoginResponse
            {
                Token = newToken,
                RefreshToken = existingToken.Token, // Same refresh token is reused
                Username = existingToken.User.Username,
                FamilyId = existingToken.User.FamilyId
            });
        }
    }
}
