using System.Collections.Concurrent;
using System.Security;
using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Core.Resolvers;

/// <summary>
/// aaa
/// </summary>
/// <param name="attributes">a</param>
/// <param name="cancellationToken">b</param>
/// <returns></returns>
public delegate Task<IEnumerable<AttributeMatch>> LeafResolver(IEnumerable<AttributeMatch> attributes, CancellationToken cancellationToken);

/// <summary>
/// summary
/// </summary>
/// <param name="resourceName">a</param>
/// <param name="resolvers">resovles</param>
public abstract class AttributeResolver(string resourceName, params IAttributeResolver[] resolvers) : IAttributeResolver
{
    /// <summary>
    /// ResouceName
    /// </summary>
    public string ResourceName { get; } = resourceName;

    /// <summary>
    /// summary
    /// </summary>
    public IAttributeResolver[] Resolvers { get; } = resolvers;

    /// <summary>
    /// summary
    /// </summary>
    public virtual List<AttributeResolution> LeafResolvers { get; } = [];

    /// <summary>
    /// asas
    /// </summary>
    /// <param name="needs">needs</param>
    /// <param name="resolves">resolves</param>
    /// <param name="resolver">resolver</param>
    public virtual IAttributeResolver AddLeaf(IEnumerable<string> needs, IEnumerable<string> resolves, LeafResolver resolver)
    {
        LeafResolvers.Add(new(needs, resolves, resolver));
        return this;
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<AttributeMatch>> Resolve(IEnumerable<AttributeMatch> attributes, IEnumerable<string> wants, CancellationToken cancellationToken)
    {
        var result = new ConcurrentBag<AttributeMatch>();
        foreach (var attribute in attributes)
        {
            result.Add(attribute);
        }

        for (int count = 0; count != result.Count; count = result.Count)
        {
            await ResolveInternalNodes(attributes, wants, result, cancellationToken);
            await ResolveLeafNodes(wants, result, cancellationToken);
        }

        return result.Distinct();
    }

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="wants">a</param>
    /// <param name="result">b</param>
    /// <param name="cancellationToken">c</param>
    private async Task ResolveLeafNodes(IEnumerable<string> wants, ConcurrentBag<AttributeMatch> result, CancellationToken cancellationToken) =>
        await Task.WhenAll(LeafResolvers.Select(async resolver =>
        {
            var resolverExecutionCondtions = new List<bool>()
            {
                DoesResolverHaveItsNeeds(resolver, result),
                DoesResolverAddTheWants(wants, resolver),
                DoesResolverAddNewAttributes(result, resolver)
            };

            if (resolverExecutionCondtions.TrueForAll(condition => condition))
            {
                foreach (var attribute in await resolver.Resolver(result, cancellationToken))
                {
                    result.Add(attribute);
                }
            }
        })).WaitAsync(cancellationToken);

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="attributes">a</param>
    /// <param name="wants">b</param>
    /// <param name="result">c</param>
    /// <param name="cancellationToken">d</param>
    private async Task ResolveInternalNodes(IEnumerable<AttributeMatch> attributes, IEnumerable<string> wants, ConcurrentBag<AttributeMatch> result, CancellationToken cancellationToken) =>
        await Task.WhenAll(Resolvers.Select(async resolver =>
        {
            if (DoesInternalNodeMatchWants(wants))
            {
                foreach (var attribute in await resolver.Resolve(attributes, wants, cancellationToken))
                {
                    result.Add(attribute);
                }
            }
        })).WaitAsync(cancellationToken);

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="result">a</param>
    /// <param name="type">b</param>
    /// <param name="value">c</param>
    protected static void AddResult(List<AttributeMatch> result, string type, object value)
    {
        result.Add(new()
        {
            Id = type,
            Value = value.ToString(),
        });
    }

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="attributes">a</param>
    /// <param name="type">b</param>
    /// <returns></returns>
    protected static string GetAttributeString(IEnumerable<AttributeMatch> attributes, string type) =>
        attributes.First(attribute => attribute.Id.Equals(type, StringComparison.InvariantCultureIgnoreCase)).Value;

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="attributes">a</param>
    /// <param name="type">b</param>
    /// <returns></returns>
    protected static int GetAttributeInt(IEnumerable<AttributeMatch> attributes, string type) =>
        int.Parse(GetAttributeString(attributes, type));

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="wants">a</param>
    /// <returns></returns>
    private bool DoesInternalNodeMatchWants(IEnumerable<string> wants) =>
        wants.Any(want => want.StartsWith(ResourceName, StringComparison.CurrentCultureIgnoreCase) || ResourceName.StartsWith(want, StringComparison.InvariantCultureIgnoreCase)) || !wants.Any();

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="resolution">a</param>
    /// <param name="result">b</param>
    /// <returns></returns>
    private static bool DoesResolverHaveItsNeeds(AttributeResolution resolution, ConcurrentBag<AttributeMatch> result) =>
        resolution.Needs.All(need => result.Any(result => result.Id.Equals(need, StringComparison.InvariantCultureIgnoreCase)));

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="wants">a</param>
    /// <param name="resolver">b</param>
    /// <returns></returns>
    private static bool DoesResolverAddTheWants(IEnumerable<string> wants, AttributeResolution resolver) =>
        wants.Any(want => resolver.Resolves.Any(resolve => resolve.StartsWith(want, StringComparison.InvariantCultureIgnoreCase))) || !wants.Any();

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="result">a</param>
    /// <param name="resolver">b</param>
    /// <returns></returns>
    private static bool DoesResolverAddNewAttributes(ConcurrentBag<AttributeMatch> result, AttributeResolution resolver) =>
        !resolver.Resolves.All(resolve => result.Any(attribute => attribute.Id.Equals(resolve, StringComparison.InvariantCultureIgnoreCase)));
}