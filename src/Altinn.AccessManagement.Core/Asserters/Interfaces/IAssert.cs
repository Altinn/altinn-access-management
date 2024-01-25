using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Core.Asserts;

/// <summary>
/// summary
/// </summary>
/// <typeparam name="TModel">a</typeparam>
public interface IAssert<TModel>
{
    /// <summary>
    /// Summary
    /// </summary>
    /// <param name="evaluations">a</param>
    /// <returns></returns>
    public ValidationProblemDetails Join(params ValidationProblemDetails[] evaluations);
    
    /// <summary>
    /// Summary
    /// </summary>
    /// <param name="values">a</param>
    /// <param name="actions">b</param>
    /// <returns></returns>
    ValidationProblemDetails Evaluate(IEnumerable<TModel> values, params Assertion<TModel>[] actions);
    
    /// <summary>
    /// Summary
    /// </summary>
    /// <param name="actions">a</param>
    /// <returns></returns>
    Assertion<TModel> Single(params Assertion<TModel>[] actions);

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="actions">a</param>
    /// <returns></returns>
    Assertion<TModel> All(params Assertion<TModel>[] actions);

    /// <summary>
    /// summary
    /// </summary>
    /// <param name="actions">a</param>
    /// <returns></returns>
    Assertion<TModel> Any(params Assertion<TModel>[] actions);
}