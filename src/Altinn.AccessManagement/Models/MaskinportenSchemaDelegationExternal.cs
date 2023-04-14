namespace Altinn.AccessManagement.Models
{
    /// <summary>
    /// This model describes a delegation of a Maskinporten Schema from a client/consumer organization to a supplier organization, giving the supplier access to the scopes covered by the schema.
    /// </summary>
    public class MaskinportenSchemaDelegationExternal
    {
        /// <summary>
        /// Gets or sets the PartyId of the client/consumer organization having delegated the maskinporten schema
        /// </summary>
        public int OfferedByPartyId { get; set; }

        /// <summary>
        /// Gets or sets the name of the client/consumer organization having delegated the maskinporten schema
        /// </summary>
        public string OfferedByName { get; set; }

        /// <summary>
        /// Gets or sets the organization number of the client/consumer organization having delegated the maskinporten schema
        /// </summary>
        public string OfferedByOrganizationNumber { get; set; }

        /// <summary>
        /// Gets or sets the PartyId of the supplier organization having received the delegated maskinporten schema
        /// </summary>
        public int? CoveredByPartyId { get; set; }

        /// <summary>
        /// Gets or sets the name of the supplier organization having received the delegated maskinporten schema
        /// </summary>
        public string CoveredByName { get; set; }

        /// <summary>
        /// Gets or sets the organization number of the supplier organization having received the delegated maskinporten schema
        /// </summary>
        public string CoveredByOrganizationNumber { get; set; }

        /// <summary>
        /// Gets or sets the user id of the user that performed the delegation
        /// </summary>
        public int PerformedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the date and timestamp when the delegation was performed
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the maskinporten schema resource registered in the resource registry
        /// </summary>
        public string ResourceId { get; set; }
    }
}
