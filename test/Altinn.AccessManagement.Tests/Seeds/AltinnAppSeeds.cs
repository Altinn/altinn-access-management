#pragma warning disable SA1600

using System;
using System.Collections.Generic;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;

namespace Altinn.AccessManagement.Tests.Seeds;

public static class AltinnAppSeeds
{
    public class AltinnAppBase : ServiceResource, IAccessManagementResource
    {
        public ServiceResource Resource => this;
    }

    public class AltinnApp : AltinnAppBase
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
                Id = "urn:altinn:org",
                Value = "ttd"
            },
            new()
            {
                Id = "urn:altinn:app",
                Value = "am-devtest-person-to-org"
            }
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
