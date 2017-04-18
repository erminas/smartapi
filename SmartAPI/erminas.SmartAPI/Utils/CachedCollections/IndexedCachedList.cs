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
using System.Collections.Generic;
using System.Linq;

namespace erminas.SmartAPI.Utils.CachedCollections
{
    /// <summary>
    ///     Implementation of IIndexedCachedList
    /// </summary>
    public class IndexedCachedList<TK, T> : CachedList<T>, IIndexedCachedList<TK, T> where T : class
    {
        //TODO temporary hack to stay API compatible while still allowing multiple locales with the same LCID
        private readonly bool _allowDuplicates;

        private readonly Func<T, TK> _indexFunc;
        private Dictionary<TK, T> _index = new Dictionary<TK, T>();

        public IndexedCachedList(Func<List<T>> retrieveFunc, Func<T, TK> indexFunc, Caching caching) : base(retrieveFunc, caching)
        {
            _indexFunc = indexFunc;
        }

        protected IndexedCachedList(Func<T, TK> indexFunc, Caching caching) : base(caching)
        {
            _indexFunc = indexFunc;
        }

        //TODO temporary hack to stay API compatible while still allowing multiple locales with the same LCID
        internal IndexedCachedList(Func<List<T>> retrieveFunc, Func<T, TK> indexFunc, Caching caching,
            bool allowDuplicates) : this(retrieveFunc, indexFunc, caching)
        {
            _allowDuplicates = allowDuplicates;
        }

        protected override List<T> List
        {
            set
            {
                base.List = value;
                if (IsCachingEnabled)
                {
                    _index.Clear();
                    if (value != null)
                    {
                        try
                        {
                            _index = value.ToDictionary(_indexFunc);
                        }
                        catch (ArgumentException e)
                        {
                            if (_allowDuplicates)
                            {
                                //TODO temporary hack to stay API compatible while still allowing multiple locales with the same LCID
                                _index = new Dictionary<TK, T>();
                                foreach (var curEntry in value)
                                {
                                    var curKey = _indexFunc(curEntry);
                                    if (!_index.ContainsKey(curKey))
                                    {
                                        _index[curKey] = curEntry;
                                    }
                                }
                                return;
                            }
                            ThrowDuplicatesException(value, e);
                        }
                    }
                }
            }
        }

        private void ThrowDuplicatesException(List<T> value, ArgumentException e)
        {
            var duplicates = value.GroupBy(_indexFunc)
                .Where(g => g.Count() > 1)
                .Select(y => y.Key)
                .ToArray();
            throw new Exception("Duplicate key/s: " + string.Join(", ", duplicates), e);
        }

        #region IIndexedCachedList<TK,T> Members

        public bool ContainsKey(TK key)
        {
            EnsureListIsLoaded();
            return _index.ContainsKey(key);
        }

        public T Get(TK key)
        {
            EnsureListIsLoaded();
            try
            {
                return IsCachingEnabled
                           ? _index[key]
                           : List.First(
                                        x => _indexFunc(x)
                                                 .Equals(key));
            }
            catch (InvalidOperationException e)
            {
                throw new KeyNotFoundException(String.Format("No element with key '{0}' found", key), e);
            }
        }

        public override bool IsCachingEnabled
        {
            set
            {
                if (value && !base.IsCachingEnabled && List != null)
                {
                    try
                    {
                        _index = List.ToDictionary(_indexFunc);
                    }
                    catch (ArgumentException e)
                    {
                        ThrowDuplicatesException(List, e);
                    }
                }
                else
                {
                    if (!value && base.IsCachingEnabled)
                    {
                        _index = null;
                    }
                }

                base.IsCachingEnabled = value;
            }
        }

        public T this[TK key]
        {
            get { return Get(key); }
        }

        public new IIndexedCachedList<TK, T> Refreshed()
        {
            Refresh();
            return this;
        }

        public bool TryGet(TK key, out T obj)
        {
            if (key == null)
            {
                obj = null;
                return false;
            }
            EnsureListIsLoaded();
            if (IsCachingEnabled)
            {
                return _index.TryGetValue(key, out obj);
            }

            obj = List.FirstOrDefault(
                                      x => _indexFunc(x)
                                               .Equals(key));
            return obj != null;
        }

        public void WaitFor(Predicate<IIndexedCachedList<TK, T>> predicate, TimeSpan maxWait, TimeSpan retryPeriod)
        {
            Wait.For(() => predicate(Refreshed()), maxWait, retryPeriod);
        }

        #endregion
    }
}
