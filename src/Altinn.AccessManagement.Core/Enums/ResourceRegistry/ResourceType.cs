using NpgsqlTypes;

namespace Altinn.AccessManagement.Core.Models.ResourceRegistry
{
    /// <summary>
    /// Enum representation of the different types of resources supported by the resource registry
    /// </summary>
    [Flags]
    public enum ResourceType
    {
        [PgName("default")]
        Default = 0,

        [PgName("systemresource")]
        Systemresource = 1 << 0,

        [PgName("maskinportenschema")]
        MaskinportenSchema = 1 << 1,

        [PgName("altinn2service")]
        Altinn2Service = 1 << 2,

        [PgName("altinnapp")]
        AltinnApp = 1 << 3,

        [PgName("genericaccessresource")]
        GenericAccessResource = 1 << 4,

        [PgName("brokerservice")]
        BrokerService = 1 << 5,

        [PgName("correspondenceservice")]
        CorrespondenceService = 1 << 6,

        All = ~Default
    }
}
