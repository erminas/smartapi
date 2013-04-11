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
using System.Xml;
using erminas.SmartAPI.CMS.Project;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Administration
{
    [Flags]
    public enum ExtendedUserRoles
    {
        None = 0,
        TemplateEditor = 1,
        TranslationEditor = 2
    }

    public enum UserRole
    {
        None = 0,
        Administrator = 1,
        SiteBuilder = 2,
        Editor = 3,
        Author = 4,
        Visitor = 5
    }

    public interface IUserProjectAssignment : ISessionObject, IDeletable, ICached
    {
        void Commit();
        bool IsTemplateEditor { get; set; }
        bool IsTranslationEditor { get; set; }
        IProject Project { get; }
        IUser User { get; }
        UserRole UserRole { get; set; }
        IUserProjectAssignment Refreshed();
    }

    internal class UserProjectAssignment : IUserProjectAssignment
    {
        private readonly Session _session;
        private readonly IUser _user;
        private bool _isInitialized;
        private bool? _isTemplateEditor;
        private bool? _isTranslationEditor;
        private UserRole? _userRole;

        internal UserProjectAssignment(IUser user, IProject project)
        {
            Project = project;
            _user = user;
            _session = _user.Session;
        }

        internal UserProjectAssignment(IUser user, XmlElement projectAssignment)
        {
            _user = user;
            _session = _user.Session;
            LoadXml(projectAssignment);
            _isInitialized = true;
        }

        private UserProjectAssignment(IUser user, IProject project, UserRole role, ExtendedUserRoles extendedUserRoles)
        {
            Project = project;
            _user = user;
            _session = _user.Session;
            UserRole = role;
            IsTemplateEditor = extendedUserRoles.HasFlag(ExtendedUserRoles.TemplateEditor);
            IsTranslationEditor = extendedUserRoles.HasFlag(ExtendedUserRoles.TranslationEditor);
            _isInitialized = true;
        }

        public void Commit()
        {
            EnsureInitialization();
            //TODO check results
            const string SAVE_USER_RIGHTS =
                @"<ADMINISTRATION><USER action=""save"" guid=""{0}""><PROJECTS><PROJECT guid=""{1}"" checked=""1"" lm=""{2}"" te=""{3}"" userlevel=""{4}""/></PROJECTS></USER></ADMINISTRATION>";

            User.Session.ExecuteRQL(SAVE_USER_RIGHTS.RQLFormat(User, Project, IsTranslationEditor, IsTemplateEditor,
                                                               (int) UserRole));
        }

        public bool IsTemplateEditor
        {
            get
            {
                EnsureInitialization();
// ReSharper disable PossibleInvalidOperationException
                return _isTemplateEditor.Value;
// ReSharper restore PossibleInvalidOperationException
            }
            set { _isTemplateEditor = value; }
        }

        public bool IsTranslationEditor
        {
            get
            {
                EnsureInitialization();
// ReSharper disable PossibleInvalidOperationException
                return _isTranslationEditor.Value;
// ReSharper restore PossibleInvalidOperationException
            }
            set { _isTranslationEditor = value; }
        }

        private void EnsureInitialization()
        {
            if (!_isInitialized)
            {
                var xml = RetrieveObjectElement();
                LoadXml(xml);
                _isInitialized = true;
            }
        }

        private XmlElement RetrieveObjectElement()
        {
            const string LOAD_ACCESS_LEVEL =
               @"<ADMINISTRATION><USER guid=""{0}"" ><PROJECT guid=""{1}"" action=""load"" extendedinfo=""1""/></USER></ADMINISTRATION>";
            var xmlDoc = Project.ExecuteRQL(LOAD_ACCESS_LEVEL.RQLFormat(User, Project));
            return xmlDoc.GetSingleElement("PROJECT");
        }

        public IProject Project { get; private set; }

        public Session Session
        {
            get { return _session; }
        }

        public IUser User
        {
            get { return _user; }
        }

        public UserRole UserRole
        {
              get
            {
                EnsureInitialization();
// ReSharper disable PossibleInvalidOperationException
                return _userRole.Value;
// ReSharper restore PossibleInvalidOperationException
            }
            set { _userRole = value; }
        }

        public IUserProjectAssignment Refreshed()
        {
            Refresh();
            return this;
        }

        public void Delete()
        {
            Delete(Project, User);
        }

        internal static void Delete(IProject project, IUser user)
        {
            const string UNASSING_PROJECT =
              @"<ADMINISTRATION><USER action=""save"" guid=""{0}""><PROJECTS><PROJECT guid=""{1}"" checked=""0""/></PROJECTS><CCSCONNECTIONS/></USER></ADMINISTRATION>";

            project.Session.ExecuteRQL(UNASSING_PROJECT.RQLFormat(user, project));

            project.Users.InvalidateCache();
            user.Projects.InvalidateCache();
        }

        /// <summary>
        ///     TODO warum ist das nicht oeffentlich?
        /// </summary>
        /// <param name="user"></param>
        /// <param name="project"></param>
        /// <param name="role"></param>
        /// <param name="extendedUserRoles"></param>
        /// <returns></returns>
        internal static UserProjectAssignment Create(IUser user, IProject project, UserRole role, ExtendedUserRoles extendedUserRoles)
        {
            var assignment = new UserProjectAssignment(user, project, role, extendedUserRoles);
            assignment.Commit();


            user.Projects.InvalidateCache();
            project.Users.InvalidateCache();

            return assignment;
        }

        private bool HasRight(XmlElement projectElement, string attributeName)
        {
            var intAttributeValue = projectElement.GetIntAttributeValue(attributeName);
            if (intAttributeValue == null)
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Missing attribute '{0}' in user/project assignment",
                                                          attributeName));
            }

            return intAttributeValue.Value == -1;
        }

        private void LoadXml(XmlElement projectAssignment)
        {
            if (Project == null)
            {
                Project = new Project.Project(_user.Session, projectAssignment.GetGuid())
                    {
                        Name = projectAssignment.GetName()
                    };
            }
            // we check for null, because a properties could have been set by the user, before the element was initialized
            // and those values should not get lost on loading of the other values
            if (_userRole == null)
            {
                _userRole = (UserRole) projectAssignment.GetIntAttributeValue("userlevel").GetValueOrDefault();
            }
            if (_isTemplateEditor == null)
            {
                _isTemplateEditor = HasRight(projectAssignment, "templateeditorright");
            }
            if (_isTranslationEditor == null)
            {
                _isTranslationEditor= HasRight(projectAssignment, "languagemanagerright");
            }
        }

        public void InvalidateCache()
        {
            _userRole = null;
            _isTemplateEditor = null;
            _isTranslationEditor = null;
            _isInitialized = false;
        }

        public void Refresh()
        {
            InvalidateCache();
            EnsureInitialization();
        }
    }
}