using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Platform.Register.Models;
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
    }
}
