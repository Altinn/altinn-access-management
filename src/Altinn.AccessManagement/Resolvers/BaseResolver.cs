using System.Collections.Concurrent;
using Altinn.AccessManagement.Core.Models;
using Microsoft.AspNetCore.Mvc.TagHelpers;

namespace Altinn.AccessManagement.Resovers;

/// <summary>
/// baser resolver
/// </summary>
/// <param name="resourceName">resource names</param>
/// <param name="resolvers">resovles</param>
public abstract class BaseResolver(string resourceName, params IAttributeResolver[] resolvers) : IAttributeResolver
{
    /// <inheritdoc/>
    public string ResourceName { get; } = resourceName;

    /// <summary>
    /// jaje
    /// </summary>
    public virtual IDictionary<string, LeafResolver> LeafResolvers { get; } = new Dictionary<string, LeafResolver>();

    /// <summary>
    /// kake
    /// </summary>
    /// <param name="attributes">a</param>
    /// <param name="wants">v</param>
    /// <returns></returns>
    public delegate Task<IEnumerable<AttributeMatch>> LeafResolver(IEnumerable<AttributeMatch> attributes, IEnumerable<string> wants);

    /// <summary>
    /// a
    /// </summary>
    public IAttributeResolver[] Resolvers { get; } = resolvers;

    /// <inheritdoc/>
    public virtual Task<IEnumerable<AttributeMatch>> Resolve(IEnumerable<AttributeMatch> attributes, IEnumerable<string> wants)
    {
        var result = new ConcurrentBag<AttributeMatch>();
        wants = wants.Select(p => RemoveResourceNamePrefix(p));
        var childs = attributes
            .Where(attribute => !string.IsNullOrEmpty(attribute.Id))
            .Select(attribute => new AttributeMatch() { Id = RemoveResourceNamePrefix(attribute.Id), Value = attribute.Value });

        Parallel.ForEach(Resolvers, async resolver =>
        {
            foreach (var data in await resolver.Resolve(childs, wants))
            {
                result.Add(new()
                {
                    Id = $"{resolver.ResourceName}:{data.Id}",
                    Value = data.Value
                });
            }
        });

        return Task.FromResult(result.Distinct());
    }

    /// <summary>
    /// dispatches the recursive handler
    /// </summary>
    /// <param name="resolvers">a</param>
    /// <param name="attributes">b</param>
    /// <param name="wants">c</param>
    /// <returns></returns>
    public async Task<IEnumerable<AttributeMatch>> ResolveLeaf(IDictionary<string, LeafResolver> resolvers, IEnumerable<AttributeMatch> attributes, IEnumerable<string> wants)
    {
        var result = new ConcurrentBag<AttributeMatch>();
        foreach (var item in attributes)
        {
            result.Add(item);
        }

        await ResolveLeaf(resolvers, attributes, wants, result, -1);
        return await Resolve(result, wants);
    }

    /// <summary>
    /// Recursive handler
    /// </summary>
    /// <param name="resolvers">a</param>
    /// <param name="attributes">b</param>
    /// <param name="wants">c</param>
    /// <param name="result">d</param>
    /// <param name="count">e</param>
    /// <returns></returns>
    public async Task<IEnumerable<AttributeMatch>> ResolveLeaf(IDictionary<string, LeafResolver> resolvers, IEnumerable<AttributeMatch> attributes, IEnumerable<string> wants, ConcurrentBag<AttributeMatch> result, int count)
    {
        if (resolvers.Count == 0 || count == result.Count)
        {
            return result;
        }

        count = result.Count;
        var exhaustedHandlers = new ConcurrentBag<string>();
        Parallel.ForEach(attributes, async attribute =>
        {
            if (resolvers.ContainsKey(attribute.Id))
            {
                var handlerResult = await resolvers[attribute.Id](attributes, wants);
                exhaustedHandlers.Add(attribute.Id);
            }
        });

        foreach (var key in exhaustedHandlers)
        {
            resolvers.Remove(key);
        }

        return await ResolveLeaf(resolvers, result.ToList(), wants, result, count);
    }

    /// <summary>
    /// kake
    /// </summary>
    /// <param name="result">a</param>
    /// <param name="attribute">b</param>
    /// <param name="value">c</param>
    /// <param name="wants">d</param>
    public void AddAttribute(List<AttributeMatch> result, string attribute, string value, IEnumerable<string> wants)
    {
        if (wants.Any(want => want.Equals(attribute, StringComparison.InvariantCultureIgnoreCase)))
        {
            result.Add(new AttributeMatch()
            {
                Id = attribute,
                Value = value
            });
        }
    }

    /// <summary>
    /// a
    /// </summary>
    /// <param name="resourceName">resourceName</param>
    /// <param name="delimiter">delimiter default value is ":"</param>
    /// <returns></returns>
    internal string RemoveResourceNamePrefix(string resourceName, string delimiter = ":") =>
        resourceName.Contains(':') ? resourceName.Substring(resourceName.IndexOf(':') + 1) : string.Empty;
}