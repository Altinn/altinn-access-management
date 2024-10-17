using System.Text.Json;

namespace Altinn.AccessManagement.SystemIntegrationTests.Utils;

public class ConfigurationLoader
{
    public static PlatformEnvironment? LoadEnvironment(string filePath)
    {
        var environmentFile = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<PlatformEnvironment>(environmentFile);
    }
}