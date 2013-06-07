// Smart API - .Net programmatic access to RedDot servers
//  
// Copyright (C) 2013 erminas GbR
// 
// This program is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with this program.
// If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas_Smart_API_Unit_Tests
{
    [TestClass]
    public class CachedListTest : AbstractCachedListTest
    {
        // ReSharper disable ReturnValueOfPureMethodIsNotUsed

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

        // ReSharper restore ReturnValueOfPureMethodIsNotUsed
    }
}