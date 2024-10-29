#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.Swashbuckle.Examples;

namespace Altinn.AccessManagement.Core.Models.ResourceRegistry;

/// <summary>
/// A valid resource identifier.
/// </summary>
[JsonConverter(typeof(JsonConverter))]
[DebuggerDisplay("{_value}")]
public sealed record ResourceIdentifier
    : ISpanParsable<ResourceIdentifier>,
    ISpanFormattable,
    IExampleDataProvider<ResourceIdentifier>
{
    private readonly string _value;

    private ResourceIdentifier(string value)
    {
        _value = value;
    }

    /// <inheritdoc/>
    public static IEnumerable<ResourceIdentifier>? GetExamples(ExampleDataOptions options)
    {
        yield return new ResourceIdentifier("example-resourceid");
        yield return new ResourceIdentifier("app_skd_flyttemelding");
    }

    /// <summary>
    /// Creates a new <see cref="ResourceIdentifier"/> from the specified value without validation.
    /// </summary>
    /// <param name="value">The resource identifier.</param>
    /// <returns>A <see cref="ResourceIdentifier"/>.</returns>
    public static ResourceIdentifier CreateUnchecked(string value)
        => new(value);

    /// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
    public static ResourceIdentifier Parse(string s)
        => Parse(s, provider: null);

    /// <inheritdoc/>
    public static ResourceIdentifier Parse(string s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
        ? result
        : throw new FormatException("Invalid resource identifier");

    /// <inheritdoc cref="ISpanParsable{TSelf}.Parse(ReadOnlySpan{char}, IFormatProvider?)"/>
    public static ResourceIdentifier Parse(ReadOnlySpan<char> s)
        => Parse(s, provider: null);

    /// <inheritdoc/>
    public static ResourceIdentifier Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
        ? result
        : throw new FormatException("Invalid resource identifier");

    /// <inheritdoc/>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out ResourceIdentifier result)
        => TryParse(s.AsSpan(), s, out result);

    /// <inheritdoc/>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out ResourceIdentifier result)
        => TryParse(s, original: null, out result);

    private static bool TryParse(ReadOnlySpan<char> s, string? original, [MaybeNullWhen(false)] out ResourceIdentifier result)
    {
        if (!ServiceResourceHelper.ResourceIdentifierRegex().IsMatch(s))
        {
            result = null;
            return false;
        }

        result = new ResourceIdentifier(original ?? new string(s));
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

    private sealed class JsonConverter : JsonConverter<ResourceIdentifier>
    {
        public override ResourceIdentifier? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (!TryParse(str, null, out var result))
            {
                throw new JsonException("Invalid resource identifier");
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, ResourceIdentifier value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value._value);
        }
    }
}
