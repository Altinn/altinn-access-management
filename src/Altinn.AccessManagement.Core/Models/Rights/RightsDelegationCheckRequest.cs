using System.ComponentModel.DataAnnotations;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// Request model for a list of all rights for a specific resource, that a user can delegate from a given reportee to a given recipient.
    /// </summary>
    public class RightsDelegationCheckRequest
    {
        /// <summary>
        /// Gets or sets the set of Attribute Id and Attribute Value for the reportee party to delegate rights on behalf of
        /// </summary>
        [Required]
        public List<AttributeMatch> From { get; set; }

        /// <summary>
        /// Gets or sets the set of Attribute Id and Attribute Value for identifying the resource of the rights to be checked
        /// </summary>
        [Required]
        public List<AttributeMatch> Resource { get; set; }
    }
}
