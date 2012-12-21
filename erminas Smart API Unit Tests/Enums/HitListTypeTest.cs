using Microsoft.VisualStudio.TestTools.UnitTesting;
using erminas.SmartAPI.CMS.CCElements;

namespace erminas_Smart_API_Unit_Tests.Enums
{
    [TestClass]
    public class HitListTypeTest
    {
        [TestMethod]
        public void TestConversion()
        {
            EnumConversionTester<HitListType>.TestConversionWithoutExcludedValues(HitListTypeUtils.ToRQLString,
                                                             HitListTypeUtils.ToHitListType);
        }
    }
}