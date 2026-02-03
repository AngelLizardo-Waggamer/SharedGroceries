using BackSharedGroceries.Data;
using BackSharedGroceries.Interfaces.Repositories;
using BackSharedGroceries.Models;
using Microsoft.EntityFrameworkCore;

namespace BackSharedGroceries.Repositories
{
    /// <summary>
    /// Repository implementation for RefreshToken entity data access operations.
    /// Implements the Repository pattern to abstract database access for refresh token management.
    /// </summary>
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Initializes a new instance of the RefreshTokenRepository class.
        /// </summary>
        /// <param name="context">The database context for data access.</param>
        public RefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a refresh token by its token string, including the associated user entity.
        /// This is useful for validating tokens during the refresh flow.
        /// </summary>
        /// <param name="token">The refresh token string to search for.</param>
        /// <returns>The refresh token with user data if found, otherwise null.</returns>
        public async Task<RefreshToken?> GetByTokenWithUserAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token);
        }

        /// <summary>
        /// Retrieves all refresh tokens associated with a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A list of refresh tokens belonging to the user.</returns>
        public async Task<List<RefreshToken>> GetByUserIdAsync(Guid userId)
        {
            return await _context.RefreshTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

        /// <summary>
        /// Creates a new refresh token in the database.
        /// </summary>
        /// <param name="token">The refresh token entity to create.</param>
        /// <returns>The created refresh token with any auto-generated values populated.</returns>
        public async Task<RefreshToken> CreateAsync(RefreshToken token)
        {
            _context.RefreshTokens.Add(token);
            await _context.SaveChangesAsync();
            return token;
        }

        /// <summary>
        /// Deletes all refresh tokens associated with a specific user.
        /// This is typically used during logout or when invalidating all sessions for security purposes.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose tokens should be deleted.</param>
        public async Task DeleteAllForUserAsync(Guid userId)
        {
            // Retrieve all tokens for the user
            var tokens = await GetByUserIdAsync(userId);
            // Remove them from the database
            _context.RefreshTokens.RemoveRange(tokens);
            await _context.SaveChangesAsync();
        }
    }
}
