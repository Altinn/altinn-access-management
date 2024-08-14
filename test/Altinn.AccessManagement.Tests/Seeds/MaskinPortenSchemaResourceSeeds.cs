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
            public new static readonly string Identifier = "scope-access-schema";

            public new static readonly string Status = "Active";

            public new static readonly bool Delegable = true;

            public new static readonly ResourceType ResourceType = ResourceType.MaskinportenSchema;

            public new static readonly List<AttributeMatch> AuthorizationReference = new List<AttributeMatch>
            {
                new()
                {
                    Id = "urn:altinn:resource",
                    Value = "scope-access-schema"
                }
            };

            public static MaskinPortenSchema Defaults { get; } = new MaskinPortenSchema();

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