using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Models;

namespace Altinn.AccessManagement.Mappers
{
    /// <summary>
    /// A class that holds the delegation mapper configuration
    /// </summary>
    public class DelegationMapper : AutoMapper.Profile
    {
        /// <summary>
        /// Configuration for delegation mapper
        /// </summary>
        public DelegationMapper() 
        {
            CreateMap<Delegation, DelegationExternal>();
            CreateMap<Delegation, MPDelegationExternal>()
                .ForMember(dest => dest.SupplierOrg, act => act.MapFrom(src => src.OfferedByOrganizationNumber))
                .ForMember(dest => dest.ConsumerOrg, act => act.MapFrom(src => src.CoveredByOrganizationNumber))
                .ForMember(dest => dest.DelegationSchemeId, act => act.MapFrom(src => src.AltinnIIResourceId))
                .ForMember(dest => dest.Created, act => act.MapFrom(src => src.Created))
                .ForMember(dest => dest.ResourceId, act => act.MapFrom(src => src.ResourceId));
        }
    }
}
