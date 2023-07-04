namespace Altinn.AccessManagement.Enums
{
    /// <summary>
    /// Enum for different right delegation status responses
    /// </summary>
    public enum DelegableStatusExternal
    {
        /// <summary>
        /// User is not able to delegate the right
        /// </summary>
        NotDelegable = 1,

        /// <summary>
        /// User is able to delegate the right
        /// </summary>
        Delegable = 2,

        /// <summary>
        /// Right is already delegated to the recipient
        /// </summary>
        AlreadyDelegated = 3
    }
}
