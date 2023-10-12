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
        /// Gets or sets the set of Attribute Id and Attribute Value for identifying the resource the rights 
        /// </summary>
        public List<AttributeMatch> Resource { get; set; }

        /// <summary>
        /// Gets or sets the Attribute Id and Attribute Value for a specific action, to identify the action this right applies to
        /// </summary>
        public AttributeMatch Action { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the right was delegated or not
        /// </summary>
        public DelegationStatus Status { get; set; }

        /// <summary>
        /// Gets or sets a list of details describing why or why not the right is valid in the current user and reportee party context
        /// </summary>
        public List<Detail> Details { get; set; }
    }
}
