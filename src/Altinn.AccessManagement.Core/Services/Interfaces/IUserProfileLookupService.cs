using Altinn.AccessManagement.Core.Models.Profile;
using Altinn.Platform.Profile.Models;

namespace Altinn.AccessManagement.Core.Services.Interfaces
{
    /// <summary>
    /// Defines the interface for the service wrapping UserProfile lookup with lastname verification and preventing users for guessing too many faulty combination of SSN/Username and Last Name
    /// </summary>
    public interface IUserProfileLookupService
    {
        /// <summary>
        /// Gets the UserProfile of a user if the provided identifier and lastname is matching last name from freg
        /// </summary>
        /// <returns>Party</returns>
        Task<UserProfile> GetUserProfile(int authnUserId, UserProfileLookup lookupIdentifier, string lastName);
    }
}
