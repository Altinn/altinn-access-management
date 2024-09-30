using System.Text.Json;
using Altinn.Swashbuckle.Examples;
using Altinn.Swashbuckle.Filters;
using Altinn.Urn.Json;

namespace Altinn.AccessManagement.Models;

/// <summary>
/// This model describes a single right
/// </summary>
[SwaggerExampleFromExampleProvider]
public class SimpleRightV2 : IExampleDataProvider<SimpleRightV2>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Gets or sets the list of resource matches which uniquely identifies the resource this right applies to.
    /// </summary>
    public IEnumerable<UrnJsonTypeValue> Resource { get; set; }

    /// <summary>
    /// Gets or sets the set of Attribute Id and Attribute Value for a specific action, to identify the action this right applies to
    /// </summary>
    public string Action { get; set; }

    /// <summary>
    /// Example data provider for RightV2
    /// </summary>
    /// <param name="options">Options</param>
    /// <returns></returns>
    public static IEnumerable<SimpleRightV2> GetExamples(ExampleDataOptions options)
    {
        var json = """
            {
            "resource": [
                {
                  "type": "urn:altinn:resource:task",
                  "value": "task_1"
                }
              ],
              "action": "read"
            }
            """;
        yield return JsonSerializer.Deserialize<SimpleRightV2>(json, SerializerOptions);
    }
}
