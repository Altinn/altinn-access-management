using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Persistence.Configuration;
using Altinn.AccessManagement.Persistence.Extensions;
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

        private readonly string insertResorceAccessManagment = "select * from accessmanagement.upsert_resourceregistryresource(@_resourceregistryid, @_resourcetype)";

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceMetadataRepository"/> class
        /// </summary>
        /// <param name="config">The postgreSQL configurations for AuthorizationDB</param>
        public ResourceMetadataRepository(IOptions<PostgreSQLSettings> config)
        {
            _connectionString = string.Format(config.Value.ConnectionString, config.Value.AuthorizationDbPwd);
        }

        /// <inheritdoc />
        public async Task<AccessManagementResource> InsertAccessManagementResource(AccessManagementResource resource, CancellationToken cancellationToken = default)
        {
            using var activity = TelemetryConfig.ActivitySource.StartActivity(ActivityKind.Client);
            try
            {
                await using NpgsqlConnection conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                NpgsqlCommand pgcom = new NpgsqlCommand(insertResorceAccessManagment, conn);
                pgcom.Parameters.AddWithValue("_resourceregistryid", resource.ResourceRegistryId);
                pgcom.Parameters.AddWithValue("_resourcetype", NpgsqlTypes.NpgsqlDbType.Text, resource.ResourceType.ToString().ToLower());

                using NpgsqlDataReader reader = await pgcom.ExecuteReaderAsync(cancellationToken);
                if (reader.Read())
                {
                    return GetAccessManagementResource(reader);
                }

                return null;
            }
            catch (Exception ex)
            {
                activity?.StopWithError(ex);
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
