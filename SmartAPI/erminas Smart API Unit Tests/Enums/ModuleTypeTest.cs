using Microsoft.VisualStudio.TestTools.UnitTesting;
using erminas.SmartAPI.CMS;
using erminas.SmartAPI.CMS.Administration;
using erminas.SmartAPI.Exceptions;

namespace erminas_Smart_API_Unit_Tests.Enums
{
    [TestClass]
    public class ModuleTypeTest
    {
        [TestMethod]
        public void TestConversion()
        {
            EnumConversionTester<ModuleType>.TestConversionWithoutExcludedValues(ModuleTypeUtils.ToRQLString, ModuleTypeUtils.ToModuleType, ModuleType.NoModule);
        }

        [TestMethod]
        [ExpectedException(typeof(SmartAPIInternalException))]
        public void TestNoModuleStringConversion()
        {
            ModuleType.NoModule.ToRQLString();
        }
    }
}