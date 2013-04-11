using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Administration;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project
{
    public interface IUserProjects : IIndexedCachedList<string, IUserProjectAssignment>, ISessionObject
    {
        IUser User { get; }

        IUserProjectAssignment AddOrSet(IProject project, UserRole role, ExtendedUserRoles extendedRoles);
        void Remove(IProject project);
        IUserProjectAssignment GetByProjectName(string projectName);
        IUserProjectAssignment GetByProjectGuid(Guid projectGuid);
        IUserProjectAssignment GetByProject(IProject project);
        IUserProjectAssignment this[IProject project] { get; }
        bool TryGetByProjectName(string projectName, out IUserProjectAssignment assignment);
        bool TryGetByProjectGuid(Guid projectGuid, out IUserProjectAssignment assignment);
        bool TryGetByProject(IProject project, out IUserProjectAssignment assignment);
        bool ContainsProjectName(string projectName);
        bool ContainsProjectGuid(Guid projectGuid);
        bool ContainsProject(IProject project);
    }

    internal class UserProjects : IndexedCachedList<string, IUserProjectAssignment>, IUserProjects
    {
        private readonly IUser _user;

        internal UserProjects(IUser user, Caching caching) : base(assignment => assignment.Project.Name, caching)
        {
            _user = user;
            RetrieveFunc = GetProjectAssignments;
        }

        public IUser User { get { return _user; } }

        public IUserProjectAssignment AddOrSet(IProject project, UserRole role, ExtendedUserRoles extendedRoles)
        {
            return UserProjectAssignment.Create(_user, project, role, extendedRoles);
        }

        public void Remove(IProject project)
        {
            UserProjectAssignment.Delete(project, User);
        }

        public IUserProjectAssignment GetByProjectName(string projectName)
        {
            return this[projectName];
        }

        public IUserProjectAssignment GetByProjectGuid(Guid projectGuid)
        {
            return this.First(assignment => assignment.Project.Guid == projectGuid);
        }

        public IUserProjectAssignment GetByProject(IProject project)
        {
            return this[project];
        }

        public IUserProjectAssignment this[IProject project]
        {
            get { return GetByProjectGuid(project.Guid); }
        }

        public bool TryGetByProjectName(string projectName, out IUserProjectAssignment assignment)
        {
            return TryGet(projectName, out assignment);
        }

        public bool TryGetByProjectGuid(Guid projectGuid, out IUserProjectAssignment assignment)
        {
            assignment = this.FirstOrDefault(projectAssignment => projectAssignment.Project.Guid == projectGuid);
            return assignment != null;
        }

        public bool TryGetByProject(IProject project, out IUserProjectAssignment assignment)
        {
            return TryGetByProjectName(project.Name, out assignment);
        }

        public bool ContainsProjectName(string projectName)
        {
            return ContainsKey(projectName);
        }

        public bool ContainsProjectGuid(Guid projectGuid)
        {
            return this.Any(assignment => assignment.Project.Guid == projectGuid);
        }

        public bool ContainsProject(IProject project)
        {
            return ContainsProjectGuid(project.Guid);
        }

        public Session Session { get { return _user.Session; } }

        private List<IUserProjectAssignment> GetProjectAssignments()
        {
            const string LIST_USER_PROJECTS =
                @"<ADMINISTRATION><USER guid=""{0}""><PROJECTS action=""list"" extendedinfo=""1""/></USER></ADMINISTRATION>";

            var xmlDoc = Session.ExecuteRQL(LIST_USER_PROJECTS.RQLFormat(User));
            return (from XmlElement assignmentElement in xmlDoc.GetElementsByTagName("PROJECT")
                    select (IUserProjectAssignment)new UserProjectAssignment(_user, assignmentElement)).ToList();
        }
    }
}
