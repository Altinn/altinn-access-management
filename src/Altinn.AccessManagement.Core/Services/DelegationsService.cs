﻿using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Core.Utilities;
using Altinn.Platform.Register.Models;
using Microsoft.Extensions.Logging;
using static System.Formats.Asn1.AsnWriter;

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
        public Task<List<Delegation>> GetAllOutboundDelegationsAsync(string who, ResourceType resourceType)
        {
            int offeredByPartyId = 0;

            offeredByPartyId = GetParty(who);
            if (offeredByPartyId == 0)
            {
                throw new ArgumentException("OfferedByPartyId does not have a valid value");
            }

            return GetOutboundDelegations(offeredByPartyId, resourceType);
        }

        /// <inheritdoc/>
        public Task<List<Delegation>> GetAllInboundDelegationsAsync(string who, ResourceType resourceType)
        {
            int coveredByPartyId = 0;

            coveredByPartyId = GetParty(who);
            if (coveredByPartyId == 0)
            {
                throw new ArgumentException();
            }

            return GetInboundDelegations(coveredByPartyId, resourceType);
        }

        /// <inheritdoc/>
        public Task<List<Delegation>> GetAllDelegationsForAdminAsync(int supplierOrg, int consumerOrg, string scope)
        {
            int consumerPartyId = GetParty(consumerOrg.ToString());
            int supplierPartyId = GetParty(supplierOrg.ToString());
            if (consumerPartyId == 0 || supplierPartyId == 0)
            {
                throw new ArgumentException("Either the supplier or the consumer organisation number is not valid");
            }

            if (!RegexUtil.IsValidMaskinportenScope(scope))
            {
                throw new ArgumentException("Scope is not well formatted");
            }

            return GetResourceRegistryDelegationChangesForAdmin(supplierPartyId, consumerPartyId, scope);
        }

        #region private methods

        private async Task<List<Delegation>> GetOutboundDelegations(int offeredByPartyId, ResourceType resourceType)
        {
            List<DelegationChange> delegationChanges = await _delegationRepository.GetOfferedResourceRegistryDelegations(offeredByPartyId, resourceType);
            List<int> parties = new List<int>();
            foreach (int party in delegationChanges.Select(d => d.CoveredByPartyId).Where(c => c != null).OfType<int>())
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
                delegation.PerformedByPartyId = delegationChange.PerformedByPartyId;
                delegation.Created = delegationChange.Created.Value;
                delegation.ResourceId = delegationChange.ResourceId;
                ServiceResource resource = resources.Find(r => r.Identifier == delegationChange.ResourceId);
                delegation.ResourceTitle = resource?.Title;
                ////delegation.DelegationResourceType = resource.ResourceType;
                delegation.ResourceType = resource.ResourceType;
                delegations.Add(delegation);
            }

            return delegations;
        }

        private async Task<List<Delegation>> GetInboundDelegations(int coveredByPartyId, ResourceType resourceType)
        {
            List<DelegationChange> delegationChanges = await _delegationRepository.GetReceivedResourceRegistryDelegationsForCoveredByPartys(coveredByPartyId, resourceType);
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
                delegation.Created = delegationChange.Created.Value;
                delegation.ResourceId = delegationChange.ResourceId;
                ServiceResource resource = resources.Find(r => r.Identifier == delegationChange.ResourceId);
                delegation.ResourceTitle = resource?.Title;
                ////delegation.DelegationResourceType = resource.ResourceType;
                delegation.ResourceType = resource.ResourceType;
                delegations.Add(delegation);
            }

            return delegations;
        }

        private async Task<List<Delegation>> GetResourceRegistryDelegationChangesForAdmin(int supplierPartyId, int consumerPartyId, string scopes)
        {
            List<ServiceResource> resources;
            List<string> resourceIds;
            
            resources = await _resourceRegistryClient.SearchResources(scopes);
            resourceIds = resources.Select(d => d.Identifier).ToList();

            List<DelegationChange> delegationChanges = await _delegationRepository.GetResourceRegistryDelegationChangesForAdmin(resourceIds, supplierPartyId, consumerPartyId, ResourceType.MaskinportenSchema);
            List<int> parties;
            parties = delegationChanges.Select(d => d.OfferedByPartyId).ToList();
            parties.AddRange(delegationChanges.Select(d => d.CoveredByPartyId).Select(ds => Convert.ToInt32(ds)).ToList());

            List<Party> partyList = await _partyClient.GetPartiesAsync(parties);
            List<Delegation> delegations = new List<Delegation>();

            foreach (DelegationChange delegationChange in delegationChanges)
            {
                Delegation delegation = new Delegation();
                Party partyInfo = partyList.Find(p => p.PartyId == delegationChange.OfferedByPartyId);
                Party coveredByPartyInfo = partyList.Find(p => p.PartyId == delegationChange.CoveredByPartyId);
                delegation.OfferedByName = partyInfo?.Name;
                delegation.OfferedByOrganizationNumber = Convert.ToInt32(partyInfo?.OrgNumber);
                delegation.CoveredByName = coveredByPartyInfo?.Name;
                delegation.CoveredByOrganizationNumber = Convert.ToInt32(coveredByPartyInfo?.OrgNumber);
                delegation.CoveredByPartyId = delegationChange.CoveredByPartyId;
                delegation.OfferedByPartyId = delegationChange.OfferedByPartyId;
                delegation.PerformedByUserId = delegationChange.PerformedByUserId;
                delegation.Created = delegationChange.Created ?? DateTime.MinValue;
                delegation.ResourceId = delegationChange.ResourceId;
                ServiceResource resource = resources.Find(r => r.Identifier == delegationChange.ResourceId);
                delegation.ResourceTitle = resource?.Title;
                delegation.AltinnIIResourceId = delegationChange.ResourceId.StartsWith("appid-") ? Guid.Parse(DelegationHelper.GetReferenceValue(resource, ReferenceSource.Altinn2, ReferenceType.DelegationSchemeId)) : null;
                delegation.Scopes = new HashSet<string>(resource?.ResourceReferences?.Where(rf => string.Equals(rf.ReferenceType, ReferenceType.MaskinportenScope)).Select(rf => rf.Reference).ToList());
                delegation.ResourceType = resource.ResourceType;
                delegations.Add(delegation);
            }

            return delegations;
        }

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
                throw new ArgumentNullException("the parameter who does not have a value");
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

                _ = int.TryParse(who, out int orgno);
                return _partyClient.GetPartyId(orgno);
            }   
            catch (Exception ex)
            {
                _logger.LogError("//DelegationsService //GetParty failed to fetch partyid", ex);
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
