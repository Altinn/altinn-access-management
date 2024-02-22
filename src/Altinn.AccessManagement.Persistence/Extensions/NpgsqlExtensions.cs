#nullable enable

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Npgsql;
using NpgsqlTypes;

namespace Altinn.AccessManagement.Persistence.Extensions;

/// <summary>
/// Helper extensions for Npgsql.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class NpgsqlExtensions
{
    /// <summary>
    /// Create a <see cref="NpgsqlCommand"/> with the command text set.
    /// </summary>
    /// <param name="conn">The <see cref="NpgsqlConnection"/>.</param>
    /// <param name="sql">The command text as a string.</param>
    /// <returns>A <see cref="NpgsqlCommand"/>.</returns>
    public static NpgsqlCommand CreateCommand(this NpgsqlConnection conn, string sql)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        return cmd;
    }

    /// <summary>
    /// Executes a command against the database, returning a <see cref="IAsyncEnumerable{T}"/>
    /// that can be easily mapped over.
    /// </summary>
    /// <param name="cmd">The <see cref="NpgsqlCommand"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns></returns>
    public static async IAsyncEnumerable<NpgsqlDataReader> ExecuteEnumerableAsync(
        this NpgsqlCommand cmd,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return reader;
        }
    }

    /// <summary>
    /// Adds a <see cref="NpgsqlParameter"/> to the <see cref="NpgsqlParameterCollection"/> given the parameter name and the data type
    /// and value.
    /// </summary>
    /// <param name="parameters">The <see cref="NpgsqlParameterCollection"/> to add the parameter to.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <param name="parameterType">One of the <see cref="NpgsqlDbType"/> values.</param>
    /// <param name="value">The parameter value. If this is <see langword="null"/>, <see cref="DBNull.Value"/> is used instead.</param>
    /// <returns>The index of the new <see cref="NpgsqlParameter"/> object.</returns>
    public static NpgsqlParameter AddWithNullableValue(
        this NpgsqlParameterCollection parameters,
        string parameterName,
        NpgsqlDbType parameterType,
        object? value)
        => parameters.Add(parameterName, parameterType).SetNullableValue(value);

    /// <summary>
    /// Sets the value of the <see cref="NpgsqlParameter"/> to the given value.
    /// </summary>
    /// <param name="parameter">The <see cref="NpgsqlParameter"/></param>
    /// <param name="value">The new value</param>
    /// <returns><paramref name="parameter"/></returns>
    public static NpgsqlParameter SetValue(
        this NpgsqlParameter parameter,
        object value)
    {
        parameter.Value = value;

        return parameter;
    }

    /// <summary>
    /// Sets the value of the <see cref="NpgsqlParameter"/> to the given value, converting <see langword="null"/>
    /// to <see cref="DBNull.Value"/>.
    /// </summary>
    /// <param name="parameter">The <see cref="NpgsqlParameter"/></param>
    /// <param name="value">The new value</param>
    /// <returns><paramref name="parameter"/></returns>
    public static NpgsqlParameter SetNullableValue(
        this NpgsqlParameter parameter,
        object? value)
    {
        if (value is not null)
        {
            parameter.Value = value;
        }
        else
        {
            parameter.Value = DBNull.Value;
        }

        return parameter;
    }

    /// <summary>
    /// Sets the value of the <see cref="NpgsqlParameter"/> to the given value, converting <see langword="default"/>
    /// to <see cref="DBNull.Value"/>.
    /// </summary>
    /// <typeparam name="T">The array item type</typeparam>
    /// <param name="parameter">The <see cref="NpgsqlParameter"/></param>
    /// <param name="value">The new value</param>
    /// <returns><paramref name="parameter"/></returns>
    public static NpgsqlParameter SetOptionalImmutableArrayValue<T>(
        this NpgsqlParameter<IList<T>> parameter,
        ImmutableArray<T> value)
    {
        if (!value.IsDefault)
        {
            parameter.TypedValue = value;
        }
        else
        {
            parameter.TypedValue = null;
        }

        return parameter;
    }

    /// <summary>
    /// Gets the value of the specified column as an instance of <see langword="string"/> or <see langword="null"/>
    /// if the value is <see cref="DBNull.Value"/>.
    /// </summary>
    /// <param name="reader">The <see cref="NpgsqlDataReader"/></param>
    /// <param name="ordinal">The column ordinal</param>
    /// <returns></returns>
    public static string? GetStringOrNull(
        this NpgsqlDataReader reader,
        int ordinal)
        => reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);

    /// <summary>
    /// Gets the value of the specified column as an instance of <see langword="string"/> or <see langword="null"/>
    /// if the value is <see cref="DBNull.Value"/>.
    /// </summary>
    /// <param name="reader">The <see cref="NpgsqlDataReader"/></param>
    /// <param name="name">The column name</param>
    /// <returns>The value of the specified column.</returns>
    public static string? GetStringOrNull(
        this NpgsqlDataReader reader,
        string name)
        => reader.GetStringOrNull(reader.GetOrdinal(name));

    /// <summary>
    /// Adds a <see cref="NpgsqlParameter"/> to the <see cref="NpgsqlParameterCollection"/> given the specified parameter name and
    /// data type.
    /// </summary>
    /// <param name="parameters">The <see cref="NpgsqlParameterCollection"/> to add the parameter to.</param>
    /// <param name="parameterName">The name of the <see cref="NpgsqlParameter"/>.</param>
    /// <param name="parameterType">One of the NpgsqlDbType values.</param>
    /// <returns>The parameter that was added.</returns>
    public static NpgsqlParameter<T> Add<T>(
        this NpgsqlParameterCollection parameters,
        string parameterName,
        NpgsqlDbType parameterType)
    {
        var parameter = new NpgsqlParameter<T>(parameterName, parameterType);
        parameters.Add(parameter);
        return parameter;
    }

    /// <summary>
    /// Reads a column as a immutable array of values.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="reader">The reader.</param>
    /// <param name="ordinal">The column index.</param>
    /// <param name="cancellationToken">The async cancellation token.</param>
    /// <returns>The column data</returns>
    public static async ValueTask<ImmutableArray<T>> GetFieldValueArrayAsync<T>(
        this NpgsqlDataReader reader,
        int ordinal,
        CancellationToken cancellationToken)
    {
        if (await reader.IsDBNullAsync(ordinal, cancellationToken))
        {
            return default;
        }

        var list = await reader.GetFieldValueAsync<IList<T>>(ordinal, cancellationToken);
        return list.ToImmutableArray();
    }

    /// <summary>
    /// Reads a column as a immutable array of values.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="reader">The reader.</param>
    /// <param name="name">The column name.</param>
    /// <param name="cancellationToken">The async cancellation token.</param>
    /// <returns>The column data</returns>
    public static async ValueTask<ImmutableArray<T>> GetFieldValueArrayAsync<T>(
        this NpgsqlDataReader reader,
        string name,
        CancellationToken cancellationToken)
        => await reader.GetFieldValueArrayAsync<T>(reader.GetOrdinal(name), cancellationToken);
}
