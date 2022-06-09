using Altinn.AuthorizationAdmin.Core.Enums;
using Altinn.AuthorizationAdmin.Core.Models;
using Altinn.AuthorizationAdmin.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Altinn.AuthorizationAdmin.Tests.Mocks
{
    public class DelegationRequestMock : IDelegationRequestsWrapper
    {
        public Task<DelegationRequests> GetDelegationRequestsAsync(string who, string? serviceCode, int? serviceEditionCode, RestAuthorizationRequestDirection direction, List<RestAuthorizationRequestStatus>? status, string? continuation)
        {

            DelegationRequests delRequests=  new DelegationRequests();

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
                        DelegationRequest? delegationRequest = JsonSerializer.Deserialize<DelegationRequest>(content, new System.Text.Json.JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

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
            string? unitTestFolder = Path.GetDirectoryName(new Uri(typeof(DelegationRequestMock).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, @"..\..\..\Data\DelegationRequests\");
        }

        private string GetFilterFileName(string requestedFromParty, RestAuthorizationRequestDirection direction)
        {
            return "coveredby_UID1337";
        }
    }
}
