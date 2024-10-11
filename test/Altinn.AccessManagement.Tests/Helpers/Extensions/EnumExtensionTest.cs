using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Enums;

namespace Altinn.AccessManagement.Tests.Helpers.Extensions
{
    public class EnumExtensionTest
    {
        [Fact]
        public void ConvertEnumMemberValueStringToEnum()
        {
            string enumMemberValueString = "urn:altinn:person:uuid";
            bool result = EnumExtensions.EnumValue<UuidType>(enumMemberValueString, out UuidType enumValue);
            Assert.True(result);
            Assert.Equal(UuidType.Person, enumValue);
        }
    }
}
