using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Models
{
    /// <summary>
    /// Response model for the result of a delegation of one or more rights to a recipient.
    /// </summary>
    public class DelegationOutputExternal
    {
        /// <summary>
        /// Gets or sets a set of Attribute Id and Attribute Value for the single entity receiving rights
        /// </summary>
        public List<AttributeMatchExternal> To { get; set; }

        /// <summary>
        /// Gets or sets a list of rights delegation results which is to be delegated to the To recipient.
        /// NOTE:
        /// If the right only specifies the top-level resource identifier or org/app without an action specification,
        /// delegation will find and delegate all the rights the delegating user have for the resource.
        /// </summary>
        public List<BaseRightExternal> RightDelegationResults { get; set; }
    }
}
