using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Helpers;
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
        public Task<DelegationCheckResponse> PostDelegationCheck(int userId, int partyId, string serviceCode, string serviceEditionCode)
        {
            if (partyId == 50001337 && serviceCode == "1337" && serviceEditionCode == "1338")
            {
                string content = File.ReadAllText($"Data/Json/DelegationCheck/se_1337_1338/from_p50001337/authn_u20001337_from_sbl_bridge.json");
                List<RightDelegationCheckResult> results = JsonSerializer.Deserialize<List<RightDelegationCheckResult>>(content);
                return Task.FromResult(new DelegationCheckResponse { From = new List<AttributeMatch> { new AttributeMatch { Id = "thing", Value = partyId.ToString() } }, RightDelegationCheckResults = results });
            }

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<DelegationActionResult> PostDelegation(int authenticatedUserId, int reporteePartyId, SblRightDelegationRequest delegationRequest)
        {
            DelegationHelper.TryGetResourceFromAttributeMatch(delegationRequest.Rights.First().Resource, out ResourceAttributeMatchType _, out string _, out string _, out string _, out string serviceCode, out string serviceEditionCode);

            DelegationActionResult result = new DelegationActionResult();
            result.Rights = GetSblRightsDelegationResult($"se_{serviceCode}_{serviceEditionCode}", reporteePartyId.ToString(), delegationRequest.To.Value, authenticatedUserId.ToString());

            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> ClearReporteeRights(int fromPartyId, int toPartyId, int toUserId = 0, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }

        private static List<RightDelegationResult> GetSblRightsDelegationResult(string resourceId, string reporteePartyId, string to, string by)
        {
            JsonSerializerOptions serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            serializerOptions.Converters.Add(new JsonStringEnumConverter());

            string content = File.ReadAllText($"Data/Json/RightsDelegation/{resourceId}/from_{reporteePartyId}/to_{to}/by_{by}/sbl_response.json");
            return (List<RightDelegationResult>)JsonSerializer.Deserialize(content, typeof(List<RightDelegationResult>), serializerOptions);
        }
    }
}
