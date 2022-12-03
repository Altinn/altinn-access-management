using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Models;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Mappers
{
    /// <summary>
    /// A class that holds the access management mapper configuration
    /// </summary>
    public class AccessManagementMapper : AutoMapper.Profile
    {
        /// <summary>
        /// Configuration for accessmanagement mapper
        /// </summary>
        public AccessManagementMapper() 
        {
            CreateMap<Delegation, DelegationExternal>();
            CreateMap<Party, PartyExternal>();
        }
    }
}
