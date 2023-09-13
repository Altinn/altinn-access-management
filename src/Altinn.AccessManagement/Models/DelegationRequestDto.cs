// <copyright file="DelegationRequestDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Models
{
    /// <summary>
    ///     This model describes a single right
    /// </summary>
    public class DelegationRequestDto
    {
        /// <summary>
        ///     Gets or sets the list of resource matches which uniquely identifies the resource this right applies to.
        /// </summary>
        [Required]
        public List<IdValuePair> Resource { get; set; }

        /// <summary>
        ///     Gets or sets the set of Attribute Id and Attribute Value for a specific action, to identify the action this right
        ///     applies to
        /// </summary>
        public string? Action { get; set; }
    }
}
