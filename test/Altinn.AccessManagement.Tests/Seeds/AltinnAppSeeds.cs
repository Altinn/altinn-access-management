using System;
using System.Collections.Generic;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;

namespace Altinn.AccessManagement.Tests.Seeds;

public static class AltinnAppSeeds
{
    public class AltinnApp : ServiceResource
    {
        public new static string Identifier = "app_ttd_am-devtest-person-to-org";

        public new static string Status = "Active";

        public new static bool Delegable = true;

        public new static ResourceType ResourceType = ResourceType.AltinnApp;

        public new static List<ResourceReference> ResourceReferences = new List<ResourceReference>
        {
            new()
            {
                ReferenceSource = ReferenceSource.Altinn3,
                Reference = "ttd/am-devtest-person-to-org",
                ReferenceType = ReferenceType.ApplicationId
            }
        };

        public new static List<AttributeMatch> AuthorizationReference = new List<AttributeMatch>
        {
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
        };

        public static ServiceResource Defaults { get; } = new AltinnApp();

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
