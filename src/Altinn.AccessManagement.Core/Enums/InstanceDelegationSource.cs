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
        /// Normal instance delegation
        /// </summary>
        [EnumMember(Value = "user")]
        [PgName("user")]
        User,

        /// <summary>
        /// Special case of instance delegation extends delegations to organizations to all users in the receiving organization with parallel role/package to also getting access to the instance
        /// </summary>
        [EnumMember(Value = "app")]
        [PgName("app")]
        App
    }
}
