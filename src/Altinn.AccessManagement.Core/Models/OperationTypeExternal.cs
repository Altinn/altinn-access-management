using System.Runtime.Serialization;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// Enum definition of the AltinnII external operation types
    /// </summary>
    public enum OperationTypeExternal
    {
        /// <summary>
        /// Operation type is Read operation in ServiceEngine database
        /// </summary>
        [EnumMember]
        Read,

        /// <summary>
        /// Operation Type is Write in ServiceEngine database.
        /// This represents Create, FillIn, SendIn, SendBack and Delete actions.
        /// </summary>
        [EnumMember]
        Write,

        /// <summary>
        /// Operation Type is Sign in ServiceEngine database .
        /// </summary>
        [EnumMember]
        Sign,

        /// <summary>
        /// Operation used for Link services (at least for now) which are external services
        /// which only require a single general operation 
        /// </summary>
        [EnumMember]
        Access
    }
}
