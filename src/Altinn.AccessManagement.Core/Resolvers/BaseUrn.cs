using Altinn.AccessManagement.Core.Constants;

namespace Altinn.AccessManagement.Core.Resolvers;

/// <summary>
/// Urn
/// </summary>
public static class BaseUrn
{
    /// <summary>
    /// Urn
    /// </summary>
    /// <returns></returns>
    public static string String() => $"urn";

    /// <summary>
    /// InternalIds of delegatable entities 
    /// </summary>
    public static string[] InternalIds => [Altinn.Organization.PartyId, Altinn.Person.PartyId, Altinn.Person.UserId, Altinn.EnterpriseUser.UserId, AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute];

    /// <summary>
    /// InternalIds from Altinn 2
    /// </summary>
    public static string[] Altinn2InternalIds => [AltinnXacmlConstants.MatchAttributeIdentifiers.UserAttribute, AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute];

    /// <summary>
    /// Resources that belongs to Altinn 
    /// </summary>
    public static class Altinn
    {
        /// <summary>
        /// Urn.Altinn
        /// </summary>
        public static string String() => $"{BaseUrn.String()}:{nameof(Altinn)}".ToLower();

        /// <summary>
        /// Urn.Altinn.Person
        /// </summary>
        public static class Person
        {
            /// <summary>
            /// A person Social security number
            /// </summary>
            public static string IdentifierNo => $"{String()}:identifier-no";

            /// <summary>
            /// Uuid
            /// </summary>
            public static string Uuid => $"{String()}:uuid";

            /// <summary>
            /// UserId
            /// </summary>
            public static string UserId => $"{String()}:userid";

            /// <summary>
            /// PartyId
            /// </summary>
            public static string PartyId => $"{String()}:partyid";

            /// <summary>
            /// A person's first name
            /// </summary>
            public static string Firstname => $"{String()}:firstname";

            /// <summary>
            /// A Person's shortname
            /// </summary>
            public static string Shortname => $"{String()}:shortname";

            /// <summary>
            /// A Person's middlename
            /// </summary>
            public static string Middlename => $"{String()}:middlename";

            /// <summary>
            /// A Person's lastname
            /// </summary>
            public static string Lastname => $"{String()}:lastname";

            /// <summary>
            /// Urn.Altinn.Person
            /// </summary>
            public static string String() => $"{Altinn.String()}:{nameof(Person)}".ToLower();
        }

        /// <summary>
        /// Urn.Altinn.Organization
        /// </summary>
        public static class Organization
        {
            /// <summary>
            /// An organization brreg number
            /// </summary>
            public static string IdentifierNo => $"{String()}:identifier-no";

            /// <summary>
            /// Organization's name
            /// </summary>
            public static string Name => $"{String()}:name";

            /// <summary>
            /// Organzation's Party Id
            /// </summary>
            public static string PartyId => $"{String()}:partyid";

            /// <summary>
            /// Uuid of organization
            /// </summary>
            public static string Uuid => $"{String()}:uuid";

            /// <summary>
            /// Urn.Altinn.Organization
            /// </summary>
            public static string String() => $"{Altinn.String()}:{nameof(Organization)}".ToLower();
        }

        /// <summary>
        /// Urn.Altinn.EnterpriseUser
        /// </summary>
        public static class EnterpriseUser
        {
            /// <summary>
            /// username
            /// </summary>
            public static string Username => $"{String()}:username";

            /// <summary>
            /// uuid
            /// </summary>
            public static string Uuid => $"{String()}:uuid";

            /// <summary>
            /// UserId
            /// </summary>
            public static string UserId => $"{String()}:userid";

            /// <summary>
            /// Urn.Altinn.EnterpriseUser.Organization
            /// </summary>
            public static class Organization
            {
                /// <summary>
                /// uuid
                /// </summary>
                public static string Uuid => $"{String()}:uuid";

                /// <summary>
                /// Urn.Altinn.EnterpriseUser.Organization
                /// </summary>
                public static string String() => $"{EnterpriseUser.String()}:{nameof(Organization)}".ToLower();
            }

            /// <summary>
            /// Urn.Altinn.EnterpriseUser
            /// </summary>
            public static string String() => $"{Altinn.String()}:{nameof(EnterpriseUser)}".ToLower();
        }

        /// <summary>
        /// Urn.Altinn.Resource
        /// </summary>
        public static class Resource
        {
            /// <summary>
            /// The resource regigistry identifier
            /// </summary>
            public static string ResourceRegistryId => "urn:altinn:resource";

            /// <summary>
            /// Owner of the altinn App
            /// </summary>
            public static string AppOwner => "urn:altinn:org";

            /// <summary>
            /// Altinn AppId
            /// </summary>
            public static string AppId => "urn:altinn:app";

            /// <summary>
            /// Specifies the type of the resource
            /// </summary>
            public static string Type => $"{String()}:type".ToLower();

            /// <summary>
            /// boolean that specifies of the resource is delegable or not
            /// </summary>
            public static string Delegable => $"{String()}:delegable".ToLower();

            /// <summary>
            /// Urn.Altinn.Resource
            /// </summary>
            public static string String() => $"{Altinn.String()}:{nameof(Resource)}".ToLower();
        }
    }
}