using System.Text.Json;
using Altinn.AuthorizationAdmin.Models;

namespace Altinn.AuthorizationAdmin.Services
{
    public class FrontEndEntrypointService : IFrontEndEntrypoints
    {
        private enum EntryType
        {
            css,
            js
        }

        private static FrontEndManifest? manifest;
        private static void LoadManifest()
        {
            // This file is created when building the front end
            string frontendLocation = Environment.GetEnvironmentVariable("FRONTEND_PROD_FOLDER") ?? "wwwroot/AuthorizationAdmin/";
            string jsonData = File.ReadAllText(frontendLocation + "manifest.json");
            manifest = JsonSerializer.Deserialize<FrontEndManifest>(jsonData);
        }

        private static String GetManifestEntry(EntryType entry)
        {
            if (manifest == null)
            {
                LoadManifest();
            }

            if (entry == EntryType.css)
            {
                return manifest?.Entrypoint?.css?[0] ?? "";
            }

            return manifest?.Entrypoint?.file ?? "";
        }

        public String GetCSSEntrypoint()
        {
            return GetManifestEntry(EntryType.css);
        }

        public String GetJSEntrypoint()
        {
            return GetManifestEntry(EntryType.js);
        }
    }
}
