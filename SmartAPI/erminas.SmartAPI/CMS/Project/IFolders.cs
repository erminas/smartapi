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
using System.Xml;
using erminas.SmartAPI.CMS.Project.Folder;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project
{
    //public interface IFolders : IIndexedRDList<string, IFolder>, IProjectObject
    //{
    //    IEnumerable<IFolder> ForAssetManager();
    //}

    //internal class Folders : NameIndexedRDList<IFolder>, IFolders
    //{
    //    private readonly IProject _project;

    //    internal Folders(IProject project, Caching caching) : base(caching)
    //    {
    //        _project = project;
    //        RetrieveFunc = GetFolders;
    //    }

    //    public IEnumerable<IFolder> ForAssetManager()
    //    {
    //        return this.Where(folder => folder.IsAssetManagerFolder).ToList();
    //    }

    //    public IProject Project
    //    {
    //        get { return _project; }
    //    }

    //    public ISession Session
    //    {
    //        get { return _project.Session; }
    //    }

    //    private List<IFolder> GetFolders()
    //    {
    //        const string LIST_FILE_FOLDERS =
    //            @"<PROJECT><FOLDERS action=""list"" foldertype=""0"" withsubfolders=""1""/></PROJECT>";
    //        var xmlDoc = Project.ExecuteRQL(LIST_FILE_FOLDERS);

    //        return
    //            (from XmlElement curNode in xmlDoc.GetElementsByTagName("FOLDER")
    //             select (IFolder) new Folder(Project, curNode)).ToList();
    //    }
    //}

    public interface IFolders : IIndexedRDList<string, IFolder>, IProjectObject
    {
        IRDEnumerable<IFolder> AllIncludingSubFolders { get; }
        IRDEnumerable<IAssetManagerFolder> AssetManagerFolders { get; }
    }

    internal class Folders : NameIndexedRDList<IFolder>, IFolders
    {
        private readonly IProject _project;

        internal Folders(IProject project, Caching caching) : base(caching)
        {
            _project = project;
            RetrieveFunc = GetFolders;
        }

        public IRDEnumerable<IFolder> AllIncludingSubFolders
        {
            get
            {
                return
                    this.Union(
                        this.Where(folder => folder is IAssetManagerFolder)
                            .Cast<IAssetManagerFolder>()
                            .SelectMany(folder => folder.SubFolders)).ToRDEnumerable();
            }
        }

        public IRDEnumerable<IAssetManagerFolder> AssetManagerFolders
        {
            get { return this.Where(folder => folder is IAssetManagerFolder).Cast<IAssetManagerFolder>().ToRDEnumerable(); }
        }

        public IFolder GetByGuidIncludingSubFolders(Guid folderGuid)
        {
            return AllIncludingSubFolders.First(folder => folder.Guid == folderGuid);
        }

        public IProject Project
        {
            get { return _project; }
        }

        public ISession Session
        {
            get { return _project.Session; }
        }

        public bool TryGetByGuidIncludingSubFolders(Guid folderGuid, out IFolder folder)
        {
            folder = AllIncludingSubFolders.FirstOrDefault(folder2 => folder2.Guid == folderGuid);
            return folder != null;
        }

        private List<IFolder> GetFolders()
        {
            const string LIST_FILE_FOLDERS = @"<PROJECT><FOLDERS action=""list"" withsubfolders=""0""/></PROJECT>";
            var xmlDoc = Project.ExecuteRQL(LIST_FILE_FOLDERS);

            return (from XmlElement curNode in xmlDoc.GetElementsByTagName("FOLDER")
                    where InternalFolderFactory.HasSupportedFolderType(curNode)
                    select InternalFolderFactory.CreateFolder(_project, curNode)).ToList();
        }
    }
}