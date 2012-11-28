using Microsoft.VisualStudio.TestTools.UnitTesting;
using erminas.SmartAPI.CMS;

namespace erminas_Smart_API_Unit_Tests.Enums
{
    [TestClass]
    public class PdfOrientationTest
    {
        [TestMethod]
        public void TestConversion()
        {
            EnumConversionTester<PdfOrientation>.TestConversion(PdfOrientationUtils.ToRQLString,
                                                                PdfOrientationUtils.ToPdfOrientation);
        }
    }
}