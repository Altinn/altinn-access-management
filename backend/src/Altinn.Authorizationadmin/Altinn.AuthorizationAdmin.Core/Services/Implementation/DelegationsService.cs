using Altinn.AuthorizationAdmin.Core.Models;
using Altinn.AuthorizationAdmin.Core.Models.ResourceRegistry;
using Altinn.AuthorizationAdmin.Core.Repositories.Interface;
using Altinn.AuthorizationAdmin.Core.Services.Interfaces;
using Npgsql;

namespace Altinn.AuthorizationAdmin.Core.Services.Implementation
{
    /// <inheritdoc/>
    public class DelegationsService : IDelegationsService
    {
        private readonly IDelegationMetadataRepository _delegationRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationsService"/> class.
        /// </summary>
        /// <param name="delegationRepository">delgation change handler</param>
        public DelegationsService(IDelegationMetadataRepository delegationRepository)
        {
            _delegationRepository = delegationRepository;
        }

        /// <inheritdoc/>
        public async Task<List<ResourceDelegation>> GetDelegatedResourcesAsync(int offeredbyPartyId)
        {
            List<ServiceResource> serviceResources = await _delegationRepository.GetDelegations(offeredbyPartyId);
            List<Delegation> delegations = await _delegationRepository.GetDelegatedResources(offeredbyPartyId);
            List<ResourceDelegation> resourceDelegations = new List<ResourceDelegation>();
            foreach (ServiceResource resource in serviceResources)
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
                        resourceDelegation.Delegations.Add(delegation);
                    }

                    resourceDelegations.Add(resourceDelegation);
                }
            }

            return resourceDelegations;
        }

        /// <inheritdoc/>
        public async Task<List<Delegation>> GetReceivedDelegationsAsync(int coveredByPartyId)
        {
            return await _delegationRepository.GetReceivedDelegations(coveredByPartyId);
        }
    }
}
