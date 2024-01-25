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
            public static string Firstname => $"{ToString()}:{nameof(Firstname)}";

            /// <summary>
            /// summary
            /// </summary>
            public static string Shortname => $"{ToString()}:{nameof(Shortname)}";

            /// <summary>
            /// summary
            /// </summary>
            public static string Middlename => $"{ToString()}:{nameof(Middlename)}";

            /// <summary>
            /// summary
            /// </summary>
            public static string Lastname => $"{ToString()}:{nameof(Lastname)}";

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
            public static string PartyId => $"{ToString()}:{PartyId}";

            /// <summary>
            /// summary
            /// </summary>
            public static string Uuid => $"{ToString()}:{Uuid}";

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
            public static string Username => $"{ToString()}:{nameof(Username)}";

            /// <summary>
            /// summary
            /// </summary>
            public static string Uuid => $"{ToString()}:{nameof(Uuid)}";

            /// <summary>
            /// summary
            /// </summary>
            public static class Organization
            {
                /// <summary>
                /// summary
                /// </summary>
                public static string Uuid => $"{ToString()}:{nameof(Uuid)}";
            
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
            public static string ResourceRegistryId => $"{ToString()}:{nameof(ResourceRegistryId)}".ToLower();

            /// <summary>
            /// summary
            /// </summary>
            public static string AppId => $"{ToString()}:{AppId}".ToLower();

            /// <summary>
            /// summary
            /// </summary>
            /// <returns></returns>
            public new static string ToString() => $"{Altinn.ToString()}:{nameof(Resource)}".ToLower();
        } 
    }
}