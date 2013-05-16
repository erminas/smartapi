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
using System.Linq;
using erminas.SmartAPI.CMS.Project.Folder;
using erminas.SmartAPI.Exceptions;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes
{
    internal class FolderXmlNodeAttribute : AbstractGuidXmlNodeAttribute<IFolder>
    {
        private readonly IContentClass _contentClass;

        public FolderXmlNodeAttribute(IContentClassElement parent, string name)
            : this(parent, parent.ContentClass, name)
        {
        }

        private FolderXmlNodeAttribute(IAttributeContainer parent, IContentClass cc, string name)
            : base(cc.Project.Session, parent, name)
        {
            _contentClass = cc;
        }

        protected override string GetTypeDescription()
        {
            return "folder";
        }

        protected override IFolder RetrieveByGuid(Guid guid)
        {
            return _contentClass.Project.Folders.AllIncludingSubFolders.First(folder => folder.Guid == guid);
        }

        protected override IFolder RetrieveByName(string name)
        {
            var folders = _contentClass.Project.Folders.AllIncludingSubFolders;
            var folder = folders.FirstOrDefault(folder1 => folder1.Name == name);
            if (folder == null)
            {
                throw new SmartAPIException(_contentClass.Session.ServerLogin,
                                            string.Format("Could not find a folder with name {0} in project {1}", name,
                                                          _contentClass.Project));
            }
            return folder;
        }
    }
}