#nullable enable

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Swashbuckle.Examples;
using Altinn.Swashbuckle.Filters;

namespace Altinn.AccessManagement.Core.Models.Rights;

/// <summary>
/// A xacml action string.
/// </summary>
[JsonConverter(typeof(ActionIdentifier.JsonConverter))]
[SwaggerString]
public class ActionIdentifier : ISpanParsable<ActionIdentifier>,
    ISpanFormattable,
    IExampleDataProvider<ActionIdentifier>
{
    private readonly string _value;

    private ActionIdentifier(string value)
    {
        _value = value;
    }

    /// <summary>
    /// Creates a new <see cref="ActionIdentifier"/> from the specified value without validation.
    /// </summary>
    /// <param name="value">The action identifier.</param>
    /// <returns>A <see cref="ActionIdentifier"/>.</returns>
    public static ActionIdentifier CreateUnchecked(string value)
        => new(value);

    /// <inheritdoc/>
    public static IEnumerable<ActionIdentifier>? GetExamples(ExampleDataOptions options)
    {
        yield return new ActionIdentifier("read");
        yield return new ActionIdentifier("write");
    }

    /// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
    public static ActionIdentifier Parse(string s)
        => Parse(s, provider: null);

    /// <inheritdoc/>
    public static ActionIdentifier Parse(string s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
        ? result
        : throw new FormatException("Invalid action");

    /// <inheritdoc cref="ISpanParsable{TSelf}.Parse(ReadOnlySpan{char}, IFormatProvider?)"/>
    public static ActionIdentifier Parse(ReadOnlySpan<char> s)
        => Parse(s, provider: null);

    /// <inheritdoc/>
    public static ActionIdentifier Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
        ? result
        : throw new FormatException("Invalid action");

    /// <inheritdoc/>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out ActionIdentifier result)
        => TryParse(s.AsSpan(), s, out result);

    /// <inheritdoc/>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out ActionIdentifier result)
        => TryParse(s, original: null, out result);

    private static bool TryParse(ReadOnlySpan<char> s, string? original, [MaybeNullWhen(false)] out ActionIdentifier result)
    {
        result = new ActionIdentifier(original ?? new string(s));
        return true;
    }

    /// <inheritdoc/>
    public override string ToString()
        => _value;

    /// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
    public string ToString(string? format)
        => _value;

    /// <inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider)
        => _value;

    /// <inheritdoc/>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (destination.Length < _value.Length)
        {
            charsWritten = 0;
            return false;
        }

        _value.AsSpan().CopyTo(destination);
        charsWritten = _value.Length;
        return true;
    }

    private sealed class JsonConverter : JsonConverter<ActionIdentifier>
    {
        public override ActionIdentifier? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (!TryParse(str, null, out var result))
            {
                throw new JsonException("Invalid action");
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, ActionIdentifier value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value._value);
        }
    }
}
