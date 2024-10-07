#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Swashbuckle.Examples;

namespace Altinn.AccessManagement.Core.Models.ResourceRegistry;

/// <summary>
/// A valid resource identifier.
/// </summary>
[JsonConverter(typeof(JsonConverter))]
[DebuggerDisplay("{_value}")]
public sealed record ResourceInstanceIdentifier
    : ISpanParsable<ResourceInstanceIdentifier>,
    ISpanFormattable,
    IExampleDataProvider<ResourceInstanceIdentifier>
{
    private readonly string _value;

    private ResourceInstanceIdentifier(string value)
    {
        _value = value;
    }

    /// <inheritdoc/>
    public static IEnumerable<ResourceInstanceIdentifier>? GetExamples(ExampleDataOptions options)
    {
        yield return new ResourceInstanceIdentifier("0191579e-72bc-7977-af5d-f9e92af4393b");
        yield return new ResourceInstanceIdentifier("ext_1337");
    }

    /// <summary>
    /// Creates a new <see cref="ResourceInstanceIdentifier"/> from the specified value without validation.
    /// </summary>
    /// <param name="value">The resource instance identifier.</param>
    /// <returns>A <see cref="ResourceInstanceIdentifier"/>.</returns>
    public static ResourceInstanceIdentifier CreateUnchecked(string value)
        => new(value);

    /// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
    public static ResourceInstanceIdentifier Parse(string s)
        => Parse(s, provider: null);

    /// <inheritdoc/>
    public static ResourceInstanceIdentifier Parse(string s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
        ? result
        : throw new FormatException("Invalid resource instance identifier");

    /// <inheritdoc cref="ISpanParsable{TSelf}.Parse(ReadOnlySpan{char}, IFormatProvider?)"/>
    public static ResourceInstanceIdentifier Parse(ReadOnlySpan<char> s)
        => Parse(s, provider: null);

    /// <inheritdoc/>
    public static ResourceInstanceIdentifier Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
        ? result
        : throw new FormatException("Invalid resource instance identifier");

    /// <inheritdoc/>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out ResourceInstanceIdentifier result)
        => TryParse(s.AsSpan(), s, out result);

    /// <inheritdoc/>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out ResourceInstanceIdentifier result)
        => TryParse(s, original: null, out result);

    private static bool TryParse(ReadOnlySpan<char> s, string? original, [MaybeNullWhen(false)] out ResourceInstanceIdentifier result)
    {
        result = new ResourceInstanceIdentifier(original ?? new string(s));
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

    private sealed class JsonConverter : JsonConverter<ResourceInstanceIdentifier>
    {
        public override ResourceInstanceIdentifier? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (!TryParse(str, null, out var result))
            {
                throw new JsonException("Invalid resource instance identifier");
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, ResourceInstanceIdentifier value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value._value);
        }
    }
}
