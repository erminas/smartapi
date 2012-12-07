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
using System.IO;
using System.ServiceModel;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.RedDotCmsXmlServer;
using erminas.SmartAPI.Utils;
using log4net;

namespace erminas.SmartAPI.CMS
{
    /// <summary>
    ///   This represents a client for RedDot CMS TODO merge with Session
    /// </summary>
    public class CmsClient : IDisposable
    {
        #region CONFIG

        /// <summary>
        ///   Forcelogin=true means that if the user was already logged in the old session will be closed and a new one started.
        /// </summary>
        private const bool FORCE_LOGIN = true;

        #endregion

        #region IODataFormat enum

        /// <summary>
        ///   TypeId of query formats. Determines usage/placement of logon guid/session key in a query.
        /// </summary>
        public enum IODataFormat
        {
            /// <summary>
            ///   Only use the logon guid in the IODATA element. Insert the query into the IODATA element.
            /// </summary>
            LogonGuidOnly,

            /// <summary>
            ///   Use the session key and the logon guid in the IODATA element. Insert the query into the IODATA element.
            /// </summary>
            SessionKeyAndLogonGuid,

            /// <summary>
            ///   Use the logon guid in the IODATA element and insert a PROJECT element with the session key. The query gets inserted into the PROJECT element
            /// </summary>
            SessionKeyInProjectElement,

            /// <summary>
            ///   Insert the query into a plain IODATA element.
            /// </summary>
            Plain
        }

        #endregion

        private const string RQL_IODATA = "<IODATA>{0}</IODATA>";
        private const string RQL_IODATA_LOGONGUID = @"<IODATA loginguid=""{0}"">{1}</IODATA>";
        private const string RQL_IODATA_SESSIONKEY = @"<IODATA loginguid=""{0}"" sessionkey=""{1}"">{2}</IODATA>";

        private const string RQL_IODATA_PROJECT_SESSIONKEY =
            @"<IODATA loginguid=""{0}""><PROJECT sessionkey=""{1}"">{2}</PROJECT></IODATA>";

        private const string RQL_LOGIN = @"<ADMINISTRATION action=""login"" name=""{0}"" password=""{1}""/>";

        private const string RQL_LOGIN_FORCE =
            @"<ADMINISTRATION action=""login"" name=""{0}"" password=""{1}"" loginguid=""{2}""/>";

        private const string RQL_SELECT_PROJECT =
            @"<ADMINISTRATION action=""validate"" guid=""{0}"" useragent=""script""><PROJECT guid=""{1}""/></ADMINISTRATION>";

        private static readonly ILog LOG = LogManager.GetLogger("CmsClient");
        private string _loginGuidStr;
        private string _sessionKeyStr;

        /// <summary>
        ///   Create a CMS Client, connect to the given server and optionally log in
        /// </summary>
        /// <param name="autoLogin"> if true, login in automatically, otherweise "connect" must be called to login </param>
        /// <param name="serverLogin"> Server and login data </param>
        public CmsClient(ServerLogin serverLogin, bool autoLogin)
        {
            ServerLogin = serverLogin;
            CmsServerConnectionUrl = serverLogin.Address;
            if (autoLogin)
            {
                Login(serverLogin.AuthData);
            }
        }


        /// <summary>
        ///   Create a CMS Client, connect to the given server and log in
        /// </summary>
        /// <param name="serverLogin"> Server and login data </param>
        public CmsClient(ServerLogin serverLogin) : this(serverLogin, true)
        {
        }

        /// <summary>
        ///   Create a CMS Client, connect to given server and log in
        /// </summary>
        /// <param name="passwordAuthentication"> Authentication data (username and password) </param>
        /// <param name="cmsServerConnectionUrl"> URL of Server </param>
        public CmsClient(PasswordAuthentication passwordAuthentication, Uri cmsServerConnectionUrl)
        {
            CmsServerConnectionUrl = cmsServerConnectionUrl;

            Login(passwordAuthentication);
        }

        /// <summary>
        ///   Creates a CMS Client object to given server and not loggeg in Can be used in a plugin when server Connection already exists
        /// </summary>
        /// <param name="serverLogin"> Server Data </param>
        /// <param name="logonGuid"> Login Guid of existing Login </param>
        /// <param name="sessionKey"> Session Key of existing Session </param>
        public CmsClient(ServerLogin serverLogin, Guid logonGuid, Guid sessionKey)
        {
            ServerLogin = serverLogin;
            CmsServerConnectionUrl = serverLogin.Address;
            LogonGuid = logonGuid;
            SessionKey = sessionKey;
        }

        /// <summary>
        ///   Creates a CMS Client object to given server and not loggeg in Can be used in a plugin when server Connection already exists
        /// </summary>
        /// <param name="serverLogin"> Server Data </param>
        /// <param name="logonGuid"> Login Guid of existing Login </param>
        /// <param name="sessionKey"> Session Key of existing Session </param>
        /// <param name="projectGuid"> Porjectkey of already selected Project </param>
        public CmsClient(ServerLogin serverLogin, Guid logonGuid, Guid sessionKey, Guid projectGuid)
        {
            ServerLogin = serverLogin;
            CmsServerConnectionUrl = serverLogin.Address;
            LogonGuid = logonGuid;
            SessionKey = sessionKey;
            SelectedProjectGuid = projectGuid;
        }

        public Uri CmsServerConnectionUrl { get; private set; }

        public Guid LogonGuid
        {
            get { return Guid.Parse(_loginGuidStr); }
            private set { _loginGuidStr = value.ToRQLString(); }
        }

        public Guid SessionKey
        {
            get { return Guid.Parse(_sessionKeyStr); }
            private set { _sessionKeyStr = value.ToRQLString(); }
        }

        public ServerLogin ServerLogin { get; set; }

        /// <summary>
        ///   The currently connected user.
        /// </summary>
        public User CurrentUser { get; private set; }

        public Guid SelectedProjectGuid { get; private set; }

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                Disconnect();
            }
// ReSharper disable EmptyGeneralCatchClause
            catch
// ReSharper restore EmptyGeneralCatchClause
            {
                // exceptions are no longer relevant
            }
        }

        #endregion

        /// <summary>
        ///   Execute an RQL statement. The format of the query (usage of session key/logon guid can be chosen).
        /// </summary>
        /// <param name="query"> Statement to execute </param>
        /// <param name="ioDataFormat"> Defines the format of the iodata element / placement of sessionkey of the RQL query </param>
        /// <returns> String returned from the server </returns>
        public string ExecuteRql(string query, IODataFormat ioDataFormat)
        {
            string tmpQuery = query.Replace(Session.SESSIONKEY_PLACEHOLDER, "#" + _sessionKeyStr);
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
                default:
                    throw new ArgumentException(String.Format("Unknown IODataFormat: {0}", ioDataFormat));
            }
            return SendRQLToServer(rqlQuery);
        }

        /// <summary>
        ///   Send RQL statement to CMS server and return result.
        /// </summary>
        /// <param name="rqlQuery"> Query to send to CMS server </param>
        /// <param name="debugRQLQuery"> Query to save in log file (this is used to hide passwords in the log files) </param>
        /// <exception cref="RedDotConnectionException">CMS Server not found or couldn't establish connection</exception>
        /// <returns> Result of RQL query </returns>
        protected string SendRQLToServer(string rqlQuery, string debugRQLQuery = null)
        {
            try
            {
                LOG.DebugFormat("Sending RQL [{0}]: {1}", ServerLogin.Name, debugRQLQuery ?? rqlQuery);

                object error = "x";
                object resultInfo = "";

                BasicHttpBinding binding = new BasicHttpBinding();
                binding.ReaderQuotas.MaxStringContentLength = 2097152*10; //20MB
                binding.ReaderQuotas.MaxArrayLength = 2097152*10; //20mb
                binding.MaxReceivedMessageSize = 2097152*10; //20mb


                EndpointAddress add = new EndpointAddress(CmsServerConnectionUrl);

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
                }
                catch (Exception e)
                {
                    LOG.Error(e.Message);
                    LOG.Debug(e.StackTrace);
                    throw;
                }
            }
            catch (EndpointNotFoundException e)
            {
                LOG.ErrorFormat("Server not found: {0}", CmsServerConnectionUrl);
                throw new RedDotConnectionException(RedDotConnectionException.FailureTypes.ServerNotFound,
                                                    string.Format(@"Server ""{0}"" not found", CmsServerConnectionUrl),
                                                    e);
            }
        }

        private void Login(PasswordAuthentication pa)
        {
            string rql = string.Format(RQL_IODATA, string.Format(RQL_LOGIN, pa.Username, pa.Password));

            //hide password in log messages
            string debugOutputRQL = string.Format(RQL_IODATA, string.Format(RQL_LOGIN, pa.Username, "*****"));

            var xmlDoc = new XmlDocument();
            try
            {
                string result = SendRQLToServer(rql, debugOutputRQL);
                xmlDoc.LoadXml(result);
            }
            catch (RQLException e)
            {
                if (e.ErrorCode != ErrorCode.RDError101)
                {
                    throw;
                }
                xmlDoc.LoadXml(e.Response);
            }

            var xmlNodes = xmlDoc.GetElementsByTagName("LOGIN");

            if (xmlNodes.Count > 0)
            {
                // check if already logged in
                var xmlNode = (XmlElement) xmlNodes[0];
                var oldLoginGuid = CheckAlreadyLoggedIn(xmlNode);
// ReSharper disable ConditionIsAlwaysTrueOrFalse
                if (oldLoginGuid != "" && !FORCE_LOGIN)
// ReSharper restore ConditionIsAlwaysTrueOrFalse
                {
                    throw new RedDotConnectionException(RedDotConnectionException.FailureTypes.AlreadyLoggedIn,
                                                        "User already logged in.");
                }
                if (oldLoginGuid != "")
                {
                    // forcelogin is true -> force the login
                    xmlNode = GetForceLoginXmlNode(pa, oldLoginGuid);
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
                    var userNodes = xmlDoc.GetElementsByTagName("USER");
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
            else
            {
                // didn't get a valid logon xml node
                throw new RedDotConnectionException(RedDotConnectionException.FailureTypes.CouldNotLogin,
                                                    "Could not login.");
            }
        }

        private XmlElement GetForceLoginXmlNode(PasswordAuthentication pa, string oldLoginGuid)
        {
            LOG.InfoFormat("User login will be forced. Old login guid was: {0}", oldLoginGuid);
            //hide user password in log message
            string rql = string.Format(RQL_IODATA,
                                       string.Format(RQL_LOGIN_FORCE, pa.Username, pa.Password, oldLoginGuid));
            string debugRQLOutput = string.Format(RQL_IODATA,
                                                  string.Format(RQL_LOGIN_FORCE, pa.Username, "*****", oldLoginGuid));
            var result = SendRQLToServer(rql, debugRQLOutput);
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(result);
            var xmlNodes = xmlDoc.GetElementsByTagName("LOGIN");
            return (XmlElement) (xmlNodes.Count > 0 ? xmlNodes[0] : null);
        }

        private static string CheckAlreadyLoggedIn(XmlElement xmlElement)
        {
            return xmlElement.GetAttributeValue("loginguid") ?? "";
        }

        private void Logout(Guid logonGuid)
        {
            const string RQL_LOGOUT = @"<ADMINISTRATION><LOGOUT guid=""{0}""/></ADMINISTRATION>";
            ExecuteRql(string.Format(RQL_LOGOUT, logonGuid.ToRQLString()), IODataFormat.LogonGuidOnly);
        }


        /// <summary>
        ///   Select a project. Subsequent queries will be executed in the context of this project.
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
                result = ExecuteRql(
                    string.Format(RQL_SELECT_PROJECT, _loginGuidStr, projectGuid.ToRQLString().ToUpperInvariant()),
                    IODataFormat.LogonGuidOnly
                    );
            }
            catch (RQLException e)
            {
                result = e.Response;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(result);
            var xmlNodes = xmlDoc.GetElementsByTagName("SERVER");
            if (xmlNodes.Count > 0)
            {
                SessionKey = ((XmlElement) xmlNodes[0]).GetGuid("key");
                SelectedProjectGuid = projectGuid;
                return;
            }

            throw new Exception(String.Format("Couldn't select project {0}", projectGuid.ToRQLString()));
        }

        ///<summary>
        ///  Disconnectes the client from the server. Object cannot be used afterwards.
        ///</summary>
        public void Disconnect()
        {
            Logout(LogonGuid);

            // invalidate this object
            LogonGuid = default(Guid);
        }


        /// <summary>
        ///   Get the text content of a text element. This method exists, because it needs a different RQL element layout than all other queries.
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
            const string SAVE_TEXT_CONTENT =
                @"<IODATA loginguid=""{0}"" format=""1"" sessionkey=""{1}""><PROJECT><TEXT action=""save"" guid=""{2}"" texttype=""{3}"" >{4}</TEXT></PROJECT></IODATA>";
            SelectProject(projectGuid);
            languageVariant.Select();
            string rqlResult = SendRQLToServer(string.Format(SAVE_TEXT_CONTENT, _loginGuidStr, _sessionKeyStr,
                                                             textElementGuid == Guid.Empty
                                                                 ? ""
                                                                 : textElementGuid.ToRQLString(), typeString,
                                                             HttpUtility.HtmlEncode(content)));

            var resultGuidStr = XElement.Load(new StringReader(rqlResult)).Value;
            Guid newGuid;
            if (!Guid.TryParse(resultGuidStr, out newGuid) ||
                (textElementGuid != Guid.Empty && textElementGuid != newGuid))
            {
                throw new Exception("could not set text for: " + textElementGuid.ToRQLString());
            }
            return newGuid;
        }
    }
}