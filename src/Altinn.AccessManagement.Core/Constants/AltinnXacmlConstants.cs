namespace Altinn.AccessManagement.Core.Constants
{
    /// <summary>
    /// Altinn specific XACML constants used for urn identifiers and attributes
    /// </summary>
    public static class AltinnXacmlConstants
    {
        /// <summary>
        /// Altinn specific prefixes
        /// </summary>
        public static class Prefixes
        {
            /// <summary>
            /// The Policy Id prefix.
            /// </summary>
            public const string PolicyId = "urn:altinn:policyid:";

            /// <summary>
            /// The Obligation Id prefix.
            /// </summary>
            public const string ObligationId = "urn:altinn:obligationid:";

            /// <summary>
            /// The Obligation Assignment Id prefix.
            /// </summary>
            public const string ObligationAssignmentid = "urn:altinn:obligation-assignmentid:";
        }

        /// <summary>
        /// Match attribute identifiers
        /// </summary>
        public static class MatchAttributeIdentifiers
        {
            /// <summary>
            /// Org attribute match indentifier 
            /// </summary>
            public const string OrgAttribute = "urn:altinn:org";

            /// <summary>
            /// App attribute match indentifier 
            /// </summary>
            public const string AppAttribute = "urn:altinn:app";

            /// <summary>
            /// Instance attribute match indentifier 
            /// </summary>
            public const string InstanceAttribute = "urn:altinn:instance-id";

            /// <summary>
            /// App resource attribute match indentifier 
            /// </summary>
            public const string AppResourceAttribute = "urn:altinn:appresource";

            /// <summary>
            /// Task attribute match indentifier 
            /// </summary>
            public const string TaskAttribute = "urn:altinn:task";

            /// <summary>
            /// End-event attribute match indentifier 
            /// </summary>
            public const string EndEventAttribute = "urn:altinn:end-event";

            /// <summary>
            /// Party Id attribute match indentifier 
            /// </summary>
            public const string PartyAttribute = "urn:altinn:partyid";

            /// <summary>
            /// User Id attribute match indentifier 
            /// </summary>>
            public const string UserAttribute = "urn:altinn:userid";

            /// <summary>
            /// Role Code attribute match indentifier 
            /// </summary>
            public const string RoleAttribute = "urn:altinn:rolecode";

            /// <summary>
            /// Resource Registry attribute match indentifier 
            /// </summary>
            public const string ResourceRegistryAttribute = "urn:altinn:resource";

            /// <summary>
            /// Organization name
            /// </summary>
            public const string OrganizationName = "urn:altinn:organization:name";

            /// <summary>
            /// Organization number attribute match indentifier 
            /// </summary>
            public const string OrganizationNumberAttribute = "urn:altinn:organizationnumber";

            /// <summary>
            /// Social security number attribute match indentifier 
            /// </summary>
            public const string SocialSecurityNumberAttribute = "urn:altinn:ssn";

            /// <summary>
            /// Altinn 2 service code attribute match indentifier 
            /// </summary>
            public const string ServiceCodeAttribute = "urn:altinn:servicecode";

            /// <summary>
            /// Altinn 2 service edition code attribute match indentifier 
            /// </summary>
            public const string ServiceEditionCodeAttribute = "urn:altinn:serviceeditioncode";

            /// <summary>
            /// Person uuid
            /// </summary>
            public const string PersonUuid = "urn:altinn:person:uuid";

            /// <summary>
            /// National identity number for a person
            /// </summary>
            public const string PersonId = "urn:altinn:person:identifier-no";

            /// <summary>
            /// Last name of a person 
            /// </summary>
            public const string PersonLastName = "urn:altinn:person:lastname";

            /// <summary>
            /// Person username
            /// </summary>
            public const string PersonUserName = "urn:altinn:person:username";

            /// <summary>
            /// Enterprise user uuid
            /// </summary>
            public const string EnterpriseUserUuid = "urn:altinn:enterpriseuser:uuid";

            /// <summary>
            /// Enterprise user username
            /// </summary>
            public const string EnterpriseUserName = "urn:altinn:enterpriseuser:username";

            /// <summary>
            /// Organization uuid
            /// </summary>
            public const string OrganizationUuid = "urn:altinn:organization:uuid";

            /// <summary>
            /// Organization number
            /// </summary>
            public const string OrganizationId = "urn:altinn:organization:identifier-no";

            /// <summary>
            /// SystemUser uuid
            /// </summary>
            public const string SystemUserUuid = "urn:altinn:systemuser:uuid";
        }

        /// <summary>
        /// Attribute categories.
        /// </summary>
        public static class MatchAttributeCategory
        {
            /// <summary>
            /// The minimum authentication level category.
            /// </summary>
            public const string MinimumAuthenticationLevel = "urn:altinn:minimum-authenticationlevel";

            /// <summary>
            /// The minimum authentication level for organization category
            /// </summary>
            public const string MinimumAuthenticationLevelOrg = "urn:altinn:minimum-authenticationlevel-org";
        }
    }
}
