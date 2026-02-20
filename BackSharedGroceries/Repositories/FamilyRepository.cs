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
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.FamilyId = familyId;
                await _context.SaveChangesAsync();
            }
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
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.FamilyId = null;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Gets the count of members in a family.
        /// </summary>
        /// <param name="familyId">The ID of the family.</param>
        /// <returns>The number of members in the family.</returns>
        public async Task<int> GetFamilyMemberCountAsync(Guid familyId)
        {
            return await _context.Users
                .Where(u => u.FamilyId == familyId)
                .CountAsync();
        }

        /// <summary>
        /// Deletes a family from the database.
        /// </summary>
        /// <param name="familyId">The ID of the family to delete.</param>
        public async Task DeleteFamilyAsync(Guid familyId)
        {
            var family = await _context.Families.FindAsync(familyId);
            if (family != null)
            {
                _context.Families.Remove(family);
                await _context.SaveChangesAsync();
            }
        }
    }
}