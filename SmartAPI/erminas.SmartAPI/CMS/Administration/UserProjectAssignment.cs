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
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

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
        Administrator = 1,
        SiteBuilder = 2,
        Editor = 3,
        Author = 4,
        Visitor = 5
    }

    public class UserProjectAssignment : ISessionObject
    {
        private readonly Session _session;
        private readonly User _user;

        internal UserProjectAssignment(User user, XmlElement projectAssignment)
        {
            _user = user;
            _session = _user.Session;
            LoadXml(projectAssignment);
        }

        private UserProjectAssignment(User user, Project.Project project, UserRole role,
                                      ExtendedUserRoles extendedUserRoles)
        {
            Project = project;
            _user = user;
            _session = _user.Session;
            UserRole = role;
            IsTemplateEditor = extendedUserRoles.HasFlag(ExtendedUserRoles.TemplateEditor);
            IsTranslationEditor = extendedUserRoles.HasFlag(ExtendedUserRoles.TranslationEditor);
        }

        public void Commit()
        {
            //TODO check results
            const string SAVE_USER_RIGHTS =
                @"<ADMINISTRATION><USER action=""save"" guid=""{0}""><PROJECTS><PROJECT guid=""{1}"" checked=""1"" lm=""{2}"" te=""{3}"" userlevel=""{4}""/></PROJECTS></USER></ADMINISTRATION>";

            User.Session.ExecuteRQL(SAVE_USER_RIGHTS.RQLFormat(User, Project, IsTranslationEditor, IsTemplateEditor,
                                                               (int) UserRole));
        }

        public void Delete()
        {
            User.UnassignProject(Project);
        }

        public bool IsTemplateEditor { get; set; }

        public bool IsTranslationEditor { get; set; }
        public Project.Project Project { get; private set; }

        public Session Session
        {
            get { return _session; }
        }

        public User User
        {
            get { return _user; }
        }

        public UserRole UserRole { get; set; }

        /// <summary>
        ///     TODO warum ist das nicht oeffentlich?
        /// </summary>
        /// <param name="user"></param>
        /// <param name="project"></param>
        /// <param name="role"></param>
        /// <param name="extendedUserRoles"></param>
        /// <returns></returns>
        internal static UserProjectAssignment Create(User user, Project.Project project, UserRole role,
                                                     ExtendedUserRoles extendedUserRoles)
        {
            var assignment = new UserProjectAssignment(user, project, role, extendedUserRoles);
            assignment.Commit();

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
            Project = new Project.Project(_user.Session, projectAssignment.GetGuid())
                {
                    Name = projectAssignment.GetName()
                };

            UserRole = (UserRole) projectAssignment.GetIntAttributeValue("userlevel").GetValueOrDefault();
            IsTemplateEditor = HasRight(projectAssignment, "templateeditorright");
            IsTranslationEditor = HasRight(projectAssignment, "languagemanagerright");
        }
    }
}