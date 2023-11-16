using Altinn.AccessManagement.Core.Enums;

namespace Altinn.AccessManagement.Core.Models
{
    /// <summary>
    /// This model describes a detail while providing a specific code for identifying a specific recurring detail and parameters needed for context and understanding.
    /// Can be extended for specific detailing/metadata/informational purposes.
    /// </summary>
    public class Detail
    {
        /// <summary>
        /// Gets or sets the detail identifier code
        /// </summary>
        public DetailCode Code { get; set; }

        /// <summary>
        /// Gets or sets a human readable (english) description of the detail
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of parameters which is related to the detail code and description
        /// </summary>
        public Dictionary<string, List<AttributeMatch>> Parameters { get; set; } = new Dictionary<string, List<AttributeMatch>>();
    }
}
