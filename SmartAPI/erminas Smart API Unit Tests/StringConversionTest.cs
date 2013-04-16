using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using erminas.SmartAPI.Utils;

namespace erminas_Smart_API_Unit_Tests
{
    [TestClass]
    public class StringConversionTest
    {
        [TestMethod]
        public void TestSecureRQLFormat()
        {
            const string TEMPLATE = "{0}a{1}b{2}";
            var guid = Guid.NewGuid();
            var result = TEMPLATE.SecureRQLFormat("<q", new RedDotObjectHandle(guid, ""), "&");
            Assert.AreEqual("&lt;qa"+guid.ToRQLString()+"b&amp;", result);
        }
    }
}
