namespace Altinn.AccessManagement.Core.Constants
{
    /// <summary>
    /// Constants related to authorization.
    /// </summary>
    public static class AuthzConstants
    {
        /// <summary>
        /// Policy tag for authorizing designer access
        /// </summary>
        public const string POLICY_STUDIO_DESIGNER = "StudioDesignerAccess";

        /// <summary>
        /// Policy tag for authorizing Altinn.Platform.Authorization API access from AltinnII Authorization
        /// </summary>
        public const string ALTINNII_AUTHORIZATION = "AltinnIIAuthorizationAccess";

        /// <summary>
        /// Policy tag for authorizing internal Altinn.Platform.Authorization API access
        /// </summary>
        public const string INTERNAL_AUTHORIZATION = "InternalAuthorizationAccess";

        /// <summary>
        /// Policy tag for reading an maskinporten delegation
        /// </summary>
        public const string POLICY_MASKINPORTEN_DELEGATION_READ = "MaskinportenDelegationRead";
        
        /// <summary>
        /// Policy tag for writing an maskinporten delegation
        /// </summary>
        public const string POLICY_MASKINPORTEN_DELEGATION_WRITE = "MaskinportenDelegationWrite";
    }
}
