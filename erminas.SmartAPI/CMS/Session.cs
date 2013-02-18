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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using erminas.SmartAPI.CMS.Administration;
using erminas.SmartAPI.CMS.Administration.Language;
using erminas.SmartAPI.CMS.Project;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.RedDotCmsXmlServer;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;
using log4net;
using Module = erminas.SmartAPI.CMS.Administration.Module;

namespace erminas.SmartAPI.CMS
{
    /// <summary>
    ///     Session, representing a connection to a red dot server as a specified user.
    /// </summary>
    public class Session : IDisposable
    {
        #region IODataFormat enum

        /// <summary>
        ///     TypeId of query formats. Determines usage/placement of logon guid/session key in a query.
        /// </summary>
        public enum IODataFormat
        {
            /// <summary>
            ///     Only use the logon guid in the IODATA element. Insert the query into the IODATA element.
            /// </summary>
            LogonGuidOnly,

            /// <summary>
            ///     Use the session key and the logon guid in the IODATA element. Insert the query into the IODATA element.
            /// </summary>
            SessionKeyAndLogonGuid,

            /// <summary>
            ///     Use the logon guid in the IODATA element and insert a PROJECT element with the session key. The query gets inserted into the PROJECT element
            /// </summary>
            SessionKeyInProjectElement,

            /// <summary>
            ///     Insert the query into a plain IODATA element.
            /// </summary>
            Plain,

            /// <summary>
            ///     Use session key, logon guid and format="1" in the IODATA element. Insert the query into the IODATA element.
            /// </summary>
            FormattedText,
            SessionKeyOnly
        }

        #endregion

        public const string SESSIONKEY_PLACEHOLDER = "#__SESSION_KEY__#";

        private const string RQL_IODATA = "<IODATA>{0}</IODATA>";
        private const string RQL_IODATA_LOGONGUID = @"<IODATA loginguid=""{0}"">{1}</IODATA>";
        private const string RQL_IODATA_SESSIONKEY = @"<IODATA loginguid=""{0}"" sessionkey=""{1}"">{2}</IODATA>";
        private const string RQL_IODATA_SESSIONKEY_ONLY = @"<IODATA sessionkey=""{0}"" loginguid="""">{1}</IODATA>";

        private const string RQL_IODATA_PROJECT_SESSIONKEY =
            @"<IODATA loginguid=""{0}""><PROJECT sessionkey=""{1}"">{2}</PROJECT></IODATA>";

        private const string RQL_LOGIN = @"<ADMINISTRATION action=""login"" name=""{0}"" password=""{1}""></ADMINISTRATION>";

        private const string RQL_LOGIN_FORCE =
            @"<ADMINISTRATION action=""login"" name=""{0}"" password=""{1}"" loginguid=""{2}""/>";

        private const string RQL_SELECT_PROJECT =
            @"<ADMINISTRATION action=""validate"" guid=""{0}"" useragent=""script""><PROJECT guid=""{1}""/></ADMINISTRATION>";

        private const string RQL_IODATA_FORMATTED_TEXT =
            @"<IODATA loginguid=""{0}"" sessionkey=""{1}"" format=""1"">{2}</IODATA>";

        private static readonly Regex VERSION_REGEXP =
            new Regex("(Management Server&nbsp;|CMS Version )\\d+(\\.\\d+)*&nbsp;Build&nbsp;(\\d+\\.\\d+\\.\\d+\\.\\d+)");

        private static readonly ILog LOG = LogManager.GetLogger("Session");

        /// <summary>
        ///     All database servers on the server.
        /// </summary>
        public readonly NameIndexedRDList<DatabaseServer> DatabaseServers;

        /// <summary>
        ///     All projects on the server.
        /// </summary>
        public readonly NameIndexedRDList<Project.Project> Projects;

        public readonly NameIndexedRDList<User> Users;
        private User _currentUser;

        private string _loginGuidStr;
        private string _sessionKeyStr;

        private Session()
        {
            Projects = new NameIndexedRDList<Project.Project>(GetProjects, Caching.Enabled);
            DatabaseServers = new NameIndexedRDList<DatabaseServer>(GetDatabaseServers, Caching.Enabled);
            Users = new NameIndexedRDList<User>(GetUsers, Caching.Enabled);
            Modules = new IndexedRDList<ModuleType, Module>(GetModules, x => x.Type, Caching.Enabled);
            ApplicationServers = new RDList<ApplicationServer>(GetApplicationServers, Caching.Enabled);
            Locales = new IndexedCachedList<int, Locale>(GetLocales, x => x.LCID, Caching.Enabled);
            DialogLocales = new IndexedCachedList<string, Locale>(GetDialogLocales, x => x.Id, Caching.Enabled);
        }
        public enum UseVersioning
        {
            Yes = -1,
            No = 0
        }
        public enum ProjectType
        {
            TestProject = 1,
            LiveProject = 0
        }
        //todo extrahieren und creation type machen mit default values, oberklasse fuer dingens
        public Project.Project CreateProjectMsSql(string projectName, ApplicationServer appServer, DatabaseServer dbServer, string databaseName, Locale language, ProjectType type, UseVersioning useVersioning, User user)
        {
            const string CREATE_PROJECT =
                @"<ADMINISTRATION><PROJECT action=""addnew"" projectname=""{0}"" databaseserverguid=""{1}"" editorialserverguid=""{2}"" databasename=""{3}""
versioning=""{4}"" testproject=""{5}""><LANGUAGEVARIANTS><LANGUAGEVARIANT language=""{7}"" name=""{8}"" /></LANGUAGEVARIANTS><USERS><USER action=""assign"" guid=""{6}""/></USERS></PROJECT></ADMINISTRATION>";

            var result = ParseRQLResult(ExecuteRql(CREATE_PROJECT.RQLFormat(projectName, dbServer, appServer, databaseName, (int) useVersioning,
                                                (int) type, user, language.Id, language.Language), IODataFormat.SessionKeyAndLogonGuid));

            var guidStr = result.InnerText;
            Guid projectGuid;
            if (!Guid.TryParse(guidStr, out projectGuid))
            {
                throw new SmartAPIException(ServerLogin, string.Format("Could not create project {0}", projectName));
            }

            Projects.InvalidateCache();
            return new Project.Project(this, projectGuid);
        }

        /// <summary>
        ///     Create a new session. Will use a new session key, even if the user is already logged in. If you want to create a session from a red dot plugin with an existing sesssion key, use Session(ServerLogin, String, String, String) instead.
        /// </summary>
        /// <param name="login"> Login data </param>
        public Session(ServerLogin login) : this()
        {
            ServerLogin = login;
            Login();
        }

        /// <summary>
        ///     Create an session object for an already existing session on the server, e.g. when opening a plugin from within a running session.
        /// </summary>
        public Session(ServerLogin login, Guid loginGuid, Guid sessionKey, Guid projectGuid) : this()
        {
            ServerLogin = login;
            _loginGuidStr = loginGuid.ToRQLString();
            _sessionKeyStr = sessionKey.ToRQLString();

            InitConnection();
            SelectProject(projectGuid);
        }

        #region CONFIG

        /// <summary>
        ///     Forcelogin=true means that if the user was already logged in the old session will be closed and a new one started.
        /// </summary>
        private const bool FORCE_LOGIN = true;

        #endregion

        public IRDList<ApplicationServer> ApplicationServers { get; private set; }

        /// <summary>
        ///     The currently connected user.
        /// </summary>
        public User CurrentUser
        {
            get { return _currentUser ?? (_currentUser = GetCurrentUser()); }
            private set { _currentUser = value; }
        }

        public IndexedCachedList<string, Locale> DialogLocales { get; private set; }

        /// <summary>
        ///     Close session on the server and disconnect
        /// </summary>
        public void Dispose()
        {
            try
            {
                Logout(LogonGuid);

                // invalidate this object
                LogonGuid = default(Guid);
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch
                // ReSharper restore EmptyGeneralCatchClause
            {
                // exceptions are no longer relevant
            }
        }

        /// <summary>
        ///     Select a project and execute an RQL query in its context.
        /// </summary>
        /// <param name="query"> The query string without the IODATA element </param>
        /// <param name="projectGuid"> Guid of the project </param>
        /// <returns> An XmlDocument containing the answer of the RedDot server </returns>
        /// <exception cref="Exception">Thrown, if the project couldn't get selected or an invalid response was received from the server</exception>
        /// TODO: Use different exceptions
        public XmlDocument ExecuteRQL(string query, Guid projectGuid)
        {
            SelectProject(projectGuid);
            string result = ExecuteRql(query, IODataFormat.SessionKeyAndLogonGuid);
            return ParseRQLResult(result);
        }

        /// <summary>
        ///     Execute an RQL query on the server and get its results.
        /// </summary>
        /// <param name="query"> The RQL query string without the IODATA element </param>
        /// <returns> A XmlDocument containing the answer of the RedDot server </returns>
        public XmlDocument ExecuteRQL(string query)
        {
            string result = ExecuteRql(query, IODataFormat.LogonGuidOnly);
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(result);
                return xmlDoc;
            } catch (Exception e)
            {
                throw new SmartAPIException(ServerLogin, "Illegal response from server", e);
            }
        }

        /// <summary>
        ///     Select a project and execute an RQL query in its context. The query gets embedded in a PROJECT element.
        /// </summary>
        /// <param name="projectGuid"> Guid of the project </param>
        /// <param name="query"> The RQL query string without the IODATA and PROJECT elements </param>
        /// <returns> A XmlDocument containing the answer of the RedDot server </returns>
        public XmlDocument ExecuteRQLProject(Guid projectGuid, string query)
        {
            SelectProject(projectGuid);
            string result = ExecuteRql(query, IODataFormat.SessionKeyInProjectElement);
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(result);
                return xmlDoc;
            } catch (Exception e)
            {
                LOG.Error("Illegal response from server: '" + result + "'", e);
                throw new SmartAPIException(ServerLogin, "Illegal response from server", e);
            }
        }

        /// <summary>
        ///     Execute an RQL statement. The format of the query (usage of session key/logon guid can be chosen).
        /// </summary>
        /// <param name="query"> Statement to execute </param>
        /// <param name="ioDataFormat"> Defines the format of the iodata element / placement of sessionkey of the RQL query </param>
        /// <returns> String returned from the server </returns>
        public string ExecuteRql(string query, IODataFormat ioDataFormat)
        {
            string tmpQuery = query.Replace(SESSIONKEY_PLACEHOLDER, "#" + _sessionKeyStr);
            string rqlQuery;
            switch (ioDataFormat)
            {
                case IODataFormat.SessionKeyAndLogonGuid:
                    rqlQuery = string.Format(RQL_IODATA_SESSIONKEY, _loginGuidStr, _sessionKeyStr, tmpQuery);
                    break;
                case IODataFormat.LogonGuidOnly:
                    rqlQuery = string.Format(RQL_IODATA_LOGONGUID, _loginGuidStr, tmpQuery);
                    break;
                case IODataFormat.Plain:
                    rqlQuery = string.Format(RQL_IODATA, tmpQuery);
                    break;
                case IODataFormat.SessionKeyInProjectElement:
                    rqlQuery = string.Format(RQL_IODATA_PROJECT_SESSIONKEY, _loginGuidStr, _sessionKeyStr, tmpQuery);
                    break;
                case IODataFormat.FormattedText:
                    rqlQuery = string.Format(RQL_IODATA_FORMATTED_TEXT, _loginGuidStr, _sessionKeyStr, tmpQuery);
                    break;

                case IODataFormat.SessionKeyOnly:
                    rqlQuery = string.Format(RQL_IODATA_SESSIONKEY_ONLY, _sessionKeyStr, tmpQuery);
                    break;
                default:
                    throw new ArgumentException(String.Format("Unknown IODataFormat: {0}", ioDataFormat));
            }
            return SendRQLToServer(rqlQuery);
        }

        /// <summary>
        ///     Get a project by Guid. The difference between new Project(Session, Guid) and this is that this uses a cached list of all projects to retrieve the project, while new Project() leads to a complete (albeit lazy) reload of all the project information.
        /// </summary>
        /// <param name="guid"> Guid of the project </param>
        /// <returns> Project with Guid guid </returns>
        /// <exception cref="Exception">Thrown if no project with Guid==guid could be found</exception>
        public Project.Project GetProject(Guid guid)
        {
            Project.Project project = Projects.FirstOrDefault(x => x.Guid.Equals(guid));
            if (project == null)
            {
                throw new SmartAPIException(ServerLogin, "No Project with Guid {0} found.".RQLFormat(guid));
            }

            return project;
        }

        /// <summary>
        ///     Get all projects a specific user has access to
        /// </summary>
        /// <param name="userGuid"> Guid of the user </param>
        /// <returns> All projects the user with Guid==userGuid has access to </returns>
        public List<Project.Project> GetProjectsForUser(Guid userGuid)
        {
            const string LIST_PROJECTS_FOR_USER =
                @"<ADMINISTRATION><USER guid=""{0}""><PROJECTS action=""list"" extendedinfo=""1""/></USER></ADMINISTRATION>";
            XmlDocument xmlDoc = ExecuteRQL(String.Format(LIST_PROJECTS_FOR_USER, userGuid.ToRQLString()));
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("PROJECT");
            return (from XmlElement curNode in xmlNodes select new Project.Project(this, curNode)).ToList();
        }

        /// <summary>
        ///     Get the text content of a text element. This method exists, because it needs a different RQL element layout than all other queries.
        /// </summary>
        /// <param name="projectGuid"> Guid of the project containing the element </param>
        /// <param name="lang"> Language variant to get the text from </param>
        /// <param name="elementGuid"> Guid of the text element </param>
        /// <param name="typeString"> texttype value </param>
        /// <returns> text content of the element </returns>
        public string GetTextContent(Guid projectGuid, LanguageVariant lang, Guid elementGuid, string typeString)
        {
            const string LOAD_TEXT_CONTENT =
                @"<IODATA loginguid=""{0}"" format=""1"" sessionkey=""{1}""><PROJECT><TEXT action=""load"" guid=""{2}"" texttype=""{3}""/></PROJECT></IODATA>";
            SelectProject(projectGuid);
            lang.Select();
            return
                SendRQLToServer(string.Format(LOAD_TEXT_CONTENT, _loginGuidStr, _sessionKeyStr,
                                              elementGuid.ToRQLString(), typeString));
        }

        /// <summary>
        ///     Get user by guid. The difference to new User(..., Guid) is that this method immmediatly checks wether the user exists.
        /// </summary>
        /// <param name="guid"> Guid of the user </param>
        /// <exception cref="Exception">Thrown, if no user with Guid==guid could be found</exception>
        public User GetUser(Guid guid)
        {
            const string LOAD_USER = @"<ADMINISTRATION><USER action=""load"" guid=""{0}""/></ADMINISTRATION>";
            XmlDocument xmlDoc = ExecuteRQL(string.Format(LOAD_USER, guid.ToRQLString()));
            var userElement = (XmlElement) xmlDoc.GetElementsByTagName("USER")[0];
            if (userElement == null)
            {
                throw new SmartAPIException(ServerLogin, "could not load user: " + guid.ToRQLString());
            }
            return new User(this, Guid.Parse(userElement.GetAttributeValue("guid")));
        }

        /// <summary>
        ///     All locales, indexed by LCID. The list is cached by default.
        /// </summary>
        public IIndexedCachedList<int, Locale> Locales { get; private set; }

        public Guid LogonGuid
        {
            get { return Guid.Parse(_loginGuidStr); }
            private set { _loginGuidStr = value.ToRQLString(); }
        }

        /// <summary>
        ///     All available CMS modules (e.g. SmartTree, SmartEdit, Tasks ...), indexed by ModuleType. The list is cached by default.
        /// </summary>
        public IndexedRDList<ModuleType, Module> Modules { get; private set; }

        /// <summary>
        ///     Select a project. Subsequent queries will be executed in the context of this project.
        /// </summary>
        /// <param name="projectGuid"> Guid of the project to select </param>
        public void SelectProject(Guid projectGuid)
        {
            if (SelectedProjectGuid.Equals(projectGuid))
            {
                return;
            }
            string result;
            try
            {
                result =
                    ExecuteRql(
                        string.Format(RQL_SELECT_PROJECT, _loginGuidStr, projectGuid.ToRQLString().ToUpperInvariant()),
                        IODataFormat.LogonGuidOnly);
            } catch (RQLException e)
            {
                result = e.Response;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(result);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("SERVER");
            if (xmlNodes.Count > 0)
            {
                SessionKey = ((XmlElement) xmlNodes[0]).GetGuid("key");
                SelectedProjectGuid = projectGuid;
                return;
            }

            throw new SmartAPIException(ServerLogin,
                                        String.Format("Couldn't select project {0}", projectGuid.ToRQLString()));
        }

        /// <summary>
        ///     Select a project as active project (RQL queries will be evaluated in the context of this project).
        /// </summary>
        /// <param name="project"> Project to select </param>
        /// <exception cref="Exception">Thrown, if the project could not get selected.</exception>
        public void SelectProject(Project.Project project)
        {
            SelectProject(project.Guid);
        }

        /// <summary>
        ///     Get/Set the currently selected project.
        /// </summary>
        public Project.Project SelectedProject
        {
            get { return Projects.FirstOrDefault(x => x.Guid == SelectedProjectGuid); }
            set { SelectProject(value); }
        }

        public Guid SelectedProjectGuid { get; private set; }

        public void SendMailFromCurrentUserAccount(EMail mail)
        {
            SendEmail(CurrentUser.EMail, mail);
        }

        public void SendMailFromSystemAccount(EMail mail)
        {
            var server = ApplicationServers.First();
            var fromAddress = server.From;

            SendEmail(fromAddress, mail);
        }

        /// <summary>
        ///     Login information of the session
        /// </summary>
        public ServerLogin ServerLogin { get; private set; }

        public Guid SessionKey
        {
            get
            {
                if (_sessionKeyStr == null)
                {
                    throw new SmartAPIInternalException("No session key available");
                }
                return Guid.Parse(_sessionKeyStr);
            }
            private set { _sessionKeyStr = value.ToRQLString(); }
        }

        /// <summary>
        ///     Set the text content of a text element. This method exists, because it needs a different RQL element layout than all other queries.
        /// </summary>
        /// <param name="projectGuid"> Guid of the project containing the element </param>
        /// <param name="languageVariant"> Language variant for setting the text in </param>
        /// <param name="textElementGuid"> Guid of the text element </param>
        /// <param name="typeString"> texttype value </param>
        /// <param name="content"> new value </param>
        /// <returns> Guid of the text element </returns>
        public Guid SetTextContent(Guid projectGuid, LanguageVariant languageVariant, Guid textElementGuid,
                                   string typeString, string content)
        {
            const string SAVE_TEXT_CONTENT =
                @"<IODATA loginguid=""{0}"" format=""1"" sessionkey=""{1}""><PROJECT><TEXT action=""save"" guid=""{2}"" texttype=""{3}"" >{4}</TEXT></PROJECT></IODATA>";
            SelectProject(projectGuid);
            languageVariant.Select();
            string rqlResult =
                SendRQLToServer(string.Format(SAVE_TEXT_CONTENT, _loginGuidStr, _sessionKeyStr,
                                              textElementGuid == Guid.Empty ? "" : textElementGuid.ToRQLString(),
                                              typeString, HttpUtility.HtmlEncode(content)));

            string resultGuidStr = XElement.Load(new StringReader(rqlResult)).Value;
            Guid newGuid;
            if (!Guid.TryParse(resultGuidStr, out newGuid) ||
                (textElementGuid != Guid.Empty && textElementGuid != newGuid))
            {
                throw new SmartAPIException(ServerLogin, "Could not set text for: {0}".RQLFormat(textElementGuid));
            }
            return newGuid;
        }

        public Version Version { get; private set; }
        private string CmsServerConnectionUrl { get; set; }

        internal void EnsureVersion()
        {
            var stack = new StackTrace();
// ReSharper disable PossibleNullReferenceException
            StackFrame stackFrame = stack.GetFrames()[1];
// ReSharper restore PossibleNullReferenceException
            MethodBase methodBase = stackFrame.GetMethod();
            MemberInfo info = methodBase;
            if (methodBase.IsSpecialName && (methodBase.Name.StartsWith("get_") || methodBase.Name.StartsWith("set_")))
            {
// ReSharper disable PossibleNullReferenceException
                info = methodBase.DeclaringType.GetProperty(methodBase.Name.Substring(4),
                                                            //the .Substring strips get_/set_ prefixes that get generated for properties
// ReSharper restore PossibleNullReferenceException
                                                            BindingFlags.DeclaredOnly | BindingFlags.Public |
                                                            BindingFlags.Instance | BindingFlags.NonPublic);
            }

            object[] lessThanAttributes = info.GetCustomAttributes(typeof (VersionIsLessThan), false);
            object[] greaterOrEqualAttributes = info.GetCustomAttributes(typeof (VersionIsGreaterThanOrEqual), false);
            if (lessThanAttributes.Count() != 1 && greaterOrEqualAttributes.Count() != 1)
            {
                throw new SmartAPIInternalException(string.Format("Missing version constraint attributes on {0}", info));
            }

            if (lessThanAttributes.Any())
            {
                lessThanAttributes.Cast<VersionIsLessThan>().First().Validate(ServerLogin, Version, info.Name);
            }

            if (greaterOrEqualAttributes.Any())
            {
                greaterOrEqualAttributes.Cast<VersionIsGreaterThanOrEqual>()
                                        .First()
                                        .Validate(ServerLogin, Version, info.Name);
            }
        }

        private static string CheckAlreadyLoggedIn(XmlElement xmlElement)
        {
            return xmlElement.GetAttributeValue("loginguid") ?? "";
        }

        private void CheckLoginResponse(XmlDocument xmlDoc)
        {
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("LOGIN");

            if (xmlNodes.Count > 0)
            {
                ParseLoginResponse(xmlNodes, ServerLogin.AuthData, xmlDoc);
            }
            else
            {
                // didn't get a valid logon xml node
                throw new RedDotConnectionException(RedDotConnectionException.FailureTypes.CouldNotLogin,
                                                    "Could not login.");
            }
        }

        private List<ApplicationServer> GetApplicationServers()
        {
            const string LIST_APPLICATION_SERVERS =
                @"<ADMINISTRATION><EDITORIALSERVERS action=""list""/></ADMINISTRATION>";
            var xmlDoc = ExecuteRQL(LIST_APPLICATION_SERVERS);

            var editorialServers = xmlDoc.GetElementsByTagName("EDITORIALSERVER");
            return (from XmlElement curServer in editorialServers
                    select
                        new ApplicationServer(this, curServer.GetGuid())
                            {
                                Name = curServer.GetName(),
                                IpAddress = curServer.GetAttributeValue("ip")
                            }).ToList();
            //<EDITORIALSERVER guid="ECB521367521474BA469D3A0FC98A798" name="rd75" active="0" ip="192.168.44.44" />
        }

        private User GetCurrentUser()
        {
            const string SESSION_INFO = @"<PROJECT sessionkey=""{0}""><USER action=""sessioninfo""/></PROJECT>";
            string reply = ExecuteRql(SESSION_INFO.RQLFormat(_sessionKeyStr), IODataFormat.Plain);

            var doc = new XmlDocument();
            doc.LoadXml(reply);

            var userElement = (XmlElement) doc.SelectSingleNode("/IODATA/USER");
            return new User(this, userElement.GetGuid()) {Name = userElement.GetName()};
        }

        private List<DatabaseServer> GetDatabaseServers()
        {
            const string LIST_DATABASE_SERVERS = @"<ADMINISTRATION><DATABASESERVERS action=""list""/></ADMINISTRATION>";
            XmlDocument xmlDoc = ExecuteRQL(LIST_DATABASE_SERVERS);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("DATABASESERVER");
            return (from XmlElement curNode in xmlNodes select new DatabaseServer(this, curNode)).ToList();
        }

        private List<Locale> GetDialogLocales()
        {
            const string LOAD_DIALOG_LANGUAGES = @"<DIALOG action=""listlanguages"" orderby=""2""/>";
            var resultStr = ExecuteRql(LOAD_DIALOG_LANGUAGES, IODataFormat.LogonGuidOnly);
            var xmlDoc = ParseRQLResult(resultStr);

            return
                (from XmlElement curElement in xmlDoc.GetElementsByTagName("LIST") select new Locale(this, curElement))
                    .ToList();
        }

        private XmlElement GetForceLoginXmlNode(PasswordAuthentication pa, string oldLoginGuid)
        {
            LOG.InfoFormat("User login will be forced. Old login guid was: {0}", oldLoginGuid);
            //hide user password in log message
            string rql = string.Format(RQL_IODATA,
                                       string.Format(RQL_LOGIN_FORCE, pa.Username, pa.Password, oldLoginGuid));
            string debugRQLOutput = string.Format(RQL_IODATA,
                                                  string.Format(RQL_LOGIN_FORCE, pa.Username, "*****", oldLoginGuid));
            string result = SendRQLToServer(rql, debugRQLOutput);
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(result);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("LOGIN");
            return (XmlElement) (xmlNodes.Count > 0 ? xmlNodes[0] : null);
        }

        private List<Locale> GetLocales()
        {
            const string LOAD_LOCALES = @"<LANGUAGE action=""list""/>";
            XmlDocument xmlDoc = ExecuteRQL(LOAD_LOCALES);
            var languages = xmlDoc.GetElementsByTagName("LANGUAGES")[0] as XmlElement;
            if (languages == null)
            {
                throw new SmartAPIException(ServerLogin, "Could not load languages");
            }

            return
                (from XmlElement item in languages.GetElementsByTagName("LIST") select new Locale(this, item)).ToList();
        }

        private XmlDocument GetLoginResponse()
        {
            PasswordAuthentication authData = ServerLogin.AuthData;
            string rql = string.Format(RQL_IODATA, string.Format(RQL_LOGIN, HttpUtility.HtmlEncode(authData.Username), HttpUtility.HtmlEncode(authData.Password)));

            //hide password in log messages
            string debugOutputRQL = string.Format(RQL_IODATA, string.Format(RQL_LOGIN, HttpUtility.HtmlEncode(authData.Username), "*****"));
            var xmlDoc = new XmlDocument();
            try
            {
                string result = SendRQLToServer(rql, debugOutputRQL);
                xmlDoc.LoadXml(result);
            } catch (RQLException e)
            {
                if (e.ErrorCode != ErrorCode.RDError101)
                {
                    throw;
                }
                xmlDoc.LoadXml(e.Response);
            }
            return xmlDoc;
        }

        private List<Module> GetModules()
        {
            const string LIST_MODULES = @"<ADMINISTRATION><MODULES action=""list"" /></ADMINISTRATION>";
            var xmlDoc = ExecuteRQL(LIST_MODULES);

            //we need to create an intermediate list, because the XmlNodeList returned by GetElementsByTagName gets changed in the linq/ToList() expression.
            //the change to the list occurs due to the cloning on the XmlElements in Module->AbstractAttributeContainer c'tor.
            //i have no idea why that changes the list as the same approach works without a problem everywhere else without the need for the intermediate list.
            var moduleElements = xmlDoc.GetElementsByTagName("MODULE").OfType<XmlElement>().ToList();
            return (from XmlElement curModule in moduleElements select new Module(this, curModule)).ToList();
        }

        private List<Project.Project> GetProjects()
        {
            const string LIST_PROJECTS = @"<ADMINISTRATION><PROJECTS action=""list""/></ADMINISTRATION>";
            XmlDocument xmlDoc = ExecuteRQL(LIST_PROJECTS);
            XmlNodeList projectNodes = xmlDoc.GetElementsByTagName("PROJECT");
            return (from XmlElement curNode in projectNodes select new Project.Project(this, curNode)).ToList();
        }

        private List<User> GetUsers()
        {
            const string LIST_USERS = @"<ADMINISTRATION><USERS action=""list""/></ADMINISTRATION>";
            var userListDoc = ExecuteRQL(LIST_USERS);

            return
                (from XmlElement curUserElement in userListDoc.GetElementsByTagName("USER")
                 select new User(this, curUserElement)).ToList();
        }

        private void InitConnection()
        {
            string baseURL = ServerLogin.Address.ToString();
            if (!baseURL.EndsWith("/"))
            {
                baseURL += "/";
            }
            string versionURI = baseURL + "ioVersionInfo.asp";
            try
            {
                using (var client = new WebClient())
                {
                    string responseText = client.DownloadString(versionURI);
                    Match match = VERSION_REGEXP.Match(responseText);
                    if (match.Groups.Count != 4)
                    {
                        throw new RedDotConnectionException(RedDotConnectionException.FailureTypes.ServerNotFound,
                                                            "Could not retrieve version info of RedDot server at " +
                                                            baseURL + "\n" + responseText);
                    }

                    Version = new Version(match.Groups[3].Value);
                    CmsServerConnectionUrl = baseURL +
                                             (Version.Major < 11
                                                  ? "webservice/RDCMSXMLServer.WSDL"
                                                  : "WebService/RQLWebService.svc");
                }
            } catch (RedDotConnectionException)
            {
                throw;
            } catch (WebException e)
            {
                throw new RedDotConnectionException(RedDotConnectionException.FailureTypes.ServerNotFound,
                                                    "Could not retrieve version info of RedDot server at " + baseURL +
                                                    "\n" + e.Message, e);
            } catch (Exception e)
            {
                throw new RedDotConnectionException(RedDotConnectionException.FailureTypes.Unknown,
                                                    "Could not retrieve version info of RedDot server at " + baseURL +
                                                    "\n" + e.Message, e);
            }
        }

        private void LoadSelectedProject(XmlDocument xmlDoc)
        {
            var lastModule = (XmlElement) xmlDoc.SelectSingleNode("/IODATA/USER/LASTMODULES/MODULE[@last='1']");
            if (lastModule == null)
            {
                return;
            }

            string projectStr = lastModule.GetAttributeValue("project");
            if (!string.IsNullOrEmpty(projectStr))
            {
                SelectProject(Guid.Parse(projectStr));
            }
        }

        private void Login()
        {
            InitConnection();

            var xmlDoc = GetLoginResponse();

            CheckLoginResponse(xmlDoc);

            LoadSelectedProject(xmlDoc);
        }

        private void Logout(Guid logonGuid)
        {
            const string RQL_LOGOUT = @"<ADMINISTRATION><LOGOUT guid=""{0}""/></ADMINISTRATION>";
            ExecuteRql(string.Format(RQL_LOGOUT, logonGuid.ToRQLString()), IODataFormat.LogonGuidOnly);
        }

        private void ParseLoginResponse(XmlNodeList xmlNodes, PasswordAuthentication authData, XmlDocument xmlDoc)
        {
            // check if already logged in
            var xmlNode = (XmlElement) xmlNodes[0];
            string oldLoginGuid = CheckAlreadyLoggedIn(xmlNode);
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            if (oldLoginGuid != "" && !FORCE_LOGIN) // ReSharper restore ConditionIsAlwaysTrueOrFalse
            {
                throw new RedDotConnectionException(RedDotConnectionException.FailureTypes.AlreadyLoggedIn,
                                                    "User already logged in.");
            }
            if (oldLoginGuid != "")
            {
                // forcelogin is true -> force the login
                xmlNode = GetForceLoginXmlNode(authData, oldLoginGuid);
                if (xmlNode == null)
                {
                    throw new RedDotConnectionException(RedDotConnectionException.FailureTypes.CouldNotLogin,
                                                        "Could not force login.");
                }
            }

            // here xmlNode has a valid login guid
            string loginGuid = xmlNode.GetAttributeValue("guid");
            if (string.IsNullOrEmpty(loginGuid))
            {
                throw new RedDotConnectionException(RedDotConnectionException.FailureTypes.CouldNotLogin,
                                                    "Could not login");
            }
            LogonGuid = Guid.Parse(loginGuid);

            var loginNode = (XmlElement) xmlNodes[0];
            string userGuidStr = loginNode.GetAttributeValue("userguid");
            if (string.IsNullOrEmpty(userGuidStr))
            {
                XmlNodeList userNodes = xmlDoc.GetElementsByTagName("USER");
                if (userNodes.Count != 1)
                {
                    throw new RedDotConnectionException(RedDotConnectionException.FailureTypes.CouldNotLogin,
                                                        "Could not login; Invalid user data");
                }
                CurrentUser = new User(this, Guid.Parse(((XmlElement) userNodes[0]).GetAttributeValue("guid")));
            }
            else
            {
                CurrentUser = new User(this, Guid.Parse(loginNode.GetAttributeValue("userguid")));
            }
        }

        private XmlDocument ParseRQLResult(string result)
        {
            var xmlDoc = new XmlDocument();

            if (!result.Trim().Any())
            {
                return xmlDoc;
            }

            try
            {
                xmlDoc.LoadXml(result);
                return xmlDoc;
            } catch (Exception e)
            {
                throw new SmartAPIException(ServerLogin, "Illegal response from server", e);
            }
        }

        private void SendEmail(string fromAddress, EMail mail)
        {
            //@"<ADMINISTRATION action=""sendmail"" to=""{0}"" subject=""{1}"" message=""{2}"" from=""{3}"" plaintext=""1"">{2}</ADMINISTRATION>";
            const string SEND_EMAIL =
                @"<ADMINISTRATION action=""sendmail"" to=""{0}"" subject=""{1}"" from=""{3}"" plaintext=""1"">{2}</ADMINISTRATION>";

            ExecuteRQL(SEND_EMAIL.RQLFormat(mail.To, mail.HtmlEncodedSubject, mail.HtmlEncodedMessage, fromAddress));
        }

        /// <summary>
        ///     Send RQL statement to CMS server and return result.
        /// </summary>
        /// <param name="rqlQuery"> Query to send to CMS server </param>
        /// <param name="debugRQLQuery"> Query to save in log file (this is used to hide passwords in the log files) </param>
        /// <exception cref="RedDotConnectionException">CMS Server not found or couldn't establish connection</exception>
        /// <returns> Result of RQL query </returns>
        private string SendRQLToServer(string rqlQuery, string debugRQLQuery = null)
        {
            try
            {
                LOG.DebugFormat("Sending RQL [{0}]: {1}", ServerLogin.Name, debugRQLQuery ?? rqlQuery);

                object error = "x";
                object resultInfo = "";

                var binding = new BasicHttpBinding();
                binding.ReaderQuotas.MaxStringContentLength = 2097152*10; //20MB
                binding.ReaderQuotas.MaxArrayLength = 2097152*10; //20mb
                binding.MaxReceivedMessageSize = 2097152*10; //20mb
                binding.ReceiveTimeout = TimeSpan.FromMinutes(3);
                binding.SendTimeout = TimeSpan.FromMinutes(3);

                var add = new EndpointAddress(CmsServerConnectionUrl);

                try
                {
                    var client = new XmlServerSoapPortClient(binding, add);
                    string result = client.Execute(rqlQuery, ref error, ref resultInfo);
                    string errorStr = (error ?? "").ToString();
                    if (!string.IsNullOrEmpty(errorStr))
                    {
                        throw new RQLException(ServerLogin.Name, errorStr, result);
                    }
                    LOG.DebugFormat("Received RQL [{0}]: {1}", ServerLogin.Name, result);
                    return result;
                } catch (Exception e)
                {
                    LOG.Error(e.Message);
                    LOG.Debug(e.StackTrace);
                    throw;
                }
            } catch (EndpointNotFoundException e)
            {
                LOG.ErrorFormat("Server not found: {0}", CmsServerConnectionUrl);
                throw new RedDotConnectionException(RedDotConnectionException.FailureTypes.ServerNotFound,
                                                    string.Format(@"Server ""{0}"" not found", CmsServerConnectionUrl),
                                                    e);
            }
        }
    }

    public class ApplicationServer : PartialRedDotObject
    {
        private string _from;
        private string _ipAddress;

        public ApplicationServer(Session session, Guid guid) : base(session, guid)
        {
        }

        internal ApplicationServer(Session session, XmlElement element) : base(session, element)
        {
            LoadXml();
        }

        public string From
        {
            get { return LazyLoad(ref _from); }
            internal set { _from = value; }
        }

        public string IpAddress
        {
            get { return LazyLoad(ref _ipAddress); }
            internal set { _ipAddress = value; }
        }

        protected override void LoadWholeObject()
        {
            LoadXml();
        }

        protected override XmlElement RetrieveWholeObject()
        {
            const string LOAD_APPLICATION_SERVER =
                @"<ADMINISTRATION><EDITORIALSERVER action=""load"" guid=""{0}""/></ADMINISTRATION>";

            XmlDocument xmlDoc = Session.ExecuteRQL(LOAD_APPLICATION_SERVER.RQLFormat(this));
            return xmlDoc.GetSingleElement("EDITORIALSERVER");
        }

        private void LoadXml()
        {
            _from = XmlElement.GetAttributeValue("adress");
            _ipAddress = XmlElement.GetAttributeValue("ip");
        }
    }
}