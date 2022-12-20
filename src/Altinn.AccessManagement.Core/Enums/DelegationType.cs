namespace Altinn.AccessManagement.Core.Enums
{
    /// <summary>
    /// Enum for the different the types of delegations exist for a delegated right in Altinn Authorization
    /// </summary>
    public enum DelegationType
    {
        /// <summary>
        /// Default value
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Delegated to user
        /// </summary>
        DirectUserDelegation = 1,

        /// <summary>
        /// Delegated to user, from main unit
        /// </summary>
        DirectUserDelegationFromMainUnit = 2,

        /// <summary>
        /// Delegated to organization
        /// </summary>
        DirectOrgDelegation = 3,

        /// <summary>
        /// Delegated to organization, from main unit
        /// </summary>
        DirectOrgDelegationFromMainUnit = 4,

        /// <summary>
        /// Delegated to key role relation
        /// </summary>
        KeyRoleDelegation = 5,

        /// <summary>
        /// Delegated to key role relation, from main unit
        /// </summary>
        KeyRoleDelegationFromMainUnit = 6
    }
}
