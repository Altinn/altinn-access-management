using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Altinn.AccessManagement.Enums.ResourceRegistry
{
    /// <summary>
    /// Defines the type of party that a resource is targeting
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ResourcePartyTypeExternal
    {
        [EnumMember(Value = "PrivatePerson")]
        PrivatePerson = 0,

        [EnumMember(Value = "LegalEntityEnterprise")]
        LegalEntityEnterprise = 1,

        [EnumMember(Value = "Company")]
        Company = 2,

        [EnumMember(Value = "BankruptcyEstate")]
        BankruptcyEstate = 3,

        [EnumMember(Value = "SelfRegisteredUser")]
        SelfRegisteredUser = 4
    }
}
