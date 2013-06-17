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
using System.Xml;
using erminas.SmartAPI.CMS.Project.Authorizations;
using erminas.SmartAPI.CMS.Project.ContentClasses;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements;
using erminas.SmartAPI.CMS.Project.Folder;
using erminas.SmartAPI.CMS.Project.Keywords;
using erminas.SmartAPI.CMS.Project.Pages;
using erminas.SmartAPI.CMS.Project.Publication;
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

    public interface IContentClassFolders : IIndexedRDList<string, IContentClassFolder>, IProjectObject
    {
        IIndexedRDList<string, IContentClassFolder> Broken { get; }
    }

    internal class ContentClassFolders : NameIndexedRDList<IContentClassFolder>, IContentClassFolders
    {
        private readonly Project _project;

        internal ContentClassFolders(Project project, Caching caching) : base(caching)
        {
            _project = project;
            RetrieveFunc = GetContentClassFolders;
            Broken = new NameIndexedRDList<IContentClassFolder>(GetBrokenFolders, Caching.Enabled);
        }

        public IIndexedRDList<string, IContentClassFolder> Broken { get; private set; }

        public IProject Project
        {
            get { return _project; }
        }

        public ISession Session
        {
            get { return _project.Session; }
        }

        private List<IContentClassFolder> GetBrokenFolders()
        {
            const string TREE =
                @"<TREESEGMENT type=""project.4000"" action=""load"" guid=""4AF89E44535511D4BDAB004005312B7C"" descent=""app"" parentguid=""""/>";
            var result = Project.ExecuteRQL(TREE);
            var guids = this.Select(folder => folder.Guid).ToList();
            return (from XmlElement element in result.GetElementsByTagName("SEGMENT")
                    let curGuid = element.GetGuid()
                    where !guids.Contains(curGuid)
                    select
                        (IContentClassFolder)
                        new ContentClassFolder(_project, curGuid)
                            {
                                Name = element.GetAttributeValue("value"),
                                IsBroken = true
                            }).ToList();
        }

        private List<IContentClassFolder> GetContentClassFolders()
        {
            const string LIST_CC_FOLDERS_OF_PROJECT = @"<TEMPLATEGROUPS action=""load"" />";
            //TODO project.execute
            XmlDocument xmlDoc = Session.ExecuteRQLInProjectContext(LIST_CC_FOLDERS_OF_PROJECT, _project.Guid);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("GROUP");

            return
                (from XmlElement curNode in xmlNodes
                 select (IContentClassFolder) new ContentClassFolder(_project, curNode)).ToList();
        }
    }

    public interface IProject : IPartialRedDotObject, ISessionObject
    {
        IAuthorizationPackages AuthorizationPackages { get; }

        //IClipboard Clipboard { get; }
        IProjectGroups AssignedGroups { get; }
        ICategories Categories { get; }

        /// <summary>
        ///     All content class folders, indexed by name. The list is cached by default.
        /// </summary>
        IContentClassFolders ContentClassFolders { get; }

        IContentClasses ContentClasses { get; }

        IProjectCopyJob CreateCopyJob(string newProjectName);
        IProjectExportJob CreateExportJob(string targetPath);

        /// <summary>
        ///     All database connections, indexed by name. The list is cached by default.
        /// </summary>
        IDatabaseConnections DatabaseConnections { get; }

        /// <summary>
        ///     Delete this project and its database on the database server
        /// </summary>
        void DeleteWithDatabase(string databaseUser, string password);

        /// <summary>
        ///     Execute an rql query.
        /// </summary>
        /// <param name="query"> The query string (not containing IODATA/PROJECT elements) </param>
        /// <param name="type"> Determine the attributes of IODATA/PROJECT elements in the query </param>
        /// <returns> The parsed reply of the RedDot server as XmlDocument </returns>
        XmlDocument ExecuteRQL(string query, RqlType type = RqlType.SessionKeyInIodata);

        /// <summary>
        ///     All folders, indexed by name. The list is cached by default.
        /// </summary>
        IFolders Folders { get; }

        /// <see cref="CMS.Session.GetTextContent" />
        string GetTextContent(Guid textElementGuid, ILanguageVariant lang, string typeString);

        /// <summary>
        ///     All info attributes in the project, indexed by id. The list is cached by default.
        /// </summary>
        IIndexedCachedList<int, IInfoAttribute> InfoAttributes { get; }

        bool IsArchivingActive { get; }
        bool IsLockedBySystem { get; }
        bool IsVersioningActive { get; set; }

        /// <summary>
        ///     All keywords. The list is cached by default.
        /// </summary>
        IRDList<IKeyword> Keywords { get; }

        /// <summary>
        ///     All language variants, indexed by Language. The list is cached by default.
        /// </summary>
        ILanguageVariants LanguageVariants { get; }

        /// <summary>
        ///     The project lock level.
        /// </summary>
        ProjectLockLevel LockLevel { get; }

        IPages Pages { get; }

        /// <summary>
        ///     All project variants, indexed by name. The list is cached by default.
        /// </summary>
        IProjectVariants ProjectVariants { get; }

        /// <summary>
        ///     All publication folders
        /// </summary>
        IRDList<IPublicationFolder> PublicationFolders { get; }

        /// <summary>
        ///     All publication packages
        /// </summary>
        IRDList<IPublicationPackage> PublicationPackages { get; }

        /// <summary>
        ///     All publication targets
        /// </summary>
        IRDList<IPublicationTarget> PublicationTargets { get; }

        IRecycleBin RecycleBin { get; }
        IProject Refreshed();

        /// <summary>
        ///     Select this project as active project in the current session.
        /// </summary>
        void Select();

        /// <summary>
        ///     Set the project lock.
        ///     The info message must not be empty, if lock level is different than ProjectLockLevel.None!
        /// </summary>
        /// <param name="level">level to set the locking to</param>
        /// <param name="infoMessage">info message to display to users, MUST NOT BE EMPTY if lock level is different than ProjectLockLevel.None</param>
        void SetLockLevel(ProjectLockLevel level, string infoMessage);

        /// <see cref="CMS.Session.SetTextContent" />
        Guid SetTextContent(Guid textElementGuid, ILanguageVariant languageVariant, string typeString, string content);

        /// <summary>
        ///     All Syllables, indexed by guid. The list is cached by default.
        /// </summary>
        ISyllables Syllables { get; }

        /// <summary>
        ///     All users of the project and their access levels, indexed by user name. The list is cached by default.
        /// </summary>
        IProjectUsers Users { get; }

        IProjectWorkflows Workflows { get; }
    }

    /// <summary>
    ///     Represents a RedDot Project. Most (list) properties are lazy loaded and cached by default. That means the actual content (e.g. the folders, content classes etc) is loaded on the first access of the property and then cached, so that subsequent access is done on the local cache. You can change that behaviour through
    ///     <see
    ///         cref="ICachedList{T}.IsCachingEnabled" />
    ///     or do a manual refresh of the cache either eagerly (
    ///     <see
    ///         cref="ICachedList{T}.Refresh" />
    ///     ) or lazy ( <see cref="ICachedList{T}.InvalidateCache" /> . Most of the lists are also indexed on the most frequent access property (mostly Name). See the documentation on the properties for details.
    /// </summary>
    internal class Project : PartialRedDotObject, IProject
    {
        private readonly IContentClasses _contentClasses;
        private readonly IPages _pages;
        private bool _isLockedBySystem;
        private ProjectLockLevel _locklevel;

        internal Project(ISession session, XmlElement xmlElement) : base(session, xmlElement)
        {
            _contentClasses = new ContentClasses.ContentClasses(this);
            _pages = new Pages.Pages(this);
            LoadXml();
            Init();
        }

        internal Project(ISession session, Guid guid) : base(session, guid)
        {
            _contentClasses = new ContentClasses.ContentClasses(this);
            _pages = new Pages.Pages(this);
            Init();
        }

        //public IClipboard Clipboard { get; private set; }
        public IAuthorizationPackages AuthorizationPackages { get; private set; }
        public IProjectGroups AssignedGroups { get; private set; }

        public ICategories Categories { get; private set; }

        /// <summary>
        ///     All concent class folders, indexed by name. The list is cached by default.
        /// </summary>
        public IContentClassFolders ContentClassFolders { get; private set; }

        public IContentClasses ContentClasses
        {
            get { return _contentClasses; }
        }

        public IProjectCopyJob CreateCopyJob(string newProjectName)
        {
            return new ProjectCopyJob(this, newProjectName);
        }

        public IProjectExportJob CreateExportJob(string targetPath)
        {
            return new ProjectExportJob(this, targetPath);
        }

        /// <summary>
        ///     All database connections, indexed by name. The list is cached by default.
        /// </summary>
        public IDatabaseConnections DatabaseConnections { get; private set; }

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
                    return Session.ExecuteRQLInProjectContext(query, Guid);
                case RqlType.SessionKeyInProject:
                    return Session.ExecuteRQLInProjectContextAndEmbeddedInProjectElement(query, Guid);
                default:
                    throw new ArgumentException(string.Format("Unknown query type: {0}", type));
            }
        }

        /// <summary>
        ///     All folders, indexed by guid. The list is cached by default.
        /// </summary>
        public IFolders Folders { get; private set; }

        /// <see cref="CMS.Session.GetTextContent" />
        public string GetTextContent(Guid textElementGuid, ILanguageVariant lang, string typeString)
        {
            return Session.GetTextContent(Guid, lang, textElementGuid, typeString);
        }

        /// <summary>
        ///     All info attributes in the project, indexed by id. The list is cached by default.
        /// </summary>
        public IIndexedCachedList<int, IInfoAttribute> InfoAttributes { get; private set; }

        public bool IsArchivingActive
        {
            get
            {
                EnsureInitialization();
                return XmlElement.GetIntAttributeValue("archive").GetValueOrDefault() == -1;
            }
        }

        public bool IsLockedBySystem
        {
            get { return LazyLoad(ref _isLockedBySystem); }
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
        ///     All keywords. The list is cached by default.
        /// </summary>
        public IRDList<IKeyword> Keywords { get; private set; }

        /// <summary>
        ///     All language variants, indexed by Language. The list is cached by default.
        /// </summary>
        public ILanguageVariants LanguageVariants { get; private set; }

        /// <summary>
        ///     The project lock level.
        /// </summary>
        public ProjectLockLevel LockLevel
        {
            get { return LazyLoad(ref _locklevel); }
        }

        public IPages Pages
        {
            get { return _pages; }
        }

        /// <summary>
        ///     All project variants, indexed by name. The list is cached by default.
        /// </summary>
        public IProjectVariants ProjectVariants { get; private set; }

        /// <summary>
        ///     All publication folders
        /// </summary>
        public IRDList<IPublicationFolder> PublicationFolders { get; private set; }

        /// <summary>
        ///     All publication packages
        /// </summary>
        public IRDList<IPublicationPackage> PublicationPackages { get; private set; }

        /// <summary>
        ///     All publication targets
        /// </summary>
        public IRDList<IPublicationTarget> PublicationTargets { get; private set; }

        public IRecycleBin RecycleBin { get; private set; }

        public IProject Refreshed()
        {
            Refresh();
            return this;
        }

        /// <summary>
        ///     Select this project as active project in the current session.
        /// </summary>
        public void Select()
        {
            Session.SelectProject(this);
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
                                                               ? RQL.SESSIONKEY_PLACEHOLDER
                                                               : SecurityElement.Escape(infoMessage)));
            var project = (XmlElement) xmlDoc.SelectSingleNode("//PROJECT");
            if (!project.GetAttributeValue("inhibitlevel").Equals(((int) level).ToString(CultureInfo.InvariantCulture)))
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not set project lock level to {0}", level));
            }
        }

        /// <see cref="CMS.Session.SetTextContent" />
        public Guid SetTextContent(Guid textElementGuid, ILanguageVariant languageVariant, string typeString,
                                   string content)
        {
            return Session.SetTextContent(Guid, languageVariant, textElementGuid, typeString, content);
        }

        /// <summary>
        ///     All Syllables, indexed by guid. The list is cached by default.
        /// </summary>
        public ISyllables Syllables { get; private set; }

        /// <summary>
        ///     All users of the project and their access levels, indexed by user name. The list is cached by default.
        /// </summary>
        public IProjectUsers Users { get; private set; }

        public IProjectWorkflows Workflows { get; private set; }

        protected override void LoadWholeObject()
        {
            LoadXml();
        }

        protected override XmlElement RetrieveWholeObject()
        {
            if (Session.CurrentUser.ModuleAssignment.IsServerManager)
            {
                return ((Project) Session.Projects.GetByGuid(Guid)).XmlElement;
            }
            return ((Project) Session.ProjectsForCurrentUser.GetByGuid(Guid)).XmlElement;
        }

        internal XmlDocument AllFoldersXmlDocument { get; set; }

        private List<IInfoAttribute> GetInfoAttributes()
        {
            const string LOAD_INFO_ELEMENTS = @"<TEMPLATE><INFOELEMENTS action=""list""></INFOELEMENTS></TEMPLATE>";
            XmlDocument xmlDoc = ExecuteRQL(LOAD_INFO_ELEMENTS);
            var infos = xmlDoc.GetElementsByTagName("INFOELEMENTS")[0] as XmlElement;
            if (infos == null)
            {
                throw new SmartAPIException(Session.ServerLogin, "Could not load info elements");
            }
            return
                (from XmlElement info in infos.GetElementsByTagName("PAGEINFO")
                 select (IInfoAttribute) new InfoAttribute(info)).Union(
                     (from XmlElement info in infos.GetElementsByTagName("PROJECTINFO")
                      select (IInfoAttribute) new InfoAttribute(info)))
                                                                 .Union(
                                                                     (from XmlElement info in
                                                                          infos.GetElementsByTagName("SESSIONOBJECT")
                                                                      select (IInfoAttribute) new InfoAttribute(info)))
                                                                 .ToList();
        }

        private List<IKeyword> GetKeywords()
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
                (from XmlElement curNode in xmlNodes select (IKeyword) new Keyword(this, curNode)).Union(
                    categoryKeywords).ToList();
        }

        private List<IPublicationFolder> GetPublicationFolders()
        {
            const string LIST_PUBLICATION_FOLDERS = @"<PROJECT><EXPORTFOLDERS action=""list"" /></PROJECT>";

            XmlDocument xmlDoc = ExecuteRQL(LIST_PUBLICATION_FOLDERS);
            if (xmlDoc.GetElementsByTagName("EXPORTFOLDERS").Count != 1)
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not retrieve publication folders of project {0}", this));
            }
            return (from XmlElement curFolder in xmlDoc.GetElementsByTagName("EXPORTFOLDER")
                    select (IPublicationFolder) new PublicationFolder(this, curFolder.GetGuid())).ToList();
        }

        private List<IPublicationPackage> GetPublicationPackages()
        {
            const string LIST_PUBLICATION_PACKAGES = @"<PROJECT><EXPORTPACKET action=""list""/></PROJECT>";
            XmlDocument xmlDoc = ExecuteRQL(LIST_PUBLICATION_PACKAGES);
            return (from XmlElement curPackage in xmlDoc.GetElementsByTagName("EXPORTPACKET")
                    select (IPublicationPackage) new PublicationPackage(this, curPackage.GetGuid())).ToList();
        }

        private List<IPublicationTarget> GetPublicationTargets()
        {
            const string LIST_PUBLISHING_TARGETS = @"<PROJECT><EXPORTS action=""list""/></PROJECT>";

            XmlDocument xmlDoc = ExecuteRQL(LIST_PUBLISHING_TARGETS);
            return (from XmlElement curElement in xmlDoc.GetElementsByTagName("EXPORT")
                    select (IPublicationTarget) new PublicationTarget(this, curElement)).ToList();
        }

        private void Init()
        {
            //Clipboard = new Clipboard(this);
            RecycleBin = new RecycleBin(this);
            PublicationTargets = new RDList<IPublicationTarget>(GetPublicationTargets, Caching.Enabled);
            PublicationFolders = new RDList<IPublicationFolder>(GetPublicationFolders, Caching.Enabled);
            PublicationPackages = new RDList<IPublicationPackage>(GetPublicationPackages, Caching.Enabled);
            InfoAttributes = new IndexedCachedList<int, IInfoAttribute>(GetInfoAttributes, x => x.Id, Caching.Enabled);

            ContentClassFolders = new ContentClassFolders(this, Caching.Enabled);
            Folders = new Folders(this, Caching.Enabled);
            ProjectVariants = new ProjectVariants(this, Caching.Enabled);
            LanguageVariants = new LanguageVariants(this, Caching.Enabled);

            DatabaseConnections = new DatabaseConnections(this, Caching.Enabled);
            Syllables = new Syllables(this, Caching.Enabled);
            Users = new ProjectUsers(this, Caching.Enabled);

            Workflows = new ProjectWorkflow(this, Caching.Enabled);
            Categories = new Categories(this);
            Keywords = new RDList<IKeyword>(GetKeywords, Caching.Enabled);
            AssignedGroups = new ProjectGroups(this, Caching.Enabled);

            AuthorizationPackages = new AuthorizationPackages(this);
        }

        private void LoadXml()
        {
            InitIfPresent(ref _locklevel, "inhibitlevel", x => (ProjectLockLevel) int.Parse(x));
            InitIfPresent(ref _isLockedBySystem, "lockedbysystem", BoolConvert);
        }
    }

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

    public enum NewProjectType
    {
        NormalProject = 0,
        TestProject = 1
    }
}