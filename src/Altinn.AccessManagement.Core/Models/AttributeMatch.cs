using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// This model describes a pair of AttributeId and AttributeValue for use in matching in XACML policies, for instance a resource, a user, a party or an action.
    /// </summary>
    public class AttributeMatch : IEquatable<AttributeMatch>, IEqualityComparer<AttributeMatch>
    {
        /// <summary>
        /// summary
        /// </summary>
        public AttributeMatch()
        {
        }

        /// <summary>
        /// Attribute
        /// </summary>
        /// <param name="id">a</param>
        /// <param name="value">b</param>
        public AttributeMatch(string id, object value)
        {
            Id = id;
            Value = value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Gets or sets the attribute id for the match
        /// </summary>
        [Required]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the attribute value for the match
        /// </summary>
        [Required]
        public string Value { get; set; }

        /// <inheritdoc/>
        public bool Equals(AttributeMatch other) => Id.Equals(other.Id, StringComparison.InvariantCultureIgnoreCase) && Value == other.Value;

        /// <inheritdoc/>
        public bool Equals(AttributeMatch x, AttributeMatch y) =>
            x.Id.Equals(y.Id, StringComparison.InvariantCultureIgnoreCase) && x.Value.Equals(y.Value);

        /// <inheritdoc/>
        public int GetHashCode([DisallowNull] AttributeMatch obj) =>
            obj.ToString().GetHashCode();

        /// <summary>
        /// String representation of the attribute
        /// </summary>
        /// <returns></returns>
        public override string ToString() =>
            $"{Id.ToLowerInvariant()}:{Value}";
    }
}
