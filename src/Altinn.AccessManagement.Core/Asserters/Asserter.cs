using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Core.Asserts;

/// <summary>
/// The signature of an Assertion that validates models
/// </summary>
public delegate void Assertion<T>(IDictionary<string, string[]> errors, IEnumerable<T> attributes);

/// <summary>
/// ss
/// </summary>
/// <typeparam name="TModel">aa</typeparam>
public class Asserter<TModel> : IAssert<TModel>
{
    /// <summary>
    /// If any given asserts don't add an error to the errors dictionary parameter then all other errors are ignored
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
    /// with a set of asserts. However, this can also be combined inside the Any method to make an even more complex Any statement.
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
    /// summary
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

    private static void AddError(IDictionary<string, string[]> errors, KeyValuePair<string, string[]> entry)
    {
        if (errors.TryGetValue(entry.Key, out var value))
        {
            errors[entry.Key] = value == null ? entry.Value : value.Concat(entry.Value)?.Distinct()?.ToArray();
        }
        else
        {
            errors.Add(entry);
        }
    }

    /// <summary>
    /// Evaluates the given given values by 
    /// </summary>
    /// <param name="values">the values to be asserted</param>
    /// <param name="actions">assertions</param>
    /// <returns></returns>
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
    /// Joins multiple evaluation to a single result
    /// </summary>
    /// <param name="evaluations">evaluations</param>
    /// <returns></returns>
    public ValidationProblemDetails Join(params ValidationProblemDetails[] evaluations)
    {
        var result = new ValidationProblemDetails()
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "there is an issue with the input body",
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
}