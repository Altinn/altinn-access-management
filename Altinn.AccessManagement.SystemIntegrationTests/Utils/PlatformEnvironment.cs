using System.Text.Json.Serialization;

namespace Altinn.AccessManagement.SystemIntegrationTests.Utils;

public class PlatformEnvironment
{
    public required string? platformUrl { get; set; }
    
    [JsonPropertyName("TestCredentials")]
    public required TestCredentials testCredentials { get; set; }
    public class TestCredentials
    {
        public required string username { get; set; }
        public required string password { get; set; }
    }
}