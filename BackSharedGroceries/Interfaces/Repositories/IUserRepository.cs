using BackSharedGroceries.Models;

namespace BackSharedGroceries.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for User entity data access operations.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        Task<User?> GetByIdAsync(Guid userId);

        /// <summary>
        /// Retrieves a user by their username.
        /// </summary>
        Task<User?> GetByUsernameAsync(string username);

        /// <summary>
        /// Checks if a user with the given username exists.
        /// </summary>
        Task<bool> ExistsAsync(string username);

        /// <summary>
        /// Creates a new user in the database.
        /// </summary>
        Task<User> CreateAsync(User user);

        /// <summary>
        /// Updates an existing user in the database.
        /// </summary>
        Task UpdateAsync(User user);
    }
}
