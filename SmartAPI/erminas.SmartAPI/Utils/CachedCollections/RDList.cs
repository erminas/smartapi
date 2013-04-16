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
    public class RDList<T> : CachedList<T>, IRDList<T> where T : class, IRedDotObject
    {
        public RDList(Func<List<T>> retrieveFunc, Caching caching) : base(retrieveFunc, caching)
        {
        }

        protected RDList(Caching caching) : base(caching)
        {
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
            EnsureListIsLoaded();
            return List.First(x => x.Guid == guid);
        }

        public T GetByName(string name)
        {
            EnsureListIsLoaded();
            return List.First(x => x.Name == name);
        }

        public new IRDList<T> Refreshed()
        {
            Refresh();
            return this;
        }

        public bool TryGetByGuid(Guid guid, out T output)
        {
            EnsureListIsLoaded();
            output = List.Find(x => x.Guid == guid);
            return output != null;
        }

        public bool TryGetByName(string name, out T output)
        {
            EnsureListIsLoaded();
            output = List.Find(x => x.Name == name);
            return output != null;
        }

        public void WaitFor(Predicate<IRDList<T>> predicate, TimeSpan wait, TimeSpan retryPeriod)
        {
            Wait.For(() => predicate(Refreshed()), wait, retryPeriod);
        }

        #endregion
    }
}