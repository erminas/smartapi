// SmartAPI - .Net programmatic access to RedDot servers
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
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using erminas.SmartAPI.CMS.Project;
using erminas.SmartAPI.CMS.ServerManagement;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.RedDotCmsXmlServer;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;
using log4net;

namespace erminas.SmartAPI.CMS
{
    public class RunningSessionInfo
    {
        private readonly DateTime _lastActionDate;
        private readonly DateTime _loginDate;
        private readonly Guid _loginGuid;
        private readonly string _moduleName;
        private readonly string _projectName;

        public RunningSessionInfo(Guid loginGuid, string projectName, string moduleName, DateTime loginDate,
                                  DateTime lastActionDate)
        {
            _loginGuid = loginGuid;
            _projectName = projectName;
            _moduleName = moduleName;
            _loginDate = loginDate;
            _lastActionDate = lastActionDate;
        }

        internal RunningSessionInfo(XmlElement element)
        {
            _projectName = element.GetAttributeValue("projectname");
            _moduleName = element.GetAttributeValue("moduledescription");
            _loginDate = element.GetOADate("logindate").GetValueOrDefault();
            _lastActionDate = element.GetOADate("lastactiondate").GetValueOrDefault();
            element.TryGetGuid(out _loginGuid);
        }

        public DateTime LastActionDate
        {
            get { return _lastActionDate; }
        }

        public DateTime LoginDate
        {
            get { return _loginDate; }
        }

        public Guid LoginGuid
        {
            get { return _loginGuid; }
        }

        public string ModuleName
        {
            get { return _moduleName; }
        }

        public string ProjectName
        {
            get { return _projectName; }
        }
    }

    public interface ISession : IDisposable
    {
        IUser CurrentUser { get; }
        IndexedCachedList<string, IDialogLocale> DialogLocales { get; }

        XmlDocument ExecuteRQL(string query, RQL.IODataFormat format);

        /// <summary>
        ///     Execute an RQL query on the server and get its results.
        /// </summary>
        /// <param name="query"> The RQL query string without the IODATA element </param>
        /// <returns> A XmlDocument containing the answer of the RedDot server </returns>
        XmlDocument ExecuteRQL(string query);

        /// <summary>
        ///     Select a project and execute an RQL query in its context.
        /// </summary>
        /// <param name="query"> The query string without the IODATA element </param>
        /// <param name="projectGuid"> Guid of the project </param>
        /// <returns> An XmlDocument containing the answer of the RedDot server </returns>
        XmlDocument ExecuteRQLInProjectContext(string query, Guid projectGuid);

        XmlDocument ExecuteRQLInProjectContextAndEmbeddedInProjectElement(string query, Guid projectGuid);

        /// <summary>
        ///     Execute an RQL statement. The format of the query (usage of session key/logon guid can be chosen).
        /// </summary>
        /// <param name="query"> Statement to execute </param>
        /// <param name="RQL.IODataFormat"> Defines the format of the iodata element / placement of sessionkey of the RQL query </param>
        /// <returns> String returned from the server </returns>
        string ExecuteRQLRaw(string query, RQL.IODataFormat ioDataFormat);

        /// <summary>
        ///     Get the text content of a text element. This method exists, because it needs a different RQL element layout than all other queries.
        /// </summary>
        /// <param name="projectGuid"> Guid of the project containing the element </param>
        /// <param name="lang"> Language variant to get the text from </param>
        /// <param name="elementGuid"> Guid of the text element </param>
        /// <param name="typeString"> texttype value </param>
        /// <returns> text content of the element </returns>
        string GetTextContent(Guid projectGuid, ILanguageVariant lang, Guid elementGuid, string typeString);

        /// <summary>
        ///     All locales, indexed by LCID. The list is cached by default.
        /// </summary>
        IIndexedCachedList<int, ISystemLocale> Locales { get; }

        Guid LogonGuid { get; }

        /// <summary>
        ///     Select a project. Subsequent queries will be executed in the context of this project.
        /// </summary>
        /// <param name="projectGuid"> Guid of the project to select </param>
        void SelectProject(Guid projectGuid);

        /// <summary>
        ///     Select a project as active project (RQL queries will be evaluated in the context of this project).
        /// </summary>
        /// <param name="project"> Project to select </param>
        /// <exception cref="Exception">Thrown, if the project could not get selected.</exception>
        void SelectProject(IProject project);

        /// <summary>
        ///     Get/Set the currently selected project.
        /// </summary>
        IProject SelectedProject { get; set; }

        void SendMailFromCurrentUserAccount(EMail mail);
        void SendMailFromSystemAccount(EMail mail);

        /// <summary>
        ///     Login information of the session
        /// </summary>
        ServerLogin ServerLogin { get; }

        IServerManager ServerManager { get; }

        Version ServerVersion { get; }

        string SessionKey { get; }

        /// <summary>
        /// This is only meant for internal use! It probably won't work as you expect it, so just ignore it ;)
        /// Set the text content of a (content class) text element. This method exists, because it needs a different RQL element layout than all other queries. 
        /// </summary>
        /// <param name="projectGuid"> Guid of the project containing the element </param>
        /// <param name="languageVariant"> Language variant for setting the text in </param>
        /// <param name="textElementGuid"> Guid of the text element </param>
        /// <param name="typeString"> texttype value </param>
        /// <param name="content"> new value </param>
        /// <returns> Guid of the text element </returns>
        /// <remarks></remarks>
        Guid SetTextContent(Guid projectGuid, ILanguageVariant languageVariant, Guid textElementGuid, string typeString,
                            string content);

        ISystemLocale StandardLocale { get; }

        /// <summary>
        ///     Waits for an asynchronous process to finish.
        ///     This is done by waiting for the process to spawn (or have it available on start) and then waiting for the process to disappear from the process list.
        ///     The async processes get checked every second, for other retry periods, use
        ///     <see
        ///         cref="WaitForAsyncProcess(System.TimeSpan,System.TimeSpan,System.Predicate{ServerManagement.ServerManager.AsynchronousProcess})" />
        ///     instead.
        /// </summary>
        /// <param name="maxWait">Maximum time span to wait for the process to complete</param>
        /// <param name="processPredicate">Gets checked for every process in the list to determine the process to wait for (must return true for it and only for it)</param>
        void WaitForAsyncProcess(TimeSpan maxWait, Predicate<IAsynchronousProcess> processPredicate);

        /// <summary>
        ///     Waits for an asynchronous process to finish.
        ///     This is done by waiting for the process to spawn (or have it available on start) and then waiting for the process to disappear from the process list.
        /// </summary>
        /// <param name="maxWait">Maximum time span to wait for the process to complete</param>
        /// <param name="retry">Determines how often the async processes should be checked</param>
        /// <param name="processPredicate">Gets checked for every process in the list to determine the process to wait for (must return true for it and only for it)</param>
        void WaitForAsyncProcess(TimeSpan maxWait, TimeSpan retry, Predicate<IAsynchronousProcess> processPredicate);
    }

    public static class RQL
    {
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

        public const string SESSIONKEY_PLACEHOLDER = "#__SESSION_KEY__#";
    }

    /// <summary>
    /// Session, representing a connection to a Open Text WSM Management Server / RedDot CMS server as a specified user. 
    /// To open a session you need to use the <see cref="SessionBuilder" />.
    /// </summary>
    internal class Session : ISession
    {
        static Session()
        {
            //allow custom certificates
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
        }

        private const string RQL_IODATA = "<IODATA>{0}</IODATA>";
        private const string RQL_IODATA_LOGONGUID = @"<IODATA loginguid=""{0}"">{1}</IODATA>";
        private const string RQL_IODATA_SESSIONKEY = @"<IODATA sessionkey=""{1}"" loginguid=""{0}"">{2}</IODATA>";
        private const string RQL_IODATA_SESSIONKEY_ONLY = @"<IODATA sessionkey=""{0}"" loginguid="""">{1}</IODATA>";

        private const string RQL_IODATA_PROJECT_SESSIONKEY =
            @"<IODATA loginguid=""{0}""><PROJECT sessionkey=""{1}"">{2}</PROJECT></IODATA>";

        private const string RQL_LOGIN =
            @"<ADMINISTRATION action=""login"" name=""{0}"" password=""{1}""></ADMINISTRATION>";

        private const string RQL_LOGIN_FORCE =
            @"<ADMINISTRATION action=""login"" name=""{0}"" password=""{1}"" loginguid=""{2}""/>";

        private const string RQL_SELECT_PROJECT =
            @"<ADMINISTRATION action=""validate"" guid=""{0}"" useragent=""script""><PROJECT guid=""{1}""/></ADMINISTRATION>";

        private const string RQL_IODATA_FORMATTED_TEXT =
            @"<IODATA loginguid=""{0}"" sessionkey=""{1}"" format=""1"">{2}</IODATA>";

        private static readonly Regex VERSION_REGEXP =
            new Regex(
                "(Management Server.*&nbsp;|CMS Version )\\d+(\\.\\d+)*&nbsp;Build&nbsp;(\\d+\\.\\d+\\.\\d+\\.\\d+)");

        private static readonly ILog LOG = LogManager.GetLogger("Session");

        private string _loginGuidStr;
        private string _sessionKeyStr;

        private Session()
        {
            ServerManager = new ServerManager(this);
            Locales = new IndexedCachedList<int, ISystemLocale>(GetLocales, x => x.LCID, Caching.Enabled);
            DialogLocales = new IndexedCachedList<string, IDialogLocale>(GetDialogLocales, x => x.LanguageAbbreviation,
                                                                         Caching.Enabled);
            
        }

        public Session(ServerLogin login,
                       Func<IEnumerable<RunningSessionInfo>, RunningSessionInfo> sessionReplacementSelector) : this()
        {
            ServerLogin = login;
            Login(sessionReplacementSelector);
        }

        /// <summary>
        ///     Create an session object for an already existing session on the server, e.g. when opening a plugin from within a running session.
        /// </summary>
        public Session(ServerLogin login, Guid loginGuid, string sessionKey, Guid projectGuid) : this()
        {
            ServerLogin = login;
            _loginGuidStr = loginGuid.ToRQLString();
            _sessionKeyStr = sessionKey;

            InitConnection();
            XmlElement sessionInfo = GetUserSessionInfoElement();
            SelectedProjectGuid = sessionInfo.GetGuid("projectguid");
            SelectProject(projectGuid);
        }

        /// <summary>
        ///     The asynchronous processes running on the server. The list is _NOT_ cached by default.
        /// </summary>
        /// <remarks>
        ///     Caching is disabled by default.
        /// </remarks>
        public IRDList<IAsynchronousProcess> AsynchronousProcesses { get { return ServerManager.AsynchronousProcesses; } }

        public IUser CurrentUser
        {
            get { return ServerManager.Users.Current; }
        }

        public IndexedCachedList<string, IDialogLocale> DialogLocales { get; private set; }

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

        public XmlDocument ExecuteRQL(string query, RQL.IODataFormat format)
        {
            string result = ExecuteRQLRaw(query, format);
            return ParseRQLResult(this, result);
        }

        public XmlDocument ExecuteRQL(string query)
        {
            string result = ExecuteRQLRaw(query, RQL.IODataFormat.LogonGuidOnly);
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

        public XmlDocument ExecuteRQLInProjectContext(string query, Guid projectGuid)
        {
            SelectProject(projectGuid);
            string result = ExecuteRQLRaw(query, RQL.IODataFormat.SessionKeyAndLogonGuid);
            return ParseRQLResult(this, result);
        }

        public XmlDocument ExecuteRQLInProjectContextAndEmbeddedInProjectElement(string query, Guid projectGuid)
        {
            SelectProject(projectGuid);
            string result = ExecuteRQLRaw(query, RQL.IODataFormat.SessionKeyInProjectElement);
            return ParseRQLResult(this, result);
        }

        public string ExecuteRQLRaw(string query, RQL.IODataFormat ioDataFormat)
        {
            string tmpQuery = query.Replace(RQL.SESSIONKEY_PLACEHOLDER, "#" + _sessionKeyStr);
            string rqlQuery;
            switch (ioDataFormat)
            {
                case RQL.IODataFormat.SessionKeyAndLogonGuid:
                    rqlQuery = string.Format(RQL_IODATA_SESSIONKEY, _loginGuidStr, _sessionKeyStr, tmpQuery);
                    break;
                case RQL.IODataFormat.LogonGuidOnly:
                    rqlQuery = string.Format(RQL_IODATA_LOGONGUID, _loginGuidStr, tmpQuery);
                    break;
                case RQL.IODataFormat.Plain:
                    rqlQuery = string.Format(RQL_IODATA, tmpQuery);
                    break;
                case RQL.IODataFormat.SessionKeyInProjectElement:
                    rqlQuery = string.Format(RQL_IODATA_PROJECT_SESSIONKEY, _loginGuidStr, _sessionKeyStr, tmpQuery);
                    break;
                case RQL.IODataFormat.FormattedText:
                    rqlQuery = string.Format(RQL_IODATA_FORMATTED_TEXT, _loginGuidStr, _sessionKeyStr, tmpQuery);
                    break;

                case RQL.IODataFormat.SessionKeyOnly:
                    rqlQuery = string.Format(RQL_IODATA_SESSIONKEY_ONLY, _sessionKeyStr, tmpQuery);
                    break;
                default:
                    throw new ArgumentException(String.Format("Unknown RQL.IODataFormat: {0}", ioDataFormat));
            }
            return SendRQLToServer(rqlQuery);
        }

        /// <summary>
        ///     Get the text content of a text element. This method exists, because it needs a different RQL element layout than all other queries.
        /// </summary>
        /// <param name="projectGuid"> Guid of the project containing the element </param>
        /// <param name="lang"> Language variant to get the text from </param>
        /// <param name="elementGuid"> Guid of the text element </param>
        /// <param name="typeString"> texttype value </param>
        /// <returns> text content of the element </returns>
        public string GetTextContent(Guid projectGuid, ILanguageVariant lang, Guid elementGuid, string typeString)
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
        ///     All locales, indexed by LCID. The list is cached by default.
        /// </summary>
        public IIndexedCachedList<int, ISystemLocale> Locales { get; private set; }

        public Guid LogonGuid
        {
            get { return Guid.Parse(_loginGuidStr); }
            private set { _loginGuidStr = value.ToRQLString(); }
        }

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
            RQLException exception = null;
            try
            {
                result =
                    ExecuteRQLRaw(
                        string.Format(RQL_SELECT_PROJECT, _loginGuidStr, projectGuid.ToRQLString().ToUpperInvariant()),
                        RQL.IODataFormat.LogonGuidOnly);
            } catch (RQLException e)
            {
                exception = e;
                result = e.Response;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(result);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("SERVER");
            if (xmlNodes.Count > 0)
            {
                SessionKey = ((XmlElement) xmlNodes[0]).GetAttributeValue("key");
                SelectedProjectGuid = projectGuid;
                return;
            }

            throw new SmartAPIException(ServerLogin,
                                        String.Format("Couldn't select project {0}", projectGuid.ToRQLString()),
                                        exception);
        }

        /// <summary>
        ///     Select a project as active project (RQL queries will be evaluated in the context of this project).
        /// </summary>
        /// <param name="project"> Project to select </param>
        /// <exception cref="Exception">Thrown, if the project could not get selected.</exception>
        public void SelectProject(IProject project)
        {
            SelectProject(project.Guid);
        }

        /// <summary>
        ///     Get/Set the currently selected project.
        /// </summary>
        public IProject SelectedProject
        {
            get
            {
                return ServerManager.Users.Current.ModuleAssignment.IsServerManager
                           ? ServerManager.Projects.FirstOrDefault(x => x.Guid == SelectedProjectGuid)
                           : ServerManager.Projects.ForCurrentUser.FirstOrDefault(x => x.Guid == SelectedProjectGuid);
            }
            set { SelectProject(value); }
        }

        public Guid SelectedProjectGuid { get; private set; }

        public void SendMailFromCurrentUserAccount(EMail mail)
        {
            SendEmail(ServerManager.Users.Current.EMail, mail);
        }

        public void SendMailFromSystemAccount(EMail mail)
        {
            IApplicationServer server = ServerManager.ApplicationServers.First();
            string fromAddress = server.From;

            SendEmail(fromAddress, mail);
        }

        /// <summary>
        ///     Login information of the session
        /// </summary>
        public ServerLogin ServerLogin { get; private set; }

        public IServerManager ServerManager { get; private set; }

        public Version ServerVersion { get; private set; }

        public string SessionKey
        {
            get
            {
                if (_sessionKeyStr == null)
                {
                    throw new SmartAPIInternalException("No session key available");
                }
                return _sessionKeyStr;
            }
            private set { _sessionKeyStr = value; }
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
        public Guid SetTextContent(Guid projectGuid, ILanguageVariant languageVariant, Guid textElementGuid,
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

            //in version 11.2 the server returns <guid>\r\n, but we are only interested in the guid -> Trim()
            string resultGuidStr = XElement.Load(new StringReader(rqlResult)).Value.Trim();
            
            //result guid is empty, if the value was set to the same value it had before
            if (string.IsNullOrEmpty(resultGuidStr))
            {
                return textElementGuid;
            }

            Guid newGuid;
            if (!Guid.TryParse(resultGuidStr, out newGuid) ||
                (textElementGuid != Guid.Empty && textElementGuid != newGuid))
            {
                throw new SmartAPIException(ServerLogin, "Could not set text for: {0}".RQLFormat(textElementGuid));
            }
            return newGuid;
        }

        public ISystemLocale StandardLocale
        {
            get { return Locales.First(locale => locale.IsStandardLanguage); }
        }

        /// <summary>
        ///     Waits for an asynchronous process to finish.
        ///     This is done by waiting for the process to spawn (or have it available on start) and then waiting for the process to disappear from the process list.
        ///     The async processes get checked every second, for other retry periods, use
        ///     <see
        ///         cref="WaitForAsyncProcess(System.TimeSpan,System.TimeSpan,System.Predicate{ServerManagement.ServerManager.AsynchronousProcess})" />
        ///     instead.
        /// </summary>
        /// <param name="maxWait">Maximum time span to wait for the process to complete</param>
        /// <param name="processPredicate">Gets checked for every process in the list to determine the process to wait for (must return true for it and only for it)</param>
        public void WaitForAsyncProcess(TimeSpan maxWait, Predicate<IAsynchronousProcess> processPredicate)
        {
            var retryEverySecond = new TimeSpan(0, 0, 1);
            WaitForAsyncProcess(maxWait, retryEverySecond, processPredicate);
        }

        /// <summary>
        ///     Waits for an asynchronous process to finish.
        ///     This is done by waiting for the process to spawn (or have it available on start) and then waiting for the process to disappear from the process list.
        /// </summary>
        /// <param name="maxWait">Maximum time span to wait for the process to complete</param>
        /// <param name="retry">Determines how often the async processes should be checked</param>
        /// <param name="processPredicate">Gets checked for every process in the list to determine the process to wait for (must return true for it and only for it)</param>
        public void WaitForAsyncProcess(TimeSpan maxWait, TimeSpan retry,
                                        Predicate<IAsynchronousProcess> processPredicate)
        {
            Predicate<IRDList<IAsynchronousProcess>> pred = list => list.Any(process => processPredicate(process));

            //wait for the async process to spawn first and then wait until it is done

            DateTime start = DateTime.Now;
            var retryEvery50ms = new TimeSpan(0, 0, 0, 0, 50);
            AsynchronousProcesses.WaitFor(pred, maxWait, retryEvery50ms);

            TimeSpan timeLeft = maxWait - (DateTime.Now - start);
            timeLeft = timeLeft.TotalMilliseconds > 0 ? timeLeft : new TimeSpan(0, 0, 0);

            AsynchronousProcesses.WaitFor(list => !pred(list), timeLeft, retry);
        }

        internal XmlElement GetUserSessionInfoElement()
        {
            //TODO das funktioniert nur, wenn man in nem projekt drin ist
            const string SESSION_INFO = @"<PROJECT sessionkey=""{0}""><USER action=""sessioninfo""/></PROJECT>";
            string reply = ExecuteRQLRaw(SESSION_INFO.RQLFormat(_sessionKeyStr), RQL.IODataFormat.Plain);

            var doc = new XmlDocument();
            doc.LoadXml(reply);
            return (XmlElement) doc.SelectSingleNode("/IODATA/USER");
        }

        internal void LoginToServerManager()
        {
            const string LOGIN_TO_SERVER_MANAGER =
                @"<ADMINISTRATION><MODULE action=""login"" userguid=""{0}"" projectguid=""{1}"" id=""servermanager"" /></ADMINISTRATION>";
            ExecuteRQL(LOGIN_TO_SERVER_MANAGER.RQLFormat(ServerManager.Users.Current, SelectedProjectGuid));
            SelectedProjectGuid = Guid.Empty;
        }

        internal static XmlDocument ParseRQLResult(ISession session, string result)
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
                throw new SmartAPIException(session.ServerLogin, "Illegal response from server", e);
            }
        }

        private static string CheckAlreadyLoggedIn(XmlElement xmlElement)
        {
            return xmlElement.GetAttributeValue("loginguid") ?? "";
        }

        private void CheckLoginResponse(XmlDocument xmlDoc,
                                        Func<IEnumerable<RunningSessionInfo>, RunningSessionInfo>
                                            sesssionReplacementSelector)
        {
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("LOGIN");

            if (xmlNodes.Count > 0)
            {
                ParseLoginResponse(xmlNodes, ServerLogin.AuthData, xmlDoc, sesssionReplacementSelector);
            }
            else
            {
                // didn't get a valid logon xml node
                throw new RedDotConnectionException(RedDotConnectionException.FailureTypes.CouldNotLogin,
                                                    "Could not login.");
            }
        }

        private string CmsServerConnectionUrl { get; set; }

        private static string ExtractMessagesWithInnerExceptions(Exception e)
        {
            Exception curException = e;
            var builder = new StringBuilder();
            string linePrefix = "";
            while (curException != null)
            {
                builder.Append(linePrefix);
                builder.Append(curException.Message);
                builder.Append("\n");
                curException = curException.InnerException;
                linePrefix += "* ";
            }

            return builder.ToString();
        }

        private List<IDialogLocale> GetDialogLocales()
        {
            const string LOAD_DIALOG_LANGUAGES = @"<DIALOG action=""listlanguages"" orderby=""2""/>";
            string resultStr = ExecuteRQLRaw(LOAD_DIALOG_LANGUAGES, RQL.IODataFormat.LogonGuidOnly);
            XmlDocument xmlDoc = ParseRQLResult(this, resultStr);

            return (from XmlElement curElement in xmlDoc.GetElementsByTagName("LIST")
                    select (IDialogLocale) new DialogLocale(this, curElement)).ToList();
        }

        private XmlElement GetForceLoginXmlNode(PasswordAuthentication pa, Guid oldLoginGuid)
        {
            LOG.InfoFormat("User login will be forced. Old login guid was: {0}", oldLoginGuid.ToRQLString());
            //hide user password in log message
            string rql = string.Format(RQL_IODATA, RQL_LOGIN_FORCE.RQLFormat(pa.Username, pa.Password, oldLoginGuid));
            string debugRQLOutput = string.Format(RQL_IODATA,
                                                  RQL_LOGIN_FORCE.RQLFormat(pa.Username, "*****", oldLoginGuid));
            string result = SendRQLToServer(rql, debugRQLOutput);
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(result);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("LOGIN");
            return (XmlElement) (xmlNodes.Count > 0 ? xmlNodes[0] : null);
        }

        private List<ISystemLocale> GetLocales()
        {
            const string LOAD_LOCALES = @"<LANGUAGE action=""list""/>";
            XmlDocument xmlDoc = ExecuteRQL(LOAD_LOCALES);
            var languages = xmlDoc.GetElementsByTagName("LANGUAGES")[0] as XmlElement;
            if (languages == null)
            {
                throw new SmartAPIException(ServerLogin, "Could not load languages");
            }

            return
                (from XmlElement item in languages.GetElementsByTagName("LIST")
                 select (ISystemLocale) new SystemLocale(this, item)).ToList();
        }

        private XmlDocument GetLoginResponse()
        {
            PasswordAuthentication authData = ServerLogin.AuthData;
            string rql = string.Format(RQL_IODATA,
                                       string.Format(RQL_LOGIN, HttpUtility.HtmlEncode(authData.Username),
                                                     HttpUtility.HtmlEncode(authData.Password)));

            //hide password in log messages
            string debugOutputRQL = string.Format(RQL_IODATA,
                                                  string.Format(RQL_LOGIN, HttpUtility.HtmlEncode(authData.Username),
                                                                "*****"));
            var xmlDoc = new XmlDocument();
            try
            {
                string result = SendRQLToServer(rql, debugOutputRQL);
                xmlDoc.LoadXml(result);
            } catch (RQLException e)
            {
                if (e.ErrorCode != ErrorCode.RDError101)
                {
                    throw e;
                }
                xmlDoc.LoadXml(e.Response);
            }
            return xmlDoc;
        }

        private Version GetServerVersion(string baseURL)
        {
            string versionURI = baseURL + "ioVersionInfo.asp";
            try
            {
                using (var client = new WebClient())
                {
                    if (ServerLogin.WindowsAuthentication != null)
                    {
                        var c = new CredentialCache();
                        c.Add(new Uri(baseURL), "NTLM", ServerLogin.WindowsAuthentication);
                        client.Credentials = c;
                    }
                    
                    client.Headers.Add("Referer", baseURL);

                    string responseText = client.DownloadString(versionURI);
                    Match match = VERSION_REGEXP.Match(responseText);
                    if (match.Groups.Count != 4)
                    {
                        throw new RedDotConnectionException(RedDotConnectionException.FailureTypes.ServerNotFound,
                                                            "Could not retrieve version info of RedDot server at " +
                                                            baseURL + "\n" + responseText);
                    }

                    return new Version(match.Groups[3].Value);
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

        private void InitConnection()
        {
            string baseURL = ServerLogin.Address.ToString();
            if (!baseURL.EndsWith("/"))
            {
                baseURL += "/";
            }
            ServerVersion = ServerLogin.ManualVersionOverride ?? GetServerVersion(baseURL);
            CmsServerConnectionUrl = baseURL +
                                     (ServerVersion.Major < 11
                                          ? "webservice/RDCMSXMLServer.WSDL"
                                          : "WebService/RQLWebService.svc");
        }

        private static bool IsProjectUnavailbaleException(Exception e)
        {
            return e.Message.Contains("The project you have selected is no longer available. Please select a different project via the Main Menu.") ||
                   e.Message.Contains("Access to this project has been denied, because you are not assigned to it.") || 
                   e.Message.Contains("Ihnen wird der Zugang zu diesem Projekt verweigert, da Sie ihm nicht zugewiesen sind.");
        }

        private void LoadSelectedProject(XmlNode xmlDoc)
        {
            var lastModule = (XmlElement) xmlDoc.SelectSingleNode("/IODATA/USER/LASTMODULES/MODULE[@last='1']");
            var projectStr = lastModule?.GetAttributeValue("project");

            if (string.IsNullOrEmpty(projectStr)) return;

            try
            {
                SelectProject(Guid.Parse(projectStr));
            }
            catch (SmartAPIException e)
            {
                if (IsProjectUnavailbaleException(e) || e.InnerException != null && IsProjectUnavailbaleException(e.InnerException))
                {
                    SelectedProjectGuid = Guid.Empty;
                }
                else
                {
                    throw;
                }
            }
        }

        private void Login(Func<IEnumerable<RunningSessionInfo>, RunningSessionInfo> sessionReplacementSelector)
        {
            InitConnection();

            XmlDocument xmlDoc = GetLoginResponse();

            CheckLoginResponse(xmlDoc, sessionReplacementSelector);

            //   LoadSelectedProject(xmlDoc);
        }

        private void Logout(Guid logonGuid)
        {
            const string RQL_LOGOUT = @"<ADMINISTRATION><LOGOUT guid=""{0}""/></ADMINISTRATION>";
            ExecuteRQLRaw(string.Format(RQL_LOGOUT, logonGuid.ToRQLString()), RQL.IODataFormat.LogonGuidOnly);
        }

        private void ParseLoginResponse(XmlNodeList xmlNodes, PasswordAuthentication authData, XmlDocument xmlDoc,
                                        Func<IEnumerable<RunningSessionInfo>, RunningSessionInfo>
                                            sessionReplacementSelector)
        {
            // check if already logged in
            var xmlNode = (XmlElement) xmlNodes[0];
            string oldLoginGuid = CheckAlreadyLoggedIn(xmlNode);
            if (oldLoginGuid != "")
            {
                RunningSessionInfo sessionToReplace;
                if (sessionReplacementSelector == null ||
                    !TryGetSessionInfo(xmlDoc, sessionReplacementSelector, out sessionToReplace))
                {
                    throw new RedDotConnectionException(RedDotConnectionException.FailureTypes.AlreadyLoggedIn,
                                                        "User is already logged in and no open session was selected to get replaced");
                }
                xmlNode = GetForceLoginXmlNode(authData, sessionToReplace.LoginGuid);
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
            SessionKey = LogonGuid.ToRQLString();
            LoadSelectedProject(xmlNode.OwnerDocument);
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
                var xmlElement = ((XmlElement) userNodes[0]);
                ((Users)ServerManager.Users).Current = new User(this, xmlElement.GetGuid()) { Name = xmlElement.GetAttributeValue("name") };
            }
            else
            {
                ((Users)ServerManager.Users).Current = new User(this, Guid.Parse(loginNode.GetAttributeValue("userguid")));
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

                var isUsingHttps = ServerLogin.Address.Scheme.ToLowerInvariant() == "https";
                if (isUsingHttps)
                {
                    binding.Security.Mode = BasicHttpSecurityMode.Transport;
                } 
                binding.ReaderQuotas.MaxStringContentLength = 2097152*10; //20MB
                binding.ReaderQuotas.MaxArrayLength = 2097152*10; //20mb
                binding.MaxReceivedMessageSize = 2097152*10; //20mb
                binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
                binding.SendTimeout = TimeSpan.FromMinutes(10);

                if (ServerLogin.WindowsAuthentication != null)
                {
                    binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
                    binding.Security.Mode = isUsingHttps ? BasicHttpSecurityMode.TransportWithMessageCredential : BasicHttpSecurityMode.TransportCredentialOnly;
                }

                var add = new EndpointAddress(CmsServerConnectionUrl);

                try
                {
                    var client = new RqlWebServiceClient(binding, add);
                    if (ServerLogin.WindowsAuthentication != null)
                    {
                        client.ClientCredentials.Windows.ClientCredential = ServerLogin.WindowsAuthentication;
                        //client.ClientCredentials.Windows.AllowNtlm = true;
                        client.ClientCredentials.Windows.AllowedImpersonationLevel =
                            TokenImpersonationLevel.Impersonation;
                    }
                    //var channel = client.ChannelFactory.CreateChannel();
                    //var res = channel.Execute(new ExecuteRequest(rqlQuery, error, resultInfo));
                    //var result = res.Result;

                    string result = client.Execute(rqlQuery, ref error, ref resultInfo);
                    string errorStr = (error ?? "").ToString();
                    if (!string.IsNullOrEmpty(errorStr))
                    {
                        var exception = new RQLException(ServerLogin.Name, errorStr, result);
                        if (exception.ErrorCode == ErrorCode.NoRight || exception.ErrorCode == ErrorCode.RDError110)
                        {
                            throw new MissingPrivilegesException(exception);
                        }
                        throw exception;
                    }
                    LOG.DebugFormat("Received RQL [{0}]: {1}", ServerLogin.Name, result);
                    return result;
                } catch (Exception e)
                {
                    string msg = ExtractMessagesWithInnerExceptions(e);
                    LOG.Error(msg);
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

        private static bool TryGetSessionInfo(XmlDocument xmlDoc,
                                              Func<IEnumerable<RunningSessionInfo>, RunningSessionInfo>
                                                  sessionReplacementSelector, out RunningSessionInfo sessionToReplace)
        {
            if (sessionReplacementSelector == null)
            {
                sessionToReplace = null;
                return false;
            }
            Guid loginGuid;
            sessionToReplace =
                sessionReplacementSelector(from XmlElement curLogin in xmlDoc.GetElementsByTagName("LOGIN") where curLogin.TryGetGuid(out loginGuid)
                                           select new RunningSessionInfo(curLogin));

            return sessionToReplace != null;
        }
    }

    public enum UseVersioning
    {
        Yes = -1,
        No = 0
    }

    public enum CreatedProjectType
    {
        TestProject = 1,
        LiveProject = 0
    }

    internal static class VersionVerifier
    {
        internal static void EnsureVersion(ISession session)
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
                lessThanAttributes.Cast<VersionIsLessThan>()
                                  .First()
                                  .Validate(session.ServerLogin, session.ServerVersion, info.Name);
            }

            if (greaterOrEqualAttributes.Any())
            {
                greaterOrEqualAttributes.Cast<VersionIsGreaterThanOrEqual>()
                                        .First()
                                        .Validate(session.ServerLogin, session.ServerVersion, info.Name);
            }
        }
    }
}