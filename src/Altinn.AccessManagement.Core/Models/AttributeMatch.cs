﻿using System.ComponentModel.DataAnnotations;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// This model describes a pair of AttributeId and AttributeValue for use in matching in XACML policies, for instance a resource, a user, a party or an action.
    /// </summary>
    public class AttributeMatch
    {
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

        /// <summary>
        /// String representation of the attribute
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Id}:{Value}";
        }
    }
}
