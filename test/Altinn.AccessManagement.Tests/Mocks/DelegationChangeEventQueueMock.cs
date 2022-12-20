using System;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Azure.Storage.Queues.Models;

namespace Altinn.AccessManagement.Tests.Mocks
{
    /// <inheritdoc />
    public class DelegationChangeEventQueueMock : IDelegationChangeEventQueue
    {
        /// <summary>
        /// Mocks pushing delegation changes to the event queue
        /// </summary>
        /// <param name="delegationChange">The delegation change stored in postgresql</param>
        public Task<SendReceipt> Push(DelegationChange delegationChange)
        {
            if (delegationChange.ResourceId == "error/delegationeventfail")
            {
                throw new Exception("DelegationChangeEventQueue || Push || Error");
            }

            return Task.FromResult((SendReceipt)null);
        }
    }
}
