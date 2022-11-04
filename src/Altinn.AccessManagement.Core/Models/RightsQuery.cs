using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// Queries for a list of all rights between two parties for a specific resource.
    /// If coveredby user has any key roles, those party ids should be included in the query to have the 3.0 PIP lookup rights inheirited through those as well.
    /// If offeredby is a sub unit, parenty party id should be supplied to include inheirited rights received through the main unit.
    /// </summary>
    public class RightsQuery
    {
        /// <summary>
        /// Gets or sets the party id for the reportee the rights query is to find rights for
        /// </summary>
        [Required]
        public int From { get; set; }

        /// <summary>
        /// Gets or sets the set of Attribute Id and Attribute Value for the coveredby id
        /// </summary>
        [Required]
        public List<AttributeMatch> Reportee { get; set; }

        /// <summary>
        /// Gets or sets the resource match the rights query is for
        /// </summary>
        [Required]
        public List<AttributeMatch> Resource { get; set; }
    }
}
