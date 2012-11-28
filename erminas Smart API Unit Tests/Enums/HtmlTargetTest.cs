using Microsoft.VisualStudio.TestTools.UnitTesting;
using erminas.SmartAPI.CMS.CCElements;

namespace erminas_Smart_API_Unit_Tests.Enums
{
    [TestClass]
    public class HtmlTargetTest
    {
        [TestMethod]
        public void TestConversion()
        {
            EnumConversionTester<HtmlTarget>.TestConversion(HtmlTargetUtils.ToRQLString, HtmlTargetUtils.ToHtmlTarget);
        }
    }
}