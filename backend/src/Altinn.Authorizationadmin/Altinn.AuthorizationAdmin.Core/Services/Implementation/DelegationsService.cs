using Altinn.AuthorizationAdmin.Core.Models;
using Altinn.AuthorizationAdmin.Core.Models.ResourceRegistry;
using Altinn.AuthorizationAdmin.Core.Repositories.Interface;
using Altinn.AuthorizationAdmin.Core.Services.Interfaces;
using Altinn.Platform.Register.Models;
using Microsoft.Extensions.Logging;

namespace Altinn.AuthorizationAdmin.Core.Services.Implementation
{
    /// <inheritdoc/>
    public class DelegationsService : IDelegationsService
    {
        private readonly IDelegationMetadataRepository _delegationRepository;
        private readonly ILogger<IDelegationsService> _logger;
        private readonly IPartiesWrapper _partyProxy;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationsService"/> class.
        /// </summary>
        /// <param name="delegationRepository">delgation change handler</param>
        public DelegationsService(IDelegationMetadataRepository delegationRepository, ILogger<IDelegationsService> logger, IPartiesWrapper partyProxy)
        {
            _delegationRepository = delegationRepository;
            _logger = logger;
            _partyProxy = partyProxy;
        }

        /// <inheritdoc/>
        public async Task<List<ResourceDelegation>> GetDelegatedResourcesAsync(int offeredbyPartyId)
        {
            List<Delegation> delegations = await _delegationRepository.GetDelegatedResources(offeredbyPartyId);
            List<int> parties = new List<int>();
            parties = delegations.Select(d => d.CoveredByPartyId).ToList();
            List<ServiceResource> resources = new List<ServiceResource>();
            resources = delegations.Select(d => new ServiceResource() { Identifier = d.ResourceId, Title = d.ResourceTitle }).ToList();
            List<Party> partyList = await _partyProxy.GetPartiesAsync(parties);
            List<ResourceDelegation> resourceDelegations = new List<ResourceDelegation>();
            foreach (ServiceResource resource in resources)
            {
                if (resourceDelegations.FindAll(rd => rd.ResourceId.Equals(resource.Identifier)).Count <= 0)
                {
                    ResourceDelegation resourceDelegation = new ResourceDelegation();
                    resourceDelegation.ResourceId = resource.Identifier;
                    resourceDelegation.ResourceName = resource.Title.FirstOrDefault().Value;
                    List<Delegation> query = delegations.FindAll(d => d.ResourceId.Equals(resource.Identifier));
                    resourceDelegation.Delegations = new List<Delegation>();

                    foreach (Delegation delegation in query)
                    {
                        Party partyInfo = partyList.Find(p => p.PartyId == delegation.CoveredByPartyId);
                        delegation.CoveredByName = partyInfo?.Name;
                        resourceDelegation.Delegations.Add(delegation);
                    }

                    resourceDelegations.Add(resourceDelegation);
                }
            }

            return resourceDelegations;
        }

        /// <inheritdoc/>
        public async Task<List<ReceivedDelegation>> GetReceivedDelegationsAsync(int coveredByPartyId)
        {
            List<Delegation> delegations = await _delegationRepository.GetReceivedDelegationsAsync(coveredByPartyId);
            List<int> parties = new List<int>();
            parties = delegations.Select(d => d.OfferedByPartyId).ToList();
            List<ServiceResource> resources = new List<ServiceResource>();
            resources = delegations.Select(d => new ServiceResource() { Identifier = d.ResourceId, Title = d.ResourceTitle }).ToList();
            List<Party> partyList = await _partyProxy.GetPartiesAsync(parties);
            List<ReceivedDelegation> receivedDelegations = new List<ReceivedDelegation>();
            foreach (Party party in partyList)
            {
                if (receivedDelegations.FindAll(rd => rd.OfferedByPartyId.Equals(party.PartyId)).Count <= 0)
                {
                    ReceivedDelegation receivedDelegation = new ReceivedDelegation();
                    receivedDelegation.OfferedByPartyId = party.PartyId;
                    receivedDelegation.ReporteeName = party.Name;
                    List<Delegation> query = delegations.FindAll(d => d.CoveredByPartyId.Equals(coveredByPartyId) && d.OfferedByPartyId.Equals(party.PartyId));
                    receivedDelegation.Resources = new List<ServiceResource>();

                    foreach (Delegation delegation in query)
                    {
                        receivedDelegation.Resources.Add(new ServiceResource { Identifier = delegation.ResourceId, Title = delegation.ResourceTitle });
                    }

                    receivedDelegations.Add(receivedDelegation);
                }
            }

            return receivedDelegations;
        }

    }
}
