using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services.Interfaces;

namespace Altinn.AccessManagement.Integration.Services
{
    /// <inheritdoc />
    public class EventMapperService : IEventMapperService
    {
        /// <inheritdoc/>
        public DelegationChangeEventList MapToDelegationChangeEventList(List<DelegationChange> delegationChanges)
        {
            return new DelegationChangeEventList
            {
                DelegationChangeEvents = delegationChanges.Select(delegationChange => new DelegationChangeEvent
                {
                    EventType = (DelegationChangeEventType)delegationChange.DelegationChangeType,
                    DelegationChange = new SimpleDelegationChange
                    {
                        DelegationChangeId = delegationChange.DelegationChangeId,
                        AltinnAppId = delegationChange.ResourceId,
                        OfferedByPartyId = delegationChange.OfferedByPartyId,
                        CoveredByPartyId = delegationChange.CoveredByPartyId,
                        CoveredByUserId = delegationChange.CoveredByUserId,
                        PerformedByUserId = delegationChange.PerformedByUserId,
                        Created = delegationChange.Created.Value
                    }
                }).ToList()
            };
        }
    }
}
