namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// Response model for the list of delegation changes for a resource/app that handles validation errors
    /// </summary>
    public class DelegationChangeList : ValidationErrorResult
    {
        /// <summary>
        /// Gets or sets a set of attribute id and attribute value for the party offering rights
        /// </summary>
        public List<DelegationChange> DelegationChanges { get; set; }
    }
}
