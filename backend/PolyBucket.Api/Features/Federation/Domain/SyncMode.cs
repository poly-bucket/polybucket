namespace PolyBucket.Api.Features.Federation.Domain
{
    /// <summary>
    /// Defines how model synchronization occurs
    /// </summary>
    public enum SyncMode
    {
        /// <summary>
        /// Models must be manually imported by an admin
        /// </summary>
        Manual = 0,
        
        /// <summary>
        /// Models are automatically synced based on configured interval
        /// </summary>
        Automatic = 1
    }
}

