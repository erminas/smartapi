using Microsoft.VisualStudio.TestTools.UnitTesting;
using erminas.SmartAPI.CMS.CCElements;

namespace erminas_Smart_API_Unit_Tests.Enums
{
    [TestClass]
    public class MediaConversionModeTest
    {
        [TestMethod]
        public void TestConversion()
        {
            EnumConversionTester<MediaConversionMode>.TestConversionWithoutExcludedValues(MediaConversionModeUtils.ToRQLString,
                                                                     MediaConversionModeUtils.ToMediaConversionMode);
        }
    }
}