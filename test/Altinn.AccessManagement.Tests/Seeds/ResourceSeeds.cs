using System;
using System.Collections.Generic;
using System.Linq;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;

namespace Altinn.AccessManagement.Tests.Seeds;

public static class ResourceSeeds
{
    public class ResourceBase : ServiceResource, IAccessManagementResource
    {
        public ServiceResource Resource => this;

        public AccessManagementResource DbResource => new AccessManagementResource
        {
            ResourceRegistryId = Resource.ResourceType == ResourceType.AltinnApp ? $"{Resource.AuthorizationReference.First(p => p.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute)}/{Resource.Identifier}" : Resource.Identifier,
            ResourceType = Resource.ResourceType,
        };
    }

    public class ChalkboardResource : ResourceBase
    {
        public new static readonly ResourceType ResourceType = ResourceType.GenericAccessResource;

        public new static readonly string Identifier = "chalkboard";

        public static ChalkboardResource Defaults { get; } = new ChalkboardResource();

        public ChalkboardResource(params Action<ServiceResource>[] modifiers)
        {
            base.ResourceType = ResourceType.Systemresource;
            base.Identifier = Identifier;

            foreach (var modifer in modifiers)
            {
                modifer(this);
            }
        }
    }

    public class MaskinportenSchema : ResourceBase
    {
        public new static readonly string Identifier = "maskinportenschema";

        public new static readonly string Status = "Active";

        public new static readonly bool Delegable = true;

        public new static readonly ResourceType ResourceType = ResourceType.MaskinportenSchema;

        public new static readonly List<ResourceReference> ResourceReferences =
        [
        ];

        public new static readonly List<AttributeMatch> AuthorizationReference =
        [
        ];

        public static MaskinportenSchema Defaults { get; } = new MaskinportenSchema();

        public MaskinportenSchema(params Action<ServiceResource>[] modifiers)
        {
            base.Identifier = Identifier;
            base.Status = Status;
            base.Delegable = Delegable;
            base.ResourceType = ResourceType;
            base.ResourceReferences = ResourceReferences;
            base.AuthorizationReference = AuthorizationReference;

            foreach (var modifer in modifiers)
            {
                modifer(this);
            }
        }
    }

    public class AltinnApp : ResourceBase
    {
        public new static readonly string Identifier = "app_ttd_am-devtest-person-to-org";

        public new static readonly string Status = "Active";

        public new static readonly bool Delegable = true;

        public new static readonly ResourceType ResourceType = ResourceType.AltinnApp;

        public new static readonly List<ResourceReference> ResourceReferences =
        [
            new()
            {
                ReferenceSource = ReferenceSource.Altinn3,
                Reference = "ttd/am-devtest-person-to-org",
                ReferenceType = ReferenceType.ApplicationId
            }
        ];

        public new static readonly List<AttributeMatch> AuthorizationReference =
        [
            new()
            {
                Id = AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute,
                Value = "ttd"
            },
            new()
            {
                Id = AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute,
                Value = "am-devtest-person-to-org"
            },
        ];

        public static AltinnApp Defaults { get; } = new AltinnApp();

        public AltinnApp(params Action<ServiceResource>[] modifiers)
        {
            base.Identifier = Identifier;
            base.Status = Status;
            base.Delegable = Delegable;
            base.ResourceType = ResourceType;
            base.ResourceReferences = ResourceReferences;
            base.AuthorizationReference = AuthorizationReference;

            foreach (var modifer in modifiers)
            {
                modifer(this);
            }
        }
    }
}
