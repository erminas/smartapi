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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Folder
{
    internal class EmptySubFolders : ISubFolders
    {
        internal EmptySubFolders(IAssetManagerFolder parentFolder)
        {
            ParentFolder = parentFolder;
        }

        public bool Contains(IAssetManagerFolder element)
        {
            return false;
        }

        public bool ContainsGuid(Guid guid)
        {
            return false;
        }

        public bool ContainsKey(string key)
        {
            return false;
        }

        public bool ContainsName(string name)
        {
            return false;
        }

        public int Count
        {
            get { return 0; }
        }

        public IAssetManagerFolder Get(string key)
        {
            throw new SmartAPIException(Session.ServerLogin, "Subfolders cannot contain other subfolders");
        }

        public IAssetManagerFolder GetByGuid(Guid guid)
        {
            throw new SmartAPIException(Session.ServerLogin, "Subfolders cannot contain other subfolders");
        }

        public IAssetManagerFolder GetByName(string name)
        {
            throw new SmartAPIException(Session.ServerLogin, "Subfolders cannot contain other subfolders");
        }

        public IAssetManagerFolder GetByPosition(int pos)
        {
            throw new SmartAPIException(Session.ServerLogin, "Subfolders cannot contain other subfolders");
        }

        public IEnumerator<IAssetManagerFolder> GetEnumerator()
        {
            return Enumerable.Empty<IAssetManagerFolder>().GetEnumerator();
        }

        public void InvalidateCache()
        {
        }

        public bool IsCachingEnabled { get; set; }

        public IAssetManagerFolder this[string key]
        {
            get { throw new SmartAPIException(Session.ServerLogin, "Subfolders cannot contain other subfolders"); }
        }

        public IAssetManagerFolder ParentFolder { get; private set; }

        public IProject Project
        {
            get { return ParentFolder.Project; }
        }

        public void Refresh()
        {
        }

        public IIndexedCachedList<string, IAssetManagerFolder> Refreshed()
        {
            return this;
        }

        public ISession Session
        {
            get { return ParentFolder.Session; }
        }

        ISubFolders ISubFolders.Refreshed()
        {
            return this;
        }

        public bool TryGet(string name, out IAssetManagerFolder obj)
        {
            obj = null;
            return false;
        }

        public bool TryGetByGuid(Guid guid, out IAssetManagerFolder output)
        {
            output = null;
            return false;
        }

        public bool TryGetByName(string name, out IAssetManagerFolder output)
        {
            output = null;
            return false;
        }

        public void WaitFor(Predicate<IIndexedCachedList<string, IAssetManagerFolder>> predicate, TimeSpan maxWait,
                            TimeSpan retryPeriod)
        {
            if (!predicate(this))
            {
                throw new TimeoutException(
                    string.Format(
                        "Predicate is a contradiction on subfolders of {0} as it cannot contain other subdirectories.",
                        ParentFolder));
            }
        }

        public void WaitFor(Func<IIndexedRDList<string, IAssetManagerFolder>, bool> predicate, TimeSpan maxWait,
                            TimeSpan retryEverySecond)
        {
            if (!predicate(this))
            {
                throw new TimeoutException(
                    string.Format(
                        "Predicate is a contradiction on subfolders of {0} as it cannot contain other subdirectories.",
                        ParentFolder));
            }
        }

        public void WaitFor(Predicate<IRDList<IAssetManagerFolder>> predicate, TimeSpan wait, TimeSpan retryPeriod)
        {
            if (!predicate(this))
            {
                throw new TimeoutException(
                    string.Format(
                        "Predicate is a contradiction on subfolders of {0} as it cannot contain other subdirectories.",
                        ParentFolder));
            }
        }

        public void WaitFor(Predicate<ICachedList<IAssetManagerFolder>> predicate, TimeSpan wait, TimeSpan retryPeriod)
        {
            if (!predicate(this))
            {
                throw new TimeoutException(
                    string.Format(
                        "Predicate is a contradiction on subfolders of {0} as it cannot contain other subdirectories.",
                        ParentFolder));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IRDList<IAssetManagerFolder> IRDList<IAssetManagerFolder>.Refreshed()
        {
            return this;
        }

        IIndexedRDList<string, IAssetManagerFolder> IIndexedRDList<string, IAssetManagerFolder>.Refreshed()
        {
            return this;
        }

        ICachedList<IAssetManagerFolder> ICachedList<IAssetManagerFolder>.Refreshed()
        {
            return this;
        }
    }

    public interface ISubFolders : IIndexedRDList<string, IAssetManagerFolder>, IProjectObject
    {
        IAssetManagerFolder ParentFolder { get; }
        new ISubFolders Refreshed();
    }
}