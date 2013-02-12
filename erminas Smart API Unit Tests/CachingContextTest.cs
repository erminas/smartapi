using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas_Smart_API_Unit_Tests
{
    [TestClass]
    public class CachingContextTest
    {
        [TestMethod]
        public void TestCachingContext()
        {
            var list = new CachedList<object>(() => new List<object>(), Caching.Enabled);

            using(new CachingContext<object>(list, Caching.Disabled))
            {
                Assert.IsFalse(list.IsCachingEnabled);                
            }
            Assert.IsTrue(list.IsCachingEnabled);

            using (new CachingContext<object>(list, Caching.Enabled))
            {
                Assert.IsTrue(list.IsCachingEnabled);
            }
            Assert.IsTrue(list.IsCachingEnabled);
        }
    }
}
