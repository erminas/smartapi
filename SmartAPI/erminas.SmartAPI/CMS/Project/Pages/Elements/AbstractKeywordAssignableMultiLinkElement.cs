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
    internal class LinkAssignedKeywords : RDList<IKeyword>, IAssignedKeywords
    {
        private readonly AbstractKeywordAssignableMultiLinkElement _parent;

        internal LinkAssignedKeywords(AbstractKeywordAssignableMultiLinkElement parent, Caching caching) : base(caching)
        {
            _parent = parent;
            RetrieveFunc = GetAssignedKeywords;
        }

        public void Add(IKeyword keyword)
        {
            ExecuteAssignKeywords(new []{keyword});

            ExecutePagebuilderLinkCleanup();

            InvalidateCache();
        }

        public void AddRange(IEnumerable<IKeyword> keywords)
        {
            ExecuteAssignKeywords(keywords);

            ExecutePagebuilderLinkCleanup();

            InvalidateCache();
        }

        public void Remove(IKeyword keyword)
        {
            ExecuteUnassignKeywords(new []{keyword});

            ExecutePagebuilderLinkCleanup();

            InvalidateCache();
        }

        public void Clear()
        {
            ExecuteUnassignKeywords(this);

            ExecutePagebuilderLinkCleanup();

            InvalidateCache();
        }

        public void Set(IEnumerable<IKeyword> keywords)
        {
            var currentKeywords = this.ToList();
            var newKeywords = keywords as IList<IKeyword> ?? keywords.ToList();
            ExecuteUnassignKeywords(currentKeywords.Except(newKeywords));
            ExecuteAssignKeywords(newKeywords.Except(currentKeywords));
        }

        private void ExecuteAssignKeywords(IEnumerable<IKeyword> keywords)
        {
            const string ASSING_KEYWORD =
                @"<LINK guid=""{0}"" action=""assign"" allkeywords=""0""><KEYWORDS>{1}</KEYWORDS></LINK>";
            const string SINGLE_KEYWORD = @"<KEYWORD guid=""{0}"" changed=""1"" />";

            var keywordsStr = keywords.Aggregate("", (s, keyword) => s + SINGLE_KEYWORD.RQLFormat(keyword));
            _parent.Project.ExecuteRQL(ASSING_KEYWORD.RQLFormat(_parent, keywordsStr), Project.RqlType.SessionKeyInProject);
        }

        private void ExecutePagebuilderLinkCleanup()
        {
            const string PAGEBUILDER_LINK =
                @"<PAGEBUILDER><LINKING sessionkey=""{0}""><LINKS><LINK guid=""{1}""/></LINKS><PAGES><PAGE sessionkey=""{0}"" guid=""{2}""/></PAGES></LINKING></PAGEBUILDER>";
            _parent.Project.ExecuteRQL(PAGEBUILDER_LINK.RQLFormat(_parent.Project.Session.SessionKey, _parent, _parent.Page));
        }

        private void ExecuteUnassignKeywords(IEnumerable<IKeyword> keywords)
        {
            const string UNASSIGN_KEYWORD =
                @"<LINK guid=""{0}"" action=""assign"" allkeywords=""0""><KEYWORDS>{1}</KEYWORDS></LINK>";

            const string SINGLE_KEYWORD = @"<KEYWORD guid=""{0}"" delete=""1"" changed=""1"" />";

            var keywordsStr = keywords.Aggregate("", (s, keyword) => s + SINGLE_KEYWORD.RQLFormat(keyword));

            _parent.Project.ExecuteRQL(UNASSIGN_KEYWORD.RQLFormat(_parent, keywordsStr), Project.RqlType.SessionKeyInProject);
        }

        private List<IKeyword> GetAssignedKeywords()
        {
            const string LOAD_KEYWORDS = @"<LINK guid=""{0}""><KEYWORDS action=""load""/></LINK>";
            var xmlDoc = _parent.Project.ExecuteRQL(LOAD_KEYWORDS.RQLFormat(_parent), Project.RqlType.SessionKeyInProject);

            var keywords = xmlDoc.SelectNodes("/IODATA/CATEGORIES/CATEGORY/KEYWORDS/KEYWORD");
            return keywords == null
                       ? new List<IKeyword>()
                       : (from XmlElement keyword in keywords select (IKeyword)new Keyword(_parent.Project, keyword)).ToList();
        }
    }

    public abstract class AbstractKeywordAssignableMultiLinkElement : AbstractMultiLinkElement, IKeywordAssignable
    {
        protected AbstractKeywordAssignableMultiLinkElement(Project project, Guid guid, ILanguageVariant languageVariant)
            : base(project, guid, languageVariant)
        {
            Init();
        }

        protected AbstractKeywordAssignableMultiLinkElement(Project project, XmlElement xmlElement)
            : base(project, xmlElement)
        {
            Init();
        }

        

        private void Init()
        {
            AssignedKeywords = new LinkAssignedKeywords(this, Caching.Enabled);
        }

        public IAssignedKeywords AssignedKeywords { get; private set; }
    }
}