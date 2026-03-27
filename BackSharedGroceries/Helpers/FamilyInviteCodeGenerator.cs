using BackSharedGroceries.Data;
using BackSharedGroceries.Interfaces;
using BackSharedGroceries.Interfaces.Repositories;

namespace BackSharedGroceries.Helpers
{
    /// <summary>
    /// Service responsible for generating unique invite codes for families.
    /// Ensures that generated codes are random and do not conflict with existing codes in the database.
    /// </summary>
    public class FamilyInviteCodeGenerator : IFamilyInviteCodeGenerator
    {
        /// <summary>
        /// Random number generator used to create random alphanumeric invite codes.
        /// </summary>
        private readonly Random _random = new();

        /// <summary>
        /// Repository for accessing family data, used to verify code uniqueness.
        /// </summary>
        private readonly IFamilyRepository _familyRepository;

        /// <summary>
        /// Initializes a new instance of the FamilyInviteCodeGenerator class.
        /// </summary>
        /// <param name="familyRepository">The family repository used to check for existing invite codes.</param>
        public FamilyInviteCodeGenerator(IFamilyRepository familyRepository)
        {
            _familyRepository = familyRepository;
        }

        /// <summary>
        /// Generates a unique 6-character alphanumeric invite code for a family.
        /// The code consists of uppercase letters (A-Z) and digits (0-9).
        /// Continuously generates new codes until a unique one is found that doesn't exist in the database.
        /// </summary>
        /// <returns>A unique 6-character invite code as a string.</returns>
        public async Task<string> GenerateInviteCode()
        {
            // Define the character set for the invite code: uppercase letters and digits.
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string code;
            bool exists;

            // Generate codes in a loop until we find one that doesn't exist in the database.
            do
            {
                // Generate a random 6-character code by selecting random characters from the character set.
                code = new string(Enumerable.Repeat(chars, 6)
                    .Select(s => s[_random.Next(s.Length)]).ToArray());

                // Check if the generated code already exists in the database.
                exists = await _familyRepository.ExistsByInviteCodeAsync(code);
            } while (exists); // Continue generating if the code already exists.

            // Return the unique code.
            return code;
        }
    }
}