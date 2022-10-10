using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Altinn.AuthorizationAdmin.Core.Services;
using Altinn.Platform.Register.Models;
using Newtonsoft.Json;

namespace Altinn.AuthorizationAdmin.Tests.Mocks
{
    /// <summary>
    /// Mock class for <see cref="IPartiesWrapper"></see> interface
    /// </summary>
    public class PartiesWrapperMock : IPartiesWrapper
    {
        /// <summary>
        /// Party information for a list of party numbers
        /// </summary>
        /// <param name="parties"> list of party numbers</param>
        /// <returns>party information list</returns>
        public Task<List<Party>> GetPartiesAsync(List<int> parties)
        {
            List<Party> partyList = new List<Party>();

            string path = GetPartiesPaths();
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);

                foreach (string file in files)
                {
                    if (file.Contains("offeredby50002110"))
                    {
                        string content = File.ReadAllText(Path.Combine(path, file));
                        partyList = (List<Party>)JsonConvert.DeserializeObject(content, typeof(List<Party>));
                    }
                }
            }

            return Task.FromResult(partyList);
        }

        private static string GetPartiesPaths()
        {
            string? unitTestFolder = Path.GetDirectoryName(new Uri(typeof(PartiesWrapperMock).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, @"..\..\..\Data\Parties\");
        }

        private string GetFilterFileName(int offeredByPartyId)
        {
            return "offeredBy_50002110";
        }
    }
}
