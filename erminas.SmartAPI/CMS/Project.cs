// Smart API - .Net programatical access to RedDot servers
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
using System.Web.Script.Serialization;
using System.Xml;
using erminas.SmartAPI.CMS.CCElements;
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

        public bool IsEmpty
        {
            get { return !Pages().Any(); }
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

        public IEnumerable<Page> Pages()
        {
            List<ResultGroup> searchForPagesExtended = CreatePageSearchForRecycleBin().Execute();
            return searchForPagesExtended[0].Results.Select(pageResult => pageResult.Page);
        }

        private ExtendedPageSearch CreatePageSearchForRecycleBin()
        {
            ExtendedPageSearch search = _project.CreateExtendedPageSearch();
            search.Predicates.Add(new SpecialPagePredicate(SpecialPagePredicate.PageCategoryType.RecycleBin));
            return search;
        }

        public IEnumerable<Page> PagesOfLanguageVariant(string language)
        {
            ExtendedPageSearch search = CreatePageSearchForRecycleBin();
            search.LanguageVariant = _project.LanguageVariants[language];

            IEnumerable<Result> results = search.Execute()[0].Results;
            return results.Select(pageResult => pageResult.Page);
        }
    }

    public enum ProjectLockLevel
    {
        None = 0,
        All = -1,
        Admin = 1,
        SiteBuilder = 2,
        Editor = 3,
        Author = 4,
        Visitor = 5,
        Publisher = 16,
        AdminAndPublisher = 17
    };

    /// <summary>
    ///     Represents a RedDot Project. Most (list) properties are lazy loaded and cached by default. That means the actual content (e.g. the folders, content classes etc) is loaded on the first access of the property and then cached, so that subsequent access is done on the local cache. You can change that behaviour through
    ///     <see
    ///         cref="ICachedList{T}.IsCachingEnabled" />
    ///     or do a manual refresh of the cache either eagerly (
    ///     <see
    ///         cref="ICachedList{T}.Refresh" />
    ///     ) or lazy ( <see cref="ICachedList{T}.InvalidateCache" /> . Most of the lists are also indexed on the most frequent access property (mostly Name). See the documentation on the properties for details.
    /// </summary>
    public class Project : PartialRedDotObject
    {
        #region RqlType enum

        /// <summary>
        ///     Indicate where the session key should be placed in the RQL query.
        /// </summary>
        public enum RqlType
        {
            /// <summary>
            ///     Insert the session key as attribute in the project element
            /// </summary>
            SessionKeyInProject,

            /// <summary>
            ///     Insert the session key as attribute in the iodata element
            /// </summary>
            SessionKeyInIodata
        };

        #endregion

        #region UserAccessLevel enum

        public enum UserAccessLevel
        {
            None = 0,
            Visitor = 5,
            Author = 4,
            Editor = 3,
            SiteBuilder = 2,
            Admin = 1
        };

        #endregion

        private readonly Dictionary<string, IndexedRDList<int, Page>> _pagesByLanguage =
            new Dictionary<string, IndexedRDList<int, Page>>();

        private LanguageVariant _currentLanguageVariant;
        private ProjectLockLevel _locklevel;

        public Project(Session session, XmlElement xmlElement) : base(xmlElement)
        {
            Session = session;
            LoadXml();
            Init();
        }

        public Project(Session session, Guid guid) : base(guid)
        {
            Session = session;
            Init();
        }

        /// <summary>
        ///     All info attributes in the project, indexed by id. The list is cached by default.
        /// </summary>
        [ScriptIgnore]
        public IIndexedCachedList<int, InfoAttribute> InfoAttributes { get; private set; }

        /// <summary>
        ///     All concent class folders, indexed by name. The list is cached by default.
        /// </summary>
        [ScriptIgnore]
        public NameIndexedRDList<ContentClassFolder> ContentClassFolders { get; private set; }

        /// <summary>
        ///     All folders, indexed by name. The list is cached by default.
        /// </summary>
        [ScriptIgnore]
        public NameIndexedRDList<Folder> Folders { get; private set; }

        /// <summary>
        ///     All project variants, indexed by name. The list is cached by default.
        /// </summary>
        [ScriptIgnore]
        public NameIndexedRDList<ProjectVariant> ProjectVariants { get; private set; }

        /// <summary>
        ///     All language variants, indexed by Language. The list is cached by default.
        /// </summary>
        [ScriptIgnore]
        public IndexedRDList<String, LanguageVariant> LanguageVariants { get; private set; }

        /// <summary>
        ///     Get/Set the current active language variant. This information is cached.
        /// </summary>
        [ScriptIgnore]
        public LanguageVariant CurrentLanguageVariant
        {
            get { return _currentLanguageVariant ?? (RefreshCurrentLanguageVariant()); }
            set { SelectLanguageVariant(value); }
        }

        /// <summary>
        ///     A list of all content classes, indexed by name. The list is cached by default.
        /// </summary>
        public RDList<ContentClass> ContentClasses { get; private set; }

        /// <summary>
        ///     All database connections, indexed by name. The list is cached by default.
        /// </summary>
        [ScriptIgnore]
        public NameIndexedRDList<DatabaseConnection> DatabaseConnections { get; private set; }

        /// <summary>
        ///     All Syllables, indexed by guid. The list is cached by default.
        /// </summary>
        [ScriptIgnore]
        public NameIndexedRDList<Syllable> Syllables { get; private set; }

        /// <summary>
        ///     The session object this project belongs to. All RQL queries are executed in this session.
        /// </summary>
        [ScriptIgnore]
        public Session Session { get; private set; }

        /// <summary>
        ///     The project lock level.
        /// </summary>
        [ScriptIgnore]
        public ProjectLockLevel LockLevel
        {
            get { return LazyLoad(ref _locklevel); }
        }

        [ScriptIgnore]
        public Categories Categories { get; private set; }

        /// <summary>
        ///     All keywords, indexed by name. The list is cached by default.
        /// </summary>
        [ScriptIgnore]
        public RDList<Keyword> Keywords { get; private set; }

        /// <summary>
        ///     All folders used for the asset manager (i.e. where folder.IsAssertManagerFolder == true)
        /// </summary>
        [ScriptIgnore]
        public List<Folder> AssetManagerFolders
        {
            get { return Folders.Where(x => x.IsAssetManagerFolder).ToList(); }
        }

        /// <summary>
        ///     All users of the project, indexed by name. The list is cached by default.
        /// </summary>
        [ScriptIgnore]
        public NameIndexedRDList<User> UsersOfProject { get; private set; }

        /// <summary>
        ///     All (non global) workflows.
        /// </summary>
        [ScriptIgnore]
        public NameIndexedRDList<Workflow> Workflows { get; private set; }

        public RecycleBin RecycleBin { get; private set; }

        #region Publication

        /// <summary>
        ///     All publication targets
        /// </summary>
        [ScriptIgnore]
        public IRDList<PublicationTarget> PublicationTargets { get; private set; }

        /// <summary>
        ///     All publication folders
        /// </summary>
        [ScriptIgnore]
        public IRDList<PublicationFolder> PublicationFolders { get; private set; }

        /// <summary>
        ///     All publication packages
        /// </summary>
        [ScriptIgnore]
        public IRDList<PublicationPackage> PublicationPackages { get; private set; }

        #endregion

        /// <summary>
        ///     Refresh the currently selected language variant value. You should only need to use this, if the language variant can be changed outside of this project instance (e.g. if you have to _different_ project objects for the same project).
        /// </summary>
        public LanguageVariant RefreshCurrentLanguageVariant()
        {
            const string LOAD_SESSION_INFO = @"<USER action=""sessioninfo""/>";

            XmlDocument xmlDoc = ExecuteRQL(LOAD_SESSION_INFO, RqlType.SessionKeyInProject);
            string languageId =
                ((XmlElement) xmlDoc.GetElementsByTagName("USER")[0]).GetAttributeValue("languagevariantid");
            return _currentLanguageVariant = LanguageVariants[languageId];
        }

        /// <summary>
        ///     Get the project variant used as display format (preview).
        /// </summary>
        public ProjectVariant GetDisplayFormatProjectVariant()
        {
            return ProjectVariants.FirstOrDefault(x => x.IsUsedAsDisplayFormat);
        }

        private void Init()
        {
            RecycleBin = new RecycleBin(this);
            PublicationTargets = new RDList<PublicationTarget>(GetPublicationTargets, Caching.Enabled);
            PublicationFolders = new RDList<PublicationFolder>(GetPublicationFolders, Caching.Enabled);
            PublicationPackages = new RDList<PublicationPackage>(GetPublicationPackages, Caching.Enabled);
            InfoAttributes = new IndexedCachedList<int, InfoAttribute>(GetInfoAttributes, x => x.Id, Caching.Enabled);

            ContentClasses = new RDList<ContentClass>(GetContentClasses, Caching.Enabled);
            ContentClassFolders = new NameIndexedRDList<ContentClassFolder>(GetContentClassFolders, Caching.Enabled);
            Folders = new NameIndexedRDList<Folder>(GetFolders, Caching.Enabled);
            ProjectVariants = new NameIndexedRDList<ProjectVariant>(GetProjectVariants, Caching.Enabled);
            LanguageVariants = new IndexedRDList<string, LanguageVariant>(GetLanguageVariants, x => x.Language,
                                                                          Caching.Enabled);

            DatabaseConnections = new NameIndexedRDList<DatabaseConnection>(GetDatabaseConnections, Caching.Enabled);
            Syllables = new NameIndexedRDList<Syllable>(GetSyllables, Caching.Enabled);
            UsersOfProject = new NameIndexedRDList<User>(GetUsersOfProject, Caching.Enabled);

            Workflows = new NameIndexedRDList<Workflow>(GetWorkflows, Caching.Enabled);
            Categories = new Categories(this);
            Keywords = new RDList<Keyword>(GetKeywords, Caching.Enabled);
        }

        /// <summary>
        ///     Selects the active language variant. Has the same effect as setting <see cref="CurrentLanguageVariant" />
        /// </summary>
        /// <param name="language"> Language to make active </param>
        /// <exception cref="Exception">Thrown, if language variant could not be made active</exception>
        public void SelectLanguageVariant(LanguageVariant language)
        {
            if (_currentLanguageVariant == language)
            {
                return;
            }
            const string SELECT_LANGUAGE = @"<LANGUAGEVARIANT action=""setactive"" guid=""{0}""/>";
            XmlDocument xmlDoc = ExecuteRQL(String.Format(SELECT_LANGUAGE, language.Guid.ToRQLString()),
                                            RqlType.SessionKeyInProject);
            if (!xmlDoc.InnerText.Contains("ok"))
            {
                //TODO richtige exception
                throw new Exception("could not load language variant '" + language.Language + "' for project '" +
                                    Guid.ToRQLString() + "'");
            }
            if (_currentLanguageVariant != null)
            {
                _currentLanguageVariant.IsCurrentLanguageVariant = false;
            }
            language.IsCurrentLanguageVariant = true;
            _currentLanguageVariant = language;
        }

        private void LoadXml()
        {
            InitIfPresent(ref _locklevel, "inhibitlevel", x => (ProjectLockLevel) int.Parse(x));
        }

        /// <summary>
        ///     Select this project as active project in the current session.
        /// </summary>
        public void Select()
        {
            Session.SelectProject(this);
        }

        /// <summary>
        ///     Kicks off an asynchronous project export. For success / failure check emails from RedDot.
        /// </summary>
        public void Export()
        {
            const string EXPORT = @"<ADMINISTRATION><PROJECT action=""export"" projectguid=""{0}""</ADMINISTRATION>";
            //todo oder muss das als plain (ohne sessionkey) gesendet werden?
            ExecuteRQL(String.Format(EXPORT, Guid.ToRQLString()));
        }

        /// <summary>
        ///     Changes the user access level for this project.
        /// </summary>
        public void SetUserLevel(User user, UserAccessLevel accessLevel)
        {
            const string SET_USER_LEVEL = @"<ADMINISTRATION>
                                                <USER guid=""{0}"" action=""save"">
                                                    <PROJECTS>
                                                        <PROJECT guid=""{1}"" checked=""{2}"" userlevel=""{3}"" />
                                                    </PROJECTS>
                                                </USER>
                                            </ADMINISTRATION>";

            //todo oder muss das als plain (ohne sessionkey) gesendet werden?
            //todo return value checken
            ExecuteRQL(string.Format(SET_USER_LEVEL, user.Guid.ToRQLString(), Guid.ToRQLString(), "1", (int) accessLevel));
        }

        /// <summary>
        ///     Returns the user level set for this project.
        /// </summary>
        public UserAccessLevel GetAccessLevelForUser(User user)
        {
            const string LOAD_ACCESS_LEVEL =
                @"<ADMINISTRATION><USER guid=""{0}"" ><PROJECT guid=""{1}"" action=""load""/></USER></ADMINISTRATION>";
            XmlDocument xmlDoc =
                ExecuteRQL(string.Format(LOAD_ACCESS_LEVEL, user.Guid.ToRQLString(), Guid.ToRQLString()));
            var xmlNode = (XmlElement) xmlDoc.GetElementsByTagName("PROJECT")[0];
            if (!string.IsNullOrEmpty(xmlNode.GetAttributeValue("guid")))
            {
                return (UserAccessLevel) xmlNode.GetIntAttributeValue("userlevel").Value;
            }

            return UserAccessLevel.None;
        }

        /// <summary>
        ///     Execute an rql query.
        /// </summary>
        /// <param name="query"> The query string (not containing IODATA/PROJECT elements) </param>
        /// <param name="type"> Determine the attributes of IODATA/PROJECT elements in the query </param>
        /// <returns> The parsed reply of the RedDot server as XmlDocument </returns>
        public XmlDocument ExecuteRQL(string query, RqlType type = RqlType.SessionKeyInIodata)
        {
            switch (type)
            {
                case RqlType.SessionKeyInIodata:
                    return Session.ExecuteRQL(query, Guid);
                case RqlType.SessionKeyInProject:
                    return Session.ExecuteRQLProject(Guid, query);
                default:
                    throw new ArgumentException(string.Format("Unknown query type: {0}", type));
            }
        }

        /// <see cref="CMS.Session.GetTextContent" />
        public string GetTextContent(Guid textElementGuid, LanguageVariant lang, string typeString)
        {
            return Session.GetTextContent(Guid, lang, textElementGuid, typeString);
        }

        /// <see cref="CMS.Session.SetTextContent" />
        public Guid SetTextContent(Guid textElementGuid, LanguageVariant languageVariant, string typeString,
                                   string content)
        {
            return Session.SetTextContent(Guid, languageVariant, textElementGuid, typeString, content);
        }

        protected override void LoadWholeObject()
        {
            LoadXml();
        }

        protected override XmlElement RetrieveWholeObject()
        {
            return Session.Projects.First(x => x.Guid.Equals(Guid)).XmlElement;
        }

        private List<ContentClass> GetContentClasses()
        {
            const string LIST_CC_OF_PROJECT = @"<TEMPLATES action=""list""/>";
            XmlDocument xmlDoc = Session.ExecuteRQL(LIST_CC_OF_PROJECT, Guid);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("TEMPLATE");

            return (from XmlElement curNode in xmlNodes
                    select new ContentClass(this, curNode.GetGuid()) {Name = curNode.GetAttributeValue("name")}).ToList();
        }

        #region PAGES

        /// <summary>
        ///     All pages of the current language variant, indexed by page id. The list is cached by default.
        /// </summary>
        [ScriptIgnore]
        public IndexedRDList<int, Page> PagesOfCurrentLanguageVariant
        {
            get { return GetPagesForLanguageVariant(CurrentLanguageVariant.Language); }
        }

        /// <summary>
        ///     All pages of the a specific language variant, indexed by page id. The list is cached by default.
        /// </summary>
        public IndexedRDList<int, Page> GetPagesForLanguageVariant(string language)
        {
            LanguageVariant languageVariant = LanguageVariants[language];
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
        ///     Create a new page.
        /// </summary>
        /// <param name="cc"> Content class of the page </param>
        /// <param name="headline"> The headline, or null (default) for the default headline </param>
        /// <returns> The newly created page </returns>
        public Page CreatePage(ContentClass cc, string headline = null)
        {
            XmlDocument xmlDoc = ExecuteRQL(PageCreationString(cc, headline));
            return CreatePageFromCreationReply(xmlDoc);
        }

        /// <summary>
        ///     Create a new page and link it.
        /// </summary>
        /// <param name="cc"> Content class of the page </param>
        /// <param name="linkGuid"> Guid of the link the page should be linked to </param>
        /// <param name="headline"> The headline, or null (default) for the default headline </param>
        /// <returns> The newly created (and linked) page </returns>
        public Page CreateAndConnectPage(ContentClass cc, Guid linkGuid, string headline = null)
        {
            const string CREATE_AND_LINK_PAGE = @"<LINK action=""assign"" guid=""{0}"">{1}</LINK>";
            XmlDocument xmlDoc =
                ExecuteRQL(string.Format(CREATE_AND_LINK_PAGE, linkGuid.ToRQLString(), PageCreationString(cc, headline)));
            return CreatePageFromCreationReply(xmlDoc);
        }

        private static string PageCreationString(ContentClass cc, string headline = null)
        {
            const string PAGE_CREATION_STRING = @"<PAGE action=""addnew"" templateguid=""{0}"" {1}/>";

            string headlineString = headline == null
                                        ? ""
                                        : string.Format(@"headline=""{0}""", HttpUtility.HtmlEncode(headline));
            return string.Format(PAGE_CREATION_STRING, cc.Guid.ToRQLString(), headlineString);
        }

        private Page CreatePageFromCreationReply(XmlDocument xmlDoc)
        {
            try
            {
                var pageItem = (XmlElement) xmlDoc.GetElementsByTagName("PAGE")[0];
                return new Page(this, pageItem);
            } catch (Exception e)
            {
                throw new Exception("Could not create page", e);
            }
        }

        #region Page Search

        /// <summary>
        ///     Create an extended page search on this project.
        /// </summary>
        /// <see cref="CreatePageSearch" />
        public ExtendedPageSearch CreateExtendedPageSearch()
        {
            return new ExtendedPageSearch(this);
        }

        /// <summary>
        ///     Convenience funtion for extended page searches. Creates a new PageSearchExtended object which gets configured through the configurator parameter and returns the result of the search.
        /// </summary>
        /// <param name="configurator"> An action to configure the search </param>
        /// <returns> The search results </returns>
        /// <example>
        ///     The following code searches for all pages saved as draft by the current user: <code>var results = project.SearchForPagesExtended( search => search.AddPredicate(new PageStatusPredicate(PageStatusPredicate.PageStatusType.SavedAsDraft, PageStatusPredicate.UserType.CurrentUser)));</code>
        /// </example>
        public List<ResultGroup> SearchForPagesExtended(Action<ExtendedPageSearch> configurator = null)
        {
            var search = new ExtendedPageSearch(this);
            if (configurator != null)
            {
                configurator(search);
            }

            return search.Execute();
        }

        /// <summary>
        ///     Create a simple page search on this project.
        /// </summary>
        /// <see cref="CreateExtendedPageSearch" />
        public PageSearch CreatePageSearch()
        {
            return new PageSearch(this);
        }

        /// <summary>
        ///     Convenience function for simple page searches. Creates a PageSearch object, configures it through the configurator parameter and returns the search result.
        /// </summary>
        /// <param name="configurator"> Action to configure the search </param>
        /// <returns> The search results </returns>
        /// <example>
        ///     The following code searches for all pages with headline "test": <code>var results = project.SearchForPages(search => search.Headline="test");</code>
        /// </example>
        public List<Page> SearchForPages(Action<PageSearch> configurator = null)
        {
            var search = new PageSearch(this);
            if (configurator != null)
            {
                configurator(search);
            }

            return search.Execute();
        }

        #endregion

        #endregion

        #region RetrievalFunctions

        private List<Workflow> GetWorkflows()
        {
            const string LIST_WORKFLOWS = @"<WORKFLOWS action=""list"" />";
            XmlDocument xmlDoc = ExecuteRQL(LIST_WORKFLOWS);
            return
                (from XmlElement curWorkflow in xmlDoc.GetElementsByTagName("WORKFLOW")
                 select new Workflow(this, curWorkflow)).ToList();
        }

        private List<PublicationPackage> GetPublicationPackages()
        {
            const string LIST_PUBLICATION_PACKAGES = @"<PROJECT><EXPORTPACKET action=""list""/></PROJECT>";
            XmlDocument xmlDoc = ExecuteRQL(LIST_PUBLICATION_PACKAGES);
            return (from XmlElement curPackage in xmlDoc.GetElementsByTagName("EXPORTPACKET")
                    select new PublicationPackage(this, curPackage.GetGuid())).ToList();
        }

        private List<PublicationTarget> GetPublicationTargets()
        {
            const string LIST_PUBLISHING_TARGETS = @"<PROJECT><EXPORTS action=""list""/></PROJECT>";

            XmlDocument xmlDoc = ExecuteRQL(LIST_PUBLISHING_TARGETS);
            return
                (from XmlElement curElement in xmlDoc.GetElementsByTagName("EXPORT")
                 select new PublicationTarget(this, curElement)).ToList();
        }

        private List<PublicationFolder> GetPublicationFolders()
        {
            const string LIST_PUBLICATION_FOLDERS = @"<PROJECT><EXPORTFOLDERS action=""list"" /></PROJECT>";

            XmlDocument xmlDoc = ExecuteRQL(LIST_PUBLICATION_FOLDERS);
            if (xmlDoc.GetElementsByTagName("EXPORTFOLDERS").Count != 1)
            {
                throw new Exception("Could not retrieve publication folders");
            }
            return (from XmlElement curFolder in xmlDoc.GetElementsByTagName("EXPORTFOLDER")
                    select new PublicationFolder(this, curFolder.GetGuid())).ToList();
        }

        private List<ContentClassFolder> GetContentClassFolders()
        {
            const string LIST_CC_FOLDERS_OF_PROJECT = @"<TEMPLATEGROUPS action=""load"" />";
            XmlDocument xmlDoc = Session.ExecuteRQL(LIST_CC_FOLDERS_OF_PROJECT, Guid);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("GROUP");

            return (from XmlElement curNode in xmlNodes select new ContentClassFolder(this, curNode)).ToList();
        }

        private List<Folder> GetFolders()
        {
            const string LIST_FILE_FOLDERS = @"<PROJECT><FOLDERS action=""list"" foldertype=""0""/></PROJECT>";
            XmlDocument xmlDoc = ExecuteRQL(LIST_FILE_FOLDERS);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("FOLDER");

            return (from XmlElement curNode in xmlNodes select new Folder(this, curNode)).ToList();
        }

        private List<LanguageVariant> GetLanguageVariants()
        {
            const string LIST_LANGUAGE_VARIANTS =
                @"<PROJECT projectguid=""{0}""><LANGUAGEVARIANTS action=""list""/></PROJECT>";
            XmlDocument xmlDoc = ExecuteRQL(String.Format(LIST_LANGUAGE_VARIANTS, Guid.ToRQLString()));
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("LANGUAGEVARIANT");
            var languageVariants = new List<LanguageVariant>();

            foreach (XmlElement curNode in xmlNodes)
            {
                var variant = new LanguageVariant(this, curNode);
                languageVariants.Add(variant);
                if (variant.IsCurrentLanguageVariant)
                {
                    _currentLanguageVariant = variant;
                }
            }

            return languageVariants;
        }

        private List<DatabaseConnection> GetDatabaseConnections()
        {
            const string LIST_DATABASE_CONNECTION = @"<DATABASES action=""list""/>";
            XmlDocument xmlDoc = ExecuteRQL(LIST_DATABASE_CONNECTION, RqlType.SessionKeyInProject);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("DATABASE");

            return (from XmlElement curNode in xmlNodes select new DatabaseConnection(this, curNode)).ToList();
        }

        private List<InfoAttribute> GetInfoAttributes()
        {
            const string LOAD_INFO_ELEMENTS = @"<TEMPLATE><INFOELEMENTS action=""list""></INFOELEMENTS></TEMPLATE>";
            XmlDocument xmlDoc = ExecuteRQL(LOAD_INFO_ELEMENTS);
            var infos = xmlDoc.GetElementsByTagName("INFOELEMENTS")[0] as XmlElement;
            if (infos == null)
            {
                throw new Exception("could not load info elements");
            }
            return
                (from XmlElement info in infos.GetElementsByTagName("PAGEINFO") select new InfoAttribute(info)).Union(
                    (from XmlElement info in infos.GetElementsByTagName("PROJECTINFO") select new InfoAttribute(info)))
                                                                                                               .Union(
                                                                                                                   (from
                                                                                                                        XmlElement
                                                                                                                        info
                                                                                                                        in
                                                                                                                        infos
                                                                                                                        .GetElementsByTagName
                                                                                                                        ("SESSIONOBJECT")
                                                                                                                    select
                                                                                                                        new InfoAttribute
                                                                                                                        (info)))
                                                                                                               .ToList();
        }

        private List<ProjectVariant> GetProjectVariants()
        {
            const string LIST_PROJECT_VARIANTS = @"<PROJECT><PROJECTVARIANTS action=""list""/></PROJECT>";
            XmlDocument xmlDoc = ExecuteRQL(LIST_PROJECT_VARIANTS);
            var variants = xmlDoc.GetElementsByTagName("PROJECTVARIANTS")[0] as XmlElement;
            if (variants == null)
            {
                throw new Exception("could not load project variants");
            }
            return
                (from XmlElement variant in variants.GetElementsByTagName("PROJECTVARIANT")
                 select new ProjectVariant(this, variant)).ToList();
        }

        private List<Syllable> GetSyllables()
        {
            XmlDocument xmlDoc = ExecuteRQL(@"<SYLLABLES action=""list""/>", RqlType.SessionKeyInProject);
            XmlNodeList syllablelist = xmlDoc.GetElementsByTagName("SYLLABLE");
            return (from XmlElement curNode in syllablelist select new Syllable(this, curNode)).ToList();
        }

        private List<User> GetUsersOfProject()
        {
            try
            {
                const string GET_USERS = @"<PROJECT guid=""{0}""><USERS action=""list""/></PROJECT>";
                XmlDocument xmlDoc = Session.ExecuteRQL(string.Format(GET_USERS, Guid.ToRQLString()), Guid);
                XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("USER");

                return (from XmlElement node in xmlNodes select new User(Session, node)).ToList();
            } catch (Exception e)
            {
                throw new Exception("Could not load users of Project: " + Guid.ToRQLString(), e);
            }
        }

        private List<Page> GetPages()
        {
            const string LIST_PAGES = @"<PROJECT><PAGES action=""list""/></PROJECT>";
            XmlDocument xmlDoc = ExecuteRQL(LIST_PAGES);
            return (from XmlElement curPage in xmlDoc.GetElementsByTagName("PAGE")
                    select
                        new Page(this, curPage.GetGuid(), CurrentLanguageVariant)
                            {
                                Headline = curPage.GetAttributeValue("headline"),
                                Id = curPage.GetIntAttributeValue("id").GetValueOrDefault()
                            }).ToList();
        }

        private List<Keyword> GetKeywords()
        {
            const string LIST_KEYWORDS = "<PROJECT><CATEGORY><KEYWORDS action=\"list\" /></CATEGORY></PROJECT>";
            XmlDocument xmlDoc = ExecuteRQL(LIST_KEYWORDS);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("KEYWORD");
            IEnumerable<Keyword> categoryKeywords = from curCategory in Categories
                                                    select
                                                        new Keyword(this, curCategory.Guid)
                                                            {
                                                                Name = "[category]",
                                                                Category = curCategory
                                                            };
            return
                (from XmlElement curNode in xmlNodes select new Keyword(this, curNode)).Union(categoryKeywords).ToList();
        }

        #endregion
    }
}