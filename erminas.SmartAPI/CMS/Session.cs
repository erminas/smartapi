/*
 * Smart API - .Net programatical access to RedDot servers
 * Copyright (C) 2012  erminas GbR 
 *
 * This program is free software: you can redistribute it and/or modify it 
 * under the terms of the GNU General Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details. 
 *
 * You should have received a copy of the GNU General Public License along with this program.
 * If not, see <http://www.gnu.org/licenses/>. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Utils;
using log4net;

namespace erminas.SmartAPI.CMS
{
    /// <summary>
    ///   Session, representing a connection to a red dot server as a specified user.
    /// </summary>
    public class Session : IDisposable
    {
        public const string SESSIONKEY_PLACEHOLDER = "{__SESSION_KEY__}";
        private static readonly ILog LOG = LogManager.GetLogger("Session");

        /// <summary>
        ///   All database servers on the server.
        /// </summary>
        public readonly NameIndexedRDList<DatabaseServer> DatabaseServers;

        /// <summary>
        ///   All projects on the server.
        /// </summary>
        public readonly NameIndexedRDList<Project> Projects;

        private Session()
        {
            Projects = new NameIndexedRDList<Project>(GetProjects, Caching.Enabled);
            DatabaseServers = new NameIndexedRDList<DatabaseServer>(GetDatabaseServers,
                                                                    Caching.Enabled);
        }

        /// <summary>
        ///   Create a new session. Will use a new session key, even if the user is already logged in. If you want to create a session from a red dot plugin with an existing sesssion key, use Session(ServerLogin, String, String, String) instead.
        /// </summary>
        /// <param name="login"> Login data </param>
        public Session(ServerLogin login) : this()
        {
            Login = login;
            CmsClient = new CmsClient(login);
        }


        /// <summary>
        ///   Create an session object for an already existing session on the server, e.g. when opening a plugin from within a running session.
        /// </summary>
        public Session(ServerLogin login, Guid loginGuid, Guid sessionKey, Guid projectGuid)
            : this()
        {
            CmsClient = new CmsClient(login, loginGuid, sessionKey, projectGuid);
        }

        /// <summary>
        ///   The CmsClient used to communicate with the server
        /// </summary>
        public CmsClient CmsClient { get; private set; }

        /// <summary>
        ///   The current user the session is logged in with
        /// </summary>
        public User CurrentUser
        {
            get { return CmsClient.CurrentUser; }
        }

        /// <summary>
        ///   Get/Set the currently selected project.
        /// </summary>
        public Project SelectedProject
        {
            get { return Projects.FirstOrDefault(x => x.Guid == CmsClient.SelectedProjectGuid); }
            set { SelectProject(value); }
        }

        /// <summary>
        ///   Login information of the session
        /// </summary>
        public ServerLogin Login { get; private set; }

        #region IDisposable Members

        public void Dispose()
        {
            if (CmsClient != null)
            {
                CmsClient.Dispose();
                CmsClient = null;
            }
        }

        #endregion

        /// <summary>
        ///   Get a project by Guid. The difference between new Project(Session, Guid) and this is that this uses a cached list of all projects to retrieve the project, while new Project() leads to a complete (albeit lazy) reload of all the project information.
        /// </summary>
        /// <param name="guid"> Guid of the project </param>
        /// <returns> Project with Guid guid </returns>
        /// <exception cref="Exception">Thrown if no project with Guid==guid could be found</exception>
        public Project GetProject(Guid guid)
        {
            Project project = Projects.FirstOrDefault(x => x.Guid.Equals(guid));
            if (project == null)
            {
                throw new Exception("No Project with Guid: " + guid + " found.");
            }

            return project;
        }

        /// <summary>
        ///   Get a project by Name. This method is deprecated, use Projects[name] instead.
        /// </summary>
        [Obsolete("GetProject(name) is deprected, please use Projects[name] instead")]
        public Project GetProject(string name)
        {
            return Projects.Get(name);
        }

        /// <summary>
        ///   Get all projects a specific user has access to
        /// </summary>
        /// <param name="userGuid"> Guid of the user </param>
        /// <returns> All projects the user with Guid==userGuid has access to </returns>
        public List<Project> GetProjectsForUser(Guid userGuid)
        {
            const string LIST_PROJECTS_FOR_USER =
                @"<ADMINISTRATION><USER guid=""{0}""><PROJECTS action=""list"" extendedinfo=""1""/></USER></ADMINISTRATION>";
            XmlDocument xmlDoc = ExecuteRQL(String.Format(LIST_PROJECTS_FOR_USER, userGuid.ToRQLString()));
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("PROJECT");
            return (from XmlElement curNode in xmlNodes select new Project(this, curNode)).ToList();
        }

        /// <summary>
        ///   Get user by guid. The difference to new User(..., Guid) is that this method immmediatly checks wether the user exists.
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
                throw new Exception("could not load user: " + guid.ToRQLString());
            }
            return new User(CmsClient, Guid.Parse(userElement.GetAttributeValue("guid")));
        }

        /// <summary>
        ///   Select a project as active project (RQL queries will be evaluated in the context of this project).
        /// </summary>
        /// <param name="project"> Project to select </param>
        /// <exception cref="Exception">Thrown, if the project could not get selected.</exception>
        public void SelectProject(Project project)
        {
            CmsClient.SelectProject(project.Guid);
        }

        /// <summary>
        ///   Select a project and execute an RQL query in its context.
        /// </summary>
        /// <param name="query"> The query string without the IODATA element </param>
        /// <param name="projectGuid"> Guid of the project </param>
        /// <returns> An XmlDocument containing the answer of the RedDot server </returns>
        /// <exception cref="Exception">Thrown, if the project couldn't get selected or an invalid response was received from the server</exception>
        /// TODO: Use different exceptions
        public XmlDocument ExecuteRQL(string query, Guid projectGuid)
        {
            CmsClient.SelectProject(projectGuid);
            string result = CmsClient.ExecuteRql(query, CmsClient.IODataFormat.SessionKeyAndLogonGuid);
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(result);
                return xmlDoc;
            }
            catch (Exception e)
            {
                throw new Exception("Illegal response from server", e);
            }
        }

        /// <summary>
        ///   Execute an RQL query on the server and get its results.
        /// </summary>
        /// <param name="query"> The RQL query string without the IODATA element </param>
        /// <returns> A XmlDocument containing the answer of the RedDot server </returns>
        public XmlDocument ExecuteRQL(string query)
        {
            string result = CmsClient.ExecuteRql(query, CmsClient.IODataFormat.LogonGuidOnly);
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(result);
                return xmlDoc;
            }
            catch (Exception e)
            {
                throw new Exception("Illegal response from server", e);
            }
        }

        /// <summary>
        ///   Select a project and execute an RQL query in its context. The query gets embedded in a PROJECT element.
        /// </summary>
        /// <param name="projectGuid"> Guid of the project </param>
        /// <param name="query"> The RQL query string without the IODATA and PROJECT elements </param>
        /// <returns> A XmlDocument containing the answer of the RedDot server </returns>
        public XmlDocument ExecuteRQLProject(Guid projectGuid, string query)
        {
            CmsClient.SelectProject(projectGuid);
            string result = CmsClient.ExecuteRql(query, CmsClient.IODataFormat.SessionKeyInProjectElement);
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(result);
                return xmlDoc;
            }
            catch (Exception e)
            {
                LOG.Error("Illegal response from server: '" + result + "'", e);
                throw new Exception("Illegal response from server", e);
            }
        }

        /// <summary>
        ///   Select a project and execute an RQL query in its context with all instances of <see cref="SESSIONKEY_PLACEHOLDER" /> in the querystring getting replaced by #sessionkey.
        /// </summary>
        /// <param name="projectGuid"> Guid of the project </param>
        /// <param name="query"> The RQL query string without the IODATA and PROJECT elements, every instance of <see
        ///    cref="SESSIONKEY_PLACEHOLDER" /> in the string gets replaced by #sessionkey </param>
        /// <returns> A XmlDocument containing the answer of the RedDot server </returns>
        public XmlDocument ExecuteRQLReplaceSessionKey(Guid projectGuid, string query)
        {
            CmsClient.SelectProject(projectGuid);

            string result = CmsClient.ExecuteRql(query, CmsClient.IODataFormat.ReplaceSessionKeyPlaceholder);
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(result);
                return xmlDoc;
            }
            catch (Exception e)
            {
                throw new Exception("Illegal response from server", e);
            }
        }

        /// <summary>
        ///   Get the text content of a text element. This method exists, because it needs a different RQL element layout than all other queries.
        /// </summary>
        /// <param name="projectGuid"> Guid of the project containing the element </param>
        /// <param name="lang"> Language variant to get the text from </param>
        /// <param name="textElementGuid"> Guid of the text element </param>
        /// <param name="typeString"> texttype value </param>
        /// <returns> text content of the element </returns>
        public string GetTextContent(Guid projectGuid, LanguageVariant lang, Guid textElementGuid, string typeString)
        {
            return CmsClient.GetTextContent(projectGuid, lang, textElementGuid, typeString);
        }

        /// <summary>
        ///   Set the text content of a text element. This method exists, because it needs a different RQL element layout than all other queries.
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
            return CmsClient.SetTextContent(projectGuid, languageVariant, textElementGuid, typeString, content);
        }

        private List<Project> GetProjects()
        {
            const string LIST_PROJECTS = @"<ADMINISTRATION><PROJECTS action=""list""/></ADMINISTRATION>";
            XmlDocument xmlDoc = ExecuteRQL(LIST_PROJECTS);
            XmlNodeList projectNodes = xmlDoc.GetElementsByTagName("PROJECT");
            return (from XmlElement curNode in projectNodes select new Project(this, curNode)).ToList();
        }

        private List<DatabaseServer> GetDatabaseServers()
        {
            const string LIST_DATABASE_SERVERS = @"<ADMINISTRATION><DATABASESERVERS action=""list""/></ADMINISTRATION>";
            XmlDocument xmlDoc = ExecuteRQL(LIST_DATABASE_SERVERS);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("DATABASESERVER");
            return (from XmlElement curNode in xmlNodes select new DatabaseServer(this, curNode)).ToList();
        }
    }

    public class RQLException : Exception
    {
        private const string RESPONSE = "response";
        private const string SERVER = "server";
        private const string ERROR_MESSAGE = "error_message";

        public RQLException(string server, string reason, string responseXML)
        {
            Server = server;
            ErrorMessage = reason;
            Response = responseXML;
        }

        protected RQLException(RQLException rqlException)
            : this(rqlException.Server, rqlException.ErrorMessage, rqlException.Response)
        {
        }

        public string Response
        {
            get { return (Data[RESPONSE] ?? "").ToString(); }
            private set { Data.Add(RESPONSE, value); }
        }

        public string Server
        {
            get { return (Data[SERVER] ?? "").ToString(); }
            private set { Data.Add(SERVER, value); }
        }

        public string ErrorMessage
        {
            get { return (Data[ERROR_MESSAGE] ?? "").ToString(); }
            private set { Data.Add(ERROR_MESSAGE, value); }
        }

        public override string Message
        {
            get { return string.Format("RQL request to '{0}' failed. Error message: '{1}'", Server, ErrorMessage); }
        }
    }
}