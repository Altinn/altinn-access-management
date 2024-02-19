namespace Altinn.AccessManagement.Core.Configuration
{
    /// <summary>
    /// Feature management flags
    /// </summary>
    public static class FeatureFlags
    {
        /// <summary>
        /// Feature flag for activating the Rights Delegation API
        /// </summary>
        public const string RightsDelegationApi = "RightsDelegationApi";

        /// <summary>
        /// Feature flag for activating the Rights Delegation API External
        /// </summary>
        public const string RightsDelegationApiExternal = nameof(RightsDelegationApiExternal);
    }
}
