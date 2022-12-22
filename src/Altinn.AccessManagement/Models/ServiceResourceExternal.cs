using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;

namespace Altinn.AccessManagement.Models
{
    /// <summary>
    /// This model describes a resource
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ServiceResourceExternal
    {
        /// <summary>
        /// The identifier of the resource
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// The title of service
        /// </summary>
        public Dictionary<string, string> Title { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public Dictionary<string, string> Description { get; set; }

        /// <summary>
        /// The status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// When the resource is available from
        /// </summary>
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// When the resource is available to
        /// </summary>
        public DateTime ValidTo { get; set; }

        /// <summary>
        /// ResourceType
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ResourceType ResourceType { get; set; }

        /// <summary>
        /// HasCompetentAuthority
        /// </summary>
        public CompetentAuthorityExternal HasCompetentAuthority { get; set; }
    }
}
