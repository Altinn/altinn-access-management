// <copyright file="DelegationChangeInput.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Models
{
    /// <summary>
    /// Contains id info about user, reportee, resource and resourceMatchType that's being used to check all delegation changes
    /// </summary>
    public class DelegationChangeInput
    {
        /// <summary>
        /// Id and value of the subject getting delegation changes info
        /// </summary>
        [Required]
        public AttributeMatch Subject { get; set; }

        /// <summary>
        /// Id and value of party
        /// </summary>
        [Required]
        public AttributeMatch Party { get; set; }

        /// <summary>
        /// Gets the Resource's id
        /// </summary>
        [Required]
        public List<AttributeMatch> Resource { get; set; }
    }
}
