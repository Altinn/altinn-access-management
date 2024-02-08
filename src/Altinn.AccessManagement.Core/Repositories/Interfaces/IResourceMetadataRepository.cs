using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Core.Repositories.Interfaces
{
    /// <summary>
    /// Repository implementation for PostgreSQL operations on resource register data in access management.
    /// </summary>
    public interface IResourceMetadataRepository
    {
        /// <summary>
        /// Inserts a placeholder for a resource in Resource Registry into Access Managment
        /// </summary>
        /// <param name="resource">Data to insert</param>
        /// <returns>The inserted data with data generated/fetched in db</returns>
        Task<AccessManagementResource> InsertAccessManagementResource(AccessManagementResource resource, CancellationToken cancellationToken = default);
    }
}
