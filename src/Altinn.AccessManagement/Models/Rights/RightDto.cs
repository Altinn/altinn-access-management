using System.Text.Json;
using Altinn.AccessManagement.Core.Models.Rights;
using Altinn.Swashbuckle.Examples;
using Altinn.Swashbuckle.Filters;
using Altinn.Urn.Json;

namespace Altinn.AccessManagement.Models;

/// <summary>
/// This model describes a single right
/// </summary>
[SwaggerExampleFromExampleProvider]
public class RightDto : IExampleDataProvider<RightDto>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Gets or sets the list of resource matches which uniquely identifies the resource this right applies to.
    /// </summary>
    public IEnumerable<UrnJsonTypeValue> Resource { get; set; }

    /// <summary>
    /// Gets or sets the set of Attribute Id and Attribute Value for a specific action, to identify the action this right applies to
    /// </summary>
    public UrnJsonTypeValue<ActionUrn> Action { get; set; }

    /// <summary>
    /// Example data provider for RightV2
    /// </summary>
    /// <param name="options">Options</param>
    /// <returns></returns>
    public static IEnumerable<RightDto> GetExamples(ExampleDataOptions options)
    {
        var json = """
            {
            "resource": [
                {
                  "type": "urn:altinn:resource",
                  "value": "app_ttd_apps-test"
                },
                {
                  "type": "urn:altinn:resource:instance-id",
                  "value": "0191579e-72bc-7977-af5d-f9e92af4393b"
                },
                {
                  "type": "urn:altinn:resource:task",
                  "value": "task_1"
                }
              ],
              "action":
              {
                "type": "urn:oasis:names:tc:xacml:1.0:action:action-id",
                "value": "read"
              }
            }
            """;
        yield return JsonSerializer.Deserialize<RightDto>(json, SerializerOptions);
    }
}
