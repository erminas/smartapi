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
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes;
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

    public interface IUser : IPartialRedDotObject, ISessionObject
    {
        Guid AccountSystemGuid { get; }
        string Description { get; set; }
        DirectEditActivation DirectEditActivationType { get; set; }
        string EMail { get; set; }
        string FullName { get; set; }
        int Id { get; }
        new string Name { get; set; }
        bool IsAlwaysScrollingOpenTreeSegmentsInTheVisibleArea { get; set; }
        bool IsPasswordwordChangeableByCurrentUser { get; }
        IDialogLocale LanguageOfUserInterface { get; set; }
        DateTime LastLoginDate { get; }
        ISystemLocale Locale { get; set; }
        int MaxLevel { get; }
        int MaximumNumberOfSessions { get; set; }
        UserModuleAssignment ModuleAssignment { get; }
        string NavigationType { get; }
        string Password { set; }
        int PreferredEditor { get; }

        /// <summary>
        ///     List of UserProjectAssignments for every project this user is assigned to. The UserProjectAssignment objects also contain this users role in the assigned project. The list is cached by default.
        /// </summary>
        IIndexedCachedList<string, UserProjectAssignment> ProjectAssignments { get; }

        UserPofileChangeRestrictions UserPofileChangeRestrictions { get; set; }

        UserProjectAssignment AssignProject(Project.Project project, UserRole role,
                                                            ExtendedUserRoles extendedRoles);

        void Commit();
        void Delete();
        void UnassignProject(Project.Project project);
    }

    /// <summary>
    ///     A user in the RedDot system.
    /// </summary>
    public class User : PartialRedDotObject, IUser
    {
        private Guid _accountSystemGuid;
        private string _description;
        private string _email;
        private string _fullname;
        private int _id;
        private DirectEditActivation _invertDirectEdit;
        private bool _isPasswordChangeableByCurrentUser;
        private bool _isScrolling;
        private ISystemLocale _locale;
        private DateTime _loginDate;
        private int _maxLevel;
        private int _maxSessionCount;
        private string _navigationType;
        private string _password;
        private int _preferredEditor;
        private IDialogLocale _userInterfaceLanguage;
        private UserPofileChangeRestrictions _userPofileChangeRestrictions;

        public User(Session session, Guid userGuid) : base(session, userGuid)
        {
            Init();
        }

        /// <summary>
        ///     Reads user data from XML-Element "USER" like: <pre>...</pre>
        /// </summary>
        /// <exception cref="FileDataException">Thrown if element doesn't contain valid data.</exception>
        /// <param name="session"> The cms session used to retrieve this user </param>
        /// <param name="xmlElement"> USER XML-Element to get data from </param>
        internal User(Session session, XmlElement xmlElement) : base(session, xmlElement)
        {
            Init();
            // TODO: Read all data

            LoadXml();
        }

        public new string Name { get { return base.Name; } set { _name = value; } }

        public Guid AccountSystemGuid
        {
            get { return LazyLoad(ref _accountSystemGuid); }
        }

        public UserProjectAssignment AssignProject(Project.Project project, UserRole role,
                                                   ExtendedUserRoles extendedRoles)
        {
            //TODO check result ...
            return UserProjectAssignment.Create(this, project, role, extendedRoles);
        }

        public void Commit()
        {
            const string SAVE_USER =
                @"<ADMINISTRATION><USER action=""save"" guid=""{0}"" name=""{1}"" fullname=""{2}"" description=""{3}"" email=""{4}"" userlanguage=""{5}"" maxlogin=""{6}"" invertdirectedit=""{7}"" treeautoscroll=""{8}"" preferrededitor=""{9}"" navigationtype=""{10}"" lcid=""{11}"" userlimits=""{12}"" {13}/></ADMINISTRATION>";

            var passwordAttribute = _password != null ? "password=\"" + _password + '"' : "";
            var query = SAVE_USER.SecureRQLFormat(this, Name, FullName, Description, EMail, LanguageOfUserInterface.LanguageAbbreviation,
                                                  MaximumNumberOfSessions, (int) DirectEditActivationType,
                                                  IsAlwaysScrollingOpenTreeSegmentsInTheVisibleArea, PreferredEditor,
                                                  NavigationType, Locale, (int) UserPofileChangeRestrictions,
                                                  passwordAttribute);
            Session.ExecuteRQL(query, Session.IODataFormat.LogonGuidOnly);
        }

        public string Description
        {
            get { return LazyLoad(ref _description); }
            set { _description = value; }
        }

        public DirectEditActivation DirectEditActivationType
        {
            get { return _invertDirectEdit; }
            set
            {
                EnsureInitialization();
                _invertDirectEdit = value;
            }
        }

        public string EMail
        {
            get { return LazyLoad(ref _email); }
            set
            {
                EnsureInitialization();
                _email = value;
            }
        }

        public string FullName
        {
            get { return LazyLoad(ref _fullname); }
            set
            {
                EnsureInitialization();
                _fullname = value;
            }
        }

        public void Delete()
        {
            const string DELETE_USER = @"<ADMINISTRATION><USER action=""delete"" guid=""{0}"" /></ADMINISTRATION>";
            var xmlDoc = Session.ExecuteRQL(DELETE_USER.RQLFormat(this), Session.IODataFormat.LogonGuidOnly);
            if (!xmlDoc.InnerText.Contains("ok"))
            {
                throw new SmartAPIException(Session.ServerLogin, string.Format("Could not delete user {0}", this));
            }
            Session.Users.InvalidateCache();
        }

        public int Id
        {
            get { return LazyLoad(ref _id); }
        }

        public bool IsAlwaysScrollingOpenTreeSegmentsInTheVisibleArea
        {
            get { return LazyLoad(ref _isScrolling); }
            set
            {
                EnsureInitialization();
                _isScrolling = value;
            }
        }

        public bool IsPasswordwordChangeableByCurrentUser
        {
            get { return LazyLoad(ref _isPasswordChangeableByCurrentUser); }
        }

        public IDialogLocale LanguageOfUserInterface
        {
            get { return LazyLoad(ref _userInterfaceLanguage); }
            set
            {
                EnsureInitialization();
                _userInterfaceLanguage = value;
            }
        }

        public DateTime LastLoginDate
        {
            get { return LazyLoad(ref _loginDate); }
        }

        public ISystemLocale Locale
        {
            get { return LazyLoad(ref _locale); }
            set
            {
                EnsureInitialization();
                _locale = value;
            }
        }

        public int MaxLevel
        {
            get { return LazyLoad(ref _maxLevel); }
        }

        public int MaximumNumberOfSessions
        {
            get { return LazyLoad(ref _maxSessionCount); }
            set
            {
                EnsureInitialization();
                _maxSessionCount = value;
            }
        }

        public UserModuleAssignment ModuleAssignment { get; private set; }

        public string NavigationType
        {
            get { return LazyLoad(ref _navigationType); }
        }

        public string Password
        {
            set { _password = value; }
        }

        public int PreferredEditor
        {
            get { return LazyLoad(ref _preferredEditor); }
        }

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
            get { return LazyLoad(ref _userPofileChangeRestrictions); }
            set
            {
                EnsureInitialization();
                _userPofileChangeRestrictions = value;
            }
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

            InitIfPresent(ref _userInterfaceLanguage, "userlanguage", Session.DialogLocales.Get);
            InitIfPresent(ref _locale, "lcid", s => Session.Locales[int.Parse(s)]);
            InitIfPresent(ref _isScrolling, "treeautoscroll", BoolConvert);

            _loginDate = XmlElement.GetOADate("logindate").GetValueOrDefault();

            InitIfPresent(ref _invertDirectEdit, "invertdirectedit", StringConversion.ToEnum<DirectEditActivation>);

            InitIfPresent(ref _isPasswordChangeableByCurrentUser, "disablepassword", x => !BoolConvert(x));
            InitIfPresent(ref _userPofileChangeRestrictions, "userlimits",
                          StringConversion.ToEnum<UserPofileChangeRestrictions>);
        }
    }
}