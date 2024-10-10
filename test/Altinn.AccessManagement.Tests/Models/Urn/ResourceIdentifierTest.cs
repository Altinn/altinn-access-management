using System.Text.Json;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;

namespace Altinn.AccessManagement.Tests.Models.Urn
{
    public class ResourceIdentifierTest
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// Scenario:
        /// Tests the ResourceIdentifier Json Deserializing
        /// Input:
        /// Parse a valid ResourceIdentifier
        /// Expected Result:
        /// Create a ResourceIdentifier with the value from the input
        /// </summary>
        [Fact]
        public void TestResourceIdentifierJson_DeSerializng_Success()
        {
            string resourceIdentifierString = @"""example-resourceid""";
            ResourceIdentifier resourceIdentifier = JsonSerializer.Deserialize<ResourceIdentifier>(resourceIdentifierString, JsonOptions);

            Assert.Equal("example-resourceid", resourceIdentifier.ToString());
        }

        /// <summary>
        /// Scenario:
        /// Tests the ResourceIdentifier Json Deserializing
        /// Input:
        /// Parse a invalid ResourceIdentifier
        /// Expected Result:
        /// Throws exception
        /// </summary>
        [Fact]
        public void TestResourceIdentifierJson_DeSerializng_Fail()
        {
            string resourceIdentifierString = @"""exa""";
            try
            {
                ResourceIdentifier resourceIdentifier = JsonSerializer.Deserialize<ResourceIdentifier>(resourceIdentifierString, JsonOptions);
            }
            catch
            {
                return;
            }

            Assert.Fail("Should fail and not reach here");
        }

        /// <summary>
        /// Scenario:
        /// Tests the ResourceIdentifier Json Serializing
        /// Input:
        /// Parse a valid ResourceIdentifier
        /// Expected Result:
        /// Create a ResourceIdentifier with the value from the input
        /// </summary>
        [Fact]
        public void TestResourceIdentifierJson_Serializng_Success()
        {
            ResourceIdentifier resourceIdentifier = ResourceIdentifier.Parse("example-resourceid");
            string orgNrJson = JsonSerializer.Serialize(resourceIdentifier, JsonOptions);

            Assert.Equal(@"""example-resourceid""", orgNrJson);
        }

        /// <summary>
        /// Scenario:
        /// Tests the ResourceIdentifier Parse String
        /// Input:
        /// Parse a valid ResourceIdentifier
        /// Expected Result:
        /// Create a ResourceIdentifier with the value from the input
        /// </summary>
        [Fact]
        public void TestResourceIdentifierString_Parse_Success()
        {
            ResourceIdentifier resourceIdentifier = ResourceIdentifier.Parse("example-resourceid");
            Assert.Equal("example-resourceid", resourceIdentifier.ToString());
        }

        /// <summary>
        /// Scenario:
        /// Tests the ResourceIdentifier Parse CharSpan
        /// Input:
        /// Parse a valid ResourceIdentifier
        /// Expected Result:
        /// Create a ResourceIdentifier with the value from the input
        /// </summary>
        [Fact]
        public void TestResourceIdentifierReadOnlyCharSpan_Parse_Success()
        {
            ReadOnlySpan<char> resourceIdentifierSpan = "example-resourceid";
            ResourceIdentifier resourceIdentifier = ResourceIdentifier.Parse(resourceIdentifierSpan);
            Assert.Equal("example-resourceid", resourceIdentifier.ToString());
        }

        /// <summary>
        /// Scenario:
        /// Tests the ResourceIdentifier Parse CharSpan
        /// Input:
        /// Parse a valid ResourceIdentifier
        /// Expected Result:
        /// Create a ResourceIdentifier with the value from the input
        /// </summary>
        [Fact]
        public void TestResourceIdentifierReadOnlyCharSpan_Fail()
        {
            try
            {
                ReadOnlySpan<char> resourceIdentifierSpan = "exa";
                ResourceIdentifier resourceIdentifier = ResourceIdentifier.Parse(resourceIdentifierSpan);
            }
            catch
            {
                return;
            }

            Assert.Fail("Should fail and never reach this block");
        }

        /// <summary>
        /// Scenario:
        /// Tests the ResourceIdentifier Parse String
        /// Input:
        /// Parse a valid ResourceIdentifier
        /// Expected Result:
        /// Throws an exception
        /// </summary>
        [Fact]
        public void TestResourceIdentifierString_Parse_Fail()
        {
            try
            {
                ResourceIdentifier resourceIdentifier = ResourceIdentifier.Parse("exa");
            }
            catch
            {
                return;
            }

            Assert.Fail("Should fail and never reach this block");
        }

        /// <summary>
        /// Scenario:
        /// Tests the ResourceIdentifier
        /// Input:
        /// Parse a valid ResourceIdentifier
        /// Expected Result:
        /// Create a ResourceIdentifier with the value from the input
        /// </summary>
        [Fact]
        public void TestResourceIdentifier_GetExample_Success()
        {
            List<ResourceIdentifier> expected = new List<ResourceIdentifier>();
            expected.Add(ResourceIdentifier.Parse("example-resourceid"));
            expected.Add(ResourceIdentifier.Parse("app_skd_flyttemelding"));

            List<ResourceIdentifier> actual = ResourceIdentifier.GetExamples(new Swashbuckle.Examples.ExampleDataOptions()).ToList();

            Assert.Equal(expected.Count, actual.Count);

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i].ToString(), actual[i].ToString());
            }
        }

        /// <summary>
        /// Scenario:
        /// Tests the ResourceIdentifier TryFormat
        /// Input:
        /// TryFormat an ResourceIdentifier into a Span exactly the length og orgnr
        /// Expected Result:
        /// Formats the ResourceIdentifier into the expected span and returns true and sends the number of characters formated into result
        /// </summary>
        [Fact]
        public void TestResourceIdentifier_TryFormat_Success()
        {
            string expected = "example-resourceid";
            ResourceIdentifier resourceInstanceIdentifier = ResourceIdentifier.Parse(expected);
            Span<char> result = new Span<char>(new char[expected.Length]);
            bool ok = resourceInstanceIdentifier.TryFormat(result, out int charsWritten, null, null);
            Assert.Equal(expected, result.ToString().Trim());
            Assert.Equal(expected.Length, charsWritten);
            Assert.True(ok);
        }

        /// <summary>
        /// Scenario:
        /// Tests the ResourceIdentifier TryFormat
        /// Input:
        /// TryFormat an ResourceIdentifier into a Span to small
        /// Expected Result:
        /// Does nothing and returns false and outputs 0 as the number of characters formated
        /// </summary>
        [Fact]
        public void TestResourceInstanceIdentifier_TryFormat_Fail()
        {
            string input = "example-resourceid";
            ResourceIdentifier resourceInstanceIdentifier = ResourceIdentifier.Parse(input);
            Span<char> result = new Span<char>(new char[6]);
            Span<char> expected = new Span<char>(new char[6]);
            bool ok = resourceInstanceIdentifier.TryFormat(result, out int charsWritten, null, null);
            Assert.Equal(expected, result);
            Assert.Equal(0, charsWritten);
            Assert.False(ok);
        }
    }
}
