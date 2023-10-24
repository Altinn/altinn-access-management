using System.Web.Http.ModelBinding;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// Model for error messages from SBL UserDelegationCheck
    /// </summary>
    public class SBLUserDelegationCheckError
    {
        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the model state
        /// </summary>
        public ModelStateDictionary ModelState { get; set; }
    }
}
