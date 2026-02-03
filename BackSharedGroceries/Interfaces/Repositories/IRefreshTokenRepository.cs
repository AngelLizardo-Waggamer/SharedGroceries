using BackSharedGroceries.Models;

namespace BackSharedGroceries.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for RefreshToken entity data access operations.
    /// </summary>
    public interface IRefreshTokenRepository
    {
        /// <summary>
        /// Retrieves a refresh token by its token string, including the associated user.
        /// </summary>
        Task<RefreshToken?> GetByTokenWithUserAsync(string token);

        /// <summary>
        /// Retrieves all refresh tokens for a specific user.
        /// </summary>
        Task<List<RefreshToken>> GetByUserIdAsync(Guid userId);

        /// <summary>
        /// Creates a new refresh token in the database.
        /// </summary>
        Task<RefreshToken> CreateAsync(RefreshToken token);

        /// <summary>
        /// Deletes all refresh tokens for a specific user.
        /// </summary>
        Task DeleteAllForUserAsync(Guid userId);
    }
}
