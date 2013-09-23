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
using System.Xml;
using erminas.SmartAPI.CMS.Project;
using erminas.SmartAPI.CMS.Project.Folder;

namespace erminas.SmartAPI.CMS.Converter
{
    internal class ContentClassFolderConverter : AbstractGuidElementConverter<IContentClassFolder>
    {
        public override bool IsReadOnly
        {
            get { return true; }
        }

        protected override IContentClassFolder GetFromGuid(IProjectObject parent, XmlElement element,
                                                           RedDotAttribute attribute, Guid guid)
        {
            return
                parent.Project.ContentClassFolders.Union(parent.Project.ContentClassFolders.Broken)
                      .First(folder => folder.Guid == guid);
        }

        protected override IContentClassFolder GetFromName(IProjectObject parent, IXmlReadWriteWrapper element,
                                                           RedDotAttribute attribute, IContentClassFolder value)
        {
            throw new NotImplementedException();
        }
    }
}