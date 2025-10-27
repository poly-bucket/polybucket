namespace PolyBucket.Api.Features.Federation.Domain
{
    /// <summary>
    /// Represents the current status of a federation handshake
    /// </summary>
    public enum HandshakeStatus
    {
        /// <summary>
        /// Handshake has been initiated, awaiting response
        /// </summary>
        Initiated = 0,
        
        /// <summary>
        /// Instance has accepted the handshake request
        /// </summary>
        Accepted = 1,
        
        /// <summary>
        /// Tokens have been exchanged between both instances
        /// </summary>
        TokenExchanged = 2,
        
        /// <summary>
        /// Catalog has been shared between instances
        /// </summary>
        CatalogShared = 3,
        
        /// <summary>
        /// Models have been selected by admins
        /// </summary>
        ModelsSelected = 4,
        
        /// <summary>
        /// Handshake completed successfully
        /// </summary>
        Completed = 5,
        
        /// <summary>
        /// Handshake was rejected by the responder
        /// </summary>
        Rejected = 6,
        
        /// <summary>
        /// Handshake expired before completion
        /// </summary>
        Expired = 7,
        
        /// <summary>
        /// Handshake failed due to an error
        /// </summary>
        Failed = 8
    }
}

