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
using erminas.SmartAPI.CMS.ServerManagement;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project
{
    public interface IProjectUsers : IIndexedCachedList<string, IUserProjectAssignment>, IProjectObject
    {
        IUserProjectAssignment AddOrSet(IUser user, UserRole role, ExtendedUserRoles extendedUserRoles);
        bool ContainsUser(IUser user);
        bool ContainsUserGuid(Guid userGuid);
        bool ContainsUserName(string userName);
        IUserProjectAssignment GetByUserGuid(Guid userGuid);
        IUserProjectAssignment GetByUserName(string userName);
        IUserProjectAssignment this[IUser user] { get; }
        void Remove(IUser user);
        bool TryGetByUser(IUser user, out IUserProjectAssignment assignment);
        bool TryGetByUserGuid(Guid userGuid, out IUserProjectAssignment assignment);
        bool TryGetByUserName(string userName, out IUserProjectAssignment assignment);
    }

    internal class ProjectUsers : IndexedCachedList<string, IUserProjectAssignment>, IProjectUsers
    {
        private readonly IProject _project;

        internal ProjectUsers(IProject project, Caching caching) : base(assignment => assignment.User.Name, caching)
        {
            _project = project;
            RetrieveFunc = GetUsersOfProject;
        }

        public IUserProjectAssignment AddOrSet(IUser user, UserRole role, ExtendedUserRoles extendedUserRoles)
        {
            return UserProjectAssignment.Create(user, _project, role, extendedUserRoles);
        }

        public bool ContainsUser(IUser user)
        {
            return ContainsUserGuid(user.Guid);
        }

        public bool ContainsUserGuid(Guid userGuid)
        {
            return this.Any(assignment => assignment.User.Guid == userGuid);
        }

        public bool ContainsUserName(string userName)
        {
            return this.Any(assignment => assignment.User.Name == userName);
        }

        public IUserProjectAssignment GetByUserGuid(Guid userGuid)
        {
            return this.First(x => x.User.Guid == userGuid);
        }

        public IUserProjectAssignment GetByUserName(string userName)
        {
            return this[userName];
        }

        public IUserProjectAssignment this[IUser user]
        {
            get { return this[user.Name]; }
        }

        public IProject Project
        {
            get { return _project; }
        }

        public void Remove(IUser user)
        {
            UserProjectAssignment.Delete(Project, user);
        }

        public ISession Session
        {
            get { return _project.Session; }
        }

        public bool TryGetByUser(IUser user, out IUserProjectAssignment assignment)
        {
            return TryGetByUserGuid(user.Guid, out assignment);
        }

        public bool TryGetByUserGuid(Guid userGuid, out IUserProjectAssignment assignment)
        {
            assignment = this.FirstOrDefault(projectAssignment => projectAssignment.User.Guid == userGuid);
            return assignment != null;
        }

        public bool TryGetByUserName(string userName, out IUserProjectAssignment assignment)
        {
            return TryGet(userName, out assignment);
        }

        private List<IUserProjectAssignment> GetUsersOfProject()
        {
            try
            {
                const string GET_USERS = @"<PROJECT><USERS action=""list""/></PROJECT>";
                var xmlDoc = Session.ExecuteRQLInProjectContext(GET_USERS, _project.Guid);
                var xmlNodes = xmlDoc.GetElementsByTagName("USER");

                return (from XmlElement node in xmlNodes
                        select (IUserProjectAssignment) new UserProjectAssignment(new User(Session, node), _project))
                    .ToList();
            } catch (Exception e)
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not load users of project {0} ", this), e);
            }
        }
    }
}