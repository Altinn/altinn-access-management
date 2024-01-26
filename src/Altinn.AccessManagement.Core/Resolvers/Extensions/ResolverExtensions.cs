using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Resolvers;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.AccessManagement.Core.Resolvers.Extensions;

/// <summary>
/// summary
/// </summary>
public static class ResolverExtensions
{
    /// <summary>
    /// configure all services regarding resolvers
    /// </summary>
    /// <param name="services">these are services</param>
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
        return services;
    }

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="values">a</param>
    /// <param name="attributes">b</param>
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
    /// summary
    /// </summary>
    /// <param name="values">a</param>
    /// <param name="attributes">b</param>
    /// <returns></returns>
    public static int GetInt(this IEnumerable<AttributeMatch> values, params string[] attributes) => int.Parse(values.GetString(attributes));
}