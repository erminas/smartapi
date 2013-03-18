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
using System.Globalization;
using System.Linq;
using System.Security;
using System.Web.Script.Serialization;
using System.Xml;
using erminas.SmartAPI.CMS.Administration;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements;
using erminas.SmartAPI.CMS.Project.Filesystem;
using erminas.SmartAPI.CMS.Project.Keywords;
using erminas.SmartAPI.CMS.Project.Publication;
using erminas.SmartAPI.CMS.Project.Workflows;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project
{
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

        private readonly ContentClasses.ContentClasses _contentClasses;
        private readonly Pages.Pages _pages;
        private LanguageVariant _currentLanguageVariant;
        private ProjectLockLevel _locklevel;

        internal Project(Session session, XmlElement xmlElement) : base(session, xmlElement)
        {
            _contentClasses = new ContentClasses.ContentClasses(this);
            _pages = new Pages.Pages(this);
            LoadXml();
            Init();
        }

        public Project(Session session, Guid guid) : base(session, guid)
        {
            _contentClasses = new ContentClasses.ContentClasses(this);
            _pages = new Pages.Pages(this);
            Init();
        }

        /// <summary>
        ///     All folders used for the asset manager (i.e. where folder.IsAssertManagerFolder == true).
        ///     Same as
        ///     <code>
        /// <pre>
        ///     Folders.Where(x => x.IsAssetManagerFolder).ToList()
        /// </pre>
        /// </code>
        /// </summary>
        [ScriptIgnore]
        public IEnumerable<Folder> AssetManagerFolders
        {
            get { return Folders.Where(x => x.IsAssetManagerFolder).ToList(); }
        }

        [ScriptIgnore]
        public Categories Categories { get; private set; }

        /// <summary>
        ///     All concent class folders, indexed by name. The list is cached by default.
        /// </summary>
        [ScriptIgnore]
        public NameIndexedRDList<ContentClassFolder> ContentClassFolders { get; private set; }
        
        public IProjectCopyJob CreateCopyJob(string newProjectName)
        {
            return new ProjectCopyJob(this, newProjectName);
        }

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
        ///     All database connections, indexed by name. The list is cached by default.
        /// </summary>
        [ScriptIgnore]
        public NameIndexedRDList<DatabaseConnection> DatabaseConnections { get; private set; }

        /// <summary>
        ///     Delete this project and its database on the database server
        /// </summary>
        public void DeleteWithDatabase(string databaseUser, string password)
        {
            const string DELETE_PROJECT =
                @"<ADMINISTRATION><PROJECT action=""delete"" guid=""{0}"" deletedb=""{1}"" user=""{2}"" password=""{3}""/></ADMINISTRATION>";
            Session.ExecuteRQL(DELETE_PROJECT.RQLFormat(this, true, databaseUser, password));
            //empty response on success, so errors can't be detected
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

        public IProjectExportJob CreateExportJob(string targetPath)
        {
            return new ProjectExportJob(this, targetPath);
        }

        /// <summary>
        ///     All folders, indexed by name. The list is cached by default.
        /// </summary>
        [ScriptIgnore]
        public NameIndexedRDList<Folder> Folders { get; private set; }

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
        ///     Get the project variant used as display format (preview).
        /// </summary>
        public ProjectVariant GetDisplayFormatProjectVariant()
        {
            return ProjectVariants.FirstOrDefault(x => x.IsUsedAsDisplayFormat);
        }

        /// <see cref="CMS.Session.GetTextContent" />
        public string GetTextContent(Guid textElementGuid, LanguageVariant lang, string typeString)
        {
            return Session.GetTextContent(Guid, lang, textElementGuid, typeString);
        }

        /// <summary>
        ///     All info attributes in the project, indexed by id. The list is cached by default.
        /// </summary>
        [ScriptIgnore]
        public IIndexedCachedList<int, InfoAttribute> InfoAttributes { get; private set; }

        public bool IsArchivingActive
        {
            get
            {
                EnsureInitialization();
                return XmlElement.GetIntAttributeValue("archive").GetValueOrDefault() == -1;
            }
        }
        //TODO gets stored on server immediatly, commit? Save/Set?
        public bool IsVersioningActive
        {
            get
            {
                EnsureInitialization();
                return XmlElement.GetIntAttributeValue("versioning").GetValueOrDefault() == -1;
            }

            set
            {
                const string SET_VERISONING =
                    @"<ADMINISTRATION><PROJECT action=""save"" guid=""{2}"" versioning=""{3}""/></ADMINISTRATION>";
                Session.ExecuteRQL(SET_VERISONING.RQLFormat(Session.LogonGuid, Session.CurrentUser, this, value));
            }
        }

        /// <summary>
        ///     All keywords, indexed by name. The list is cached by default.
        /// </summary>
        [ScriptIgnore]
        public RDList<Keyword> Keywords { get; private set; }

        /// <summary>
        ///     All language variants, indexed by Language. The list is cached by default.
        /// </summary>
        [ScriptIgnore]
        public IndexedRDList<String, LanguageVariant> LanguageVariants { get; private set; }

        /// <summary>
        ///     The project lock level.
        /// </summary>
        [ScriptIgnore]
        public ProjectLockLevel LockLevel
        {
            get { return LazyLoad(ref _locklevel); }
        }

        public LanguageVariant MainLanguage
        {
            get { return LanguageVariants.First(variant => variant.IsMainLanguage); }
        }

        /// <summary>
        ///     All project variants, indexed by name. The list is cached by default.
        /// </summary>
        [ScriptIgnore]
        public NameIndexedRDList<ProjectVariant> ProjectVariants { get; private set; }

        public RecycleBin RecycleBin { get; private set; }

        /// <summary>
        ///     Refresh the currently selected language variant value. You should only need to use this, if the language variant can be changed outside of this project instance (e.g. if you have two _different_ project objects for the same project).
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
        ///     Select this project as active project in the current session.
        /// </summary>
        public void Select()
        {
            Session.SelectProject(this);
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
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not load language variant '{0}' for project {1}",
                                                          language.Language, this));
            }
            if (_currentLanguageVariant != null)
            {
                _currentLanguageVariant.IsCurrentLanguageVariant = false;
            }
            language.IsCurrentLanguageVariant = true;
            _currentLanguageVariant = language;
        }

        /// <summary>
        ///     Set the project lock. 
        ///     The info message must not be empty, if lock level is different than ProjectLockLevel.None!
        /// </summary>
        /// <param name="level">level to set the locking to</param>
        /// <param name="infoMessage">info message to display to users, MUST NOT BE EMPTY if lock level is different than ProjectLockLevel.None</param>
        public void SetLockLevel(ProjectLockLevel level, string infoMessage)
        {
            if (level != ProjectLockLevel.None && string.IsNullOrEmpty(infoMessage))
            {
                throw new SmartAPIException(Session.ServerLogin, "Info message for project locking must not be empty");
            }
            const string SET_LOCKLEVEL =
                @"<ADMINISTRATION><PROJECT action=""save"" guid=""{0}"" inhibitlevel=""{1}"" lockinfo=""{2}""/></ADMINISTRATION>";

            var xmlDoc =
                Session.ExecuteRQL(SET_LOCKLEVEL.RQLFormat(this, (int) level,
                                                           level == ProjectLockLevel.None
                                                               ? Session.SESSIONKEY_PLACEHOLDER
                                                               : SecurityElement.Escape(infoMessage)));
            var project = (XmlElement) xmlDoc.SelectSingleNode("//PROJECT");
            if (!project.GetAttributeValue("inhibitlevel").Equals(((int) level).ToString(CultureInfo.InvariantCulture)))
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not set project lock level to {0}", level));
            }
        }

        /// <see cref="CMS.Session.SetTextContent" />
        public Guid SetTextContent(Guid textElementGuid, LanguageVariant languageVariant, string typeString,
                                   string content)
        {
            return Session.SetTextContent(Guid, languageVariant, textElementGuid, typeString, content);
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
        ///     All Syllables, indexed by guid. The list is cached by default.
        /// </summary>
        [ScriptIgnore]
        public NameIndexedRDList<Syllable> Syllables { get; private set; }

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

        protected override void LoadWholeObject()
        {
            LoadXml();
        }

        protected override XmlElement RetrieveWholeObject()
        {
            //TODO muss hier stattfinden, wenn user kein servermanager ist, sieht er nicht alle projekte -> Projects schlaegt fehl
            return Session.Projects.First(x => x.Guid.Equals(Guid)).XmlElement;
        }

        private List<ContentClassFolder> GetContentClassFolders()
        {
            const string LIST_CC_FOLDERS_OF_PROJECT = @"<TEMPLATEGROUPS action=""load"" />";
            XmlDocument xmlDoc = Session.ExecuteRQL(LIST_CC_FOLDERS_OF_PROJECT, Guid);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("GROUP");

            return (from XmlElement curNode in xmlNodes select new ContentClassFolder(this, curNode)).ToList();
        }

        private void Init()
        {
            RecycleBin = new RecycleBin(this);
            PublicationTargets = new RDList<PublicationTarget>(GetPublicationTargets, Caching.Enabled);
            PublicationFolders = new RDList<PublicationFolder>(GetPublicationFolders, Caching.Enabled);
            PublicationPackages = new RDList<PublicationPackage>(GetPublicationPackages, Caching.Enabled);
            InfoAttributes = new IndexedCachedList<int, InfoAttribute>(GetInfoAttributes, x => x.Id, Caching.Enabled);

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

        private void LoadXml()
        {
            InitIfPresent(ref _locklevel, "inhibitlevel", x => (ProjectLockLevel) int.Parse(x));
        }

        #region RetrievalFunctions

        private List<DatabaseConnection> GetDatabaseConnections()
        {
            const string LIST_DATABASE_CONNECTION = @"<DATABASES action=""list""/>";
            XmlDocument xmlDoc = ExecuteRQL(LIST_DATABASE_CONNECTION, RqlType.SessionKeyInProject);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("DATABASE");

            return (from XmlElement curNode in xmlNodes select new DatabaseConnection(this, curNode)).ToList();
        }

        private List<Folder> GetFolders()
        {
            const string LIST_FILE_FOLDERS =
                @"<PROJECT><FOLDERS action=""list"" foldertype=""0"" withsubfolders=""1""/></PROJECT>";
            XmlDocument xmlDoc = ExecuteRQL(LIST_FILE_FOLDERS);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("FOLDER");

            return (from XmlElement curNode in xmlNodes select new Folder(this, curNode)).ToList();
        }

        private List<InfoAttribute> GetInfoAttributes()
        {
            const string LOAD_INFO_ELEMENTS = @"<TEMPLATE><INFOELEMENTS action=""list""></INFOELEMENTS></TEMPLATE>";
            XmlDocument xmlDoc = ExecuteRQL(LOAD_INFO_ELEMENTS);
            var infos = xmlDoc.GetElementsByTagName("INFOELEMENTS")[0] as XmlElement;
            if (infos == null)
            {
                throw new SmartAPIException(Session.ServerLogin, "Could not load info elements");
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

        private List<ProjectVariant> GetProjectVariants()
        {
            const string LIST_PROJECT_VARIANTS = @"<PROJECT><PROJECTVARIANTS action=""list""/></PROJECT>";
            XmlDocument xmlDoc = ExecuteRQL(LIST_PROJECT_VARIANTS);
            var variants = xmlDoc.GetElementsByTagName("PROJECTVARIANTS")[0] as XmlElement;
            if (variants == null)
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not load project variants of project {0}", this));
            }
            return
                (from XmlElement variant in variants.GetElementsByTagName("PROJECTVARIANT")
                 select new ProjectVariant(this, variant)).ToList();
        }

        private List<PublicationFolder> GetPublicationFolders()
        {
            const string LIST_PUBLICATION_FOLDERS = @"<PROJECT><EXPORTFOLDERS action=""list"" /></PROJECT>";

            XmlDocument xmlDoc = ExecuteRQL(LIST_PUBLICATION_FOLDERS);
            if (xmlDoc.GetElementsByTagName("EXPORTFOLDERS").Count != 1)
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not retrieve publication folders of project {0}", this));
            }
            return (from XmlElement curFolder in xmlDoc.GetElementsByTagName("EXPORTFOLDER")
                    select new PublicationFolder(this, curFolder.GetGuid())).ToList();
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
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not load users of project {0} ", this), e);
            }
        }

        private List<Workflow> GetWorkflows()
        {
            const string LIST_WORKFLOWS = @"<WORKFLOWS action=""list"" listglobalworkflow=""1""/>";
            XmlDocument xmlDoc = ExecuteRQL(LIST_WORKFLOWS);
            return
                (from XmlElement curWorkflow in xmlDoc.GetElementsByTagName("WORKFLOW")
                 select new Workflow(this, curWorkflow)).ToList();
        }

        #endregion

        #region Publication

        public ContentClasses.ContentClasses ContentClasses
        {
            get { return _contentClasses; }
        }

        public Pages.Pages Pages
        {
            get { return _pages; }
        }

        /// <summary>
        ///     All publication folders
        /// </summary>
        public IRDList<PublicationFolder> PublicationFolders { get; private set; }

        /// <summary>
        ///     All publication packages
        /// </summary>
        public IRDList<PublicationPackage> PublicationPackages { get; private set; }

        /// <summary>
        ///     All publication targets
        /// </summary>
        public IRDList<PublicationTarget> PublicationTargets { get; private set; }

        #endregion
    }

    public enum NewProjectType
    {
        NormalProject = 0,
        TestProject = 1
    }
}