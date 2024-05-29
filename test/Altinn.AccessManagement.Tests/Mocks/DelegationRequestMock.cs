using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services.Interfaces;

namespace Altinn.AccessManagement.Tests.Mocks
{
    /// <summary>
    /// Mock class for <see cref="IDelegationRequestsWrapper"></see> interface
    /// </summary>
    public class DelegationRequestMock : IDelegationRequestsWrapper
    {
        /// <inheritdoc/>
        public Task<DelegationRequests> GetDelegationRequestsAsync(string who, string serviceCode, int? serviceEditionCode, RestAuthorizationRequestDirection direction, List<RestAuthorizationRequestStatus> status, string continuation)
        {
            DelegationRequests delRequests = [];

            string path = GetDelegationRequestPaths();

            string filterFileName = GetFilterFileName(who, direction);

            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);

                foreach (string file in files)
                {
                    if (file.Contains(filterFileName))
                    {
                        string content = File.ReadAllText(Path.Combine(path, file));
                        DelegationRequest delegationRequest = JsonSerializer.Deserialize<DelegationRequest>(content, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                        if (delegationRequest != null)
                        {
                            delRequests.Add(delegationRequest);
                        }
                    }
                }
            }

            return Task.FromResult(delRequests);
        }

        private static string GetDelegationRequestPaths()
        {
            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(DelegationRequestMock).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, @"..\..\..\Data\DelegationRequests\");
        }

        private string GetFilterFileName(string requestedFromParty, RestAuthorizationRequestDirection direction)
        {
            return "coveredby_UID1337";
        }
    }
}
