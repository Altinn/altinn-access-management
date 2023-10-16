namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// Response model for the result of a delegation or revoke of one or more rights between two parties.
    /// </summary>
    public class DelegationActionResult : ValidationErrorResult
    {
        /// <summary>
        /// Gets or sets a set of attribute id and attribute value for the party offering rights
        /// </summary>
        public List<AttributeMatch> From { get; set; }

        /// <summary>
        /// Gets or sets a set of attribute id and attribute value for the party receiving rights
        /// </summary>
        public List<AttributeMatch> To { get; set; }

        /// <summary>
        /// Gets or sets a list of rights
        /// </summary>
        public List<RightDelegationResult> Rights { get; set; }
    }
}
