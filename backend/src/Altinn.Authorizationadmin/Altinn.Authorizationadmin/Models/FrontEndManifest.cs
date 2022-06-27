using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.AuthorizationAdmin.Models
{
    public class RootEntrypoint
    {
        public string? file { get; set; }
        public string? src { get; set; }
        public bool? isEntry { get; set; }
        public List<string>? css { get; set; }
        public List<string>? assets { get; set; }
    }

    public class FrontEndManifest
    {
        [JsonPropertyName("entrypoint.js")]
        public RootEntrypoint? Entrypoint { get; set; }
    }
}
