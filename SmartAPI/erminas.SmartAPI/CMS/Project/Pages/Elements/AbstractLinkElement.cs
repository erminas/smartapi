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
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Pages.Elements
{
    internal abstract class AbstractLinkElement : PageElement, ILinkElement
    {
        private LinkType _linkType;

        protected AbstractLinkElement(IProject project, Guid guid, ILanguageVariant languageVariant)
            : base(project, guid, languageVariant)
        {
            Connections = new LinkConnections(this, Caching.Enabled);
            ReferencedFrom = new RDList<ILinkElement>(GetReferencingLinks, Caching.Enabled);
        }

        protected AbstractLinkElement(IProject project, XmlElement xmlElement) : base(project, xmlElement)
        {
            Connections = new LinkConnections(this, Caching.Enabled);
            ReferencedFrom = new RDList<ILinkElement>(GetReferencingLinks, Caching.Enabled);
            LoadXml();
        }

        public ILinkConnections Connections { get; protected set; }

        public LinkType LinkType
        {
            get { return LazyLoad(ref _linkType); }
        }

        public IRDList<ILinkElement> ReferencedFrom { get; private set; }

        protected abstract void LoadWholeLinkElement();

        protected override sealed void LoadWholePageElement()
        {
            LoadXml();
            LoadWholeLinkElement();
        }

        private List<ILinkElement> GetReferencingLinks()
        {
            const string LIST_REFERENCES = @"<REFERENCE action=""list"" guid=""{0}"" />";
            XmlDocument xmlDoc = Project.ExecuteRQL(LIST_REFERENCES.RQLFormat(this), RqlType.SessionKeyInProject);

            //theoretically through an anchor the language variant of the target could be changed, but this is also not considered in the SmartTree,
            //so we ignore it, to be consistent with the SmartTree.
            return (from XmlElement curLink in xmlDoc.GetElementsByTagName("LINK")
                    select (ILinkElement) CreateElement(Project, curLink.GetGuid(), LanguageVariant)).ToList();
        }

        private void LoadXml()
        {
            InitIfPresent(ref _linkType, "islink", x => (LinkType) int.Parse(x));
        }
    }
}