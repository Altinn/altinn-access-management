namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// Response model for the result of a delegation status check, for which rights a user is able to delegate between two parties.
    /// </summary>
    public class DelegationCheckResult : ValidationErrorResult
    {
        /// <summary>
        /// Gets or sets a set of attribute id and attribute value for the party offering rights
        /// </summary>
        public List<AttributeMatch> From { get; set; }

        /// <summary>
        /// Gets or sets a list of right delegation check results
        /// </summary>
        public List<RightDelegationCheckResult> DelegationCheckResults { get; set; }
    }
}
