using Microsoft.VisualStudio.TestTools.UnitTesting;
using erminas.SmartAPI.CMS.CCElements;

namespace erminas_Smart_API_Unit_Tests.Enums
{
    [TestClass]
    public class FrameborderTest
    {
        [TestMethod]
        public void TestConversion()
        {
            EnumConversionTester<Frameborder>.TestConversionWithoutExcludedValues(FrameborderUtils.ToRQLString,
                                                             FrameborderUtils.ToFrameborder);
        }
    }
}