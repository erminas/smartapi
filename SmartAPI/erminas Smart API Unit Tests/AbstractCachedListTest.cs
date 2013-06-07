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
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using erminas.SmartAPI.Utils;

namespace erminas_Smart_API_Unit_Tests
{
    [TestClass]
    public class AbstractCachedListTest
    {
        private static readonly List<RedDotObjectHandle> ORIG_OBJECTS = new List<RedDotObjectHandle>
            {
                new RedDotObjectHandle(Guid.Empty, "a"),
                new RedDotObjectHandle(Guid.Empty, "e"),
                new RedDotObjectHandle(Guid.Empty, "b")
            };

        protected int _callCount = 0;
        protected List<RedDotObjectHandle> _objects;

        [TestInitialize]
        public void Setup()
        {
            _callCount = 0;
            _objects = ORIG_OBJECTS.ToList();
        }

        protected List<RedDotObjectHandle> Objects()
        {
            ++_callCount;
            return _objects;
        }
    }
}