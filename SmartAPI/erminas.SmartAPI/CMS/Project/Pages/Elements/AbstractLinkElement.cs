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
            ConnectedPages = new RDList<IPage>(GetLinkedPages, Caching.Enabled);
            ReferencedBy = new RDList<ILinkElement>(GetReferencingLinks, Caching.Enabled);
        }

        protected AbstractLinkElement(IProject project, XmlElement xmlElement) : base(project, xmlElement)
        {
            ConnectedPages = new RDList<IPage>(GetLinkedPages, Caching.Enabled);
            ReferencedBy = new RDList<ILinkElement>(GetReferencingLinks, Caching.Enabled);
            LoadXml();
        }

        public void Connect(IPage page)
        {
            const string CONNECT_PREPARE =
                @"<LINK action=""save"" reddotcacheguid="""" guid=""{0}"" value=""" + Session.SESSIONKEY_PLACEHOLDER +
                @""" />";

            Project.ExecuteRQL(CONNECT_PREPARE.RQLFormat(this));

            const string CONNECT =
                @"<LINKSFROM action=""save"" pageid="""" pageguid=""{0}"" reddotcacheguid=""""><LINK guid=""{1}""/></LINKSFROM>";

            Project.ExecuteRQL(CONNECT.RQLFormat(page, this));
        }

        /// <summary>
        ///     All pages connected to this link.
        ///     Theoretically the language variant of the target page could be different through a language change in an anchor.
        ///     BUT, this is not considered here (and it is not considered in the SmartTree, too), so alle pages are
        ///     of the same language variant as this page.
        /// </summary>
        public IRDList<IPage> ConnectedPages { get; private set; }

        public void DeleteReference()
        {
            Reference(null);
        }

        public void Disconnect(IPage page)
        {
            DisconnectPages(new List<IPage> {page});
        }

        public bool IsReference
        {
            get { return LinkType == LinkType.Reference; }
        }

        public LinkType LinkType
        {
            get { return LazyLoad(ref _linkType); }
        }

        public void Reference(ILinkTarget target)
        {
            if (target == null)
            {
                UnlinkReference();
            }
            else
            {
                if (target is IPage)
                {
                    ReferencePage((IPage) target);
                }
                else
                {
                    ReferenceElement((ILinkElement) target);
                }
                ConnectedPages.InvalidateCache();
            }
        }

        public IRDList<ILinkElement> ReferencedBy { get; private set; }
        
        protected void DisconnectPages(IEnumerable<IPage> pages)
        {
            const string DISCONNECT_PAGES = @"<LINK action=""save"" guid=""{0}""><PAGES>{1}</PAGES></LINK>";
            const string SINGLE_PAGE = @"<PAGE deleted=""1"" guid=""{0}"" />";

            string pagesStr = pages.Aggregate("", (x, page) => x + string.Format(SINGLE_PAGE, page.Guid.ToRQLString()));
            Project.ExecuteRQL(String.Format(DISCONNECT_PAGES, Guid.ToRQLString(), pagesStr));
            ConnectedPages.InvalidateCache();
        }

        protected abstract void LoadWholeLinkElement();

        protected override sealed void LoadWholePageElement()
        {
            LoadXml();
            LoadWholeLinkElement();
        }

        private List<IPage> GetLinkedPages()
        {
            const string LIST_LINKED_PAGES = @"<LINK guid=""{0}""><PAGES action=""list"" /></LINK>";
            XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(LIST_LINKED_PAGES, Guid.ToRQLString()));
            return (from XmlElement curPage in xmlDoc.GetElementsByTagName("PAGE")
                    let page =
                        (IPage)
                        new Page(Project, curPage.GetGuid(), LanguageVariant)
                            {
                                Id = curPage.GetIntAttributeValue("id").GetValueOrDefault(),
                                Headline = curPage.GetAttributeValue("headline")
                            }
                    select page).ToList();
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

        private void ReferenceElement(ILinkElement element)
        {
            const string LINK_TO_ELEMENT =
                @"<PAGE><LINK action=""assign"" guid=""{0}""><LINK guid=""{1}"" /></LINK></PAGE>";
            //we can't really check the success, because an empty iodata element is returned on success as on (at least some) errors
            Project.ExecuteRQL(LINK_TO_ELEMENT.RQLFormat(this, element));
            ConnectedPages.InvalidateCache();
        }

        private void ReferencePage(IPage target)
        {
            const string LINK_TO_PAGE =
                @"<PAGE><LINK action=""reference"" guid=""{0}""><PAGE guid=""{1}"" /></LINK></PAGE>";
            //we can't really check the success, because an empty iodata element is returned on success as on (at least some) errors
            Project.ExecuteRQL(LINK_TO_PAGE.RQLFormat(this, target));
        }

        private void UnlinkReference()
        {
            const string UNLINK_ELEMENT =
                @"<LINK guid=""{0}""><LINK action=""unlink"" reddotcacheguid=""""/><URL action=""unlink""/></LINK>";
            //we can't really check the success, because an empty iodata element is returned on success as on (at least some) errors
            Project.ExecuteRQL(UNLINK_ELEMENT.RQLFormat(this));
        }
    }
}