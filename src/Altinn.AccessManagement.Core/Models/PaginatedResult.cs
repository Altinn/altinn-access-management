#nullable enable

namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// A paginated <see cref="ListObjectResult{T}"/>.
/// </summary>
public static class PaginatedResult
{
    /// <summary>
    /// Create a new <see cref="PaginatedResult{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of items</typeparam>
    /// <param name="items">The items</param>
    /// <param name="next">The optional next-link</param>
    /// <returns>A new <see cref="PaginatedResult{T}"/>.</returns>
    public static PaginatedResult<T> Create<T>(
        IEnumerable<T> items,
        string? next)
        => new(new(next), items);
}

/// <summary>
/// A paginated <see cref="ListObjectResult{T}"/>.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
/// <param name="Links">Pagination links.</param>
/// <param name="Items">The items.</param>
public record PaginatedResult<T>(
    PaginatedResultLinks Links,
    IEnumerable<T> Items)
    : ListObjectResult<T>(Items);

/// <summary>
/// Pagination links.
/// </summary>
/// <param name="Next">Link to the next page of items (if any).</param>
public record PaginatedResultLinks(
    string? Next);
