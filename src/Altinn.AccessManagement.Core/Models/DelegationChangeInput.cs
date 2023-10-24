using System.ComponentModel.DataAnnotations;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// Contains attribute match info about user, reportee, resource and resourceMatchType that's being used to check all delegation changes for the resource
    /// </summary>
    public class DelegationChangeInput
    {
        /// <summary>
        /// Id and value of the subject getting delegation changes info
        /// </summary>
        [Required]
        public AttributeMatch Subject { get; set; }

        /// <summary>
        /// Id and value of party
        /// </summary>
        [Required]
        public AttributeMatch Party { get; set; }

        /// <summary>
        /// Gets the Resource's id
        /// </summary>
        [Required]
        public List<AttributeMatch> Resource { get; set; }
    }
}
