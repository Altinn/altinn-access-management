using Altinn.AuthorizationAdmin.Core.Models;
using Altinn.AuthorizationAdmin.Core.Services.Interface;

namespace Altinn.AuthorizationAdmin.Core.Services.Implementation
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
                        AltinnAppId = delegationChange.AltinnAppId,
                        OfferedByPartyId = delegationChange.OfferedByPartyId,
                        CoveredByPartyId = delegationChange.CoveredByPartyId,
                        CoveredByUserId = delegationChange.CoveredByUserId,
                        PerformedByUserId = delegationChange.PerformedByUserId,
                        Created = delegationChange.Created
                    }
                }).ToList()
            };
        }
    }
}
