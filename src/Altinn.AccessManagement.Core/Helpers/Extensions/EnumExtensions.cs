using System.Reflection;
using System.Runtime.Serialization;

namespace Altinn.AccessManagement.Core.Helpers.Extensions
{
    /// <summary>
    /// Enum Extensions
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Fetch the content of the Value for a defined EnumMemberAttribute or just returns the name if no such declaration exists
        /// </summary>
        /// <param name="value">The enum to fetch data from</param>
        /// <returns>The content of the declared EnumMemberAttribute.Value for the defined enum or just ToString if no EnumMemberAttribute is defined</returns>
        public static string EnumMemberAttributeValueOrName(this Enum value)
        {
            var enumType = value.GetType();
            var enumMemeberAttribute = enumType
                .GetTypeInfo()
                .DeclaredMembers
                .Single(x => x.Name == value.ToString())
                .GetCustomAttribute<EnumMemberAttribute>(false);

            if (enumMemeberAttribute == null)
            {
                return value.ToString();
            }

            return enumMemeberAttribute.Value;
        }
    }
}
