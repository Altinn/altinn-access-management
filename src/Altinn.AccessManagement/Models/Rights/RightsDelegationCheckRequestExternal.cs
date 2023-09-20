using System.ComponentModel.DataAnnotations;

namespace Altinn.AccessManagement.Models
{
    /// <summary>
    /// Request model for a list of all rights for a specific resource, that a user can delegate from a given reportee to a given recipient.
    /// </summary>
    public class RightsDelegationCheckRequestExternal
    {
        /// <summary>
        /// Gets or sets the set of Attribute Id and Attribute Value for identifying the resource of the rights to be checked
        /// </summary>
        [Required]
        public List<AttributeMatchExternal> Resource { get; set; }
    }
}
