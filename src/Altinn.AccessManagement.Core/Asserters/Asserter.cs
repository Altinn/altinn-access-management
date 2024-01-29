using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Core.Asserters;

/// <summary>
/// The function signature of an Assertion that validates data
/// </summary>
public delegate void Assertion<T>(IDictionary<string, string[]> errors, IEnumerable<T> attributes);

/// <inheritdoc/>
public class Asserter<TModel> : IAssert<TModel>
{
    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public Assertion<TModel> All(params Assertion<TModel>[] actions) => (errors, values) =>
    {
        foreach (var action in actions)
        {
            action(errors, values);
        }
    };

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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