namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// Enum for determining the status of an Authorization Request 
    /// </summary>
    public enum RestAuthorizationRequestStatus
    {
        /// <summary>
        /// Should not be used as a status for an AuthorizationRequest.
        /// </summary>
        None = 0,

        /// <summary>
        /// Used when a AuthorizationRequest is unopened.
        /// </summary>
        Unopened = 1,

        /// <summary>
        /// Used when a AuthorizationRequest is opened.
        /// </summary>
        Opened = 2,

        /// <summary>
        /// Used when a AuthorizationRequest is accepted.
        /// </summary>
        Accepted = 3,

        /// <summary>
        /// Used when a AuthorizationRequest is rejected.
        /// </summary>
        Rejected = 4,

        /// <summary>
        /// Used when a AuthorizationRequest is created.
        /// </summary>
        Created = 6
    }
}
