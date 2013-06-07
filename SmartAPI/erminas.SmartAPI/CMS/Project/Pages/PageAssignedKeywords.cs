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
using erminas.SmartAPI.CMS.Project.Keywords;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Pages
{
    internal class PageAssignedKeywords : RDList<IKeyword>, IAssignedKeywords
    {
        private readonly IPage _page;

        internal PageAssignedKeywords(IPage page, Caching caching) : base(caching)
        {
            _page = page;
            RetrieveFunc = GetKeywords;
        }

        public void Add(IKeyword keyword)
        {
            if (ContainsGuid(keyword.Guid))
            {
                return;
            }

            const string ADD_KEYWORD =
                @"<PAGE guid=""{0}"" action=""assign""><KEYWORDS><KEYWORD guid=""{1}"" changed=""1"" /></KEYWORDS></PAGE>";
            _page.Project.ExecuteRQL(ADD_KEYWORD.RQLFormat(_page, keyword), RqlType.SessionKeyInProject);
            //server sends empty reply

            InvalidateCache();
        }

        public void AddRange(IEnumerable<IKeyword> keywords)
        {
            Set(this.Union(keywords));
        }

        public void Clear()
        {
            Set(new IKeyword[0]);
        }

        public void Remove(IKeyword keyword)
        {
            const string DELETE_KEYWORD =
                @"<PROJECT><PAGE guid=""{0}"" action=""unlink""><KEYWORD guid=""{1}"" /></PAGE></PROJECT>";
            _page.Project.ExecuteRQL(DELETE_KEYWORD.RQLFormat(_page, keyword));

            InvalidateCache();
        }

        public void Set(IEnumerable<IKeyword> newKeywords)
        {
            const string SET_KEYWORDS = @"<PAGE guid=""{0}"" action=""assign""><KEYWORDS>{1}</KEYWORDS></PAGE>";
            const string REMOVE_SINGLE_KEYWORD = @"<KEYWORD guid=""{0}"" delete=""1"" changed=""1"" />";
            const string ADD_SINGLE_KEYWORD = @"<KEYWORD guid=""{0}"" changed=""1"" />";

            var newKeywordsAsList = newKeywords as IList<IKeyword> ?? newKeywords.ToList();
            string toRemove = this.Except(newKeywordsAsList)
                                  .Aggregate("", (x, y) => x + REMOVE_SINGLE_KEYWORD.RQLFormat(y));

            string toAdd = newKeywordsAsList.Except(this).Aggregate("", (x, y) => x + ADD_SINGLE_KEYWORD.RQLFormat(y));

            if (string.IsNullOrEmpty(toRemove) && string.IsNullOrEmpty(toAdd))
            {
                return;
            }

            _page.Project.ExecuteRQL(SET_KEYWORDS.RQLFormat(_page, toRemove + toAdd), RqlType.SessionKeyInProject);

            InvalidateCache();
        }

        private List<IKeyword> GetKeywords()
        {
            const string LOAD_KEYWORDS = @"<PROJECT><PAGE guid=""{0}""><KEYWORDS action=""load"" /></PAGE></PROJECT>";
            var xmlDoc = _page.Project.ExecuteRQL(LOAD_KEYWORDS.RQLFormat(_page));
            return
                (from XmlElement curNode in xmlDoc.GetElementsByTagName("KEYWORD")
                 select (IKeyword) new Keyword(_page.Project, curNode)).ToList();
        }
    }
}