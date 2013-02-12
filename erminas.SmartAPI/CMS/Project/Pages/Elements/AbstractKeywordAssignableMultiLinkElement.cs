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
using erminas.SmartAPI.CMS.Project.Keywords;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Pages.Elements
{
    public abstract class AbstractKeywordAssignableMultiLinkElement : AbstractMultiLinkElement, IKeywordAssignable
    {
        protected AbstractKeywordAssignableMultiLinkElement(Project project, Guid guid, LanguageVariant languageVariant)
            : base(project, guid, languageVariant)
        {
            Init();
        }

        protected AbstractKeywordAssignableMultiLinkElement(Project project, XmlElement xmlElement)
            : base(project, xmlElement)
        {
            Init();
        }

        public void AssignKeyword(Keyword keyword)
        {
            ExecuteAssignKeyword(keyword);

            ExecutePagebuilderLinkCleanup();

            AssignedKeywords.InvalidateCache();
        }

        public IRDList<Keyword> AssignedKeywords { get; private set; }

        public void UnassignKeyword(Keyword keyword)
        {
            ExecuteUnassignKeyword(keyword);

            ExecutePagebuilderLinkCleanup();

            AssignedKeywords.InvalidateCache();
        }

        private void ExecuteAssignKeyword(Keyword keyword)
        {
            const string ASSING_KEYWORD =
                @"<LINK guid=""{0}"" action=""assign"" allkeywords=""0""><KEYWORDS><KEYWORD guid=""{1}"" changed=""1"" /></KEYWORDS></LINK>";
            Project.ExecuteRQL(ASSING_KEYWORD.RQLFormat(this, keyword), Project.RqlType.SessionKeyInProject);
        }

        private void ExecutePagebuilderLinkCleanup()
        {
            const string PAGEBUILDER_LINK =
                @"<PAGEBUILDER><LINKING sessionkey=""{0}""><LINKS><LINK guid=""{1}""/></LINKS><PAGES><PAGE sessionkey=""{0}"" guid=""{2}""/></PAGES></LINKING></PAGEBUILDER>";
            Project.ExecuteRQL(PAGEBUILDER_LINK.RQLFormat(Project.Session.SessionKey, this, Page));
        }

        private void ExecuteUnassignKeyword(Keyword keyword)
        {
            const string UNASSIGN_KEYWORD =
                @"<LINK guid=""{0}"" action=""assign"" allkeywords=""0""><KEYWORDS><KEYWORD guid=""{1}"" delete=""1"" changed=""1"" /></KEYWORDS></LINK>";
            Project.ExecuteRQL(UNASSIGN_KEYWORD.RQLFormat(this, keyword), Project.RqlType.SessionKeyInProject);
        }

        private List<Keyword> GetAssignedKeywords()
        {
            const string LOAD_KEYWORDS = @"<LINK guid=""{0}""><KEYWORDS action=""load""/></LINK>";
            var xmlDoc = Project.ExecuteRQL(LOAD_KEYWORDS.RQLFormat(this), Project.RqlType.SessionKeyInProject);

            var keywords = xmlDoc.SelectNodes("/IODATA/CATEGORIES/CATEGORY/KEYWORDS/KEYWORD");
            return keywords == null
                       ? new List<Keyword>()
                       : (from XmlElement keyword in keywords select new Keyword(Project, keyword)).ToList();
        }

        private void Init()
        {
            AssignedKeywords = new RDList<Keyword>(GetAssignedKeywords, Caching.Enabled);
        }
    }
}