using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Resolvers;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.AccessManagement.Core.Resolvers.Extensions;

/// <summary>
/// General extensions for the resolvers
/// </summary>
public static class ResolverExtensions
{
    /// <summary>
    /// configure DPI for resolvers
    /// </summary>
    /// <param name="services">service collection</param>
    /// <returns></returns>
    public static IServiceCollection ConfigureResolvers(this IServiceCollection services)
    {
        services.AddTransient<IAttributeResolver, UrnResolver>();
        services.AddTransient<UrnResolver>();
        services.AddTransient<AltinnEnterpriseUserResolver>();
        services.AddTransient<AltinnResolver>();
        services.AddTransient<AltinnResourceResolver>();
        services.AddTransient<AltinnOrganizationResolver>();
        services.AddTransient<AltinnPersonResolver>();
        services.AddTransient<PartyAttributeResolver>();
        services.AddTransient<UserAttributeResolver>();
        return services;
    }

    /// <summary>
    /// Gets first match of specific attribute value as string
    /// </summary>
    /// <param name="values">list of attributes</param>
    /// <param name="attributes">attributes types / URN's</param>
    /// <returns></returns>
    public static string GetRequiredString(this IEnumerable<AttributeMatch> values, params string[] attributes)
    {
        foreach (var attribute in attributes)
        {
            if (values.FirstOrDefault(value => value.Id.Equals(attribute, StringComparison.InvariantCultureIgnoreCase)) is var result && result != null)
            {
                return result.Value;
            }
        }

        throw new ArgumentException($"couldn't find any [{string.Join(",", attributes)}] in list of attributes");
    }

    /// <summary>
    /// Gets first match of specific attribute value as string
    /// </summary>
    /// <param name="values">list of attributes</param>
    /// <param name="attributes">attributes types / URN's</param>
    /// <returns></returns>
    public static string GetString(this IEnumerable<AttributeMatch> values, params string[] attributes)
    {
        foreach (var attribute in attributes)
        {
            if (values.FirstOrDefault(value => value.Id.Equals(attribute, StringComparison.InvariantCultureIgnoreCase)) is var result && result != null)
            {
                return result.Value;
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Gets first match of specific attribute value as integer.
    /// </summary>
    /// <param name="values">list of attributes</param>
    /// <param name="attributes">attributes types / URN's</param>
    /// <returns></returns>
    public static int GetRequiredInt(this IEnumerable<AttributeMatch> values, params string[] attributes) => int.Parse(values.GetRequiredString(attributes));
}