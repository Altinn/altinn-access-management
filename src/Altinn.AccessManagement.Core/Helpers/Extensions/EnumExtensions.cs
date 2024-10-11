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

            if (enumMemeberAttribute == null || enumMemeberAttribute.Value == null)
            {
                return value.ToString();
            }

            return enumMemeberAttribute.Value;
        }

        /// <summary>
        /// Tries to parse a string as a given Enum on the EnumMemberAttribute rater than the Name of the value in the enum returns false if no matching vale was found true if parsed ok
        /// </summary>
        /// <param name="value">The string to use for value in the enum</param>
        /// <param name="enumValue">Out parameter containing The parsed enum or default value</param>
        /// <typeparam name="T">The Enum type to Parse</typeparam>
        /// <returns>Value indicating the parsing went ok or not</returns>
        public static bool EnumValue<T>(string value, out T enumValue)
        {
            string[] names = Enum.GetNames(typeof(T));
            string name = Array.Find(names, name => EnumMemberAttributeValueOrName((Enum)Enum.Parse(typeof(T), name)).Equals(value));
            
            if (name != null)
            {
                enumValue = (T)Enum.Parse(typeof(T), name);
                return true;
            }

            enumValue = default;
            return false;
        }
    }
}
