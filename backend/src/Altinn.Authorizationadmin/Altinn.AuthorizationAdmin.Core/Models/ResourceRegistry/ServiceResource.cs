using Altinn.AuthorizationAdmin.Core.Enums.ResourceRegistry;
using System.Text.Json.Serialization;

namespace Altinn.AuthorizationAdmin.Core.Models.ResourceRegistry
{
    public class ServiceResource
    {
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
        /// 
        /// </summary>
        public Dictionary<string, string> RightDescription { get; set;  }

        /// <summary>
        /// 
        /// </summary>
        public string Homepage { get; set; }    

        /// <summary>
        /// 
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime ValidFrom { get; set; } 

        /// <summary>
        /// 
        /// </summary>
        public DateTime ValidTo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string IsPartOf { get; set; }


        public bool IsPublicService { get; set; }

        public string? ThematicArea { get; set; }

        public ResourceReference? ResourceReference { get; set;  }

        public bool? IsComplete { get; set; }

        public CompetentAuthority HasCompetentAuthority { get; set; }

        public List<Keyword> Keywords { get; set; }

        public List<string> Sector { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ResourceType ResourceType { get; set; }
    }
}
