namespace BackSharedGroceries.DTOs
{
    /// <summary>
    /// DTO for creating a new shopping list request.
    /// </summary>
    public class CreateShoppingListRequest
    {
        /// <summary>
        /// Name of the shopping list to be created.
        /// </summary>
        /// <example>Weekly Groceries</example>
        public required string Name { get; init; }
    }
}
