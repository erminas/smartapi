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

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Folder
{
    internal class SubFolders : NameIndexedRDList<IAssetManagerFolder>, ISubFolders
    {
        private readonly IAssetManagerFolder _folder;

        internal SubFolders(IAssetManagerFolder folder, Caching caching) : base(caching)
        {
            _folder = folder;
            RetrieveFunc = GetSubFolders;
        }

        public override void InvalidateCache()
        {
            ((Project) Project).AllFoldersXmlDocument = null;
            base.InvalidateCache();
        }

        public IAssetManagerFolder ParentFolder
        {
            get { return _folder; }
        }

        public IProject Project
        {
            get { return _folder.Project; }
        }

        public ISession Session
        {
            get { return _folder.Session; }
        }

        public new ISubFolders Refreshed()
        {
            Refresh();
            return this;
        }

        private List<IAssetManagerFolder> GetSubFolders()
        {
            if (((Project) Project).AllFoldersXmlDocument == null)
            {
                const string LOAD_FOLDERS =
                    @"<PROJECT><FOLDERS action=""list"" foldertype=""0"" withsubfolders=""1""/></PROJECT>";
                ((Project) Project).AllFoldersXmlDocument = Project.ExecuteRQL(LOAD_FOLDERS);
            }

            var parentFolderXPath = "//FOLDERS/FOLDER[@guid='{0}']".RQLFormat(ParentFolder);
            var parentNode = ((Project) Project).AllFoldersXmlDocument.SelectSingleNode(parentFolderXPath);
            if (parentNode == null)
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not load subfolders of {0}", _folder));
            }

            var subFolders = parentNode.SelectNodes(".//SUBFOLDER");
            return
                (from XmlElement curSubNode in subFolders
                 select (IAssetManagerFolder) new AssetManagerFolder(_folder, curSubNode)).ToList();
        }
    }
}