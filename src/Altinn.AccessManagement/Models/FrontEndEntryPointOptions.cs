namespace Altinn.AccessManagement.Models
{
    /// <summary>
    /// Configuration of front end entry points
    /// </summary>
    public class FrontEndEntryPointOptions
    {
        /// <summary>
        /// SectionName
        /// </summary>
        public const string SectionName = "entrypoint.js";

        /// <summary>
        /// File reference
        /// </summary>
        public string? File { get; set; }

        /// <summary>
        /// Source
        /// </summary>
        public string? Src { get; set; }

        /// <summary>
        /// IsEntry
        /// </summary>
        public bool? IsEntry { get; set; }

        /// <summary>
        /// Css
        /// </summary>
        public List<string>? Css { get; set; }

        /// <summary>
        /// Assets
        /// </summary>
        public List<string>? Assets { get; set; }
    }
}
