using Altinn.Platform.Register.Models;

namespace Altinn.AuthorizationAdmin.Core.Models
{
    /// <summary>
    /// The internal wrapper model for expressing a list of parties from SBL Bridge
    /// </summary>
    public class PartyList
    {
        /// <summary>
        /// Gets or sets the parties.
        /// </summary>
        /// <value>
        /// The parties.
        /// </value>
        public List<Party> Parties{ get; set; }
    }
}
