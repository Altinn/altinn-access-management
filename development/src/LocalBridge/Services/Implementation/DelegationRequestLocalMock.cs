using Altinn.Brigde.Enums;
using Altinn.Brigde.Models;
using Altinn.Brigde.Services;
using System.Text.Json;

namespace Altinn.Bridge.Mocks
{
    public class DelegationRequestLocalMock : IDelegationRequestsWrapper
    {
        public Task<DelegationRequests> GetDelegationRequestsAsync(string who, string? serviceCode, int? serviceEditionCode, RestAuthorizationRequestDirection direction, List<RestAuthorizationRequestStatus>? status, string? continuation)
        {

            DelegationRequests delRequests=  new DelegationRequests();

            string path = GetDelegationRequestPaths();

            string filterFileName = GetFilterFileName(who, serviceCode, serviceEditionCode, direction, status, continuation);

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
            string? unitTestFolder = Path.GetDirectoryName(new Uri(typeof(DelegationRequestLocalMock).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, @"..\..\..\..\..\TestData\DelegationRequests\");
        }

        private string GetFilterFileName(string who, string? serviceCode, int? serviceEditionCode, RestAuthorizationRequestDirection direction, List<RestAuthorizationRequestStatus>? status, string? continuation)
        {
            if(string.IsNullOrEmpty(who))
            {
                who = "PID500700";
            }

            if (direction.Equals(RestAuthorizationRequestDirection.Outgoing))
            {
                return "coveredby_"+ who;
            }

            if (direction.Equals(RestAuthorizationRequestDirection.Incoming))
            {
                return "offeredby_" + who;
            }


            return "coveredby_PID500700";
        }
    }
}
