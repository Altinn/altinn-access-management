namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// Model for performing a delegation of one or more rights to a recipient.
    /// </summary>
    public class DelegationInput
    {
        /// <summary>
        /// Gets or sets a set of Attribute Id and Attribute Value for the single entity receiving rights
        /// </summary>
        public List<AttributeMatch> To { get; set; }

        /// <summary>
        /// Gets or sets a list of rights which is to be delegated to the To recipient.
        /// NOTE:
        /// If the right only specifies the top-level resource identifier or org/app without an action specification,
        /// delegation will find and delegate all the rights the delegating user have for the resource.
        /// </summary>
        public List<Right> Rights { get; set; }
    }
}
