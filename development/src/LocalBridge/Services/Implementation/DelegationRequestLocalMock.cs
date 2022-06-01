using Altinn.Brigde.Models;
using Altinn.Brigde.Services;
using System.Text.Json;

namespace Altinn.Bridge.Mocks
{
    public class DelegationRequestLocalMock : IDelegationRequestsWrapper
    {
        public Task<DelegationRequests> GetDelegationRequestsAsync(int requestedFromParty, int requestedToParty, string direction)
        {

            DelegationRequests delRequests=  new DelegationRequests();

            string path = GetDelegationRequestPaths();

            string filterFileName = GetFilterFileName(requestedFromParty, requestedToParty, direction);

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

        private string GetFilterFileName(int requestedFromParty, int requestedToParty, string direction)
        {
            return "coveredby_UID1337";
        }
    }
}
