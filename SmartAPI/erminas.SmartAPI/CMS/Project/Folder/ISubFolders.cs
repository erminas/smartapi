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

        public ISession Session
        {
            get { return ParentFolder.Session; }
        }

        public bool TryGet(string name, out IAssetManagerFolder obj)
        {
            obj = null;
            return false;
        }

        public void WaitFor(Predicate<IIndexedCachedList<string, IAssetManagerFolder>> predicate, TimeSpan maxWait, TimeSpan retryPeriod)
        {
            if (!predicate(this))
            {
                throw new TimeoutException(
                    string.Format(
                        "Predicate is a contradiction on subfolders of {0} as it cannot contain other subdirectories.",
                        ParentFolder));
            }
        }

        public IIndexedCachedList<string, IAssetManagerFolder> Refreshed()
        {
            return this;
        }

        public void WaitFor(Func<IIndexedRDList<string, IAssetManagerFolder>, bool> predicate, TimeSpan maxWait, TimeSpan retryEverySecond)
        {
            if (!predicate(this))
            {
                throw new TimeoutException(
                    string.Format(
                        "Predicate is a contradiction on subfolders of {0} as it cannot contain other subdirectories.",
                        ParentFolder));
            }
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
    }
}