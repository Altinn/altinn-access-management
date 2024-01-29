namespace Altinn.AccessManagement.Core.Resolvers;

/// <summary>
/// summary
/// </summary>
public static class Urn
{
    /// <summary>
    /// summary
    /// </summary>
    /// <returns></returns>
    public static string String() => $"{nameof(Urn)}".ToLower();

    /// <summary>
    /// summary
    /// </summary>
    public static string[] PartyIds => [Altinn.Organization.PartyId, Altinn.Person.PartyId, Altinn.EnterpriseUser.PartyId];

    /// <summary>
    /// summary
    /// </summary>
    public static string[] Identifiers => [Altinn.Organization.IdentifierNo, Altinn.Person.IdentifierNo];

    /// <summary>
    /// altinn
    /// </summary>
    public static class Altinn
    {
        /// <summary>
        /// summary
        /// </summary>
        public static string String() => $"{Urn.String()}:{nameof(Altinn)}".ToLower();

        /// <summary>
        /// summary
        /// </summary>
        public static class Person
        {
            /// <summary>
            /// summary
            /// </summary>
            public static string IdentifierNo => $"{String()}:identifier-no";

            /// <summary>
            /// summary
            /// </summary>
            public static string Uuid => $"{String()}:uuid";

            /// <summary>
            /// summary
            /// </summary>
            public static string PartyId => $"{String()}:partyid";

            /// <summary>
            /// summary
            /// </summary>
            public static string Firstname => $"{String()}:firstname";

            /// <summary>
            /// summary
            /// </summary>
            public static string Shortname => $"{String()}:shortname";

            /// <summary>
            /// summary
            /// </summary>
            public static string Middlename => $"{String()}:middlename";

            /// <summary>
            /// summary
            /// </summary>
            public static string Lastname => $"{String()}:lastname";

            /// <summary>
            /// summary
            /// </summary>
            /// <returns></returns>
            public static string String() => $"{Altinn.String()}:{nameof(Person)}".ToLower();
        }

        /// <summary>
        /// summary
        /// </summary>
        public static class Organization
        {
            /// <summary>
            /// summary
            /// </summary>
            public static string IdentifierNo => $"{String()}:identifier-no";

            /// <summary>
            /// summary
            /// </summary>
            public static string Name => $"{String()}:name";

            /// <summary>
            /// summary
            /// </summary>
            public static string PartyId => $"{String()}:partyid";

            /// <summary>
            /// summary
            /// </summary>
            public static string Uuid => $"{String()}:uuid";

            /// <summary>
            /// summary
            /// </summary>
            /// <returns></returns>
            public static string String() => $"{Altinn.String()}:{nameof(Organization)}".ToLower();
        }

        /// <summary>
        /// summary
        /// </summary>
        public static class EnterpriseUser
        {
            /// <summary>
            /// summary
            /// </summary>
            public static string Username => $"{String()}:username";

            /// <summary>
            /// summary
            /// </summary>
            public static string Uuid => $"{String()}:uuid";

            /// <summary>
            /// summary
            /// </summary>
            public static string PartyId => $"{String()}:partyid";

            /// <summary>
            /// summary
            /// </summary>
            public static class Organization
            {
                /// <summary>
                /// summary
                /// </summary>
                public static string Uuid => $"{String()}:uuid";

                /// <summary>
                /// summary
                /// </summary>
                /// <returns></returns>
                public static string String() => $"{EnterpriseUser.String()}:{nameof(Organization)}".ToLower();
            }

            /// <summary>
            /// summary
            /// </summary>
            /// <returns></returns>
            public static string String() => $"{Altinn.String()}:{nameof(EnterpriseUser)}".ToLower();
        }

        /// <summary>
        /// summary
        /// </summary>
        public static class Resource
        {
            /// <summary>
            /// summary
            /// </summary>
            public static string ResourceRegistryId => "urn:altinn:resource";

            /// <summary>
            /// summary
            /// </summary>
            public static string AppOwner => "urn:altinn:org";

            /// <summary>
            /// summary
            /// </summary>
            public static string AppId => "urn:altinn:app".ToLower();

            /// <summary>
            /// summary
            /// </summary>
            public static string Type => $"{String()}:type".ToLower();

            /// <summary>
            /// summary
            /// </summary>
            public static string Delegable => $"{String()}:delegable".ToLower();

            /// <summary>
            /// summary
            /// </summary>
            /// <returns></returns>
            public static string String() => $"{Altinn.String()}:{nameof(Resource)}".ToLower();
        }
    }
}