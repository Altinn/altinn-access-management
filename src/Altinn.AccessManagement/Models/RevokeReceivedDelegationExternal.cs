using System.ComponentModel.DataAnnotations;

namespace Altinn.AccessManagement.Models
{
    /// <summary>
    /// Model for revoking a delegation of one or more rights a reportee has received from another party.
    /// </summary>
    public class RevokeReceivedDelegationExternal
    {
        /// <summary>
        /// Gets or sets a set of Attribute Id and Attribute Value identifying the party the delegated rights to be revoked, have been received from.
        /// </summary>
        [Required]
        public List<AttributeMatchExternal> From { get; set; }

        /// <summary>
        /// Gets or sets a list of rights identifying what is to be revoked
        /// NOTE:
        /// If the right only specifies the top-level resource identifier or org/app without an action specification,
        /// the operation will find and revoke all the rights received from the delegating party.
        /// </summary>
        [Required]
        public List<BaseRightExternal> Rights { get; set; }
    }
}
