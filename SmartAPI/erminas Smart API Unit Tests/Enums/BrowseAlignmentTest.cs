using Microsoft.VisualStudio.TestTools.UnitTesting;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements;

namespace erminas_Smart_API_Unit_Tests.Enums
{
    [TestClass]
    public class BrowseAlignmentTest
    {
        [TestMethod]
        public void TestConversion()
        {
            EnumConversionTester<BrowseAlignment>.TestConversionWithoutExcludedValues(BrowseAlignmentUtils.ToRQLString,
                                                                 BrowseAlignmentUtils.ToBrowseAlignment);
        }
    }
}