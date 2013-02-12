using Microsoft.VisualStudio.TestTools.UnitTesting;
using erminas.SmartAPI.CMS;
using erminas.SmartAPI.CMS.Project.ContentClasses;

namespace erminas_Smart_API_Unit_Tests.Enums
{
    [TestClass]
    public class PdfOrientationTest
    {
        [TestMethod]
        public void TestConversion()
        {
            EnumConversionTester<PdfOrientation>.TestConversionWithoutExcludedValues(PdfOrientationUtils.ToRQLString,
                                                                PdfOrientationUtils.ToPdfOrientation);
        }
    }
}