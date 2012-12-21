﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using erminas.SmartAPI.CMS.CCElements;

namespace erminas_Smart_API_Unit_Tests.Enums
{
    [TestClass]
    public class ListTypeTest
    {
        [TestMethod]
        public void TestConversion()
        {
            EnumConversionTester<ListType>.TestConversionWithoutExcludedValues(ListTypeUtils.ToRQLString, ListTypeUtils.ToListType);
        }
    }
}