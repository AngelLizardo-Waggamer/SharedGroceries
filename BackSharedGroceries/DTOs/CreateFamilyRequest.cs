namespace BackSharedGroceries.DTOs
{
    /// <summary>
    /// DTO for creating a new family request.
    /// </summary>
    public class CreateFamilyRequest
    {
        /// <summary>
        /// Name of the family to be created.
        /// </summary>
        /// <example>Best Family</example>
        public required string FamilyName { get; init; }
    }
}