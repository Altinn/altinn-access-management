using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Models;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Mappers
{
    /// <summary>
    /// A class that holds the access management mapper configuration
    /// </summary>
    public class AccessManagementMapper : AutoMapper.Profile
    {
        /// <summary>
        /// Configuration for accessmanagement mapper
        /// </summary>
        public AccessManagementMapper() 
        {
            AllowNullCollections = true;
            CreateMap<Delegation, DelegationExternal>();
            CreateMap<Party, PartyExternal>();
            CreateMap<Delegation, MPDelegationExternal>()
                .ForMember(dest => dest.SupplierOrg, act => act.MapFrom(src => src.OfferedByOrganizationNumber))
                .ForMember(dest => dest.ConsumerOrg, act => act.MapFrom(src => src.CoveredByOrganizationNumber))
                .ForMember(dest => dest.DelegationSchemeId, act => act.MapFrom(src => src.ResourceReferences.Find(rf => rf.ReferenceType == Core.Models.ResourceRegistry.ReferenceType.DelegationSchemeId).Reference))
                .ForMember(dest => dest.Scopes, act => act.MapFrom(src => src.ResourceReferences.Where(rf => string.Equals(rf.ReferenceType, ReferenceType.MaskinportenScope)).Select(rf => rf.Reference).ToList()))
                .ForMember(dest => dest.Created, act => act.MapFrom(src => src.Created))
                .ForMember(dest => dest.ResourceId, act => act.MapFrom(src => src.ResourceId));
            CreateMap<ServiceResource, ServiceResourceExternal>()
                .ForMember(dest => dest.Identifier, act => act.MapFrom(src => src.Identifier))
                .ForMember(dest => dest.Title, act => act.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, act => act.MapFrom(src => src.Description))
                .ForMember(dest => dest.RightDescription, act => act.MapFrom(src => src.RightDescription))
                .ForMember(dest => dest.ValidFrom, act => act.MapFrom(src => src.ValidFrom))
                .ForMember(dest => dest.ValidTo, act => act.MapFrom(src => src.ValidTo))
                .ForMember(dest => dest.Status, act => act.MapFrom(src => src.Status))
                .ForMember(dest => dest.ResourceType, act => act.MapFrom(src => src.ResourceType))
                .ForMember(dest => dest.ResourceReferences, act => act.MapFrom(src => src.ResourceReferences))
                .ForMember(dest => dest.HasCompetentAuthority, act => act.MapFrom(src => src.HasCompetentAuthority));
            CreateMap<CompetentAuthority, CompetentAuthorityExternal>()
                .ForMember(dest => dest.Orgcode, act => act.MapFrom(src => src.Orgcode))
                .ForMember(dest => dest.Organization, act => act.MapFrom(src => src.Organization))
                .ForMember(dest => dest.Name, act => act.MapFrom(src => src.Name));
            CreateMap<ResourceReference, ResourceReferenceExternal>()
                .ForMember(dest => dest.ReferenceType, act => act.MapFrom(src => src.ReferenceType))
                .ForMember(dest => dest.ReferenceSource, act => act.MapFrom(src => src.ReferenceSource))
                .ForMember(dest => dest.Reference, act => act.MapFrom(src => src.Reference));
        }
    }
}
