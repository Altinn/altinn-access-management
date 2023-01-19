using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Platform.Register.Models;
using Azure.Core;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessManagement.Core.Services
{
    /// <inheritdoc/>
    public class RegisterService : IRegister
    {
        private readonly ILogger<RegisterService> _logger;
        private readonly IPartiesClient _partyClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterService"/> class.
        /// </summary>
        /// <param name="logger">handler for logger</param>
        /// <param name="partyClient">handler for party</param>
        public RegisterService(
            ILogger<RegisterService> logger, 
            IPartiesClient partyClient)
        { 
            _logger = logger;
            _partyClient = partyClient;
        }

        /// <inheritdoc/>
        public Task<Party> GetOrganisation(string organisationNumber)
        {
            return _partyClient.LookupPartyBySSNOrOrgNo(organisationNumber);
        }

        /// <inheritdoc/>
        public Task<Party> GetPartyForPartyId(int partyId, int userId)
        {
            return _partyClient.GetPartyAsync(partyId);
        }

        /// <inheritdoc/>
        public async Task<Party> GetPartiesForUser(int userId, int partyId)
        {
            List<Party> partyList = await _partyClient.GetPartiesForUserAsync(userId);

            if (partyList.Count > 0)
            {
                foreach (Party party in partyList)
                {
                    if (party != null && party.PartyId == partyId)
                    {
                        return party;
                    }
                    else if (party != null && party.ChildParties != null && party.ChildParties.Count > 0)
                    {
                        foreach (Party childParty in party.ChildParties)
                        {
                            if (childParty.PartyId == partyId)
                            {
                                return party;
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
