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
using System.Xml;
using erminas.SmartAPI.CMS.Administration.Language;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Administration
{
    [Flags]
    public enum UserPofileChangeRestrictions
    {
        NameAndDescription = 1,
        EMailAdress = 2,
        ConnectedDirectoryService = 4,
        Password = 8,
        InterfaceLanguageAndLocale = 16,
        SmartEditNavigation = 32,
        PreferredTextEditor = 64,
        DirectEdit = 128
    }

    public enum DirectEditActivation
    {
        CtrlAndMouseButton = 0,
        MouseButtonOnly = 1
    }

    /// <summary>
    ///     A user in the RedDot system.
    /// </summary>
    public class User : PartialRedDotObject
    {
        public readonly Session Session;
        private Guid _accountSystemGuid;
        private string _description;
        private string _email;
        private string _fullname;
        private int _id;
        private DirectEditActivation _invertDirectEdit;
        private bool _isPasswordChangeableByCurrentUser;
        private Locale _locale;
        private DateTime _loginDate;
        private int _maxLevel;
        private int _maxSessionCount;
        private string _navigationType;
        private int _preferredEditor;
        private Locale _userLanguage;
        private UserPofileChangeRestrictions _userPofileChangeRestrictions;

        public User(Session session, Guid userGuid) : base(userGuid)
        {
            Session = session;
            Init();
        }

        /// <summary>
        ///     Reads user data from XML-Element "USER" like: <pre>...</pre>
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

        public UserProjectAssignment AssignProject(Project.Project project, UserRole role, ExtendedUserRoles extendedRoles)
        {
            //TODO check result ...
            return UserProjectAssignment.Create(this, project, role, extendedRoles);
        }

        public UserModuleAssignment ModuleAssignment { get; private set; }

        /// <summary>
        ///     List of UserProjectAssignments for every project this user is assigned to. The UserProjectAssignment objects also contain this users role in the assigned project. The list is cached by default.
        /// </summary>
        public IIndexedCachedList<string, UserProjectAssignment> ProjectAssignments { get; private set; }

        public void UnassignProject(Project.Project project)
        {
            const string UNASSING_PROJECT =
                @"<ADMINISTRATION><USER action=""save"" guid=""{0}""><PROJECTS><PROJECT guid=""{1}"" checked=""0""/></PROJECTS><CCSCONNECTIONS/></USER></ADMINISTRATION>";

            //TODO check result ...
            Session.ExecuteRQL(UNASSING_PROJECT.RQLFormat(this, project));
            ProjectAssignments.InvalidateCache();
        }

        public UserPofileChangeRestrictions UserPofileChangeRestrictions
        {
            get { return _userPofileChangeRestrictions; }
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

        private List<UserProjectAssignment> GetProjectAssignments()
        {
            const string LIST_USER_PROJECTS =
                @"<ADMINISTRATION><USER guid=""{0}""><PROJECTS action=""list"" extendedinfo=""1""/></USER></ADMINISTRATION>";

            var xmlDoc = Session.ExecuteRQL(LIST_USER_PROJECTS.RQLFormat(this));
            return (from XmlElement assignmentElement in xmlDoc.GetElementsByTagName("PROJECT")
                    select new UserProjectAssignment(this, assignmentElement)).ToList();
        }

        private void Init()
        {
            ProjectAssignments = new IndexedCachedList<string, UserProjectAssignment>(GetProjectAssignments,
                                                                                      assignment =>
                                                                                      assignment.Project.Name,
                                                                                      Caching.Enabled);
            ModuleAssignment = new UserModuleAssignment(this);
        }

        private void LoadXml()
        {
            InitIfPresent(ref _id, "id", int.Parse);
            InitIfPresent(ref _maxLevel, "maxlevel", int.Parse);
            InitIfPresent(ref _maxSessionCount, "maxlogin", int.Parse);
            InitIfPresent(ref _preferredEditor, "preferrededitor", int.Parse);

            _fullname = XmlElement.GetAttributeValue("fullname");
            _description = XmlElement.GetAttributeValue("description");
            _email = XmlElement.GetAttributeValue("email");

            XmlElement.TryGetGuid("accountsystemguid", out _accountSystemGuid);

            InitIfPresent(ref _userLanguage, "userlanguage", Session.DialogLocales.Get);
            InitIfPresent(ref _locale, "lcid", s => Session.Locales[int.Parse(s)]);

            _loginDate = XmlElement.GetOADate("logindate").GetValueOrDefault();

            InitIfPresent(ref _invertDirectEdit, "invertdirectedit", s => (DirectEditActivation) int.Parse(s));

            InitIfPresent(ref _isPasswordChangeableByCurrentUser, "disablepassword", x => !BoolConvert(x));
            InitIfPresent(ref _userPofileChangeRestrictions, "userlimits",
                          s => (UserPofileChangeRestrictions) Enum.Parse(typeof (UserPofileChangeRestrictions), s));
        }

        #region Properties

        public Guid AccountSystemGuid
        {
            get { return LazyLoad(ref _accountSystemGuid); }
        }

        public string Description
        {
            get { return LazyLoad(ref _description); }
        }

        public DirectEditActivation DirectEditActivationType
        {
            get { return _invertDirectEdit; }
        }

        public string EMail
        {
            get { return LazyLoad(ref _email); }
        }

        public string Fullname
        {
            get { return LazyLoad(ref _fullname); }
        }

        public int Id
        {
            get { return LazyLoad(ref _id); }
        }

        public bool IsPasswordwordChangeableByCurrentUser
        {
            get { return LazyLoad(ref _isPasswordChangeableByCurrentUser); }
        }

        public Locale LanguageOfUserInterface
        {
            get { return LazyLoad(ref _userLanguage); }
        }

        public DateTime LastLoginDate
        {
            get { return LazyLoad(ref _loginDate); }
        }

        public Locale Locale
        {
            get { return _locale; }
        }

        public int MaxLevel
        {
            get { return LazyLoad(ref _maxLevel); }
        }

        public int MaximumNumberOfSessions
        {
            get { return LazyLoad(ref _maxSessionCount); }
        }

        public string NavigationType
        {
            get { return LazyLoad(ref _navigationType); }
        }

        public int PreferredEditor
        {
            get { return LazyLoad(ref _preferredEditor); }
        }

        #endregion
    }
}