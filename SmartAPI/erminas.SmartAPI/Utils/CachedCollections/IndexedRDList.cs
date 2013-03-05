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
using erminas.SmartAPI.CMS;

namespace erminas.SmartAPI.Utils.CachedCollections
{
    public class IndexedRDList<TK, T> : IndexedCachedList<TK, T>, IRDList<T> where T : class, IRedDotObject
    {
        public IndexedRDList(Func<List<T>> retrieveFunc, Func<T, TK> indexFunc, Caching caching)
            : base(retrieveFunc, indexFunc, caching)
        {
        }

        protected IndexedRDList(Func<T, TK> indexFunc, Caching caching) : base(indexFunc, caching)
        {
        }

        public new ICachedList<T> Refreshed()
        {
            Refresh();
            return this;
        }

        #region IRDList<T> Members

        public bool ContainsGuid(Guid guid)
        {
            T tmp;
            return TryGetByGuid(guid, out tmp);
        }

        public bool ContainsName(string name)
        {
            T tmp;
            return TryGetByName(name, out tmp);
        }

        public T GetByGuid(Guid guid)
        {
            CheckList();
            return List.First(x => x.Guid == guid);
        }

        public virtual T GetByName(string name)
        {
            CheckList();
            return List.First(x => x.Name == name);
        }

        public bool TryGetByGuid(Guid guid, out T output)
        {
            CheckList();
            output = List.FirstOrDefault(x => x.Guid == guid);
            return output != null;
        }

        public virtual bool TryGetByName(string name, out T output)
        {
            CheckList();
            output = List.FirstOrDefault(x => x.Name == name);
            return output != null;
        }

        #endregion
    }

    /// <summary>
    ///     Convenience class for a IndexedRDList with an index on the Name attribute GetByName and TryGetByName both use the index to access the elements.
    /// </summary>
    /// <typeparam name="T"> TypeId of the stored elements </typeparam>
    public class NameIndexedRDList<T> : IndexedRDList<String, T> where T : class, IRedDotObject
    {
        public NameIndexedRDList(Func<List<T>> retrieveFunc, Caching caching) : base(retrieveFunc, x => x.Name, caching)
        {
        }

        protected NameIndexedRDList(Caching caching) : base(x => x.Name, caching)
        {
        }

        public override T GetByName(string name)
        {
            return this[name];
        }

        public new NameIndexedRDList<T> Refreshed()
        {
            Refresh();
            return this;
        }

        public override bool TryGetByName(string name, out T output)
        {
            return TryGet(name, out output);
        }
    }
}