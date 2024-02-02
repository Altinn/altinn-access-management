using Altinn.AccessManagement.Core.Asserters;

namespace Altinn.AccessManagement.Tests.Asserters;

/// <summary>
/// summary
/// </summary>
public class AsserterTests
{
    /// <summary>
    /// summary
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <returns></returns>
    public static IAssert<TModel> Asserter<TModel>() => new Asserter<TModel>();
}