namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// Model for looking up, identify or represent a delegation of one or more rights delegated from one party/organization/user to another party/organization/user
    /// </summary>
    public class DelegationLookup
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
        public List<Right> Rights { get; set; }
    }
}
