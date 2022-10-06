namespace Altinn.AuthorizationAdmin.Core.Models.ResourceRegistry
{
    /// <summary>
    /// Model representation of Competent Authority part of the ServiceResource model
    /// </summary>
    public class CompetentAuthority
    {
        /// <summary>
        /// The organization number
        /// </summary>
        public string Organization { get; set; }

        /// <summary>
        /// The organization code
        /// </summary>
        public string Orgcode { get; set; }
    }
}
