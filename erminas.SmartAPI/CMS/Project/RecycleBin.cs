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
using erminas.SmartAPI.CMS.Project.Pages;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project
{
    public class RecycleBin : IProjectObject
    {
        private readonly Project _project;

        internal RecycleBin(Project project)
        {
            _project = project;
        }

        public void DeleteAllPages()
        {
            const string DELETE_ALL = @"<PAGES action=""deleteallfinally"" alllanguages=""1""/>";
            _project.ExecuteRQL(DELETE_ALL);
        }

        public void DeleteAllPagesOfLanguageVariant(string language)
        {
            using (new LanguageContext(_project.LanguageVariants[language]))
            {
                const string DELETE_ALL_IN_CURRENT_LANGUAGE = @"<PAGES action=""deleteallfinally"" alllanguages=""0""/>";
                _project.ExecuteRQL(DELETE_ALL_IN_CURRENT_LANGUAGE);
            }
        }

        public bool IsEmpty
        {
            get { return !Pages().Any(); }
        }

        public IEnumerable<Page> Pages()
        {
            List<ResultGroup> searchForPagesExtended = CreatePageSearchForRecycleBin().Execute();
            return searchForPagesExtended[0].Results.Select(pageResult => pageResult.Page);
        }

        public IEnumerable<Page> PagesOfLanguageVariant(string language)
        {
            ExtendedPageSearch search = CreatePageSearchForRecycleBin();
            search.LanguageVariant = _project.LanguageVariants[language];

            IEnumerable<Result> results = search.Execute()[0].Results;
            return results.Select(pageResult => pageResult.Page);
        }

        public Project Project
        {
            get { return _project; }
        }

        public Session Session
        {
            get { return Project.Session; }
        }

        private ExtendedPageSearch CreatePageSearchForRecycleBin()
        {
            ExtendedPageSearch search = _project.Pages.CreateExtendedSearch();
            search.Filters.Add(new SpecialPageFilter(SpecialPageFilter.PageCategoryType.RecycleBin));
            return search;
        }
    }
}