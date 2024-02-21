﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Altinn.AccessManagement.Models;

/// <summary>
/// This model describes a an Attribute consisting of an Attribute Type and Attribute Value which can also be represented as a Urn by combining the properties as '{type}:{value}'
/// It's used both for external API input/output but also internally for working with attributes and matching to XACML-attributes used in policies, indentifying for instance a resource, a user, a party or an action.
/// </summary>
public class BaseAttributeExternal
{
    /// <summary>
    /// Gets or sets the attribute id for the match
    /// </summary>
    [Required]
    [JsonPropertyName("type")]
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the attribute value for the match
    /// </summary>
    [Required]
    [JsonPropertyName("value")]
    public string Value { get; set; }
}