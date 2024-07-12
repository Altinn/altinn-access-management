using System.Text.Json.Serialization;
using Altinn.AccessManagement.Enums;

namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// This model describes a an Attribute consisting of an Attribute Type and Attribute Value which can also be represented as a Urn by combining the properties as '{type}:{value}'
/// It's used both for external API input/output but also internally for working with attributes and matching to XACML-attributes used in policies, indentifying for instance a resource, a user, a party or an action.
/// </summary>
public class BaseAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseAttribute"/> class.
    /// </summary>
    public BaseAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseAttribute"/> class.
    /// </summary>
    public BaseAttribute(string type, string value)
    {
        Type = type;
        Value = value;
        Urn = $"{type}:{value}";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseAttribute"/> class.
    /// </summary>
    public BaseAttribute(UuidType type, Guid value) : this(type.ToString(), value.ToString())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseAttribute"/> class.
    /// </summary>
    public BaseAttribute(string urn)
    {
        int valuePos = urn.LastIndexOf(':') + 1;
        Urn = urn;
        Type = urn.Substring(0, valuePos - 1);
        Value = urn.Substring(valuePos);
    }

    /// <summary>
    /// Gets or sets the attribute id for the match
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the attribute value for the match
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; }

    /// <summary>
    /// Gets or sets the attribute value for the match
    /// </summary>
    [JsonPropertyName("urn")]
    public string Urn { get; set; }
}