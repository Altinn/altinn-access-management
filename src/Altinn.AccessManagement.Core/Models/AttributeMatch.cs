using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Altinn.Urn;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// This model describes a pair of AttributeId and AttributeValue for use in matching in XACML policies, for instance a resource, a user, a party or an action.
    /// </summary>
    public class AttributeMatch : IEqualityComparer<AttributeMatch>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public AttributeMatch()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="id">type</param>
        /// <param name="value">value</param>
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
        public bool Equals(AttributeMatch x, AttributeMatch y) => x.Equals(y);

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as AttributeMatch);

        private bool Equals(AttributeMatch other) => Id.Equals(other?.Id, StringComparison.InvariantCultureIgnoreCase) && Value == other?.Value;

        /// <inheritdoc/>
        public int GetHashCode([DisallowNull] AttributeMatch obj) => obj.GetHashCode();

        /// <inheritdoc/>
        public override int GetHashCode() => (Id, Value).GetHashCode();

        /// <summary>
        /// String representation of the attribute
        /// </summary>
        /// <returns></returns>
        public override string ToString() =>
            $"{Id.ToLowerInvariant()}:{Value}";

        /// <summary>
        /// Creates a KeyValueUrn from the attribute match
        /// </summary>
        /// <returns>KeyValueUrn</returns>
        public KeyValueUrn ToKeyValueUrn() =>
            KeyValueUrn.CreateUnchecked($"{Id.ToLowerInvariant()}:{Value}", Id.Length + 1);
    }
}
