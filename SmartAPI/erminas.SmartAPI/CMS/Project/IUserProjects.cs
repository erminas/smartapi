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
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Administration;
using erminas.SmartAPI.CMS.ServerManagement;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project
{
    public interface IUserProjects : IIndexedCachedList<string, IUserProjectAssignment>, ISessionObject
    {
        IUserProjectAssignment AddOrSet(IProject project, UserRole role, ExtendedUserRoles extendedRoles);
        bool ContainsProject(IProject project);
        bool ContainsProjectGuid(Guid projectGuid);
        bool ContainsProjectName(string projectName);
        IUserProjectAssignment GetByProject(IProject project);
        IUserProjectAssignment GetByProjectGuid(Guid projectGuid);
        IUserProjectAssignment GetByProjectName(string projectName);
        IUserProjectAssignment this[IProject project] { get; }
        void Remove(IProject project);
        bool TryGetByProject(IProject project, out IUserProjectAssignment assignment);
        bool TryGetByProjectGuid(Guid projectGuid, out IUserProjectAssignment assignment);
        bool TryGetByProjectName(string projectName, out IUserProjectAssignment assignment);
        IUser User { get; }
    }

    internal class UserProjects : IndexedCachedList<string, IUserProjectAssignment>, IUserProjects
    {
        private readonly IUser _user;

        internal UserProjects(IUser user, Caching caching) : base(assignment => assignment.Project.Name, caching)
        {
            _user = user;
            RetrieveFunc = GetProjectAssignments;
        }

        public IUserProjectAssignment AddOrSet(IProject project, UserRole role, ExtendedUserRoles extendedRoles)
        {
            return UserProjectAssignment.Create(_user, project, role, extendedRoles);
        }

        public bool ContainsProject(IProject project)
        {
            return ContainsProjectGuid(project.Guid);
        }

        public bool ContainsProjectGuid(Guid projectGuid)
        {
            return this.Any(assignment => assignment.Project.Guid == projectGuid);
        }

        public bool ContainsProjectName(string projectName)
        {
            return ContainsKey(projectName);
        }

        public IUserProjectAssignment GetByProject(IProject project)
        {
            return this[project];
        }

        public IUserProjectAssignment GetByProjectGuid(Guid projectGuid)
        {
            return this.First(assignment => assignment.Project.Guid == projectGuid);
        }

        public IUserProjectAssignment GetByProjectName(string projectName)
        {
            return this[projectName];
        }

        public IUserProjectAssignment this[IProject project]
        {
            get { return GetByProjectGuid(project.Guid); }
        }

        public void Remove(IProject project)
        {
            UserProjectAssignment.Delete(project, User);
        }

        public ISession Session
        {
            get { return _user.Session; }
        }

        public bool TryGetByProject(IProject project, out IUserProjectAssignment assignment)
        {
            return TryGetByProjectName(project.Name, out assignment);
        }

        public bool TryGetByProjectGuid(Guid projectGuid, out IUserProjectAssignment assignment)
        {
            assignment = this.FirstOrDefault(projectAssignment => projectAssignment.Project.Guid == projectGuid);
            return assignment != null;
        }

        public bool TryGetByProjectName(string projectName, out IUserProjectAssignment assignment)
        {
            return TryGet(projectName, out assignment);
        }

        public IUser User
        {
            get { return _user; }
        }

        private List<IUserProjectAssignment> GetProjectAssignments()
        {
            const string LIST_USER_PROJECTS =
                @"<ADMINISTRATION><USER guid=""{0}""><PROJECTS action=""list"" extendedinfo=""1""/></USER></ADMINISTRATION>";

            var xmlDoc = Session.ExecuteRQL(LIST_USER_PROJECTS.RQLFormat(User));
            return (from XmlElement assignmentElement in xmlDoc.GetElementsByTagName("PROJECT")
                    select (IUserProjectAssignment) new UserProjectAssignment(_user, assignmentElement)).ToList();
        }
    }
}