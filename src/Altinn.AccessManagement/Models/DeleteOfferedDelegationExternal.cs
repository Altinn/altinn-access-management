using System.ComponentModel.DataAnnotations;

namespace Altinn.AccessManagement.Models
{
    /// <summary>
    /// Model for deleting a delegation of one or more rights a reportee has offered to another party.
    /// </summary>
    public class DeleteOfferedDelegationExternal
    {
        /// <summary>
        /// Gets or sets a set of Attribute Id and Attribute Value identifying the party to delete rights offered to
        /// </summary>
        [Required]
        public List<AttributeMatchExternal> To { get; set; }

        /// <summary>
        /// Gets or sets a list of rights identifying what is to be deleted
        /// NOTE:
        /// If the right only specifies the top-level resource identifier or org/app without an action specification,
        /// delete will find and delete all the rights the recipient party have received.
        /// </summary>
        [Required]
        public List<BaseRightExternal> Rights { get; set; }
    }
}
