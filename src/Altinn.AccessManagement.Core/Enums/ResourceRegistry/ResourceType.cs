﻿using System.Runtime.Serialization;
using NpgsqlTypes;

namespace Altinn.AccessManagement.Core.Models.ResourceRegistry
{
    /// <summary>
    /// Enum representation of the different types of resources supported by the resource registry
    /// </summary>
    public enum ResourceType
    {
        [PgName("default")]
        Default = 0,

        [PgName("systemresource")]
        SystemResource = 1,

        [PgName("maskinportenschema")]
        MaskinportenSchema = 2
    }
}
