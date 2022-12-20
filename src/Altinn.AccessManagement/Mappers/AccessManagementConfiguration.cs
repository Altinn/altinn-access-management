using AutoMapper;

namespace Altinn.AccessManagement.Mappers
{
    /// <summary>
    /// Configuration for automapper for access management
    /// </summary>
    public class AccessManagementConfiguration : MapperConfigurationExpression
    {
        /// <summary>
        /// access management mapping configuration
        /// </summary>
        public AccessManagementConfiguration() 
        {
            this.AllowNullCollections = true;   
        }
    }
}
