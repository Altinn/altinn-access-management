using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Enums;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// Response model describing the delegation result for a given single right, whether the authenticated user was able to delegate the right or not on behalf of the from part.
    /// </summary>
    public class RightDelegationResult
    {
        /// <summary>
        /// Gets or sets the right key
        /// </summary>
        public string RightKey
        {
            get
            {
                return string.Join(",", Resource.OrderBy(m => m.Id).Select(m => m.Value)) + ":" + Action.Value;
            }
        }

        /// <summary>
        /// Gets or sets the set of Attribute Id and Attribute Value for identifying the resource which the right provides access to
        /// </summary>
        public List<AttributeMatch> Resource { get; set; }

        /// <summary>
        /// Gets or sets the Attribute Id and Attribute Value identifying action the right gives access to perform on the resource
        /// </summary>
        public AttributeMatch Action { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the right was successfully delegated or not
        /// </summary>
        public DelegationStatus Status { get; set; }

        /// <summary>
        /// Gets or sets a list of details describing the reason(s) behind the status (if any can be provided)
        /// </summary>
        public List<Detail> Details { get; set; }
    }
}
