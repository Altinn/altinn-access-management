namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// Response model for the list of delegation changes for a resource/app that handles validation errors
    /// </summary>
    public class DelegationChangeList : ValidationErrorResult
    {
        /// <summary>
        /// The list of delegation changes for a resource/app
        /// </summary>
        public List<DelegationChange> DelegationChanges { get; set; }
    }
}
