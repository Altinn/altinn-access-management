using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;

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
        /// Gets or sets the type of rights query to perform
        /// </summary>
        public RightsQueryType Type { get; set; }

        /// <summary>
        /// Gets or sets the set of Attribute Id and Attribute Value for the entity having offered rights
        /// </summary>
        public List<AttributeMatch> From { get; set; }

        /// <summary>
        /// Gets or sets the set of Attribute Id and Attribute Value for the entity having received rights
        /// </summary>
        public List<AttributeMatch> To { get; set; }

        /// <summary>
        /// Gets or sets the service resource model of the rights 
        /// </summary>
        public ServiceResource Resource { get; set; }
    }
}
