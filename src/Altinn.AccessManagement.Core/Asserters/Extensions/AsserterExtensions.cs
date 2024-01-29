using Altinn.AccessManagement.Core.Asserts;
using Altinn.AccessManagement.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.AccessManagement.Core.Resolvers.Extensions;

/// <summary>
/// General extensions for the Asserters
/// </summary>
public static class AsserterExtensions
{
    /// <summary>
    /// configure DPI for asserters
    /// </summary>
    /// <param name="services">service collection</param>
    /// <returns></returns>
    public static IServiceCollection ConfigureAsserters(this IServiceCollection services)
    {
        services.AddTransient<IAssert<AttributeMatch>, Asserter<AttributeMatch>>();
        services.AddAuthorization();
        return services;
    }
}