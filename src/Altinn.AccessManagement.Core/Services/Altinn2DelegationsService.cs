using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Platform.Register.Enums;

namespace Altinn.AccessManagement.Core.Services
{
    public class Altinn2DelegationsService : IAltinn2DelegationsService
    {
        private readonly IDelegationMetadataRepository _delegationRepository;
        private readonly IContextRetrievalService _contextRetrievalService;
        private readonly IProfileClient _profile;

        public Altinn2DelegationsService(IDelegationMetadataRepository delegationRepository, IContextRetrievalService contextRetrievalService, IProfileClient profile)
        {
            _delegationRepository = delegationRepository;
            _contextRetrievalService = contextRetrievalService;
            _profile = profile;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<RightDelegation>> GetOfferedRightsDelegations(AttributeMatch reportee, CancellationToken cancellationToken = default)
        {
            var delegations = await GetOfferedDelegationsFromRepository(reportee, cancellationToken);
            return await MapDelegationResponse(delegations, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<List<RightDelegation>> GetReceivedRightsDelegations(AttributeMatch reportee, CancellationToken cancellationToken = default)
        {
            var delegations = await GetReceivedDelegationFromRepository(reportee, cancellationToken);
            return await MapDelegationResponse(delegations, cancellationToken);
        }

        /// <summary>
        /// summary
        /// </summary>
        /// <param name="reportee">a</param>
        /// <param name="cancellationToken">b</param>
        /// <returns></returns>
        private async Task<IEnumerable<DelegationChange>> GetReceivedDelegationFromRepository(AttributeMatch reportee, CancellationToken cancellationToken)
        {
            if (reportee.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute)
            {
                return await GetDelegationsFromRepositoryUsingParty(reportee, GetReceivedDelegationFromRepository, cancellationToken);
            }

            if (reportee.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.SocialSecurityNumberAttribute)
            {
                var user = await _profile.GetUser(new()
                {
                    Ssn = reportee.Value
                });

                var keyRoles = await _contextRetrievalService.GetKeyRolePartyIds(user.UserId, cancellationToken);
                return await _delegationRepository.GetAllDelegationChangesForAuthorizedParties(user.UserId.SingleToList(), keyRoles, cancellationToken);
            }

            if (reportee.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationNumberAttribute)
            {
                var party = await _contextRetrievalService.GetPartyForOrganization(reportee.Value);
                return await _delegationRepository.GetAllDelegationChangesForAuthorizedParties(null, party.PartyId.SingleToList(), cancellationToken);
            }

            throw new ArgumentException($"failed to interpret attribute of type '{reportee.Id}'");
        }

        /// <summary>
        /// summary
        /// </summary>
        /// <param name="reportee">a</param>
        /// <param name="cancellationToken">b</param>
        /// <returns></returns>
        private async Task<IEnumerable<DelegationChange>> GetOfferedDelegationsFromRepository(AttributeMatch reportee, CancellationToken cancellationToken)
        {
            if (reportee.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute)
            {
                return await GetDelegationsFromRepositoryUsingParty(reportee, GetOfferedDelegationsFromRepository, cancellationToken);
            }

            if (reportee.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.SocialSecurityNumberAttribute)
            {
                var user = await _profile.GetUser(new()
                {
                    Ssn = reportee.Value
                });

                var keyRoles = await _contextRetrievalService.GetKeyRolePartyIds(user.UserId, cancellationToken);
                var offeredBy = keyRoles.Concat(user.PartyId.SingleToList()).ToList() ?? [user.PartyId];
                return await _delegationRepository.GetReceivedDelegations(offeredBy, cancellationToken);
            }

            if (reportee.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationNumberAttribute)
            {
                var party = await _contextRetrievalService.GetPartyForOrganization(reportee.Value);
                var mainUnits = await _contextRetrievalService.GetMainUnits(party.PartyId.SingleToList(), cancellationToken);
                var parties = party.PartyId.SingleToList();
                if (mainUnits?.FirstOrDefault() is var mainUnit && mainUnit?.PartyId != null)
                {
                    parties.Add((int)mainUnit.PartyId);
                }

                return await _delegationRepository.GetReceivedDelegations(parties, cancellationToken);
            }

            throw new ArgumentException($"failed to interpret attribute of type '{reportee.Id}'");
        }

        private async Task<IEnumerable<DelegationChange>> GetDelegationsFromRepositoryUsingParty(AttributeMatch reportee, Func<AttributeMatch, CancellationToken, Task<IEnumerable<DelegationChange>>> callback, CancellationToken cancellationToken)
        {
            if (int.TryParse(reportee.Value, out var partyId))
            {
                var party = await _contextRetrievalService.GetPartyAsync(partyId);
                if (party.PartyTypeName == PartyType.Person)
                {
                    return await callback(new(AltinnXacmlConstants.MatchAttributeIdentifiers.SocialSecurityNumberAttribute, party.Person.SSN), cancellationToken);
                }

                if (party.PartyTypeName == PartyType.Organisation)
                {
                    return await callback(new(AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationNumberAttribute, party.Organization.OrgNumber), cancellationToken);
                }
            }

            throw new ArgumentException("given party is not a person, nor an organization");
        }

        private async Task<List<RightDelegation>> MapDelegationResponse(IEnumerable<DelegationChange> delegations, CancellationToken cancellationToken = default)
        {
            var result = new List<RightDelegation>();
            var resources = await _contextRetrievalService.GetResourceList();

            foreach (var delegation in delegations)
            {
                var entry = new RightDelegation();
                if (delegation.CoveredByUserId != null)
                {
                    entry.To.Add(new(AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, delegation.CoveredByUserId.ToString()));
                }

                if (delegation.CoveredByPartyId != null)
                {
                    entry.To.Add(new(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, delegation.CoveredByPartyId.ToString()));
                }

                entry.From.Add(new(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, delegation.OfferedByPartyId.ToString()));

                var resourcePath = delegation.ResourceId.Split("/");
                if (delegation.ResourceType.Contains("AltinnApp", StringComparison.InvariantCultureIgnoreCase) && resourcePath.Length > 1)
                {
                    entry.Resource.AddRange(resources
                        .Where(a => a.AuthorizationReference.Any(p => p.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute && p.Value == resourcePath[0]))
                        .Where(a => a.AuthorizationReference.Any(p => p.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute && p.Value == resourcePath[1]))
                        .First().AuthorizationReference);
                }
                else
                {
                    entry.Resource.AddRange(resources.First(r => r.Identifier == delegation.ResourceId).AuthorizationReference);
                }

                result.Add(entry);
            }

            return result;
        }
    }
}