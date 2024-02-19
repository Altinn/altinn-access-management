namespace Altinn.AccessManagement.Core.Models.SblBridge
{
    /// <summary>
    /// Model for validation problem error messages from SBL Bridge
    /// </summary>
    public class SblValidationProblemResponse
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
