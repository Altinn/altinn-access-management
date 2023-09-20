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
        Systemresource = 1,

        [PgName("maskinportenschema")]
        MaskinportenSchema = 2,

        [PgName("altinn2service")]
        Altinn2Service = 4,

        [PgName("altinnapp")]
        AltinnApp = 8,

        [PgName("genericaccessresource")]
        GenericAccessResource = 16,
    }
}
