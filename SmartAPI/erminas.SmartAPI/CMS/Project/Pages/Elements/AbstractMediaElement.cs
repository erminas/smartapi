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
using System.Linq;
using System.Web;
using System.Xml;
using erminas.SmartAPI.CMS.Project.Folder;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.Pages.Elements
{
    public interface IMediaElementBase : IValueElement<IFile>
    {
    }

    internal abstract class AbstractMediaElement : PageElement, IMediaElementBase
    {
        private IFile _file;

        protected AbstractMediaElement(IProject project, Guid guid, ILanguageVariant languageVariant)
            : base(project, guid, languageVariant)
        {
        }

        protected AbstractMediaElement(IProject project, XmlElement xmlElement) : base(project, xmlElement)
        {
            LoadWholePageElement();
        }

        public void Commit()
        {
            const string COMMIT =
                @"<ELT action=""save"" reddotcacheguid="""" guid=""{0}"" value=""{1}"" {2} extendedinfo=""""></ELT>";

            string rqlStr = Value == null
                                ? string.Format(COMMIT, Guid.ToRQLString(), RQL.SESSIONKEY_PLACEHOLDER, "")
                                : string.Format(COMMIT, Guid.ToRQLString(), HttpUtility.HtmlEncode(Value.Name),
                                                IsFileInSubFolder ? "subdirguid=\"{0}\"".RQLFormat(Value.Folder) : "");

            Project.ExecuteRQL(rqlStr);
        }

        public IFile Value
        {
            get { return LazyLoad(ref _file); }
            set { _file = value; }
        }

        protected override sealed void LoadWholePageElement()
        {
            var folder = GetFolder();
            if (folder == null)
            {
                return;
            }
            InitFileValue(folder);
        }

        private IFolder GetFolder()
        {
            Guid folderGuid;
            if (!XmlElement.TryGetGuid("folderguid", out folderGuid))
            {
                _file = null;
                return null;
            }

            Guid subFolderGuid;
            var folderList = Project.Folders.AllIncludingSubFolders;
            return XmlElement.TryGetGuid("subdirguid", out subFolderGuid)
                       ? folderList.FirstOrDefault(folder => folder.Guid == subFolderGuid)
                       : folderList.FirstOrDefault(folder => folder.Guid == folderGuid);
        }

        private void InitFileValue(IFolder folder)
        {
            var fileName = XmlElement.GetAttributeValue("value");
            if (string.IsNullOrEmpty(fileName))
            {
                _file = null;
                return;
            }

            var files = folder.Files.GetByNamePattern(fileName);
            _file = files.FirstOrDefault(file => file.Name == fileName);
        }

        private bool IsFileInSubFolder
        {
            get
            {
                EnsureInitialization();
                if (Value == null)
                {
                    return false;
                }
                var folderGuid = XmlElement.GetGuid("folderguid");
                return folderGuid != Value.Folder.Guid;
            }
        }
    }
}