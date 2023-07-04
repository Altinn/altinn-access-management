using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Enums;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// Response model describing the delegation status for a given single right, whether the authenticated user is able to delegate the right or not on behalf of the from part, or whether the to recipient party already have the right.
    /// </summary>
    public class RightDelegationStatus
    {
        /// <summary>
        /// Gets or sets the right key
        /// </summary>
        [Required]
        public string RightKey { get; set; }

        /// <summary>
        /// Gets or sets the set of Attribute Id and Attribute Value for identifying the resource the rights 
        /// </summary>
        [Required]
        public List<AttributeMatch> Resource { get; set; }

        /// <summary>
        /// Gets or sets the set of Attribute Id and Attribute Value for a specific action, to identify the action this right applies to
        /// </summary>
        [Required]
        public AttributeMatch Action { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the right is delegable or not
        /// </summary>
        [Required]
        public DelegableStatus Status { get; set; }

        /// <summary>
        /// Gets or sets a code identifying the reason behind the status
        /// </summary>
        public string ReasonCode { get; set; }

        /// <summary>
        /// Gets or sets a human readable description of the reason code
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of parameters used in the reason description. 
        /// </summary>
        public Dictionary<string, string> ReasonParams { get; set; }
    }
}
