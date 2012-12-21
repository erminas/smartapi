using Microsoft.VisualStudio.TestTools.UnitTesting;
using erminas.SmartAPI.CMS.CCElements;

namespace erminas_Smart_API_Unit_Tests.Enums
{
    [TestClass]
    public class BasicAlignmentTest
    {
        [TestMethod]
        public void TestConversion()
        {
            EnumConversionTester<BasicAlignment>.TestConversion(BasicAlignmentUtils.ToRQLString,
                                                                BasicAlignmentUtils.ToBasicAlignment);
        }
    }
}