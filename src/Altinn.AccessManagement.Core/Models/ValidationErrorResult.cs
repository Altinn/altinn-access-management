namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// Response model for any errors occuring during processing of a model in the core service layer
    /// </summary>
    public class ValidationErrorResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the processing was a success i.e. no errors has been added
        /// </summary>
        public bool IsValid
        {
            get { return Errors.Count == 0; }
        }

        /// <summary>
        /// Gets or sets a dictionary of errors occured during processing
        /// </summary>
        public Dictionary<string, string> Errors { get; set; } = new Dictionary<string, string>();
    }
}
