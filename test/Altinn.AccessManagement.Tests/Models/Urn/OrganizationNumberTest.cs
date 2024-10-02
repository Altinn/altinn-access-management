using Altinn.AccessManagement.Core.Models.Register;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.AccessManagement.Tests.Models.Urn
{
    public class OrganizationNumberTest
    {
        /// <summary>
        /// Scenario:
        /// Tests the OrganizationNumber
        /// Input:
        /// Parse a valid OrganizationNumber
        /// Expected Result:
        /// Create a Organization number with the value from the input
        /// </summary>
        [Fact]
        public void TestOrganizationNumber_Success()
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
