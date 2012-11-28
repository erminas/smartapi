using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using erminas.SmartAPI.Utils;

namespace erminas_Smart_API_Unit_Tests
{
    [TestClass]
    public class NameIndexedRDListTest : AbstractCachedListTest
    {
        private readonly NameIndexedRDList<RedDotObjectHandle> _listCaching;
        private readonly NameIndexedRDList<RedDotObjectHandle> _listNonCaching;

        public NameIndexedRDListTest()
        {
            _listCaching = new NameIndexedRDList<RedDotObjectHandle>(Objects, Caching.Enabled);
            _listNonCaching = new NameIndexedRDList<RedDotObjectHandle>(Objects, Caching.Disabled);
        }


        [TestMethod]
        public void TestRDIndexedListGetByNameCaching()
        {
            Assert.IsNotNull(_listCaching.Get("a"));

// ReSharper disable ReturnValueOfPureMethodIsNotUsed
            _listCaching.Count();
// ReSharper restore ReturnValueOfPureMethodIsNotUsed
            Assert.AreEqual(1, _callCount);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void TestRDIndexedListGetByNameCachingError()
        {
            _listCaching.Get("fail");
        }

        [TestMethod]
        public void TestRDindexListTryGetByNameCaching()
        {
            RedDotObjectHandle result;
            Assert.IsTrue(_listCaching.TryGet("b", out result));
            Assert.IsNotNull(result);

            Assert.IsFalse(_listCaching.TryGet("fail", out result));
            Assert.AreEqual(3, _listCaching.Count());
            Assert.AreEqual(1, _callCount);
        }

        [TestMethod]
        public void TestRDIndexedListGetByNameNonCaching()
        {
            Assert.AreEqual(0, _callCount);
            Assert.IsNotNull(_listNonCaching.Get("a"));
            Assert.AreEqual(1, _callCount);
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
            _listCaching.Count();
// ReSharper restore ReturnValueOfPureMethodIsNotUsed
            Assert.AreEqual(2, _callCount);
        }

        [TestMethod]
        public void TestRDindexListTryGetByNameNonCaching()
        {
            RedDotObjectHandle result;
            Assert.IsTrue(_listNonCaching.TryGet("b", out result));
            Assert.IsNotNull(result);

            Assert.IsFalse(_listNonCaching.TryGet("fail", out result));

            Assert.AreEqual(2, _callCount);
        }
    }
}
