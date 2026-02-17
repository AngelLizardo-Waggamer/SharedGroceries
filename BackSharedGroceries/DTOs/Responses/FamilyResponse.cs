namespace BackSharedGroceries.DTOs.Responses
{
    /// <summary>
    /// DTO for family information response.
    /// </summary>
    public class FamilyResponse
    {
        /// <summary>
        /// Unique identifier of the family.
        /// </summary>
        public required Guid Id { get; init; }

        /// <summary>
        /// Name of the family.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Generated invite code of the family.
        /// </summary>
        public required string InviteCode { get; init; }
    }
}