using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models.Profile;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Platform.Profile.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Core.Services
{
    /// <summary>
    /// Service implementation wrapping UserProfile lookup with lastname verification and preventing users for guessing too many faulty combination of SSN/Username and Last Name
    /// </summary>
    public class UserProfileLookupService : IUserProfileLookupService
    {
        private const string UserProfileLookupFailedAttempts = "UserProfile-Lookup-Failed-Attempts";

        private readonly UserProfileLookupSettings _userProfileLookupSettings;
        private readonly ILogger<UserProfileLookupService> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IProfileClient _profile;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileLookupService"/> class
        /// </summary>
        /// <param name="logger">Logger service</param>
        /// <param name="userProfileLookupSettings">Cache config</param>
        /// <param name="memoryCache">The cache handler </param>
        /// <param name="profile">The client for integration with profile API</param>
        public UserProfileLookupService(ILogger<UserProfileLookupService> logger, IOptions<UserProfileLookupSettings> userProfileLookupSettings, IMemoryCache memoryCache, IProfileClient profile)
        {
            _userProfileLookupSettings = userProfileLookupSettings.Value;
            _logger = logger;
            _memoryCache = memoryCache;
            _profile = profile;
        }

        /// <inheritdoc/>
        public async Task<UserProfile> GetUserProfile(int authnUserId, UserProfileLookup lookupIdentifier, string lastName)
        {
            string uniqueCacheKey = UserProfileLookupFailedAttempts + authnUserId;

            _ = _memoryCache.TryGetValue(uniqueCacheKey, out int failedAttempts);
            if (failedAttempts >= _userProfileLookupSettings.MaximumFailedAttempts)
            {
                _logger.LogInformation(
                    "User {userId} has performed too many failed UserProfile lookup attempts.", authnUserId);

                throw new TooManyFailedLookupsException();
            }

            UserProfile userProfile = await _profile.GetUser(lookupIdentifier);

            string nameFromParty = userProfile?.Party.Person.LastName ?? string.Empty;

            if (nameFromParty.Length > 0 && nameFromParty.IsSimilarTo(lastName))
            {
                return userProfile;
            }

            failedAttempts += 1;
            MemoryCacheEntryOptions memoryCacheOptions = new()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_userProfileLookupSettings.FailedAttemptsCacheLifetimeSeconds)
            };
            _memoryCache.Set(uniqueCacheKey, failedAttempts, memoryCacheOptions);
            return null;
        }
    }
}
