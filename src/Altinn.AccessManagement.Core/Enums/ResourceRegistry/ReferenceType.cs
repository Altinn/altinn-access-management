using System.Runtime.Serialization;

namespace Altinn.AccessManagement.Core.Models.ResourceRegistry
{
    /// <summary>
    /// Enum for reference types of resources in the resource registry
    /// </summary>
    public enum ReferenceType : int
    {
        [EnumMember(Value = "Default")]
        Default = 0,

        [EnumMember(Value = "ServiceCodeVersion")]
        ServiceCodeVersion = 1,

        [EnumMember(Value = "OrgApp")]
        OrgApp = 2,

        [EnumMember(Value = "Uri")]
        Uri = 3,

        [EnumMember(Value = "DelegationSchemeId")]
        DelegationSchemeId = 4,

        [EnumMember(Value = "DelegationScope")]
        DelegationScope = 5,
    }
}
