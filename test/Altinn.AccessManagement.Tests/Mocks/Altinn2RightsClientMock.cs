using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Integration.Clients;
using Altinn.AccessManagement.Models;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Altinn.AccessManagement.Tests.Mocks
{
    /// <summary>
    /// Mock class for <see cref="IResourceRegistryClient"></see> interface
    /// </summary>
    public class Altinn2RightsClientMock : IAltinn2RightsClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Altinn2RightsClientMock"/> class
        /// </summary>
        public Altinn2RightsClientMock()
        {
        }

        /// <inheritdoc/>
        public async Task<DelegationCheckResponse> PostDelegationCheck(int userId, RightsDelegationCheckRequest request)
        {
            if (request.From[0].Value == "50001337" && request.Resource[0].Value == "1337" && request.Resource[1].Value == "1338")
            {
                string content = File.ReadAllText($"Data/Json/DelegationCheck/se_1337_1338/from_p50001337/authn_u20001337.json");
                List<RightDelegationCheckResult> results = JsonSerializer.Deserialize<List<RightDelegationCheckResult>>(content);
                return new DelegationCheckResponse { From = request.From, RightDelegationCheckResults = results };
            }

            throw new NotImplementedException();
        }

        private static string GetResourcePath(string resourceRegistryId)
        {
            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(ResourceRegistryClientMock).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "..", "..", "..", "Data", "ResourceRegistryResources", $"{resourceRegistryId}", "resource.json");
        }

        private static string GetDataPathForResources()
        {
            string? unitTestFolder = Path.GetDirectoryName(new Uri(typeof(ResourceRegistryClientMock).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "Data", "Resources");
        }
    }
}
