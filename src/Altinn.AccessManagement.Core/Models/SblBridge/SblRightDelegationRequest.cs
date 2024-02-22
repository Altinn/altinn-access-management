namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// Model for delegation of rights to SBL Bridge, identify or represent a delegation of one or more rights for an Altinn 2 service to be delegated to a party or user
    /// </summary>
    public class SblRightDelegationRequest
    {
        /// <summary>
        /// Gets or sets the attribute id and attribute value for the user or party receiving rights
        /// </summary>
        public AttributeMatch To { get; set; }

        /// <summary>
        /// Gets or sets a list of rights
        /// </summary>
        public List<Right> Rights { get; set; }
    }
}
