using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using erminas.SmartAPI.Utils;

namespace erminas_Smart_API_Unit_Tests
{
    [TestClass]
    public class CachedListTest : AbstractCachedListTest
    {
        // ReSharper disable ReturnValueOfPureMethodIsNotUsed
        [TestMethod]
        public void TestRDListWithCaching()
        {
            var list = new CachedList<RedDotObjectHandle>(Objects, Caching.Enabled);

            Assert.AreEqual(true, list.IsCachingEnabled);
            Assert.AreEqual(3, list.Count());
            Assert.AreEqual(1, _callCount);
            list.Count();
            Assert.AreEqual(1, _callCount);
            var newItem = new RedDotObjectHandle(Guid.Empty, "c");
            _objects.Add(newItem);
            list.Refresh();

            Assert.AreEqual(4, list.Count());
            Assert.AreEqual(2, _callCount);

        }

        [TestMethod]
        public void TestRDListWithoutCaching()
        {
            var list = new CachedList<RedDotObjectHandle>(Objects, Caching.Disabled);
            Assert.AreEqual(false, list.IsCachingEnabled);
            Assert.AreEqual(3, list.Count());
            list.Count();
            Assert.AreEqual(2, _callCount);
        }

        [TestMethod]
        public void TestRDListSwitchToCaching()
        {
            var list = new CachedList<RedDotObjectHandle>(Objects, Caching.Disabled);
            list.Count();

            list.IsCachingEnabled = true;
            list.Count();
            list.Count();
            Assert.AreEqual(1, _callCount);
        }

        [TestMethod]
        public void TestRDListSwitchToNonCaching()
        {
            var list = new CachedList<RedDotObjectHandle>(Objects, Caching.Enabled);

            list.Count();
            list.Count();
            list.IsCachingEnabled = false;
            list.Count();
            list.Count();
            Assert.AreEqual(3, _callCount);
        }

        [TestMethod]
        public void TestRDListInvalidateCache()
        {
            var list = new CachedList<RedDotObjectHandle>(Objects, Caching.Enabled);
            list.Count();
            list.Count();
            list.InvalidateCache();
            list.Count();
            list.Count();
            Assert.AreEqual(2, _callCount);
        }

        [TestMethod]
        public void TestRDListRefresh()
        {
            var list = new CachedList<RedDotObjectHandle>(Objects, Caching.Enabled);
            list.Count();
            list.Count();
            list.Refresh();
            Assert.AreEqual(2, _callCount);
            list.Count();
            list.Count();
            Assert.AreEqual(2, _callCount);
        }
        // ReSharper restore ReturnValueOfPureMethodIsNotUsed
    }
}
