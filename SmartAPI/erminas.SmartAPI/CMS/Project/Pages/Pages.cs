using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml;
using erminas.SmartAPI.CMS.Project.ContentClasses;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Pages
{
    public class Pages
    {
        private readonly Project _project;

        private readonly Dictionary<string, IndexedRDList<int, Page>> _pagesByLanguage =
            new Dictionary<string, IndexedRDList<int, Page>>();

        public Pages(Project project)
        {
            _project = project;
        }

        /// <summary>
        ///     All pages of the current language variant, indexed by page id. The list is cached by default.
        /// </summary>
        public IndexedRDList<int, Page> OfCurrentLanguage
        {
            get { return GetPagesForLanguageVariant(_project.CurrentLanguageVariant.Language); }
        }

        /// <summary>
        ///     Create a new page in the current language variant and link it.
        /// </summary>
        /// <param name="cc"> Content class of the page </param>
        /// <param name="linkGuid"> Guid of the link the page should be linked to </param>
        /// <param name="headline"> The headline, or null (default) for the default headline </param>
        /// <returns> The newly created (and linked) page </returns>
        public Page CreateAndConnect(ContentClass cc, Guid linkGuid, string headline = null)
        {
            const string CREATE_AND_LINK_PAGE = @"<LINK action=""assign"" guid=""{0}"">{1}</LINK>";
            XmlDocument xmlDoc = _project.ExecuteRQL(string.Format(CREATE_AND_LINK_PAGE, linkGuid.ToRQLString(), PageCreationString(cc, headline)));
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
        ///     Create a new page.
        /// </summary>
        /// <param name="cc"> Content class of the page </param>
        /// <param name="headline"> The headline, or null (default) for the default headline </param>
        /// <returns> The newly created page </returns>
        public Page Create(ContentClass cc, string headline = null)
        {
            XmlDocument xmlDoc = _project.ExecuteRQL(PageCreationString(cc, headline));
            return CreatePageFromCreationReply(xmlDoc);
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
        public IndexedRDList<int, Page> this[string language]
        {
            get { return GetPagesForLanguageVariant(language); }
        }

        private IndexedRDList<int, Page> GetPagesForLanguageVariant(string language)
        {
            LanguageVariant languageVariant = _project.LanguageVariants[language];
            using (new LanguageContext(languageVariant))
            {
                return _pagesByLanguage.GetOrAdd(language, () => new IndexedRDList<int, Page>(() =>
                    {
                        using (new LanguageContext(languageVariant))
                        {
                            return GetPages();
                        }
                    }, x => x.Id, Caching.Enabled));
            }
        }

        /// <summary>
        ///     Convenience function for simple page searches. Creates a PageSearch object, configures it through the configurator parameter and returns the search result.
        /// </summary>
        /// <param name="configurator"> Action to configure the search </param>
        /// <returns> The search results </returns>
        /// <example>
        ///     The following code searches for all pages with headline "test": <code>var results = project.SearchForPages(search => search.Headline="test");</code>
        /// </example>
        public IEnumerable<Page> Search(Action<PageSearch> configurator = null)
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
        /// var results = project.SearchForPagesExtended( 
        ///     search => search.AddPredicate(
        ///          new PageStatusPredicate(PageStatusPredicate.PageStatusType.SavedAsDraft, PageStatusPredicate.UserType.CurrentUser)
        ///     )
        /// );
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

        private Page CreatePageFromCreationReply(XmlDocument xmlDoc)
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

        private static string PageCreationString(ContentClass cc, string headline = null)
        {
            const string PAGE_CREATION_STRING = @"<PAGE action=""addnew"" templateguid=""{0}"" {1}/>";

            string headlineString = headline == null
                                        ? ""
                                        : string.Format(@"headline=""{0}""", HttpUtility.HtmlEncode(headline));
            return string.Format(PAGE_CREATION_STRING, cc.Guid.ToRQLString(), headlineString);
        }

        private List<Page> GetPages()
        {
            const string LIST_PAGES = @"<PROJECT><PAGES action=""list""/></PROJECT>";
            XmlDocument xmlDoc = _project.ExecuteRQL(LIST_PAGES);
            return (from XmlElement curPage in xmlDoc.GetElementsByTagName("PAGE")
                    select
                        new Page(_project, curPage.GetGuid(), _project.CurrentLanguageVariant)
                            {
                                Headline = curPage.GetAttributeValue("headline"),
                                Id = curPage.GetIntAttributeValue("id").GetValueOrDefault()
                            }).ToList();
        }
    }
}