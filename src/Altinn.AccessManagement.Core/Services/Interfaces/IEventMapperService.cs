using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Core.Services.Interfaces
{
    /// <summary>
    /// Service mapping internal delegation changes to delegation change events
    /// </summary>
    public interface IEventMapperService
    {
        /// <summary>
        /// Maps to DelegationChangeEventList used for pushing delegation events to the event queue
        /// </summary>
        /// <param name="delegationChanges">List of delegation changes from postgreSQL</param>
        /// <returns>DelegationChangeEventList</returns>
        public DelegationChangeEventList MapToDelegationChangeEventList(List<DelegationChange> delegationChanges);
    }
}
