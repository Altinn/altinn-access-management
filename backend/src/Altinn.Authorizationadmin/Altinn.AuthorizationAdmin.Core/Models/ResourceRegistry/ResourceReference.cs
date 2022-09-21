using Altinn.AuthorizationAdmin.Core.Enums.ResourceRegistry;
using System.Text.Json.Serialization;

namespace Altinn.AuthorizationAdmin.Core.Models.ResourceRegistry
{
    public class ResourceReference
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ReferenceSource? ReferenceSource { get; set; }

        public string? Reference { get; set; }


        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ReferenceType? ReferenceType { get; set; }
    }
}
