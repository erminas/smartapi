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
using System.Web;
using System.Xml;
using erminas.SmartAPI.CMS.Project.ContentClasses;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Pages
{
    public interface IPages
    {
        /// <summary>
        ///     Create a new page.
        /// </summary>
        /// <param name="cc"> Content class of the page </param>
        /// <param name="headline"> The headline, or null (default) for the default headline </param>
        /// <returns> The newly created page </returns>
        IPage Create(ContentClass cc, string headline = null);

        /// <summary>
        ///     Create a new page in the current language variant and link it.
        /// </summary>
        /// <param name="cc"> Content class of the page </param>
        /// <param name="linkGuid"> Guid of the link the page should be linked to </param>
        /// <param name="headline"> The headline, or null (default) for the default headline </param>
        /// <returns> The newly created (and linked) page </returns>
        IPage CreateAndConnect(ContentClass cc, Guid linkGuid, string headline = null);

        /// <summary>
        ///     Create an extended page search on this project.
        /// </summary>
        /// <see cref="Pages.CreateSearch" />
        ExtendedPageSearch CreateExtendedSearch();

        /// <summary>
        ///     Create a simple page search on this project.
        /// </summary>
        /// <see cref="Pages.CreateExtendedSearch" />
        PageSearch CreateSearch();

        /// <summary>
        ///     All pages of the a specific language variant, indexed by page id. The list is cached by default.
        /// </summary>
        IndexedRDList<int, IPage> this[string language] { get; }

        /// <summary>
        ///     All pages of the current language variant, indexed by page id. The list is cached by default.
        /// </summary>
        IndexedRDList<int, IPage> OfCurrentLanguage { get; }

        /// <summary>
        ///     Convenience function for simple page searches. Creates a PageSearch object, configures it through the configurator parameter and returns the search result.
        /// </summary>
        /// <param name="configurator"> Action to configure the search </param>
        /// <returns> The search results </returns>
        /// <example>
        ///     The following code searches for all pages with headline "test": <code>var results = project.SearchForPages(search => search.Headline="test");</code>
        /// </example>
        IEnumerable<IPage> Search(Action<PageSearch> configurator = null);

        /// <summary>
        ///     Convenience funtion for extended page searches. Creates a new PageSearchExtended object which gets configured through the configurator parameter and returns the result of the search.
        /// </summary>
        /// <param name="configurator"> An action to configure the search </param>
        /// <returns> The search results </returns>
        /// <example>
        ///     The following code searches for all pages saved as draft by the current user:
        ///     <code>
        /// <pre>
        ///     var results = project.SearchForPagesExtended(
        ///     search => search.AddPredicate(
        ///     new PageStatusPredicate(PageStatusPredicate.PageStatusType.SavedAsDraft, PageStatusPredicate.UserType.CurrentUser)
        ///     )
        ///     );
        /// </pre>
        ///      </code>
        /// </example>
        List<ResultGroup> SearchExtended(Action<ExtendedPageSearch> configurator = null);
    }

    public class Pages : IPages
    {
        private readonly Dictionary<string, IndexedRDList<int, IPage>> _pagesByLanguage =
            new Dictionary<string, IndexedRDList<int, IPage>>();

        private readonly Project _project;

        public Pages(Project project)
        {
            _project = project;
        }

        /// <summary>
        ///     Create a new page.
        /// </summary>
        /// <param name="cc"> Content class of the page </param>
        /// <param name="headline"> The headline, or null (default) for the default headline </param>
        /// <returns> The newly created page </returns>
        public IPage Create(ContentClass cc, string headline = null)
        {
            XmlDocument xmlDoc = _project.ExecuteRQL(PageCreationString(cc, headline));
            return CreatePageFromCreationReply(xmlDoc);
        }

        /// <summary>
        ///     Create a new page in the current language variant and link it.
        /// </summary>
        /// <param name="cc"> Content class of the page </param>
        /// <param name="linkGuid"> Guid of the link the page should be linked to </param>
        /// <param name="headline"> The headline, or null (default) for the default headline </param>
        /// <returns> The newly created (and linked) page </returns>
        public IPage CreateAndConnect(ContentClass cc, Guid linkGuid, string headline = null)
        {
            const string CREATE_AND_LINK_PAGE = @"<LINK action=""assign"" guid=""{0}"">{1}</LINK>";
            XmlDocument xmlDoc =
                _project.ExecuteRQL(string.Format(CREATE_AND_LINK_PAGE, linkGuid.ToRQLString(),
                                                  PageCreationString(cc, headline)));
            return CreatePageFromCreationReply(xmlDoc);
        }

        /// <summary>
        ///     Create an extended page search on this project.
        /// </summary>
        /// <see cref="CreateSearch" />
        public ExtendedPageSearch CreateExtendedSearch()
        {
            return new ExtendedPageSearch(_project);
        }

        /// <summary>
        ///     Create a simple page search on this project.
        /// </summary>
        /// <see cref="CreateExtendedSearch" />
        public PageSearch CreateSearch()
        {
            return new PageSearch(_project);
        }

        /// <summary>
        ///     All pages of the a specific language variant, indexed by page id. The list is cached by default.
        /// </summary>
        public IndexedRDList<int, IPage> this[string language]
        {
            get { return GetPagesForLanguageVariant(language); }
        }

        /// <summary>
        ///     All pages of the current language variant, indexed by page id. The list is cached by default.
        /// </summary>
        public IndexedRDList<int, IPage> OfCurrentLanguage
        {
            get { return GetPagesForLanguageVariant(_project.LanguageVariants.Current.Abbreviation); }
        }

        /// <summary>
        ///     Convenience function for simple page searches. Creates a PageSearch object, configures it through the configurator parameter and returns the search result.
        /// </summary>
        /// <param name="configurator"> Action to configure the search </param>
        /// <returns> The search results </returns>
        /// <example>
        ///     The following code searches for all pages with headline "test": <code>var results = project.SearchForPages(search => search.Headline="test");</code>
        /// </example>
        public IEnumerable<IPage> Search(Action<PageSearch> configurator = null)
        {
            var search = new PageSearch(_project);
            if (configurator != null)
            {
                configurator(search);
            }

            return search.Execute();
        }

        /// <summary>
        ///     Convenience funtion for extended page searches. Creates a new PageSearchExtended object which gets configured through the configurator parameter and returns the result of the search.
        /// </summary>
        /// <param name="configurator"> An action to configure the search </param>
        /// <returns> The search results </returns>
        /// <example>
        ///     The following code searches for all pages saved as draft by the current user:
        ///     <code>
        /// <pre>
        ///     var results = project.SearchForPagesExtended(
        ///     search => search.AddPredicate(
        ///     new PageStatusPredicate(PageStatusPredicate.PageStatusType.SavedAsDraft, PageStatusPredicate.UserType.CurrentUser)
        ///     )
        ///     );
        /// </pre>
        ///      </code>
        /// </example>
        public List<ResultGroup> SearchExtended(Action<ExtendedPageSearch> configurator = null)
        {
            var search = new ExtendedPageSearch(_project);
            if (configurator != null)
            {
                configurator(search);
            }

            return search.Execute();
        }

        private IPage CreatePageFromCreationReply(XmlDocument xmlDoc)
        {
            try
            {
                var pageItem = (XmlElement) xmlDoc.GetElementsByTagName("PAGE")[0];
                return new Page(_project, pageItem);
            } catch (Exception e)
            {
                throw new SmartAPIException(_project.Session.ServerLogin, "Could not create page", e);
            }
        }

        private List<IPage> GetPages()
        {
            const string LIST_PAGES = @"<PROJECT><PAGES action=""list""/></PROJECT>";
            XmlDocument xmlDoc = _project.ExecuteRQL(LIST_PAGES);
            return (from XmlElement curPage in xmlDoc.GetElementsByTagName("PAGE")
                    select
                        (IPage)new Page(_project, curPage.GetGuid(), _project.LanguageVariants.Current)
                            {
                                Headline = curPage.GetAttributeValue("headline"),
                                Id = curPage.GetIntAttributeValue("id").GetValueOrDefault()
                            }).ToList();
        }

        private IndexedRDList<int, IPage> GetPagesForLanguageVariant(string language)
        {
            ILanguageVariant languageVariant = _project.LanguageVariants[language];
            using (new LanguageContext(languageVariant))
            {
                return _pagesByLanguage.GetOrAdd(language, () => new IndexedRDList<int, IPage>(() =>
                    {
                        using (new LanguageContext(languageVariant))
                        {
                            return GetPages();
                        }
                    }, x => x.Id, Caching.Enabled));
            }
        }

        private static string PageCreationString(ContentClass cc, string headline = null)
        {
            const string PAGE_CREATION_STRING = @"<PAGE action=""addnew"" templateguid=""{0}"" {1}/>";

            string headlineString = headline == null
                                        ? ""
                                        : string.Format(@"headline=""{0}""", HttpUtility.HtmlEncode(headline));
            return string.Format(PAGE_CREATION_STRING, cc.Guid.ToRQLString(), headlineString);
        }
    }
}