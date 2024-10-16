#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Swashbuckle.Examples;

namespace Altinn.AccessManagement.Core.Models.ResourceRegistry;

/// <summary>
/// A organization number (a string of 9 digits).
/// </summary>
[JsonConverter(typeof(ResourceId.JsonConverter))]
public class ResourceId : ISpanParsable<ResourceId>,
    ISpanFormattable,
    IExampleDataProvider<ResourceId>
{
    private readonly string _value;

    private ResourceId(string value)
    {
        _value = value;
    }

    /// <inheritdoc/>
    public static IEnumerable<ResourceId>? GetExamples(ExampleDataOptions options)
    {
        yield return new ResourceId("app_digdir_myapp");
        yield return new ResourceId("app_digdir_myotherapp");
    }

    /// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
    public static ResourceId Parse(string s)
        => Parse(s, provider: null);

    /// <inheritdoc/>
    public static ResourceId Parse(string s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
        ? result
        : throw new FormatException("Invalid resource id");

    /// <inheritdoc cref="ISpanParsable{TSelf}.Parse(ReadOnlySpan{char}, IFormatProvider?)"/>
    public static ResourceId Parse(ReadOnlySpan<char> s)
        => Parse(s, provider: null);

    /// <inheritdoc/>
    public static ResourceId Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        => TryParse(s, provider, out var result)
        ? result
        : throw new FormatException("Invalid resource id");

    /// <inheritdoc/>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out ResourceId result)
        => TryParse(s.AsSpan(), s, out result);

    /// <inheritdoc/>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out ResourceId result)
        => TryParse(s, original: null, out result);

    private static bool TryParse(ReadOnlySpan<char> s, string? original, [MaybeNullWhen(false)] out ResourceId result)
    {
        result = new ResourceId(original ?? new string(s));
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

    private sealed class JsonConverter : JsonConverter<ResourceId>
    {
        public override ResourceId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (!TryParse(str, null, out var result))
            {
                throw new JsonException("Invalid resource id");
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, ResourceId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value._value);
        }
    }
}