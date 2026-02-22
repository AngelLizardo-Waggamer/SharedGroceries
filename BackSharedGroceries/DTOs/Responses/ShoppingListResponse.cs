namespace BackSharedGroceries.DTOs.Responses
{
    /// <summary>
    /// DTO for shopping list information response.
    /// </summary>
    public class ShoppingListResponse
    {
        /// <summary>
        /// Unique identifier of the shopping list.
        /// </summary>
        public required Guid Id { get; init; }

        /// <summary>
        /// Name of the shopping list.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Indicates whether the shopping list is currently active.
        /// </summary>
        public required bool IsActive { get; init; }

        /// <summary>
        /// Date and time when the shopping list was created.
        /// </summary>
        public required DateTime CreatedAt { get; init; }

        /// <summary>
        /// Unique identifier of the family that owns this shopping list.
        /// </summary>
        public required Guid FamilyId { get; init; }
    }
}
