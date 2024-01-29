using System.Collections.Concurrent;
using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Core.Resolvers;

/// <summary>
/// Leaf resolver is a function that fetches new requested attributes based on given attributes
/// </summary>
/// <param name="attributes">current attributes</param>
/// <param name="cancellationToken">Cancellation token</param>
public delegate Task<IEnumerable<AttributeMatch>> LeafResolver(IEnumerable<AttributeMatch> attributes, CancellationToken cancellationToken);

/// <summary>
/// A generic node in the parse tree. Root node should be the UrnResolver
/// </summary>
/// <param name="resourceName">Name of the resource / Urn </param>
/// <param name="internalNodes">Internal nodes in the parse tree</param>
public abstract class AttributeResolver(string resourceName, params IAttributeResolver[] internalNodes) : IAttributeResolver
{
    /// <summary>
    /// Name / URN of the resource.
    /// </summary>
    public string ResourceName { get; } = resourceName;

    /// <summary>
    /// List of internal nodes
    /// </summary>
    public IAttributeResolver[] InternalNodes { get; } = internalNodes;

    /// <summary>
    /// List of Leaf Nodes / 
    /// </summary>
    public virtual List<AttributeResolution> LeafResolvers { get; } = [];

    /// <summary>
    /// Adds a leaf node this internal node
    /// </summary>
    /// <param name="needs">The required attributes to be present in order for the attribute to run</param>
    /// <param name="resolves">The attributes the resolver are able to fetch if provided it needs.</param>
    /// <param name="resolver">A function reference which when called upon does the work.</param>
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
    /// Executes all leaf nodes if following conditions are met
    /// - The resolver has all required attributes (needs)
    /// - The resolver actually add new attributes based on requested attributes (wants)
    /// - The attributes haven't already been added by other resolvers
    /// </summary>
    /// <param name="wants">list of attribute types that are requested by callee</param>
    /// <param name="result">current result of all attributes</param>
    /// <param name="cancellationToken">Cancellation token</param>
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
    /// Traverses the internal tree
    /// </summary>
    /// <param name="attributes">list of given attributes</param>
    /// <param name="wants">list of attribute types that are requested by callee</param>
    /// <param name="result">current result of all attributes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    private async Task ResolveInternalNodes(IEnumerable<AttributeMatch> attributes, IEnumerable<string> wants, ConcurrentBag<AttributeMatch> result, CancellationToken cancellationToken) =>
        await Task.WhenAll(InternalNodes.Select(async resolver =>
        {
            foreach (var attribute in await resolver.Resolve(attributes, wants, cancellationToken))
            {
                result.Add(attribute);
            }
        })).WaitAsync(cancellationToken);

    /// <summary>
    /// Adds the attributes to the result list
    /// </summary>
    /// <param name="result">current list</param>
    /// <param name="type">attribute type</param>
    /// <param name="value">attribute value</param>
    protected static void AddResult(List<AttributeMatch> result, string type, object value)
    {
        result.Add(new()
        {
            Id = type,
            Value = value.ToString(),
        });
    }

    /// <summary>
    /// Checks if the resolver has all required attributes (needs).
    /// </summary>
    /// <param name="resolution">leaf resolver</param>
    /// <param name="result">current result</param>
    /// <returns></returns>
    private static bool DoesResolverHaveItsNeeds(AttributeResolution resolution, ConcurrentBag<AttributeMatch> result) =>
        resolution.Needs.All(need => result.Any(result => result.Id.Equals(need, StringComparison.InvariantCultureIgnoreCase)));

    /// <summary>
    /// Checks if the resolver actually adds new attributes based on requested attributes (wants).
    /// </summary>
    /// <param name="wants">a</param>
    /// <param name="resolver">b</param>
    /// <returns></returns>
    private static bool DoesResolverAddTheWants(IEnumerable<string> wants, AttributeResolution resolver) =>
        wants.Any(want => resolver.Resolves.Any(resolve => resolve.StartsWith(want, StringComparison.InvariantCultureIgnoreCase))) || !wants.Any();

    /// <summary>
    /// Checks if the attributes haven't already been added by other resolvers.
    /// </summary>
    /// <param name="result">a</param>
    /// <param name="resolver">b</param>
    /// <returns></returns>
    private static bool DoesResolverAddNewAttributes(ConcurrentBag<AttributeMatch> result, AttributeResolution resolver) =>
        !resolver.Resolves.All(resolve => result.Any(attribute => attribute.Id.Equals(resolve, StringComparison.InvariantCultureIgnoreCase)));
}