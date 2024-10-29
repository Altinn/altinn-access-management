using System.Text.Json;
using Altinn.AccessManagement.Core.Models.Rights;

namespace Altinn.AccessManagement.Tests.Models.Urn
{
    public class ActionIdentifierTest
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// Scenario:
        /// Tests the ActionIdentifier Json deserializing
        /// Input:
        /// Parse a valid ActionIdentifier
        /// Expected Result:
        /// Create a ActionIdentifier with the value from the input
        /// </summary>
        [Fact]
        public void TestActionIdentifierJson_DeSerializng_Success()
        {
            string actionIdentifierString = @"""read""";
            ActionIdentifier actionIdentifier = JsonSerializer.Deserialize<ActionIdentifier>(actionIdentifierString, JsonOptions);

            Assert.Equal("read", actionIdentifier.ToString());
        }

        /// <summary>
        /// Scenario:
        /// Tests the ActionIdentifier Json serializing
        /// Input:
        /// Parse a valid ActionIdentifier
        /// Expected Result:
        /// Create a ActionIdentifier with the value from the input
        /// </summary>
        [Fact]
        public void TestActionIdentifierJson_Serializng_Success()
        {
            ActionIdentifier actionIdentifier = ActionIdentifier.Parse("read");
            string actionIdentifierJson = JsonSerializer.Serialize(actionIdentifier, JsonOptions);

            Assert.Equal(@"""read""", actionIdentifierJson);
        }

        /// <summary>
        /// Scenario:
        /// Tests the ActionIdentifier Parse string
        /// Input:
        /// Parse a valid ActionIdentifier
        /// Expected Result:
        /// Create a Organization number with the value from the input
        /// </summary>
        [Fact]
        public void TestActionIdentifierString_Parse_Success()
        {
            ActionIdentifier actionIdentifier = ActionIdentifier.Parse("read");
            Assert.Equal("read", actionIdentifier.ToString());
        }

        /// <summary>
        /// Scenario:
        /// Tests the ActionIdentifier Parse ReadOnlyCharSpan
        /// Input:
        /// Parse a valid ActionIdentifier
        /// Expected Result:
        /// Create a ActionIdentifier with the value from the input
        /// </summary>
        [Fact]
        public void TestActionIdentifierReadOnlyCharSpan_Parse_Success()
        {
            ReadOnlySpan<char> actionIdentifierSpan = "read";
            ActionIdentifier actionIdentifier = ActionIdentifier.Parse(actionIdentifierSpan);
            Assert.Equal("read", actionIdentifier.ToString());
        }

        /// <summary>
        /// Scenario:
        /// Tests the ActionIdentifier GetExample
        /// Input:
        /// Get expected Example
        /// Expected Result:
        /// Create a ActionIdentifier with the value from the input
        /// </summary>
        [Fact]
        public void TestActionIdentifier_GetExample_Success()
        {
            List<ActionIdentifier> expected = new List<ActionIdentifier>();
            expected.Add(ActionIdentifier.Parse("read"));
            expected.Add(ActionIdentifier.Parse("write"));

            List<ActionIdentifier> actual = ActionIdentifier.GetExamples(new Swashbuckle.Examples.ExampleDataOptions()).ToList();

            Assert.Equal(expected.Count, actual.Count);

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i].ToString(), actual[i].ToString());
            }
        }

        /// <summary>
        /// Scenario:
        /// Tests the ActionIdentifier TryFormat
        /// Input:
        /// TryFormat an ActionIdentifier into a Span exactly the length og orgnr
        /// Expected Result:
        /// Formats the ActionIdentifier into the expected span and returns true and sends the number of characters formated into result
        /// </summary>
        [Fact]
        public void TestActionIdentifier_TryFormat_Success()
        {
            string expected = "read";
            ActionIdentifier actionIdentifier = ActionIdentifier.Parse(expected);
            Span<char> result = new Span<char>(new char[expected.Length]);
            bool ok = actionIdentifier.TryFormat(result, out int charsWritten, null, null);
            Assert.Equal(expected, result.ToString().Trim());
            Assert.Equal(expected.Length, charsWritten);
            Assert.True(ok);
        }

        /// <summary>
        /// Scenario:
        /// Tests the ActionIdentifier TryFormat
        /// Input:
        /// TryFormat an ActionIdentifier into a Span to small
        /// Expected Result:
        /// Does nothing and returns false and outputs 0 as the number of characters formated
        /// </summary>
        [Fact]
        public void TestActionIdentifier_TryFormat_Fail()
        {
            string input = "read";
            ActionIdentifier actionIdentifier = ActionIdentifier.Parse(input);
            Span<char> result = new Span<char>(new char[3]);
            Span<char> expected = new Span<char>(new char[3]);
            bool ok = actionIdentifier.TryFormat(result, out int charsWritten, null, null);
            Assert.Equal(expected, result);
            Assert.Equal(0, charsWritten);
            Assert.False(ok);
        }
    }
}
