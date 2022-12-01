using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.AccessManagement.Core.Models
{
    
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
        public string ResourceRegistryId { get; set; }

        /// <summary>
        /// The type of resource
        /// </summary>
        public string ResourceType { get; set; }

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
