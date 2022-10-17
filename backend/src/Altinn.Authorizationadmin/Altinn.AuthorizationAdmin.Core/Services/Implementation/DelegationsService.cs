using Altinn.AuthorizationAdmin.Core.Clients;
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
        private readonly IPartiesClient _partyProxy;
        private readonly IResourceRegistryClient _resourceRegistryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationsService"/> class.
        /// </summary>
        /// <param name="delegationRepository">delgation change handler</param>
        public DelegationsService(IDelegationMetadataRepository delegationRepository, ILogger<IDelegationsService> logger, IPartiesClient partyProxy, IResourceRegistryClient resourceRegistryClient)
        {
            _delegationRepository = delegationRepository;
            _logger = logger;
            _partyProxy = partyProxy;
            _resourceRegistryClient = resourceRegistryClient;
        }

        /// <inheritdoc/>
        public async Task<List<OfferedDelegations>> GetAllOfferedDelegations(int offeredbyPartyId, ResourceType resourceType)
        {
            List<DelegationChange> delegations = await _delegationRepository.GetAllOfferedDelegations(offeredbyPartyId, resourceType);
            List<int> parties = new List<int>();
            foreach (DelegationChange delegationChange in delegations)
            {
                if (delegationChange.CoveredByPartyId != null)
                {
                    parties.Add(delegationChange.CoveredByPartyId ?? 0);
                }
            }

            List<ServiceResource> resources = new List<ServiceResource>();
            List<string> resourceIds;
            resourceIds = delegations.Select(d => d.ResourceId).Distinct().ToList();

            foreach (string id in resourceIds)
            {
                ServiceResource resource = null;
                try
                {
                    resource = await _resourceRegistryClient.GetResource(id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    throw;
                }
                
                if (resource == null)
                {
                    ServiceResource unavailableResource = new ServiceResource
                    {
                        Identifier = id,
                        Title = new Dictionary<string, string>
                        {
                            { "en", "Not Available" },
                            { "nb-no", "ikke tilgjengelig" },
                            { "nn-no", "ikkje tilgjengelig" }
                        }
                    };
                    resources.Add(unavailableResource);
                }
                else
                {
                    resources.Add(resource);
                }
            }

            List<Party> partyList = await _partyProxy.GetPartiesAsync(parties);
            List<OfferedDelegations> resourceDelegations = new List<OfferedDelegations>();
            foreach (ServiceResource resource in resources)
            {
                OfferedDelegations resourceDelegation = new OfferedDelegations();
                resourceDelegation.ResourceId = resource.Identifier;
                resourceDelegation.ResourceTitle = resource.Title.FirstOrDefault().Value;
                List<DelegationChange> query = delegations.FindAll(d => d.ResourceId.Equals(resource.Identifier));
                resourceDelegation.Delegations = new List<Delegation>();

                foreach (DelegationChange delegationChange in query)
                {
                    Delegation delegation = new Delegation();
                    Party partyInfo = partyList.Find(p => p.PartyId == delegationChange.CoveredByPartyId);
                    delegation.CoveredByName = partyInfo?.Name;
                    delegation.CoveredByOrganizationNumber = Convert.ToInt32(partyInfo?.OrgNumber);
                    delegation.CoveredByPartyId = delegationChange.CoveredByPartyId;
                    delegation.OfferedByPartyId = delegationChange.OfferedByPartyId;
                    delegation.PerformedByUserId = delegationChange.PerformedByUserId;
                    delegation.Created = delegationChange.Created;
                    resourceDelegation.Delegations.Add(delegation);
                }

                resourceDelegations.Add(resourceDelegation);
            }

            return resourceDelegations;
        }
    }
}
