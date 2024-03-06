using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Utilities
{
    /// <summary>
    /// Utility class for working with internal and external identifiers
    /// </summary>
    public static class IdentifierUtil
    {
        private const string PersonHeaderTrigger = "person";
        private const string OrganizationHeaderTrigger = "organization";
        
        /// <summary>
        /// Default HTTP header for SSN input
        /// </summary>
        public const string PersonHeader = "Altinn-Party-SocialSecurityNumber";
        
        /// <summary>
        /// Default HTTP header for Organization number input
        /// </summary>
        public const string OrganizationNumberHeader = "Altinn-Party-OrganizationNumber";

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
        public static bool IsValidOrganizationNumber(string orgNo)
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

        /// <summary>
        /// Validates that a given social security number is valid.
        /// </summary>
        /// <param name="ssnNo">
        /// Social security number to validate
        /// </param>
        /// <returns>
        /// true if valid, false otherwise.
        /// </returns>
        /// <remarks>
        /// Validates length, numeric and modulus 11.
        /// </remarks>
        public static bool IsValidSSN(string ssnNo)
        {
            int[] weightDigit10 = { 3, 7, 6, 1, 8, 9, 4, 5, 2 };
            int[] weightDigit11 = { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };

            // Validation only done for 11 digit numbers
            if (ssnNo.Length == 11)
            {
                try
                {
                    int currentDigit = 0;
                    int sumCtrlDigit10 = 0;
                    int sumCtrlDigit11 = 0;
                    int ctrlDigit10 = -1;
                    int ctrlDigit11 = -1;

                    // Calculate control digits
                    for (int i = 0; i < 9; i++)
                    {
                        currentDigit = int.Parse(ssnNo.Substring(i, 1));
                        sumCtrlDigit10 += currentDigit * weightDigit10[i];
                        sumCtrlDigit11 += currentDigit * weightDigit11[i];
                    }

                    ctrlDigit10 = 11 - (sumCtrlDigit10 % 11);
                    if (ctrlDigit10 == 11)
                    {
                        ctrlDigit10 = 0;
                    }

                    sumCtrlDigit11 += ctrlDigit10 * weightDigit11[9];
                    ctrlDigit11 = 11 - (sumCtrlDigit11 % 11);
                    if (ctrlDigit11 == 11)
                    {
                        ctrlDigit11 = 0;
                    }

                    // Validate control digits in ssn
                    bool digit10Valid = ctrlDigit10 == int.Parse(ssnNo.Substring(9, 1));
                    bool digit11Valid = ctrlDigit11 == int.Parse(ssnNo.Substring(10, 1));
                    return digit10Valid && digit11Valid;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Masks an incoming SSN.
        /// </summary>
        /// <param name="input">SSN that should be masked</param>
        /// <returns>Masked SSN</returns>
        public static string MaskSSN(string input)
        {
            return string.Concat(input.AsSpan(0, 6), "*****");
        }

        /// <summary>
        /// Route allowes for specifying the reportee party in the path.
        /// This value must either be a PartyId, or the placeholder values: "organization" or "person"
        /// if the placeholder value is used the organization number or social security number must be specified in a corresponding header value: "Altinn-Party-OrganizationNumber" or "Altinn-Party-SocialSecurityNumber"
        /// </summary>
        /// <param name="party">The party value from route</param>
        /// <param name="context">The httpcontext for getting header values</param>
        /// <returns>AttributeMatch model representation of the identifier</returns>
        public static AttributeMatch GetIdentifierAsAttributeMatch(string party, HttpContext context)
        {
            if (party.Equals(OrganizationHeaderTrigger))
            {
                if (!context.Request.Headers.ContainsKey(OrganizationNumberHeader))
                {
                    throw new ArgumentException($"When using the '{OrganizationHeaderTrigger}' path parameter the organization number must be provided as a request header value: '{OrganizationNumberHeader}'");
                }

                if (!IsValidOrganizationNumber(context.Request.Headers[OrganizationNumberHeader]))
                {
                    throw new ArgumentException($"The request header '{OrganizationNumberHeader}' does not provide a well-formed organization number value: '{context.Request.Headers[OrganizationNumberHeader]}'");
                }

                return new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.OrganizationNumberAttribute, Value = context.Request.Headers[OrganizationNumberHeader] };
            }
            
            if (party.Equals(PersonHeaderTrigger))
            {
                if (!context.Request.Headers.ContainsKey(PersonHeader))
                {
                    throw new ArgumentException($"When using the '{PersonHeaderTrigger}' path parameter the national identity number must be provided as a request header value: '{PersonHeader}'");
                }

                if (!IsValidSSN(context.Request.Headers[PersonHeader]))
                {
                    throw new ArgumentException($"The request header '{PersonHeader}' does not provide a well-formed national identity number value: '{context.Request.Headers[PersonHeader]}'");
                }

                return new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.SocialSecurityNumberAttribute, Value = context.Request.Headers[PersonHeader] };
            }
            
            if (int.TryParse(party, out int partyId) && partyId != 0)
            {
                return new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, Value = partyId.ToString() };
            }
            else
            {
                throw new ArgumentException($"The party path parameter is not a well-formed party id: '{party}'");
            }
        }
    }
}
