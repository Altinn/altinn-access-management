﻿using Altinn.AccessManagement.Core.Models;
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
            CreateMap<Party, PartyExternal>();
            CreateMap<Delegation, DelegationExternal>();
            CreateMap<Delegation, MaskinportenSchemaDelegationExternal>();
            CreateMap<Delegation, MPDelegationExternal>()
                .ForMember(dest => dest.SupplierOrg, act => act.MapFrom(src => src.CoveredByOrganizationNumber))
                .ForMember(dest => dest.ConsumerOrg, act => act.MapFrom(src => src.OfferedByOrganizationNumber))
                .ForMember(dest => dest.DelegationSchemeId, act => act.MapFrom(src => src.ResourceReferences.Find(rf => rf.ReferenceType == ReferenceType.DelegationSchemeId).Reference))
                .ForMember(dest => dest.Scopes, act => act.MapFrom(src => src.ResourceReferences.Where(rf => string.Equals(rf.ReferenceType, ReferenceType.MaskinportenScope)).Select(rf => rf.Reference).ToList()))
                .ForMember(dest => dest.Created, act => act.MapFrom(src => src.Created))
                .ForMember(dest => dest.ResourceId, act => act.MapFrom(src => src.ResourceId));
            CreateMap<ServiceResource, ServiceResourceExternal>();
            CreateMap<CompetentAuthority, CompetentAuthorityExternal>()
                .ForMember(dest => dest.Orgcode, act => act.MapFrom(src => src.Orgcode))
                .ForMember(dest => dest.Organization, act => act.MapFrom(src => src.Organization))
                .ForMember(dest => dest.Name, act => act.MapFrom(src => src.Name));
            CreateMap<ResourceReference, ResourceReferenceExternal>()
                .ForMember(dest => dest.ReferenceType, act => act.MapFrom(src => src.ReferenceType))
                .ForMember(dest => dest.ReferenceSource, act => act.MapFrom(src => src.ReferenceSource))
                .ForMember(dest => dest.Reference, act => act.MapFrom(src => src.Reference));
            CreateMap<AttributeMatch, AttributeMatchExternal>();
            CreateMap<AttributeMatchExternal, AttributeMatch>();
            CreateMap<PolicyAttributeMatch, PolicyAttributeMatchExternal>();
            CreateMap<PolicyAttributeMatchExternal, PolicyAttributeMatch>();

            // Rights
            CreateMap<RightsQueryExternal, RightsQuery>();
            CreateMap<RightSource, RightSourceExternal>();
            CreateMap<RightSourceExternal, RightSource>();
            CreateMap<Right, RightExternal>()
                .ForMember(dest => dest.Action, act => act.MapFrom(src => src.Action.Value));
            CreateMap<BaseRightExternal, Right>()
                .ForMember(dest => dest.Action, opt => opt.MapFrom(src =>
                    new AttributeMatch
                    {
                        Id = "urn:oasis:names:tc:xacml:1.0:action:action-id",
                        Value = src.Action
                    }));
            CreateMap<Right, BaseRightExternal>()
                .ForMember(dest => dest.Action, act => act.MapFrom(src => src.Action.Value));
            
            // Delegation
            CreateMap<DelegationInputExternal, DelegationLookup>();
            CreateMap<DelegationActionResult, DelegationOutputExternal>()
                .ForMember(dest => dest.RightDelegationResults, act => act.MapFrom(src => src.Rights));
            CreateMap<RevokeOfferedDelegationExternal, DelegationLookup>();
            CreateMap<RevokeReceivedDelegationExternal, DelegationLookup>();
        }
    }
}
