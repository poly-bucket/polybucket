namespace Core.Enumerations
{
    public enum PrivacySettings
    {
        /// <summary>
        /// The model is public and can be viewed by anyone.
        /// </summary>
        Public = 1,

        /// <summary>
        /// The model is private and can only be viewed by the owner.
        /// </summary>
        Private = 2,

        /// <summary>
        /// The model is unlisted and can only be viewed by people with the link.
        /// </summary>
        Unlisted = 3
    }
}