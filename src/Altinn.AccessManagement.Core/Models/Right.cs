﻿namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// This model describes a single right
    /// </summary>
    public class Right
    {
        /// <summary>
        /// Gets or sets the right key
        /// </summary>
        public string RightKey { get; set; }

        /// <summary>
        /// Gets or sets the list of resource matches which uniquely identifies the resource this right applies to.
        /// </summary>
        public List<AttributeMatch> Resource { get; set; }

        /// <summary>
        /// Gets or sets the set of Attribute Id and Attribute Value for a specific action, to identify the action this right applies to
        /// </summary>
        public AttributeMatch Action { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user or party has the right
        /// </summary>
        public bool HasPermit { get; set; }

        /// <summary>
        /// Gets or sets the set of identified sources providing the right
        /// </summary>
        public List<RightSource> RightSources { get; set; }
    }
}
