using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using erminas.SmartAPI.CMS;

namespace erminas.SmartAPI.Utils.CachedCollections
{
    public interface IRDEnumerable<T> : IEnumerable<T> where T: class, IRedDotObject
    {

        bool Contains(T element);
        bool ContainsGuid(Guid guid);
        bool ContainsName(string name);

        T GetByGuid(Guid guid);
        bool TryGetByGuid(Guid guid, out T output);

        /// <summary>
        ///     Get the first element with Name == name
        /// </summary>
        T GetByName(string name);

        /// <summary>
        ///     Try to get the first element with Name == name
        /// </summary>
        /// <returns> true, if an element could be found, false otherwise </returns>
        bool TryGetByName(string name, out T output);

        int Count { get; }
    }

    internal static class RDEnumerableWrapper
    {
        internal static IRDEnumerable<T> ToRDEnumerable<T>(this IEnumerable<T> enumerable) where T : class, IRedDotObject
        {
            return new RDEnumerable<T>(enumerable);
        }
    }

    internal class RDEnumerable<T> : IRDEnumerable<T> where T: class, IRedDotObject
    {
        private readonly IEnumerable<T> _wrappedList;

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _wrappedList.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable) _wrappedList).GetEnumerator();
        }

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

        public T GetByGuid(Guid guid)
        {
            return _wrappedList.First(arg => arg.Guid == guid);
        }

        public bool TryGetByGuid(Guid guid, out T output)
        {
            output = _wrappedList.FirstOrDefault(arg => arg.Guid == guid);
            return output != null;
        }

        public T GetByName(string name)
        {
            return _wrappedList.First(arg => arg.Name == name);
        }

        public bool TryGetByName(string name, out T output)
        {
            output = _wrappedList.FirstOrDefault(arg => arg.Name == name);
            return output != null;
        }

        public int Count { get { return _wrappedList.Count(); } }
    }
}