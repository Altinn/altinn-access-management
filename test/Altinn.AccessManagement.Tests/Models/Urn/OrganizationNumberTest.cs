using System.Text.Json;
using Altinn.AccessManagement.Core.Models.Register;

namespace Altinn.AccessManagement.Tests.Models.Urn
{
    [Collection("Models Test")]
    public class OrganizationNumberTest
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// Scenario:
        /// Tests the OrganizationNumber Json Deserializing
        /// Input:
        /// Parse a valid OrganizationNumber
        /// Expected Result:
        /// Create a OrganizationNumber with the value from the input
        /// </summary>
        [Fact]
        public void TestOrganizationNumberJson_DeSerializng_Success()
        {
            string orgNumberString = @"""123456789""";
            OrganizationNumber orgNr = JsonSerializer.Deserialize<OrganizationNumber>(orgNumberString, JsonOptions);

            Assert.Equal("123456789", orgNr.ToString());
        }

        /// <summary>
        /// Scenario:
        /// Tests the OrganizationNumber Json Deserializing 
        /// Input:
        /// Parse a invalid OrganizationNumber
        /// Expected Result:
        /// Throws an exception
        /// </summary>
        [Fact]
        public void TestOrganizationNumberJson_DeSerializng_Fail()
        {
            string orgNumberString = @"""123456f89""";
            try
            {
                OrganizationNumber orgNr = JsonSerializer.Deserialize<OrganizationNumber>(orgNumberString, JsonOptions);
            }
            catch
            {
                return;
            }

            Assert.Fail("Should fail and not reach here");
        }

        /// <summary>
        /// Scenario:
        /// Tests the OrganizationNumber Json Serializing 
        /// Input:
        /// Parse a valid OrganizationNumber
        /// Expected Result:
        /// Create a OrganizationNumber with the value from the input
        /// </summary>
        [Fact]
        public void TestOrganizationNumberJson_Serializng_Success()
        {
            OrganizationNumber orgNr = OrganizationNumber.Parse("123456789");
            string orgNrJson = JsonSerializer.Serialize(orgNr, JsonOptions);

            Assert.Equal(@"""123456789""", orgNrJson);
        }

        /// <summary>
        /// Scenario:
        /// Tests the OrganizationNumber Parse String
        /// Input:
        /// Parse a valid OrganizationNumber
        /// Expected Result:
        /// Create a OrganizationNumber with the value from the input
        /// </summary>
        [Fact]
        public void TestOrganizationNumberString_Parse_Success()
        {
            OrganizationNumber orgNr = OrganizationNumber.Parse("123456789");
            Assert.Equal("123456789", orgNr.ToString());
        }

        /// <summary>
        /// Scenario:
        /// Tests the OrganizationNumber
        /// Input:
        /// Parse a valid OrganizationNumber Parse CharSpan
        /// Expected Result:
        /// Create a Organization number with the value from the input
        /// </summary>
        [Fact]
        public void TestOrganizationNumberReadOnlyCharSpan_Parse_Success()
        {
            ReadOnlySpan<char> orgNrSpan = "123456789";
            OrganizationNumber orgNr = OrganizationNumber.Parse(orgNrSpan);
            Assert.Equal("123456789", orgNr.ToString());
        }

        /// <summary>
        /// Scenario:
        /// Tests the OrganizationNumber Parse CharSpan
        /// Input:
        /// Parse a invalid OrganizationNumber
        /// Expected Result:
        /// Fails with exception
        /// </summary>
        [Fact]
        public void TestOrganizationNumberReadOnlyCharSpan_Fail()
        {
            try
            {
                ReadOnlySpan<char> orgNrSpan = "1234567890";
                OrganizationNumber orgNr = OrganizationNumber.Parse(orgNrSpan);
            }
            catch
            {
                return;
            }

            Assert.Fail("Should fail and never reach this block");
        }

        /// <summary>
        /// Scenario:
        /// Tests the OrganizationNumber Parse String
        /// Input:
        /// Parse a invalid OrganizationNumber
        /// Expected Result:
        /// Fails with exception
        /// </summary>
        [Fact]
        public void TestOrganizationNumberString_Parse_Fail()
        {
            try
            {
                OrganizationNumber orgNr = OrganizationNumber.Parse("123456g89");
            }
            catch
            {
                return;
            }

            Assert.Fail("Should fail and never reach this block");
        }

        /// <summary>
        /// Scenario:
        /// Tests the OrganizationNumber GetExample
        /// Input:
        /// Get expected Example
        /// Expected Result:
        /// Gets the expected Example
        /// </summary>
        [Fact]
        public void TestOrganizationNumber_GetExample_Success()
        {
            List<OrganizationNumber> expected = new List<OrganizationNumber>();
            expected.Add(OrganizationNumber.Parse("987654321"));
            expected.Add(OrganizationNumber.Parse("123456789"));

            List<OrganizationNumber> actual = OrganizationNumber.GetExamples(new Swashbuckle.Examples.ExampleDataOptions()).ToList();

            Assert.Equal(expected.Count, actual.Count);

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i].ToString(), actual[i].ToString());
            }
        }

        /// <summary>
        /// Scenario:
        /// Tests the OrganizationNumber TryFormat
        /// Input:
        /// TryFormat an OrganizationNumber into a Span exactly the length og orgnr
        /// Expected Result:
        /// Formats the organizationNumber into the expected span and returns true and sends the number of characters formated into result
        /// </summary>
        [Fact]
        public void TestOrganizationNumber_TryFormat_Success()
        {
            string expected = "987654321";
            OrganizationNumber organizationNumber = OrganizationNumber.Parse(expected);
            Span<char> result = new Span<char>(new char[expected.Length]);
            bool ok = organizationNumber.TryFormat(result, out int charsWritten, null, null);
            Assert.Equal(expected, result.ToString().Trim());
            Assert.Equal(expected.Length, charsWritten);
            Assert.True(ok);
        }

        /// <summary>
        /// Scenario:
        /// Tests the OrganizationNumber TryFormat
        /// Input:
        /// TryFormat an OrganizationNumber into a Span to small
        /// Expected Result:
        /// Does nothing and returns false and outputs 0 as the number of characters formated
        /// </summary>
        [Fact]
        public void TestOrganizationNumber_TryFormat_Fail()
        {
            string input = "987654321";
            OrganizationNumber organizationNumber = OrganizationNumber.Parse(input);
            Span<char> result = new Span<char>(new char[6]);
            Span<char> expected = new Span<char>(new char[6]);
            bool ok = organizationNumber.TryFormat(result, out int charsWritten, null, null);
            Assert.Equal(expected, result);
            Assert.Equal(0, charsWritten);
            Assert.False(ok);
        }
    }
}
