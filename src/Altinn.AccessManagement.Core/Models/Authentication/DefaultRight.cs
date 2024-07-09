namespace Altinn.AccessManagement.Core.Models.Authentication
{
    /// <summary>
    /// DTO for a Default Right on a Registered System
    /// </summary>
    public class DefaultRight
    {
        /// <summary>
        /// The list of resources at the Service Provider which the Right is for.
        /// </summary>
        public List<AttributeMatch> Resource { get; set; } = [];
    }
}