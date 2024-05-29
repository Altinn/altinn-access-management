using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models.Profile;
using Altinn.Platform.Profile.Models;

namespace Altinn.AccessManagement.Tests.Contexts;

/// <inheritdoc/>
public class ProfileClientMock(MockContext context) : IProfileClient
{
    private MockContext Context { get; } = context;

    /// <inheritdoc/>
    public Task<UserProfile> GetUser(UserProfileLookup userProfileLookup, CancellationToken cancellationToken = default) =>
        Task.FromResult(Context.UserProfiles.FirstOrDefault(profile =>
            profile.UserId == userProfileLookup.UserId ||
            profile.UserName == userProfileLookup.Username ||
            profile.Party.SSN == userProfileLookup.Ssn ||
            profile.UserUuid == userProfileLookup.UserUuid));
}