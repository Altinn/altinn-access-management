using System.Text.Json;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;

namespace Altinn.AccessManagement.Tests.Models.Urn
{
    public class ResourceInstanceIdentifierTest
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// Scenario:
        /// Tests the ResourceInstanceIdentifier Json Deserializing
        /// Input:
        /// Parse a valid ResourceInstanceIdentifier
        /// Expected Result:
        /// Create a ResourceInstanceIdentifier with the value from the input
        /// </summary>
        [Fact]
        public void TestResourceInstanceIdentifierJson_DeSerializng_Success()
        {
            string resourceInstanceIdentifierString = @"""0191579e-72bc-7977-af5d-f9e92af4393b""";
            ResourceInstanceIdentifier resourceInstanceIdentifier = JsonSerializer.Deserialize<ResourceInstanceIdentifier>(resourceInstanceIdentifierString, JsonOptions);

            Assert.Equal("0191579e-72bc-7977-af5d-f9e92af4393b", resourceInstanceIdentifier.ToString());
        }

        /// <summary>
        /// Scenario:
        /// Tests the ResourceInstanceIdentifier Json Deserializing
        /// Input:
        /// Parse a valid ResourceInstanceIdentifier
        /// Expected Result:
        /// Create a ResourceInstanceIdentifier with the value from the input
        /// </summary>
        [Fact]
        public void TestResourceInstanceIdentifierJson_Serializng_Success()
        {
            ResourceInstanceIdentifier resourceInstanceIdentifier = ResourceInstanceIdentifier.Parse("0191579e-72bc-7977-af5d-f9e92af4393b");
            string orgNrJson = JsonSerializer.Serialize(resourceInstanceIdentifier, JsonOptions);

            Assert.Equal(@"""0191579e-72bc-7977-af5d-f9e92af4393b""", orgNrJson);
        }

        /// <summary>
        /// Scenario:
        /// Tests the ResourceInstanceIdentifier Parse String
        /// Input:
        /// Parse a valid ResourceInstanceIdentifier
        /// Expected Result:
        /// Create a ResourceInstanceIdentifier with the value from the input
        /// </summary>
        [Fact]
        public void TestResourceInstanceIdentifierString_Parse_Success()
        {
            ResourceInstanceIdentifier resourceInstanceIdentifier = ResourceInstanceIdentifier.Parse("0191579e-72bc-7977-af5d-f9e92af4393b");
            Assert.Equal("0191579e-72bc-7977-af5d-f9e92af4393b", resourceInstanceIdentifier.ToString());
        }

        /// <summary>
        /// Scenario:
        /// Tests the ResourceInstanceIdentifier Parse CharSpan
        /// Input:
        /// Parse a valid ResourceInstanceIdentifier
        /// Expected Result:
        /// Create a ResourceInstanceIdentifier with the value from the input
        /// </summary>
        [Fact]
        public void TestResourceInstanceIdentifierReadOnlyCharSpan_Parse_Success()
        {
            ReadOnlySpan<char> resourceInstanceIdentifierSpan = "0191579e-72bc-7977-af5d-f9e92af4393b";
            ResourceInstanceIdentifier resourceInstanceIdentifier = ResourceInstanceIdentifier.Parse(resourceInstanceIdentifierSpan);
            Assert.Equal("0191579e-72bc-7977-af5d-f9e92af4393b", resourceInstanceIdentifier.ToString());
        }

        /// <summary>
        /// Scenario:
        /// Tests the ResourceInstanceIdentifier GetExample
        /// Input:
        /// Nothing
        /// Expected Result:
        /// Returns ResourceInstanceIdentifier Examples
        /// </summary>
        [Fact]
        public void TestResourceInstanceIdentifier_GetExample_Success()
        {
            List<ResourceIdentifier> expected = new List<ResourceIdentifier>();
            expected.Add(ResourceIdentifier.Parse("0191579e-72bc-7977-af5d-f9e92af4393b"));
            expected.Add(ResourceIdentifier.Parse("ext_1337"));

            List<ResourceInstanceIdentifier> actual = ResourceInstanceIdentifier.GetExamples(new Swashbuckle.Examples.ExampleDataOptions()).ToList();

            Assert.Equal(expected.Count, actual.Count);

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i].ToString(), actual[i].ToString());
            }
        }

        /// <summary>
        /// Scenario:
        /// Tests the ResourceInstanceIdentifier TryFormat
        /// Input:
        /// TryFormat an ResourceInstanceIdentifier into a Span exactly the length og orgnr
        /// Expected Result:
        /// Formats the ResourceInstanceIdentifier into the expected span and returns true and sends the number of characters formated into result
        /// </summary>
        [Fact]
        public void TestResourceInstanceIdentifier_TryFormat_Success()
        {
            string expected = "0191579e-72bc-7977-af5d-f9e92af4393b";
            ResourceInstanceIdentifier resourceInstanceIdentifier = ResourceInstanceIdentifier.Parse(expected);
            Span<char> result = new Span<char>(new char[36]);
            bool ok = resourceInstanceIdentifier.TryFormat(result, out int charsWritten, null, null);
            Assert.Equal(expected, result.ToString().Trim());
            Assert.Equal(expected.Length, charsWritten);
            Assert.True(ok);
        }

        /// <summary>
        /// Scenario:
        /// Tests the ResourceInstanceIdentifier TryFormat
        /// Input:
        /// TryFormat an ResourceInstanceIdentifier into a Span to small
        /// Expected Result:
        /// Does nothing and returns false and outputs 0 as the number of characters formated
        /// </summary>
        [Fact]
        public void TestResourceInstanceIdentifier_TryFormat_Fail()
        {
            string input = "0191579e-72bc-7977-af5d-f9e92af4393b";
            ResourceInstanceIdentifier resourceInstanceIdentifier = ResourceInstanceIdentifier.Parse(input);
            Span<char> result = new Span<char>(new char[25]);
            Span<char> expected = new Span<char>(new char[25]);
            bool ok = resourceInstanceIdentifier.TryFormat(result, out int charsWritten, null, null);
            Assert.Equal(expected, result);
            Assert.Equal(0, charsWritten);
            Assert.False(ok);
        }
    }
}
