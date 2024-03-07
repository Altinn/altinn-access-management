using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models.Profile;
using Altinn.Platform.Profile.Models;

namespace Altinn.AccessManagement.Tests.Mocks
{
    /// <summary>
    /// Mock class for <see cref="IProfileClient"></see> interface
    /// </summary>
    public class ProfileClientMock : IProfileClient
    {
        /// <inheritdoc/>
        public Task<UserProfile> GetUser(UserProfileLookup userProfileLookup, CancellationToken cancellationToken = default)
        {
            UserProfile userProfile = null;

            string userProfilePath = GetUserProfilePath(userProfileLookup);

            if (File.Exists(userProfilePath))
            {
                string content = File.ReadAllText(userProfilePath);
                userProfile = (UserProfile)JsonSerializer.Deserialize(content, typeof(UserProfile), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return Task.FromResult(userProfile);
        }

        private static string GetUserProfilePath(UserProfileLookup userProfileLookup)
        {
            string userIdentifier = userProfileLookup.UserId > 0 ? userProfileLookup.UserId.ToString() : userProfileLookup.Ssn ?? userProfileLookup.Username ?? userProfileLookup.UserUuid?.ToString();
            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(ProfileClientMock).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "Data", "UserProfile", $"{userIdentifier}.json");
        }
    }
}
