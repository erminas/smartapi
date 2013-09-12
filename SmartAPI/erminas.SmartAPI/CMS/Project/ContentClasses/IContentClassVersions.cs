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
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.ContentClasses
{
    internal class ContentClassVersions : RDList<IContentClassVersion>, IContentClassVersions
    {
        private readonly IContentClass _contentClass;

        internal ContentClassVersions(IContentClass contentClass, Caching caching) : base(caching)
        {
            _contentClass = contentClass;
            RetrieveFunc = GetVersions;
        }

        public IContentClass ContentClass
        {
            get { return _contentClass; }
        }

        /// <summary>
        ///     Versioning information for the latest version of the content class.
        /// </summary>
        public IContentClassVersion Current
        {
            get { return this.FirstOrDefault(); }
        }

        public IProject Project
        {
            get { return _contentClass.Project; }
        }

        public ISession Session
        {
            get { return _contentClass.Session; }
        }

        private List<IContentClassVersion> GetVersions()
        {
            const string LIST_VERSIONS =
                @"<PROJECT><TEMPLATE guid=""{0}""><ARCHIVE action=""list""/></TEMPLATE></PROJECT>";

            var xmlDoc = Project.ExecuteRQL(LIST_VERSIONS.RQLFormat(_contentClass));
            var versionNodes = xmlDoc.GetElementsByTagName("VERSION");

            return (from XmlElement curVersion in versionNodes
                    let cc = (IContentClassVersion) new ContentClass.ContentClassVersion(_contentClass, curVersion)
                    orderby cc.Date descending
                    select cc).ToList();
        }
    }

    public interface IContentClassVersions : IRDList<IContentClassVersion>, IProjectObject
    {
        IContentClassVersion Current { get; }
    }
}