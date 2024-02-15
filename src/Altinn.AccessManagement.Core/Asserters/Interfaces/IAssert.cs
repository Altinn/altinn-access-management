using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Core.Asserters;

/// <summary>
/// Contains the basic methods for combining and nesting assertions.
/// Use the <see cref="Asserter{TModel}.Evaluate"/> to make assertions for one dataset. If you have multiple datasets,
/// you can pass the evaluations to the <see cref="Asserter{TModel}.Join"/> to get a single assertions result for all
/// the datasets.
/// </summary>
/// <typeparam name="TModel">the model that should</typeparam>
public interface IAssert<TModel>
{
    /// <summary>
    /// Joins multiple evaluations to a single result
    /// </summary>
    /// <param name="evaluations">evaluations</param>
    /// <returns>
    /// returns null if all the assertions passed. If an assertion generated an error then these errors will be present in <see cref="ValidationProblemDetails.Errors"/>
    /// dictionary where the keys should be named the assertion method and value should contain the message(s).
    /// </returns>
    public ValidationProblemDetails Join(params ValidationProblemDetails[] evaluations);

    /// <summary>
    /// Executes all the assertions using the given dataset 
    /// </summary>
    /// <param name="values">the values to be asserted</param>
    /// <param name="actions">assertions</param>
    /// <returns>
    /// returns null if all the assertions passed. If an assertion generated an error then these errors will be present in <see cref="ValidationProblemDetails.Errors"/>
    /// dictionary where the keys should be named the assertion method and value should contain the message(s).
    /// </returns>
    ValidationProblemDetails Evaluate(IEnumerable<TModel> values, params Assertion<TModel>[] actions);

    /// <summary>
    /// Single will omit writing to the error dict if there is just one assertion that passes.
    /// Can be combined with <see cref="Asserter{TModel}.Any"/> and <see cref="Asserter{TModel}.All"/>
    /// to create even more complex assertion.
    /// </summary>
    /// <param name="actions">assertions</param>
    /// <returns></returns>
    Assertion<TModel> Single(params Assertion<TModel>[] actions);

    /// <summary>
    /// All errors are directly written to the errors dictionary. This method works as calling the Evaluate method directly
    /// with a set of asserts. However, this can also be combined inside <see cref="Asserter{TModel}.Any"/> or <see cref="Asserter{TModel}.Single"/>
    /// to create even more complex assertion.
    /// </summary>
    /// <param name="actions">assertions</param>
    /// <returns></returns>
    Assertion<TModel> All(params Assertion<TModel>[] actions);

    /// <summary>
    /// If any given asserts don't add an error to the errors dictionary parameter, then all other errors are ignored
    /// </summary>
    /// <param name="actions">assertions</param>
    /// <returns></returns>
    Assertion<TModel> Any(params Assertion<TModel>[] actions);
}