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

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Project.Filesystem;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project
{
    public interface IFolders : IIndexedRDList<string, IFolder>, IProjectObject
    {
        IEnumerable<IFolder> ForAssetManager();
    }

    internal class Folders : NameIndexedRDList<IFolder>, IFolders
    {
        private readonly Project _project;

        internal Folders(Project project, Caching caching) : base(caching)
        {
            _project = project;
            RetrieveFunc = GetFolders;
        }

        public Project Project
        {
            get { return _project; }
        }

        public IEnumerable<IFolder> ForAssetManager()
        {
            return this.Where(folder => folder.IsAssetManagerFolder).ToList();
        }

        public Session Session
        {
            get { return _project.Session; }
        }

        private List<IFolder> GetFolders()
        {
            const string LIST_FILE_FOLDERS =
                @"<PROJECT><FOLDERS action=""list"" foldertype=""0"" withsubfolders=""1""/></PROJECT>";
            var xmlDoc = Project.ExecuteRQL(LIST_FILE_FOLDERS);

            return
                (from XmlElement curNode in xmlDoc.GetElementsByTagName("FOLDER")
                 select (IFolder) new Folder(Project, curNode)).ToList();
        }
    }
}