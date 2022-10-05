using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;
using JsonConverterAttribute = System.Text.Json.Serialization.JsonConverterAttribute;

namespace Altinn.AuthorizationAdmin.Core.Models.ResourceRegistry
{
    /// <summary>
    /// Model describing a complete resource from the resrouce registry
    /// </summary>
    public class ServiceResource
    {
        /// <summary>
        /// The identifier of the resource
        /// </summary>
        [JsonProperty]
        public string Identifier { get; set; }

        /// <summary>
        /// The title of service
        /// </summary>
        [JsonProperty]
        public Dictionary<string, string> Title { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [JsonProperty]
        public Dictionary<string, string> Description { get; set; }

        /// <summary>
        /// Description explaining the rights a recipient will receive if given access to the resource
        /// </summary>
        [JsonProperty]
        public Dictionary<string, string> RightDescription { get; set;  }

        /// <summary>
        /// The homepage
        /// </summary>
        [JsonProperty]
        public string Homepage { get; set; }

        /// <summary>
        /// The status
        /// </summary>
        [JsonProperty]
        public string Status { get; set; }

        /// <summary>
        /// When the resource is available from
        /// </summary>
        [JsonProperty]
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// When the resource is available to
        /// </summary>
        [JsonProperty]
        public DateTime ValidTo { get; set; }

        /// <summary>
        /// IsPartOf
        /// </summary>
        [JsonProperty]
        public string IsPartOf { get; set; }

        /// <summary>
        /// IsPublicService
        /// </summary>
        [JsonProperty]
        public bool IsPublicService { get; set; }

        /// <summary>
        /// ThematicArea
        /// </summary>
        [JsonProperty]
        public string? ThematicArea { get; set; }

        /// <summary>
        /// ResourceReference
        [JsonProperty]
        public ResourceReference? ResourceReference { get; set;  }

        /// <summary>
        /// IsComplete
        [JsonProperty]
        public bool? IsComplete { get; set; }

        /// <summary>
        /// HasCompetentAuthority
        /// </summary>
        [JsonProperty]
        public CompetentAuthority HasCompetentAuthority { get; set; }

        /// <summary>
        /// Keywords
        /// </summary>
        [JsonProperty]
        public List<Keyword> Keywords { get; set; }

        /// <summary>
        /// Sector
        /// </summary>
        [JsonProperty]
        public List<string> Sector { get; set; }

        /// <summary>
        /// ResourceType
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ResourceType ResourceType { get; set; }
    }
}
