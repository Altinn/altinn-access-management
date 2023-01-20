using System.ComponentModel.DataAnnotations;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// Response model for the result of a delegation of one or more rights to a recipient.
    /// </summary>
    public class DelegationOutput
    {
        /// <summary>
        /// Gets or sets a set of Attribute Id and Attribute Value for the single entity receiving rights
        /// </summary>
        public List<AttributeMatch> To { get; set; }

        /// <summary>
        /// Gets or sets a list of rights which were delegated to the To recipient.
        /// </summary>
        public List<Right> Rights { get; set; }
    }
}
