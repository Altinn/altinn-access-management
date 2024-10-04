using Altinn.AccessManagement.Core.Models.Register;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Altinn.AccessManagement.Tests.Models.Urn
{
    public class OrganizationNumberTest
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// Scenario:
        /// Tests the OrganizationNumber
        /// Input:
        /// Parse a valid OrganizationNumber
        /// Expected Result:
        /// Create a Organization number with the value from the input
        /// </summary>
        [Fact]
        public void TestOrganizationNumber_DeSerializng_Success()
        {
            string orgNumberString = @"""123456789""";
            OrganizationNumber orgNr = JsonSerializer.Deserialize<OrganizationNumber>(orgNumberString, JsonOptions);

            Assert.Equal("123456789", orgNr.ToString());
        }

        /// <summary>
        /// Scenario:
        /// Tests the OrganizationNumber
        /// Input:
        /// Parse a valid OrganizationNumber
        /// Expected Result:
        /// Create a Organization number with the value from the input
        /// </summary>
        [Fact]
        public void TestOrganizationNumber_DeSerializng_Fail()
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
        /// Tests the OrganizationNumber
        /// Input:
        /// Parse a valid OrganizationNumber
        /// Expected Result:
        /// Create a Organization number with the value from the input
        /// </summary>
        [Fact]
        public void TestOrganizationNumber_Serializng_Success()
        {
            OrganizationNumber orgNr = OrganizationNumber.Parse("123456789");
            string orgNrJson = JsonSerializer.Serialize(orgNr, JsonOptions);

            Assert.Equal(@"""123456789""", orgNrJson);
        }

        /// <summary>
        /// Scenario:
        /// Tests the OrganizationNumber
        /// Input:
        /// Parse a valid OrganizationNumber
        /// Expected Result:
        /// Create a Organization number with the value from the input
        /// </summary>
        [Fact]
        public void TestOrganizationNumber_Parse_Success()
        {
            OrganizationNumber orgNr = OrganizationNumber.Parse("123456789");
            Assert.Equal("123456789", orgNr.ToString());
        }

        /// <summary>
        /// Scenario:
        /// Tests the OrganizationNumber
        /// Input:
        /// Parse a valid OrganizationNumber
        /// Expected Result:
        /// Create a Organization number with the value from the input
        /// </summary>
        [Fact]
        public void TestOrganizationNumber_Fail()
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
    }
}
