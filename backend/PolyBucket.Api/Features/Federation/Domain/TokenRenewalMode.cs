namespace PolyBucket.Api.Features.Federation.Domain
{
    /// <summary>
    /// Defines how federation tokens should be renewed
    /// </summary>
    public enum TokenRenewalMode
    {
        /// <summary>
        /// Tokens are automatically renewed 7 days before expiration
        /// </summary>
        Automatic = 0,
        
        /// <summary>
        /// Tokens are renewed at a specific scheduled time
        /// </summary>
        Timed = 1,
        
        /// <summary>
        /// Tokens must be manually renewed by an admin
        /// </summary>
        Manual = 2
    }
}

