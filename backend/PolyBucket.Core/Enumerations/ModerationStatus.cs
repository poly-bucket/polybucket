namespace Core.Enums
{
    /// <summary>
    /// Represents the moderation status of content in the system
    /// </summary>
    public enum ModerationStatus
    {
        /// <summary>
        /// Content is pending moderation
        /// </summary>
        Pending = 0,
        
        /// <summary>
        /// Content has been approved by moderators
        /// </summary>
        Approved = 1,
        
        /// <summary>
        /// Content has been rejected by moderators
        /// </summary>
        Rejected = 2,
        
        /// <summary>
        /// Content is currently under review
        /// </summary>
        UnderReview = 3,
        
        /// <summary>
        /// Content has been flagged by users
        /// </summary>
        Flagged = 4,
        
        /// <summary>
        /// Content is exempt from moderation
        /// </summary>
        Exempt = 5
    }
} 