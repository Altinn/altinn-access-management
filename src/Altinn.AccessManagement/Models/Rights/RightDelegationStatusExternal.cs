using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Altinn.AccessManagement.Enums;

namespace Altinn.AccessManagement.Models
{
    /// <summary>
    /// Response model describing the delegation status for a given single right, whether the authenticated user is able to delegate the right or not on behalf of the from part, or whether the to recipient party already have the right.
    /// </summary>
    public class RightDelegationStatusExternal
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
        public List<AttributeMatchExternal> Resource { get; set; }

        /// <summary>
        /// Gets or sets the set of Attribute Id and Attribute Value for a specific action, to identify the action this right applies to
        /// </summary>
        [Required]
        public AttributeMatchExternal Action { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the right is delegable or not
        /// </summary>
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DelegableStatusExternal Status { get; set; }

        /// <summary>
        /// Gets or sets a list of reasons why or why not the right is valid in the current user and reportee party context
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<RightReasonExternal> Reasons { get; set; }
    }
}
