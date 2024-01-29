using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Core.Asserts;

/// <summary>
/// The function signature of an Assertion that validates data
/// </summary>
public delegate void Assertion<T>(IDictionary<string, string[]> errors, IEnumerable<T> attributes);

/// <summary>
/// Contains the basic methods for combining and nesting assertions.
/// Use the <see cref="Asserter{TModel}.Evaluate"/> to make assertions for one dataset. If you have multiple datasets,
/// you can pass the evaluations to the <see cref="Asserter{TModel}.Join"/> to get a single assertions result for all
/// the datasets.
/// </summary>
/// <typeparam name="TModel">the model that should</typeparam>
public class Asserter<TModel> : IAssert<TModel>
{
    /// <summary>
    /// If any given asserts don't add an error to the errors dictionary parameter, then all other errors are ignored
    /// </summary>
    /// <param name="actions">assertions</param>
    /// <returns></returns>
    public Assertion<TModel> Any(params Assertion<TModel>[] actions) => (errors, values) =>
    {
        var result = new List<IDictionary<string, string[]>>();
        foreach (var action in actions)
        {
            var err = new Dictionary<string, string[]>();
            action(err, values);
            if (err.Count == 0)
            {
                return;
            }

            result.Add(err);
        }

        foreach (var entry in result)
        {
            foreach (var err in errors)
            {
                AddError(errors, err);
            }
        }
    };

    /// <summary>
    /// All errors are directly written to the errors dictionary. This method works as calling the Evaluate method directly
    /// with a set of asserts. However, this can also be combined inside <see cref="Asserter{TModel}.Any"/> or <see cref="Asserter{TModel}.Single"/>
    /// to create even more complex assertion.
    /// </summary>
    /// <param name="actions">assertions</param>
    /// <returns></returns>
    public Assertion<TModel> All(params Assertion<TModel>[] actions) => (errors, values) =>
    {
        foreach (var action in actions)
        {
            action(errors, values);
        }
    };

    /// <summary>
    /// Single will omit writing to the error dict if there is just one assertion that passes.
    /// Can be combined with <see cref="Asserter{TModel}.Any"/> and <see cref="Asserter{TModel}.All"/>
    /// to create even more complex assertion.
    /// </summary>
    /// <param name="actions">assertions</param>
    /// <returns></returns>
    public Assertion<TModel> Single(params Assertion<TModel>[] actions) => (errors, values) =>
    {
        var result = new List<IDictionary<string, string[]>>();
        foreach (var action in actions)
        {
            var err = new Dictionary<string, string[]>();
            action(err, values);
            if (err.Count > 0)
            {
                result.Add(err);
            }
        }

        if (result.Count + 1 == actions.Length)
        {
            return;
        }

        if (result.Count == 0)
        {
            errors.Add(nameof(Single), ["all assertions passed while it should only be one that passed"]);
        }

        foreach (var err in result)
        {
            foreach (var entry in err)
            {
                AddError(errors, entry);
            }
        }
    };

    /// <summary>
    /// Executes all the assertions using the given dataset 
    /// </summary>
    /// <param name="values">the values to be asserted</param>
    /// <param name="actions">assertions</param>
    /// <returns>
    /// returns null if all the assertions passed. If an assertion generated an error then these errors will be present in <see cref="ValidationProblemDetails.Errors"/>
    /// dictionary where the keys should be named the assertion method and value should contain the message(s).
    /// </returns>
    public ValidationProblemDetails Evaluate(IEnumerable<TModel> values, params Assertion<TModel>[] actions)
    {
        var result = new Dictionary<string, string[]>();
        foreach (var action in actions)
        {
            action(result, values);
        }

        if (result.Count > 0)
        {
            return new ValidationProblemDetails(result);
        }

        return null;
    }

    /// <summary>
    /// Joins multiple evaluations to a single result
    /// </summary>
    /// <param name="evaluations">evaluations</param>
    /// <returns>
    /// returns null if all the assertions passed. If an assertion generated an error then these errors will be present in <see cref="ValidationProblemDetails.Errors"/>
    /// dictionary where the keys should be named the assertion method and value should contain the message(s).
    /// </returns>
    public ValidationProblemDetails Join(params ValidationProblemDetails[] evaluations)
    {
        var result = new ValidationProblemDetails()
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "there is an issue with provided input",
        };

        foreach (var evaluation in evaluations)
        {
            if (evaluation != null)
            {
                foreach (var error in evaluation.Errors)
                {
                    AddError(result.Errors, error);
                }
            }
        }

        return result.Errors.Count > 0 ? result : null;
    }

    /// <summary>
    /// add error to the dictionary
    /// </summary>
    /// <param name="errors">error dictionary</param>
    /// <param name="entry">the key-value pair that should be written to the error dict</param>
    private static void AddError(IDictionary<string, string[]> errors, KeyValuePair<string, string[]> entry)
    {
        if (errors.TryGetValue(entry.Key, out var value))
        {
            errors[entry.Key] = value == null ? entry.Value : value.Concat(entry.Value ?? Enumerable.Empty<string>()).Distinct().ToArray();
        }
        else
        {
            errors.Add(entry);
        }
    }
}