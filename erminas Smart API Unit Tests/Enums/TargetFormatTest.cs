using Microsoft.VisualStudio.TestTools.UnitTesting;
using erminas.SmartAPI.CMS.CCElements;

namespace erminas_Smart_API_Unit_Tests.Enums
{
    [TestClass]
    public class TargetFormatTest
    {
        [TestMethod]
        public void TestConversion()
        {
            EnumConversionTester<TargetFormat>.TestConversionWithoutExcludedValues(TargetFormatUtils.ToRQLString,
                                                              TargetFormatUtils.ToTargetFormat);
        }
    }
}