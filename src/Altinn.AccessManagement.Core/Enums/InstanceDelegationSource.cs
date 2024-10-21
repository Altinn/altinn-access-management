using System.Runtime.Serialization;
using NpgsqlTypes;

namespace Altinn.AccessManagement.Core.Enums
{
    /// <summary>
    /// Enum defining delegation source (app or user)
    /// </summary>
    public enum InstanceDelegationSource
    {
        /// <summary>
        /// Delegation by user
        /// </summary>
        [EnumMember(Value = "user")]
        [PgName("user")]
        User,

        /// <summary>
        /// Delegation by app
        /// </summary>
        [EnumMember(Value = "app")]
        [PgName("app")]
        App
    }
}
