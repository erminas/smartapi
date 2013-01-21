/*
 * Smart API - .Net programatical access to RedDot servers
 * Copyright (C) 2012  erminas GbR 
 *
 * This program is free software: you can redistribute it and/or modify it 
 * under the terms of the GNU General Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details. 
 *
 * You should have received a copy of the GNU General Public License along with this program.
 * If not, see <http://www.gnu.org/licenses/>. 
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace erminas.SmartAPI.Utils
{
    /// <summary>
    ///   Implementation of IIndexedCachedList
    /// </summary>
    public class IndexedCachedList<TK, T> : CachedList<T>, IIndexedCachedList<TK, T> where T : class
    {
        private readonly Func<T, TK> _indexFunc;
        private Dictionary<TK, T> _index = new Dictionary<TK, T>();

        public IndexedCachedList(Func<List<T>> retrieveFunc, Func<T, TK> indexFunc, Caching caching)
            : base(retrieveFunc, caching)
        {
            _indexFunc = indexFunc;
        }

        protected IndexedCachedList(Func<T, TK> indexFunc, Caching caching) : base(caching)
        {
            _indexFunc = indexFunc;
        }

        public new ICachedList<T> Refreshed()
        {
            Refresh();
            return this;
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
                        _index = value.ToDictionary(_indexFunc);
                    }
                }
            }
        }

        #region IIndexedCachedList<TK,T> Members

        public override bool IsCachingEnabled
        {
            set
            {
                if (value && !base.IsCachingEnabled && List != null)
                {
                    _index = List.ToDictionary(_indexFunc);
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

        public T Get(TK key)
        {
            CheckList();
            try
            {
                return IsCachingEnabled ? _index[key] : List.First(x => _indexFunc(x).Equals(key));
            } catch (InvalidOperationException e)
            {
                throw new KeyNotFoundException(String.Format("No element with key '{0}' found", key), e);
            }
        }

        public T this[TK key]
        {
            get { return Get(key); }
        }

        public bool TryGet(TK key, out T obj)
        {
            CheckList();
            if (IsCachingEnabled)
            {
                return _index.TryGetValue(key, out obj);
            }

            obj = List.FirstOrDefault(x => _indexFunc(x).Equals(key));
            return obj != null;
        }

        public bool ContainsKey(TK key)
        {
            CheckList();
            return _index.ContainsKey(key);
        }

        #endregion
    }
}