using Microsoft.VisualStudio.TestTools.UnitTesting;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements;

namespace erminas_Smart_API_Unit_Tests.Enums
{
    [TestClass]
    public class HtmlTargetTest
    {
        [TestMethod]
        public void TestConversion()
        {
            EnumConversionTester<HtmlTarget>.TestConversionWithoutExcludedValues(HtmlTargetUtils.ToRQLString, HtmlTargetUtils.ToHtmlTarget);
        }
    }
}