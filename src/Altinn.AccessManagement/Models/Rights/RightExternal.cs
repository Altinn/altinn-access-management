using System.Text.Json.Serialization;

namespace Altinn.AccessManagement.Models
{
    /// <summary>
    /// This model describes a single right including specification of right source information and whether the user have access or delegation access for the right
    /// </summary>
    public class RightExternal : BaseRightExternal
    {
        /// <summary>
        /// Gets or sets the right key
        /// </summary>
        public string RightKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user or party has the right
        /// </summary>
        public bool HasPermit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user or party is permitted to delegate the right to others
        /// </summary>
        public bool CanDelegate { get; set; }

        /// <summary>
        /// Gets or sets the set of identified sources providing the right
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<RightSourceExternal> RightSources { get; set; }
    }
}
