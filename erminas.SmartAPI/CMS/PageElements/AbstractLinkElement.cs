/*
 * Smart API - .Net programatical access to RedDot servers
 * Copyright (C) 2012  erminas GbR 
 *
 * This program is free software: you can redistribute it and/or modify it 
 * under the terms of the GNU General Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details. 
 *
 * You should have received a copy of the GNU General Public License along with this program.
 * If not, see <http://www.gnu.org/licenses/>. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Utils;
using erminas.Utilities;

namespace erminas.SmartAPI.CMS.PageElements
{
    public abstract class AbstractLinkElement : PageElement, ILinkElement
    {
        public readonly NameIndexedRDList<Page> LinkedPages;

        protected AbstractLinkElement(Project project, Guid guid) : base(project, guid)
        {
            LinkedPages = new NameIndexedRDList<Page>(GetLinkedPages, Caching.Enabled);
        }

        protected AbstractLinkElement(Project project, XmlNode node) : base(project, node)
        {
            LinkedPages = new NameIndexedRDList<Page>(GetLinkedPages, Caching.Enabled);
        }

        #region ILinkElement Members

        public void LinkToPage(Page page)
        {
            const string LINK_TO_PAGE =
                @"<PAGE><LINK action=""reference"" guid=""{0}""><PAGE guid=""{1}"" /></LINK></PAGE>";
            //we can't really check the success, because an empty iodata element is returned on success as on (at least some) errors
            Project.ExecuteRQL(string.Format(LINK_TO_PAGE, Guid.ToRQLString(), page.Guid));
            LinkedPages.InvalidateCache();
        }

        public void DisconnectPage(Page page)
        {
            DisconnectPages(new List<Page> {page});
        }

        public void LinkToElement(PageElement element)
        {
            const string LINK_TO_PAGE =
                @"<PAGE><LINK action=""assign"" guid=""{0}""><LINK guid=""{1}"" /></LINK></PAGE>";
            //we can't really check the success, because an empty iodata element is returned on success as on (at least some) errors
            Project.ExecuteRQL(string.Format(LINK_TO_PAGE, Guid.ToRQLString(), element.Guid));
            LinkedPages.InvalidateCache();
        }

        #endregion

        protected void DisconnectPages(IEnumerable<Page> pages)
        {
            const string DISCONNECT_PAGES = @"<LINK action=""save"" guid=""{0}""><PAGES>{1}</PAGES></LINK>";
            const string SINGLE_PAGE = @"<PAGE deleted=""1"" guid=""{0}"" />";

            string pagesStr = pages.Aggregate("",
                                              (x, page) => x = x + string.Format(SINGLE_PAGE, page.Guid.ToRQLString()));
            Project.ExecuteRQL(String.Format(DISCONNECT_PAGES, Guid.ToRQLString(), pagesStr));
            LinkedPages.InvalidateCache();
        }

        private List<Page> GetLinkedPages()
        {
            const string LIST_LINKED_PAGES = @"<LINK guid=""{0}""><PAGES action=""list"" /></LINK>";
            XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(LIST_LINKED_PAGES, Guid.ToRQLString()));
            return (from XmlElement curPage in xmlDoc
                    let page = new Page(Project, curPage.GetGuid()) {Headline = curPage.GetAttributeValue("headline")}
                    select page).ToList();
        }
    }
}