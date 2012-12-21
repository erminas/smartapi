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
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    /// <summary>
    ///   A user in the RedDot system.
    /// </summary>
    public class User : PartialRedDotObject
    {
        public readonly Session Session;
        private Guid _accountSystemGuid;
        private string _acs;
        private string _action;
        private string _description;
        private string _dialogLanguageId;
        private string _disablePassword;
        private string _email;
        private string _flags1;
        private string _flags2;
        private string _fullname;
        private string _id;
        private string _invertDirectEdit;
        private string _lcid;
        private string _lm;
        private string _loginDate;
        private string _maxLevel;
        private string _maxLogin;
        private string _navigationType;
        private string _preferredEditor;
        private string _te;
        private string _userLanguage;
        private string _userLimits;

        public User(Session session, Guid userGuid) : base(userGuid)
        {
            Session = session;
            Init();
        }

        /// <summary>
        ///   Reads user data from XML-Element "USER" like: <pre>...</pre>
        /// </summary>
        /// <exception cref="FileDataException">Thrown if element doesn't contain valid data.</exception>
        /// <param name="session"> The cms session used to retrieve this user </param>
        /// <param name="xmlElement"> USER XML-Element to get data from </param>
        public User(Session session, XmlElement xmlElement) : base(xmlElement)
        {
            Session = session;
            Init();
            // TODO: Read all data

            LoadXml();
        }

        /// <summary>
        ///   List of UserProjectAssignments for every project this user is assigned to. The UserProjectAssignment objects also contain this users role in the assigned project. The list is cached by default.
        /// </summary>
        public IIndexedCachedList<string, UserProjectAssignment> ProjectAssignments { get; private set; }

        private void Init()
        {
            ProjectAssignments = new IndexedCachedList<string, UserProjectAssignment>(GetProjectAssignments,
                                                                                      assignment =>
                                                                                      assignment.Project.Name,
                                                                                      Caching.Enabled);
            ModuleAssignment = new UserModuleAssignment(this);
        }

        public UserModuleAssignment ModuleAssignment { get; private set; }

        private List<UserProjectAssignment> GetProjectAssignments()
        {
            const string LIST_USER_PROJECTS =
                @"<ADMINISTRATION><USER guid=""{0}""><PROJECTS action=""list"" extendedinfo=""1""/></USER></ADMINISTRATION>";

            var xmlDoc = Session.ExecuteRQL(LIST_USER_PROJECTS.RQLFormat(this));
            return (from XmlElement assignmentElement in xmlDoc.GetElementsByTagName("PROJECT")
                    select new UserProjectAssignment(this, assignmentElement)).ToList();
        }

        // TODO: Nothing checked in here

        private void LoadXml()
        {
            _action = XmlNode.GetAttributeValue("action");
            _id = XmlNode.GetAttributeValue("id");
            _fullname = XmlNode.GetAttributeValue("fullname");
            _description = XmlNode.GetAttributeValue("description");
            _flags1 = XmlNode.GetAttributeValue("flags1");
            _flags2 = XmlNode.GetAttributeValue("flags2");
            _maxLevel = XmlNode.GetAttributeValue("maxlevel");
            _email = XmlNode.GetAttributeValue("email");
            _acs = XmlNode.GetAttributeValue("acs");
            _dialogLanguageId = XmlNode.GetAttributeValue("dialoglanguageid");
            _userLanguage = XmlNode.GetAttributeValue("userlanguage");
            _loginDate = XmlNode.GetAttributeValue("logindate");
            _te = XmlNode.GetAttributeValue("te");
            _lm = XmlNode.GetAttributeValue("lm");
            _navigationType = XmlNode.GetAttributeValue("navigationtype");
            _lcid = XmlNode.GetAttributeValue("lcid");
            _maxLogin = XmlNode.GetAttributeValue("maxlogin");
            _preferredEditor = XmlNode.GetAttributeValue("preferrededitor");
            _invertDirectEdit = XmlNode.GetAttributeValue("invertdirectedit");
            _disablePassword = XmlNode.GetAttributeValue("disablepassword");
            _userLimits = XmlNode.GetAttributeValue("userlimits");
            XmlNode.TryGetGuid("accountsystemguid", out _accountSystemGuid);
        }

        protected override void LoadWholeObject()
        {
            LoadXml();
        }

        protected override XmlElement RetrieveWholeObject()
        {
            const string LOAD_USER = @"<ADMINISTRATION><USER action=""load"" guid=""{0}""/></ADMINISTRATION>";
            string answer = Session.ExecuteRql(String.Format(LOAD_USER, Guid.ToRQLString()),
                                               Session.IODataFormat.LogonGuidOnly);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(answer);

            return (XmlElement) xmlDocument.GetElementsByTagName("USER")[0];
        }

        public void UnassignProject(Project project)
        {
            const string UNASSING_PROJECT =
                @"<ADMINISTRATION><USER action=""save"" guid=""{0}""><PROJECTS><PROJECT guid=""{1}"" checked=""0""/></PROJECTS><CCSCONNECTIONS/></USER></ADMINISTRATION>";

            //TODO check result ...
            Session.ExecuteRQL(UNASSING_PROJECT.RQLFormat(this, project));
            ProjectAssignments.InvalidateCache();
        }

        public UserProjectAssignment AssignProject(Project project, UserRole role, ExtendedUserRoles extendedRoles)
        {
            //TODO check result ...
            return UserProjectAssignment.Create(this, project, role, extendedRoles);
        }

        #region Properties

        public Guid AccountSystemGuid
        {
            get { return LazyLoad(ref _accountSystemGuid); }
        }

        public string Action
        {
            get { return LazyLoad(ref _action); }
        }

        public string Id
        {
            get { return LazyLoad(ref _id); }
        }

        public string Fullname
        {
            get { return LazyLoad(ref _fullname); }
        }

        public string Description
        {
            get { return LazyLoad(ref _description); }
        }

        public string Flags1
        {
            get { return LazyLoad(ref _flags1); }
        }

        public string Flags2
        {
            get { return LazyLoad(ref _flags2); }
        }

        public string MaxLevel
        {
            get { return LazyLoad(ref _maxLevel); }
        }

        public string Acs
        {
            get { return LazyLoad(ref _acs); }
        }

        public string DialogLanguageId
        {
            get { return LazyLoad(ref _dialogLanguageId); }
        }

        public string UserLanguage
        {
            get { return LazyLoad(ref _userLanguage); }
        }

        public string LoginDate
        {
            get { return LazyLoad(ref _loginDate); }
        }

        public string Te
        {
            get { return LazyLoad(ref _te); }
        }

        public string Lm
        {
            get { return LazyLoad(ref _lm); }
        }

        public string NavigationType
        {
            get { return LazyLoad(ref _navigationType); }
        }

        public string Lcid
        {
            get { return LazyLoad(ref _lcid); }
        }

        public string MaxLogin
        {
            get { return LazyLoad(ref _maxLogin); }
        }

        public string PreferredEditor
        {
            get { return LazyLoad(ref _preferredEditor); }
        }

        public string InvertDirectEdit
        {
            get { return LazyLoad(ref _invertDirectEdit); }
        }

        public string DisablePassword
        {
            get { return LazyLoad(ref _disablePassword); }
        }

        public string UserLimits
        {
            get { return LazyLoad(ref _userLimits); }
        }

        public string EMail
        {
            get { return LazyLoad(ref _email); }
        }

        #endregion
    }
}