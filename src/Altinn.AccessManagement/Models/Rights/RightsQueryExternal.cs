using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using AutoMapper;

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

        /// <summary>
        /// Method to convert the external rights query to internal rights query
        /// </summary>
        /// <param name="mapper">mapper to use for subtypes</param>
        /// <returns>RightsQuery</returns>
        public RightsQuery ToRightsQueryInternal(IMapper mapper)
        {
            return new RightsQuery
            {
                Type = RightsQueryType.User,
                From = mapper.Map<List<AttributeMatch>>(From),
                To = mapper.Map<List<AttributeMatch>>(To),
                Resource = new ServiceResource
                {
                    Identifier = GetResourceIdentifier(Resource),
                    AuthorizationReference = mapper.Map<List<AttributeMatch>>(Resource)
                }
            };
        }

        private static string GetResourceIdentifier(List<AttributeMatchExternal> resource)
        {
            return resource.Find(r => r.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceRegistryAttribute)?.Value ??
                $"app_{resource.Find(r => r.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute)?.Value}_{resource.Find(r => r.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute)?.Value}";
        }
    }
}
