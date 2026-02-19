namespace BackSharedGroceries.DTOs.Responses
{
    /// <summary>
    /// Result of a batch synchronization operation.
    /// </summary>
    public class SyncResultDto
    {
        /// <summary>
        /// IDs of products that were successfully created or updated.
        /// </summary>
        public List<Guid> Synced { get; set; } = new();

        /// <summary>
        /// IDs of products that were ignored due to stale client timestamps.
        /// </summary>
        public List<Guid> Ignored { get; set; } = new();

        /// <summary>
        /// Total number of products processed.
        /// </summary>
        public int TotalProcessed { get; set; }
    }
}
