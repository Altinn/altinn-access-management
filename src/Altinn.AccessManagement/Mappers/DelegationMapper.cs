using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
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
            CreateMap<ServiceResource, ServiceResourceExternal>();
            CreateMap<ServiceResource, ServiceResourceExternal>()
                .ForMember(dest => dest.Identifier, act => act.MapFrom(src => src.Identifier))
                .ForMember(dest => dest.Title, act => act.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, act => act.MapFrom(src => src.Description))
                .ForMember(dest => dest.ValidFrom, act => act.MapFrom(src => src.ValidFrom))
                .ForMember(dest => dest.ValidTo, act => act.MapFrom(src => src.ValidTo))
                .ForMember(dest => dest.Status, act => act.MapFrom(src => src.Status))
                .ForMember(dest => dest.ResourceType, act => act.MapFrom(src => src.ResourceType));
        }
    }
}
