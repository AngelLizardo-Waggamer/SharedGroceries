using BackSharedGroceries.Models;

namespace BackSharedGroceries.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for Family entity data access operations.
    /// </summary>
    public interface IFamilyRepository
    {
        /// <summary>
        /// Retrieves a family by its invite code.
        /// </summary>
        Task<Family?> GetByInviteCodeAsync(string code);

        /// <summary>
        /// Checks if a family exists by its invite code.
        /// </summary>
        Task<bool> ExistsByInviteCodeAsync(string code);

        /// <summary>
        /// Creates a new family in the database.
        /// </summary>
        Task CreateFamilyAsync(Family family);

        /// <summary>
        /// Updates the family association for a user.
        /// </summary>
        Task UpdateUserFamilyAsync(Guid userId, Guid familyId);

        /// <summary>
        /// Retrieves the family ID associated with a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The family ID if the user belongs to a family, otherwise null.</returns>
        Task<Guid?> GetUserFamilyIdAsync(Guid userId);

        /// <summary>
        /// Removes a user from their current family by setting their FamilyId to null.
        /// </summary>
        /// <param name="userId">The ID of the user to remove from family.</param>
        Task RemoveUserFromFamilyAsync(Guid userId);
    }
}