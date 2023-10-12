using System.ComponentModel.DataAnnotations;

namespace Altinn.AccessManagement.Models
{
    /// <summary>
    /// Response model for the result of a delegation of one or more rights to a recipient.
    /// </summary>
    public class RightsDelegationResponseExternal
    {
        /// <summary>
        /// Gets or sets a set of Attribute Id and Attribute Value for the single entity receiving rights
        /// </summary>
        [Required]
        public List<AttributeMatchExternal> To { get; set; }

        /// <summary>
        /// Gets or sets a list of rights delegation results which is to be delegated to the To recipient.
        /// </summary>
        [Required]
        public List<RightDelegationResultExternal> RightDelegationResults { get; set; }
    }
}
