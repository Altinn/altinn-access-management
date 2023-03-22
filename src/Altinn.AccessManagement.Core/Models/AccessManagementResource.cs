using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// Entity holding resource information for storing in AccessManagement
    /// </summary>
    public class AccessManagementResource
    {
        #nullable enable
        /// <summary>
        /// Primary key created when inserted in Access management
        /// </summary>
        public int? ResourceId { get; set; }
        #nullable disable

        /// <summary>
        /// The resource registry id
        /// </summary>
        [Required]
        public string ResourceRegistryId { get; set; }

        /// <summary>
        /// The type of resource
        /// </summary>
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ResourceType ResourceType { get; set; }

        /// <summary>
        /// When the resource was created in access management
        /// </summary>
        public DateTime? Created { get; set; }

        /// <summary>
        /// The last time modified in access management
        /// </summary>
        public DateTime? Modified { get; set; }
    }
}
