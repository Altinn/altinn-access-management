using System.Runtime.Serialization;

namespace Altinn.AccessManagement.Enums
{
    /// <summary>
    /// Enum for different right delegation status responses
    /// </summary>
    public enum DelegableStatusExternal
    {
        /// <summary>
        /// User is not able to delegate the right
        /// </summary>
        [EnumMember(Value = "NotDelegable")]
        NotDelegable = 0,

        /// <summary>
        /// User is able to delegate the right
        /// </summary>
        [EnumMember(Value = "Delegable")]
        Delegable = 1
    }
}
