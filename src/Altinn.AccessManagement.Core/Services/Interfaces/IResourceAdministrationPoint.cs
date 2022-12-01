using Altinn.AccessManagement.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.AccessManagement.Core.Services.Interfaces
{
    /// <summary>
    /// Service for creating and updating Resources in AccessMAnagment existing in the ResourceRegister
    /// </summary>
    public interface IResourceAdministrationPoint
    {
        /// <summary>
        /// Creates or Upadetes a list of Resources from ResourceRegister
        /// </summary>
        /// <param name="resources">The resources to add or update</param>
        /// <returns></returns>
        Task<List<AccessManagementResource>> TryWriteResourceFromResourceRegister(List<AccessManagementResource> resources);
    }
}
