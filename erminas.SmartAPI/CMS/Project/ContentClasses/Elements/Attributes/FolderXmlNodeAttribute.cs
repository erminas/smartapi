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
using erminas.SmartAPI.CMS.Project.Filesystem;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes
{
    public class FolderXmlNodeAttribute : AbstractGuidXmlNodeAttribute<Folder>
    {
        private readonly ContentClass _contentClass;

        public FolderXmlNodeAttribute(ContentClassElement parent, string name) : this(parent, parent.ContentClass, name)
        {
        }

        private FolderXmlNodeAttribute(IAttributeContainer parent, ContentClass cc, string name)
            : base(cc.Project.Session, (RedDotObject) parent, name)
        {
            _contentClass = cc;
        }

        protected override string GetTypeDescription()
        {
            return "folder";
        }

        protected override Folder RetrieveByGuid(Guid guid)
        {
            return _contentClass.Project.Folders.GetByGuid(guid);
        }

        protected override Folder RetrieveByName(string name)
        {
            return _contentClass.Project.Folders[name];
        }
    }
}