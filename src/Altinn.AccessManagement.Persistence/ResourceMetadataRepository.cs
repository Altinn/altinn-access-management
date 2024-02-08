using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Persistence.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Altinn.AccessManagement.Persistence
{
    /// <summary>
    /// Storing Resource Rigistry metadata to Access management
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ResourceMetadataRepository : IResourceMetadataRepository
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;

        private readonly string insertResorceAccessManagment = "select * from accessmanagement.upsert_resourceregistryresource(@_resourceregistryid, @_resourcetype)";

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceMetadataRepository"/> class
        /// </summary>
        /// <param name="postgresSettings">The postgreSQL configurations for AuthorizationDB</param>
        /// <param name="logger">logger</param>
        public ResourceMetadataRepository(
            IOptions<PostgreSQLSettings> postgresSettings,
            ILogger<ResourceMetadataRepository> logger)
        {
            _logger = logger;
            _connectionString = string.Format(
                postgresSettings.Value.ConnectionString,
                postgresSettings.Value.AuthorizationDbPwd);
        }

        /// <inheritdoc />
        public async Task<AccessManagementResource> InsertAccessManagementResource(AccessManagementResource resource, CancellationToken cancellationToken = default)
        {
            try
            {
                await using NpgsqlConnection conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                NpgsqlCommand pgcom = new NpgsqlCommand(insertResorceAccessManagment, conn);
                pgcom.Parameters.AddWithValue("_resourceregistryid", resource.ResourceRegistryId);
                pgcom.Parameters.AddWithValue("_resourcetype", NpgsqlTypes.NpgsqlDbType.Text, resource.ResourceType.ToString().ToLower());
                
                using NpgsqlDataReader reader = await pgcom.ExecuteReaderAsync();
                if (reader.Read())
                {
                    return GetAccessManagementResource(reader);
                }

                return null;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Authorization // ResourceMetadataRepository // InsertAccessManagementResource // Exception");
                throw;
            }
        }

        private static AccessManagementResource GetAccessManagementResource(NpgsqlDataReader reader)
        {
            ResourceType resourceType;
            return new AccessManagementResource
            {
                ResourceId = reader.GetFieldValue<int>("resourceid"),
                ResourceRegistryId = reader.GetFieldValue<string>("resourceregistryid"),
                ResourceType = Enum.TryParse(reader.GetFieldValue<string>("resourcetype"), out resourceType) ? resourceType : ResourceType.Default,
                Created = reader.GetFieldValue<DateTime>("created"),
                Modified = reader.GetFieldValue<DateTime>("modified")
            };
        }
    }
}
