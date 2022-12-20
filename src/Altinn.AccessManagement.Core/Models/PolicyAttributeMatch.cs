using System.ComponentModel.DataAnnotations;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// This model extends the AttributeMatch model with a boolean value indicating whether the ABAC found a match for the attribute
    /// </summary>
    public class PolicyAttributeMatch : AttributeMatch
    {
        /// <summary>
        /// Gets or sets a value indicating whether the ABAC found a match for the attribute
        /// </summary>
        [Required]
        public bool? MatchFound { get; set; }
    }
}
