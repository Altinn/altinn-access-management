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

        [PgName("altinn2")]
        Altinn2 = 2,

        [PgName("altinn3")]
        Altinn3 = 3,

        [PgName("maskinportenschema")]
        MaskinportenSchema = 4,

        [PgName("api")]
        Api = 5
    }
}
