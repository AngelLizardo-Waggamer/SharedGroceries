using BackSharedGroceries.Data;
using BackSharedGroceries.Interfaces.Repositories;
using BackSharedGroceries.Models;
using Microsoft.EntityFrameworkCore;

namespace BackSharedGroceries.Repositories
{
    /// <summary>
    /// Repository implementation for Family entity data access operations.
    /// Implements the Repository pattern to abstract database access and provide a clean API for Family-related queries.
    /// </summary>
    public class FamilyRepository : IFamilyRepository
    {
        private readonly AppDbContext _context;

        public FamilyRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new family in the database.
        /// </summary>
        /// <param name="family">The family entity to create.</param>
        public async Task CreateFamilyAsync(Family family)
        {
            await _context.Families.AddAsync(family);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Checks if a family exists by its invite code.
        /// </summary>
        /// <param name="code">The invite code to check for existence.</param>
        /// <returns>True if a family with the given invite code exists, otherwise false.</returns>
        public async Task<bool> ExistsByInviteCodeAsync(string code)
        {
            return await _context.Families
                .AnyAsync(f => f.FamilyInviteCode == code);
        }

        /// <summary>
        /// Retrieves a family data by its invite code.
        /// </summary>
        /// <param name="code">The invite code of the family to retrieve.</param>
        /// <returns>The family entity if found, otherwise null.</returns>
        public async Task<Family?> GetByInviteCodeAsync(string code)
        {
            return await _context.Families
                .FirstOrDefaultAsync(f => f.FamilyInviteCode == code);
        }

        /// <summary>
        /// Updates the family association for a user.
        /// </summary>
        /// <param name="userId">The ID of the user to update.</param>
        /// <param name="familyId">The ID of the family to associate with the user.</param>
        public async Task UpdateUserFamilyAsync(Guid userId, Guid familyId)
        {
            await _context.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(u => u.SetProperty(user => user.FamilyId, familyId));
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves the family ID associated with a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The family ID if the user belongs to a family, otherwise null.</returns>
        public async Task<Guid?> GetUserFamilyIdAsync(Guid userId)
        {
            // Query the user and retrieve only the FamilyId property
            // Returns null if user is not found or has no family assigned
            return await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.FamilyId)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Removes a user from their current family by setting their FamilyId to null.
        /// </summary>
        /// <param name="userId">The ID of the user to remove from family.</param>
        public async Task RemoveUserFromFamilyAsync(Guid userId)
        {
            // Update the user's FamilyId to null, effectively removing them from their family
            // Uses ExecuteUpdateAsync for efficient bulk update without loading the entity
            await _context.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(u => u.SetProperty(user => user.FamilyId, (Guid?)null));
            await _context.SaveChangesAsync();
        }
    }
}