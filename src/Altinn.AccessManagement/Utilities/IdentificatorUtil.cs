using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.AccessManagement.Utilities
{
    /// <summary>
    /// Utility class for diverse utility operations on identifier
    /// </summary>
    public class IdentificatorUtil
    {
        /// <summary>
        /// Validates that a given organization number is valid.
        /// </summary>
        /// <param name="orgNo">
        /// Organization number to validate
        /// </param>
        /// <returns>
        /// true if valid, false otherwise.
        /// </returns>
        /// <remarks>
        /// Validates length, numeric and modulus 11.
        /// </remarks>
        public static bool ValidateOrganizationNumber(string orgNo)
        {
            int[] weight = { 3, 2, 7, 6, 5, 4, 3, 2 };

            // Validation only done for 8 and 9 digit numbers
            if (orgNo.Length == 9)
            {
                try
                {
                    int currentDigit = 0;
                    int sum = 0;
                    for (int i = 0; i < orgNo.Length - 1; i++)
                    {
                        currentDigit = int.Parse(orgNo.Substring(i, 1));
                        sum += currentDigit * weight[i];
                    }

                    int ctrlDigit = 11 - (sum % 11);
                    if (ctrlDigit == 11)
                    {
                        ctrlDigit = 0;
                    }

                    return int.Parse(orgNo.Substring(orgNo.Length - 1)) == ctrlDigit;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
