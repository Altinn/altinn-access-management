using System.ComponentModel.DataAnnotations;

namespace Altinn.AccessManagement.Models
{
    /// <summary>
    /// Queries for a list of all rights between two parties for a specific resource.
    /// If coveredby user has any key roles, those party ids should be included in the query to have the 3.0 PIP lookup rights inheirited through those as well.
    /// If offeredby is a sub unit, parenty party id should be supplied to include inheirited rights received through the main unit.
    /// </summary>
    public class RightsQueryExternal
    {
        /// <summary>
        /// Gets or sets the set of Attribute Id and Attribute Value for the entity having offered rights
        /// </summary>
        [Required]
        public List<AttributeMatchExternal> From { get; set; }

        /// <summary>
        /// Gets or sets the set of Attribute Id and Attribute Value for the entity having received rights
        /// </summary>
        [Required]
        public List<AttributeMatchExternal> To { get; set; }

        /// <summary>
        /// Gets or sets the set of Attribute Id and Attribute Value for identifying the resource the rights 
        /// </summary>
        [Required]
        public List<AttributeMatchExternal> Resource { get; set; }
    }
}
