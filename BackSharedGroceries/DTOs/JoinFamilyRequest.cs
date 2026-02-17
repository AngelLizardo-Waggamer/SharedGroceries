namespace BackSharedGroceries.DTOs
{
    /// <summary>
    /// DTO for joining a family request.
    /// </summary>
    public class JoinFamilyRequest
    {
        /// <summary>
        /// Invite code of the family to join.
        /// Note: The example should be replaced with a valid invite code.
        /// </summary>
        /// <example>FAMILY</example>
        public required string InviteCode { get; init; }
    }
}