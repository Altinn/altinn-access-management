using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Persistence.Configuration;
using Altinn.AccessManagement.Persistence.Extensions;
using Microsoft.Extensions.Options;
using Npgsql;
using OpenTelemetry.Trace;

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
        /// <param name="postgresSettings">The postgreSQL configurations for AuthorizationDB</param>
        public ResourceMetadataRepository(IOptions<PostgreSQLSettings> postgresSettings)
        {
            _connectionString = string.Format(
                postgresSettings.Value.ConnectionString,
                postgresSettings.Value.AuthorizationDbPwd);
        }

        /// <inheritdoc />
        public async Task<AccessManagementResource> InsertAccessManagementResource(AccessManagementResource resource)
        {
            using var activity = TelemetryConfig._activitySource.StartActivity(ActivityKind.Client);
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
            catch (Exception ex)
            {
                activity?.ErrorWithException(ex);
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
