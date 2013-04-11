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
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.ContentClasses
{
    public interface IContentClasses : IRDList<IContentClass>, IProjectObject
    {
    }

    internal class ContentClasses : RDList<IContentClass>, IContentClasses
    {
        private readonly IProject _project;

        public ContentClasses(IProject project) : base(Caching.Enabled)
        {
            _project = project;
            RetrieveFunc = GetContentClasses;
        }

        public IProject Project
        {
            get { return _project; }
        }

        public Session Session
        {
            get { return _project.Session; }
        }

        private List<IContentClass> GetContentClasses()
        {
            const string LIST_CC_OF_PROJECT = @"<TEMPLATES action=""list""/>";
            XmlDocument xmlDoc = Session.ExecuteRQL(LIST_CC_OF_PROJECT, _project.Guid);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("TEMPLATE");

            return (from XmlElement curNode in xmlNodes
                    select (IContentClass)new ContentClass(_project, curNode.GetGuid()) {Name = curNode.GetAttributeValue("name")})
                .ToList();
        }
    }
}