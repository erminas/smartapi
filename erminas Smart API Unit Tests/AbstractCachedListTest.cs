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

        protected List<RedDotObjectHandle> _objects;
        protected int _callCount = 0;

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