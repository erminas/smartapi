using System.Collections.Generic;
using System.Linq;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    public class RecycleBin
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

        private ExtendedPageSearch CreatePageSearchForRecycleBin()
        {
            ExtendedPageSearch search = _project.CreateExtendedPageSearch();
            search.Predicates.Add(new SpecialPagePredicate(SpecialPagePredicate.PageCategoryType.RecycleBin));
            return search;
        }
    }
}