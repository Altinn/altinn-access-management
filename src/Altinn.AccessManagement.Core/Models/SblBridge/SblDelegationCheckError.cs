namespace Altinn.AccessManagement.Core.Models.SblBridge
{
    /// <summary>
    /// Model for error messages from SBL UserDelegationCheck
    /// </summary>
    public class SblDelegationCheckError
    {
        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the model state
        /// </summary>
        public Dictionary<string, List<string>> ModelState { get; set; }
    }
}
