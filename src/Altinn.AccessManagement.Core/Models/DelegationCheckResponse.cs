namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// Response model for the result of a delegation status check, for which rights a user is able to delegate between two parties.
    /// </summary>
    public class DelegationCheckResponse : ValidationErrorResult
    {
        /// <summary>
        /// Gets or sets a set of attribute id and attribute value for the party offering rights
        /// </summary>
        public List<AttributeMatch> From { get; set; }

        /// <summary>
        /// Gets or sets a list of right delegation status models
        /// </summary>
        public List<RightDelegationCheckResult> RightDelegationCheckResults { get; set; }
    }
}
