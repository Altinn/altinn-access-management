using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Models;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Mappers
{
    /// <summary>
    /// A class that holds the delegation mapper configuration
    /// </summary>
    public class DelegationMapper : AutoMapper.Profile
    {
        /// <summary>
        /// Configuration for delegation mapper
        /// </summary>
        public DelegationMapper() 
        {
            CreateMap<Delegation, DelegationExternal>();
            CreateMap<Party, PartyExternal>();
        }
    }
}
