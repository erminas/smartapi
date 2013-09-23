// SmartAPI - .Net programmatic access to RedDot servers
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using erminas.SmartAPI.CMS;

namespace erminas.SmartAPI.Utils.CachedCollections
{
    public interface IRDEnumerable<T> : IEnumerable<T> where T : class, IRedDotObject
    {
        bool Contains(T element);
        bool ContainsGuid(Guid guid);
        bool ContainsName(string name);
        int Count { get; }

        T GetByGuid(Guid guid);

        /// <summary>
        ///     Get the first element with Name == name
        /// </summary>
        T GetByName(string name);

        bool TryGetByGuid(Guid guid, out T output);

        /// <summary>
        ///     Try to get the first element with Name == name
        /// </summary>
        /// <returns> true, if an element could be found, false otherwise </returns>
        bool TryGetByName(string name, out T output);
    }

    internal static class RDEnumerableWrapper
    {
        internal static IRDEnumerable<T> ToRDEnumerable<T>(this IEnumerable<T> enumerable)
            where T : class, IRedDotObject
        {
            return new RDEnumerable<T>(enumerable);
        }
    }

    internal class RDEnumerable<T> : IRDEnumerable<T> where T : class, IRedDotObject
    {
        private readonly IEnumerable<T> _wrappedList;

        internal RDEnumerable(IEnumerable<T> wrappedList)
        {
            _wrappedList = wrappedList.ToList();
        }

        public bool Contains(T element)
        {
            return _wrappedList.Contains(element);
        }

        public bool ContainsGuid(Guid guid)
        {
            return _wrappedList.Any(arg => arg.Guid == guid);
        }

        public bool ContainsName(string name)
        {
            return _wrappedList.Any(arg => arg.Name == name);
        }

        public int Count
        {
            get { return _wrappedList.Count(); }
        }

        public T GetByGuid(Guid guid)
        {
            return _wrappedList.First(arg => arg.Guid == guid);
        }

        public T GetByName(string name)
        {
            return _wrappedList.First(arg => arg.Name == name);
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable) _wrappedList).GetEnumerator();
        }

        public bool TryGetByGuid(Guid guid, out T output)
        {
            output = _wrappedList.FirstOrDefault(arg => arg.Guid == guid);
            return output != null;
        }

        public bool TryGetByName(string name, out T output)
        {
            output = _wrappedList.FirstOrDefault(arg => arg.Name == name);
            return output != null;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _wrappedList.GetEnumerator();
        }
    }
}