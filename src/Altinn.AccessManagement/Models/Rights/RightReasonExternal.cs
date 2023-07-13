namespace Altinn.AccessManagement.Models
{
    /// <summary>
    /// This model describes a reason why a right has been identified as valid or invalid for a given user and reportee party context.
    /// </summary>
    public class RightReasonExternal
    {
        /// <summary>
        /// Gets or sets the reason identifier code
        /// </summary>
        public string ReasonCode { get; set; }

        /// <summary>
        /// Gets or sets a human readable (english) description of the reason
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of parameters which is related to and adds detail to the reason
        /// </summary>
        public Dictionary<string, string> ReasonParams { get; set; } = new Dictionary<string, string>();
    }
}
