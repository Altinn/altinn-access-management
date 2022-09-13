namespace Altinn.AuthorizationAdmin.Models
{
    public class FrontEndEntryPointOptions
    {
        public const string SectionName = "entrypoint.js";

        public string? File { get; set; }
        public string? Src { get; set; }
        public bool? IsEntry { get; set; }
        public List<string>? Css { get; set; }
        public List<string>? Assets { get; set; }
    }

}
