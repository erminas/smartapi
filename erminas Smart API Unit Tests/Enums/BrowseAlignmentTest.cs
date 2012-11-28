using Microsoft.VisualStudio.TestTools.UnitTesting;
using erminas.SmartAPI.CMS.CCElements;

namespace erminas_Smart_API_Unit_Tests.Enums
{
    [TestClass]
    public class BrowseAlignmentTest
    {
        [TestMethod]
        public void TestConversion()
        {
            EnumConversionTester<BrowseAlignment>.TestConversion(BrowseAlignmentUtils.ToRQLString,
                                                                 BrowseAlignmentUtils.ToBrowseAlignment);
        }
    }
}