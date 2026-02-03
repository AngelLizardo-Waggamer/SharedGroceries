using BackSharedGroceries.Common;
using BackSharedGroceries.DTOs;
using BackSharedGroceries.DTOs.Responses;

namespace BackSharedGroceries.Interfaces.Services
{
    /// <summary>
    /// Service interface for authentication operations.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="request">Registration request containing username and password.</param>
        /// <returns>Service result indicating success or failure.</returns>
        Task<ServiceResult> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// Authenticates a user and returns JWT and refresh tokens.
        /// </summary>
        /// <param name="request">Login request containing username and password.</param>
        /// <returns>Service result with login response data.</returns>
        Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request);

        /// <summary>
        /// Refreshes an expired JWT using a valid refresh token.
        /// </summary>
        /// <param name="request">Refresh request containing the refresh token.</param>
        /// <returns>Service result with new JWT token.</returns>
        Task<ServiceResult<LoginResponse>> RefreshTokenAsync(RefreshRequest request);
    }
}
