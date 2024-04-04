using System;
using System.Collections.Generic;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;

namespace Altinn.AccessManagement.Tests.Seeds
{
    public static class MaskinPortenSchemaResourceSeeds
    {
        public class MaskinPortenSchema : ServiceResource
        {
            public new static string Identifier { get; } = "scope-access-schema";

            public new static string Status = "Active";

            public new static bool Delegable = true;

            public new static ResourceType ResourceType = ResourceType.MaskinportenSchema;

            public new static List<AttributeMatch> AuthorizationReference = new List<AttributeMatch>
            {
                new()
                {
                    Id = "urn:altinn:resource",
                    Value = "scope-access-schema"
                }
            };

            public static ServiceResource Defaults { get; } = new MaskinPortenSchema();

            public MaskinPortenSchema(params Action<ServiceResource>[] modifiers)
            {
                base.Identifier = Identifier;
                base.Status = Status;
                base.Delegable = Delegable;
                base.ResourceType = ResourceType;
                base.AuthorizationReference = AuthorizationReference;

                foreach (var modifer in modifiers)
                {
                    modifer(this);
                }
            }
        }
    }
}