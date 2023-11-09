using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models;

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
        public Task<DelegationCheckResponse> PostDelegationCheck(int userId, string partyId, string serviceCode, string serviceEditionCode)
        {
            if (partyId == "50001337" && serviceCode == "1337" && serviceEditionCode == "1338")
            {
                string content = File.ReadAllText($"Data/Json/DelegationCheck/se_1337_1338/from_p50001337/authn_u20001337_from_sbl_bridge.json");
                List<RightDelegationCheckResult> results = JsonSerializer.Deserialize<List<RightDelegationCheckResult>>(content);
                return Task.FromResult(new DelegationCheckResponse { From = new List<AttributeMatch> { new AttributeMatch { Id = "thing", Value = partyId } }, RightDelegationCheckResults = results });
            }

            throw new NotImplementedException();
        }
    }
}
