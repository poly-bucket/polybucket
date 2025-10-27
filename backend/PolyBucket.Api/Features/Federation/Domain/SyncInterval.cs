namespace PolyBucket.Api.Features.Federation.Domain
{
    /// <summary>
    /// Defines the interval for automatic model synchronization
    /// </summary>
    public enum SyncInterval
    {
        /// <summary>
        /// Sync every day
        /// </summary>
        Daily = 0,
        
        /// <summary>
        /// Sync once per week
        /// </summary>
        Weekly = 1,
        
        /// <summary>
        /// Sync once per month
        /// </summary>
        Monthly = 2
    }
}

