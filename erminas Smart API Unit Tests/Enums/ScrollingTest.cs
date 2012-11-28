using Microsoft.VisualStudio.TestTools.UnitTesting;
using erminas.SmartAPI.CMS.CCElements;

namespace erminas_Smart_API_Unit_Tests.Enums
{
    [TestClass]
    public class ScrollingTest
    {
        [TestMethod]
        public void TestConversion()
        {
            EnumConversionTester<Scrolling>.TestConversion(ScrollingUtils.ToRQLString, ScrollingUtils.ToScrolling);
        }
    }
}