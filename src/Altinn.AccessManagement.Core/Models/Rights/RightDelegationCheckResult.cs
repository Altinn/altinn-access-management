using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Altinn.AccessManagement.Core.Enums;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// Response model describing the delegation status for a given single right, whether the authenticated user is able to delegate the right or not on behalf of the from part, or whether the to recipient party already have the right.
    /// </summary>
    public class RightDelegationCheckResult
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
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DelegableStatus Status { get; set; }

        /// <summary>
        /// Gets or sets a list of details describing the reasons why or why not the right is valid in the current user and reportee party context
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<Detail> Details { get; set; }
    }
}
