#nullable enable

using System.Text.Json.Serialization;

namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// A list object is a wrapper around a list of items to allow for the API to be
/// extended in the future without breaking backwards compatibility.
/// </summary>
public abstract record ListObjectResult
{
}

/// <summary>
/// A concrete list object.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
/// <param name="Items">The items.</param>
public record ListObjectResult<T>(
    [property: JsonPropertyName("data")]
    IEnumerable<T> Items)
    : ListObjectResult;
