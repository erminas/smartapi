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

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

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
        [ExpectedException(typeof (KeyNotFoundException))]
        public void TestRDIndexedListGetByNameCachingError()
        {
            _listCaching.Get("fail");
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