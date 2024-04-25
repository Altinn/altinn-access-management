using Microsoft.OpenApi.Models;

namespace Altinn.AccessManagement;

public static class OpenAPIDefinitions
{
    public static class All
    {
        public static readonly string Name = nameof(All);

        public static OpenApiInfo V1 = new()
        {
            Title = "All Access Management Api Endpoints (for APIM deploy)",
            Version = "v1"
        };
    }

    public static class Internal
    {
        public static string Name = nameof(Internal);

        public static OpenApiInfo V1 = new()
        {
            Title = "Access Management Internal Platform Api",
            Version = "v1"
        };
    }

    public static class ResourceOwner
    {
        public static string Name = nameof(ResourceOwner);

        public static OpenApiInfo V1 = new()
        {
            Title = "Access Management Resource Owner Api",
            Version = "v1"
        };
    }

    public static class EnduserSystem
    {
        public static string Name = nameof(EnduserSystem);

        public static OpenApiInfo V1 = new()
        {
            Title = "Access Management Enduser System Api",
            Version = "v1"
        };
    }
}