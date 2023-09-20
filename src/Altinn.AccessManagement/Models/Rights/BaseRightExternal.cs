using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Altinn.AccessManagement.Models
{
    /// <summary>
    /// This model describes a single right
    /// </summary>
    public class BaseRightExternal
    {
        /// <summary>
        /// Gets or sets the list of resource matches which uniquely identifies the resource this right applies to.
        /// </summary>
        [Required]
        public List<AttributeMatchExternal> Resource { get; set; }

        /// <summary>
        /// Gets or sets the set of Attribute Id and Attribute Value for a specific action, to identify the action this right applies to
        /// </summary>
        public string Action { get; set; }
    }
}
