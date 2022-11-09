using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Platform.Register.Models;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessManagement.Core.Services
{
    /// <inheritdoc/>
    public class DelegationsService : IDelegationsService
    {
        private readonly IDelegationMetadataRepository _delegationRepository;
        private readonly ILogger<IDelegationsService> _logger;
        private readonly IPartiesClient _partyClient;
        private readonly IResourceRegistryClient _resourceRegistryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationsService"/> class.
        /// </summary>
        /// <param name="delegationRepository">delgation change handler</param>
        /// <param name="logger">handler for logger</param>
        /// <param name="partyClient">handler for party</param>
        /// <param name="resourceRegistryClient">handler for resoruce registry</param>
        public DelegationsService(IDelegationMetadataRepository delegationRepository, ILogger<IDelegationsService> logger, IPartiesClient partyClient, IResourceRegistryClient resourceRegistryClient)
        {
            _delegationRepository = delegationRepository;
            _logger = logger;
            _partyClient = partyClient;
            _resourceRegistryClient = resourceRegistryClient;
        }

        /// <inheritdoc/>
        public async Task<List<Delegation>> GetAllOutboundDelegationsAsync(string who, ResourceType resourceType)
        {
            int offeredByPartyId = 0;

            try
            {
                offeredByPartyId = GetParty(who);
                if (offeredByPartyId == 0)
                {
                    throw new ArgumentException();
                }
            }
            catch (Exception)
            {
                throw;
            }

            List<DelegationChange> delegationChanges = await _delegationRepository.GetAllOfferedDelegations(offeredByPartyId, resourceType);
            List<int> parties = new List<int>();
            foreach (int party in delegationChanges.Select(d => d.CoveredByPartyId).Where(c => c != null))
            {
                parties.Add(party);
            }

            List<ServiceResource> resources = new List<ServiceResource>();
            List<string> resourceIds;
            resourceIds = delegationChanges.Select(d => d.ResourceId).Distinct().ToList();

            resources = await _resourceRegistryClient.GetResources(resourceIds);

            List<Party> partyList = await _partyClient.GetPartiesAsync(parties);
            List<Delegation> delegations = new List<Delegation>();
            
            foreach (DelegationChange delegationChange in delegationChanges)
            {
                Delegation delegation = new Delegation();
                Party partyInfo = partyList.Find(p => p.PartyId == delegationChange.CoveredByPartyId);
                delegation.CoveredByName = partyInfo?.Name;
                delegation.CoveredByOrganizationNumber = Convert.ToInt32(partyInfo?.OrgNumber);
                delegation.CoveredByPartyId = delegationChange.CoveredByPartyId;
                delegation.OfferedByPartyId = delegationChange.OfferedByPartyId;
                delegation.PerformedByUserId = delegationChange.PerformedByUserId;
                delegation.Created = delegationChange.Created;
                delegation.ResourceId = delegationChange.ResourceId;
                ServiceResource resource = resources.Find(r => r.Identifier == delegationChange.ResourceId);
                delegation.ResourceTitle = resource?.Title;
                delegation.DelegationResourceType = resource.ResourceType;
                delegations.Add(delegation);
            }

            return delegations;
        }

        /// <inheritdoc/>
        public async Task<List<Delegation>> GetAllInboundDelegationsAsync(string who, ResourceType resourceType)
        {
            int coveredByPartyId = 0;

            try
            {
                coveredByPartyId = GetParty(who);
                if (coveredByPartyId == 0)
                {
                    throw new ArgumentException();
                }
            }
            catch (Exception)
            {
                throw;
            }

            List<DelegationChange> delegationChanges = await _delegationRepository.GetReceivedDelegationsAsync(coveredByPartyId, resourceType);
            List<int> parties = new List<int>();
            parties = delegationChanges.Select(d => d.OfferedByPartyId).ToList();

            List<ServiceResource> resources = new List<ServiceResource>();
            List<string> resourceIds;
            resourceIds = delegationChanges.Select(d => d.ResourceId).ToList();
            resources = await _resourceRegistryClient.GetResources(resourceIds);

            List<Party> partyList = await _partyClient.GetPartiesAsync(parties);
            List<Delegation> delegations = new List<Delegation>();

            foreach (DelegationChange delegationChange in delegationChanges)
            {
                Delegation delegation = new Delegation();
                Party partyInfo = partyList.Find(p => p.PartyId == delegationChange.OfferedByPartyId);
                delegation.OfferedByName = partyInfo?.Name;
                delegation.OfferedByOrganizationNumber = Convert.ToInt32(partyInfo?.OrgNumber);
                delegation.CoveredByPartyId = delegationChange.CoveredByPartyId;
                delegation.OfferedByPartyId = delegationChange.OfferedByPartyId;
                delegation.PerformedByUserId = delegationChange.PerformedByUserId;
                delegation.Created = delegationChange.Created;
                delegation.ResourceId = delegationChange.ResourceId;
                ServiceResource resource = resources.Find(r => r.Identifier == delegationChange.ResourceId);
                delegation.ResourceTitle = resource?.Title;
                delegation.DelegationResourceType = resource.ResourceType;
                delegations.Add(delegation);
            }

            return delegations;
        }

        #region private methods

        /// <summary>
        /// Gets the party identified by <paramref name="who"/>.
        /// </summary>
        /// <param name="who">
        /// Who, valid values are , an organization number, or a party ID (the letter r followed by 
        /// the party ID).
        /// </param>
        /// <returns>The party identified by <paramref name="who"/>.</returns>
        private int GetParty(string who)
        {
            if (string.IsNullOrEmpty(who))
            {
                throw new ArgumentNullException();
            }

            try
            {
                int? partyId = DelegationHelper.TryParsePartyId(who);
                if (partyId.HasValue)
                {
                    Party party = GetPartyById(partyId.Value);
                    partyId = party != null ? party.PartyId : 0;
                    return Convert.ToInt32(partyId);
                }

                int.TryParse(who, out int orgno);
                return _partyClient.GetPartyId(orgno);
            }   
            catch (Exception)
            {
                throw;
            }            
        }

        /// <summary>
        /// Gets a party by party ID.
        /// </summary>
        /// <param name="partyId">Identifies a party (organization or person).</param>
        /// <returns>The identified party.</returns>
        private Party GetPartyById(int partyId)
        {
            Party party;
            party = _partyClient.GetPartyAsync(partyId).Result;
            return party;
        }      

        #endregion
    }
}
