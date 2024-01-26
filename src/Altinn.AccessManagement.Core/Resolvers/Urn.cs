using Altinn.Platform.Register.Models;
using Azure.Core;

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
    public new static string ToString() => $"{nameof(Urn)}".ToLower();

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
        public new static string ToString() => $"{Urn.ToString()}:{nameof(Altinn)}".ToLower();

        /// <summary>
        /// summary
        /// </summary>
        public static class Person
        {
            /// <summary>
            /// summary
            /// </summary>
            public static string IdentifierNo => $"{ToString()}:identifier-no";

            /// <summary>
            /// summary
            /// </summary>
            public static string Uuid => $"{ToString()}:uuid";

            /// <summary>
            /// summary
            /// </summary>
            public static string PartyId => $"{ToString()}:partyid";

            /// <summary>
            /// summary
            /// </summary>
            public static string Firstname => $"{ToString()}:firstname";

            /// <summary>
            /// summary
            /// </summary>
            public static string Shortname => $"{ToString()}:shortname";

            /// <summary>
            /// summary
            /// </summary>
            public static string Middlename => $"{ToString()}:middlename";

            /// <summary>
            /// summary
            /// </summary>
            public static string Lastname => $"{ToString()}:lastname";

            /// <summary>
            /// summary
            /// </summary>
            /// <returns></returns>
            public new static string ToString() => $"{Altinn.ToString()}:{nameof(Person)}".ToLower();
        }

        /// <summary>
        /// summary
        /// </summary>
        public static class Organization
        {
            /// <summary>
            /// summary
            /// </summary>
            public static string IdentifierNo => $"{ToString()}:identifier-no";

            /// <summary>
            /// summary
            /// </summary>
            public static string Name => $"{ToString()}:name";

            /// <summary>
            /// summary
            /// </summary>
            public static string PartyId => $"{ToString()}:partyid";

            /// <summary>
            /// summary
            /// </summary>
            public static string Uuid => $"{ToString()}:uuid";

            /// <summary>
            /// summary
            /// </summary>
            /// <returns></returns>
            public new static string ToString() => $"{Altinn.ToString()}:{nameof(Organization)}".ToLower();
        }

        /// <summary>
        /// summary
        /// </summary>
        public static class EnterpriseUser
        {
            /// <summary>
            /// summary
            /// </summary>
            public static string Username => $"{ToString()}:username";

            /// <summary>
            /// summary
            /// </summary>
            public static string Uuid => $"{ToString()}:uuid";

            /// <summary>
            /// summary
            /// </summary>
            public static string PartyId => $"{ToString()}:partyid";

            /// <summary>
            /// summary
            /// </summary>
            public static class Organization
            {
                /// <summary>
                /// summary
                /// </summary>
                public static string Uuid => $"{ToString()}:uuid";

                /// <summary>
                /// summary
                /// </summary>
                /// <returns></returns>
                public new static string ToString() => $"{EnterpriseUser.ToString()}:{nameof(Organization)}".ToLower();
            }

            /// <summary>
            /// summary
            /// </summary>
            /// <returns></returns>
            public new static string ToString() => $"{Altinn.ToString()}:{nameof(EnterpriseUser)}".ToLower();
        }

        /// <summary>
        /// summary
        /// </summary>
        public static class Resource
        {
            /// <summary>
            /// summary
            /// </summary>
            public static string ResourceRegistryId => $"{ToString()}:resourceregistryid".ToLower();

            /// <summary>
            /// summary
            /// </summary>
            public static string AppOwner => $"{ToString()}:appowner".ToLower();

            /// <summary>
            /// summary
            /// </summary>
            public static string AppId => $"{ToString()}:appid".ToLower();

            /// <summary>
            /// summary
            /// </summary>
            public static string Type => $"{ToString()}:type".ToLower();

            /// <summary>
            /// summary
            /// </summary>
            public static string Delegable => $"{ToString()}:delegable".ToLower();

            /// <summary>
            /// summary
            /// </summary>
            /// <returns></returns>
            public new static string ToString() => $"{Altinn.ToString()}:{nameof(Resource)}".ToLower();
        }
    }
}