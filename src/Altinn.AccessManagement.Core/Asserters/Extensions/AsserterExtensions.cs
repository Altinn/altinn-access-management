using Altinn.AccessManagement.Core.Asserts;
using Altinn.AccessManagement.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.AccessManagement.Core.Resolvers.Extensions;

/// <summary>
/// summary
/// </summary>
public static class AsserterExtensions
{
    /// <summary>
    /// configure all services regarding resolvers
    /// </summary>
    /// <param name="services">these are services</param>
    /// <returns></returns>
    public static IServiceCollection ConfigureAsserters(this IServiceCollection services)
    {
        services.AddTransient<IAssert<AttributeMatch>, Asserter<AttributeMatch>>();
        return services;
    }
}