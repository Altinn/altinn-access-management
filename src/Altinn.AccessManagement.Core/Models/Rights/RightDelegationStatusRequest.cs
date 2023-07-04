using System.ComponentModel.DataAnnotations;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// Request model for a list of all rights for a specific resource, that a user can delegate from a given reportee to a given recipient.
    /// </summary>
    public class RightDelegationStatusRequest
    {
        /// <summary>
        /// Gets or sets the set of Attribute Id and Attribute Value for the entity which are to receive the rights
        /// </summary>
        [Required]
        public List<AttributeMatch> To { get; set; }

        /// <summary>
        /// Gets or sets the set of Attribute Id and Attribute Value for identifying the resource of the rights to be checked
        /// </summary>
        [Required]
        public List<AttributeMatch> Resource { get; set; }
    }
}
