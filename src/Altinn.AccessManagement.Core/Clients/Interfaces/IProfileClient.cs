using Altinn.AccessManagement.Core.Models.Profile;
using Altinn.Platform.Profile.Models;

namespace Altinn.AccessManagement.Core.Clients.Interfaces
{
    /// <summary>
    /// Interface for Profile functionality.
    /// </summary>
    public interface IProfileClient
    {
        /// <summary>
        /// Method for getting the userprofile for a given user identified by one of the available types of user identifiers:
        ///     UserId (from Altinn 2 Authn UserProfile)
        ///     Username (from Altinn 2 Authn UserProfile)
        ///     SSN/Dnr (from Freg)
        ///     Uuid (from Altinn 2 Party/UserProfile implementation will be added later)
        /// </summary>
        /// <param name="userProfileLookup">Model for specifying the user identifier to use for the UserProfile lookup</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>The UserProfile for the given user</returns>
        Task<UserProfile> GetUser(UserProfileLookup userProfileLookup, CancellationToken cancellationToken = default);
    }
}
