using Altinn.AccessManagement.Core.Repositories.Interfaces;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// Conteiner holding the data required for DB update from a written policy file
    /// </summary>
    public class PolicyWriteOutput
    {
        /// <summary>
        /// The path the policy is written to
        /// </summary>
        public string PolicyPath { get; set; }
        
        /// <summary>
        /// The Version id of the written Policy file
        /// </summary>
        public string VersionId { get; set; }

        /// <summary>
        /// The LeaseId for the policy lease
        /// </summary>
        public string LeaseId { get; set; }

        /// <summary>
        /// The delegation change type used to 
        /// </summary>
        public DelegationChangeType ChangeType { get; set; }

        /// <summary>
        /// The client to handle the policy file
        /// </summary>
        public IPolicyRepository PolicyClient { get; set; }

        /// <summary>
        /// The rules corresponding to this policy file
        /// </summary>
        public InstanceRight Rules { get; set; }
    }
}
