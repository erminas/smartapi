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
using erminas.SmartAPI.CMS.Project.ContentClasses;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Folder
{
    public interface IContentClassFolderSharing : IIndexedRDList<string, IProject>, IProjectObject
    {
        void Add(IProject project);
        void AddRange(IEnumerable<IProject> project);
        IContentClassFolder ContentClassFolder { get; }
        void Remove(IProject project);
    }

    internal class ContentClassFolderSharing : NameIndexedRDList<IProject>, IContentClassFolderSharing
    {
        private const string SHARING =
            @"<SHAREDFOLDER shared=""{0}"" action=""save"" guid=""{1}"" ><PROJECTS>{2}</PROJECTS></SHAREDFOLDER>";

        private const string SINGLE_PROJECT = @"<PROJECT guid=""{0}"" sharedrights=""{1}"" />";
        private readonly ContentClassFolder _contentClassFolder;

        internal ContentClassFolderSharing(ContentClassFolder contentClassFolder, Caching caching) : base(caching)
        {
            _contentClassFolder = contentClassFolder;
            RetrieveFunc = GetSharedToProjects;
        }

        public void Add(IProject project)
        {
            AddRange(new[] {project});
        }

        public void AddRange(IEnumerable<IProject> projects)
        {
            const bool IS_SHARED = true;
            var projectsRql = projects.Aggregate("", (s, project) => s + SINGLE_PROJECT.RQLFormat(project, IS_SHARED));

            var query = SHARING.RQLFormat(IS_SHARED, _contentClassFolder, projectsRql);
            Project.ExecuteRQL(query, RqlType.SessionKeyInProject);

            InvalidateCache();
        }

        public IContentClassFolder ContentClassFolder
        {
            get { return _contentClassFolder; }
        }

        public IProject Project
        {
            get { return _contentClassFolder.Project; }
        }

        public void Remove(IProject project)
        {
            var isStillSharing = Count > 1 || (Count == 1 && !this.First().Equals(project));
            Project.ExecuteRQL(
                SHARING.RQLFormat(isStillSharing, _contentClassFolder, SINGLE_PROJECT.RQLFormat(project, 0)),
                RqlType.SessionKeyInProject);

            InvalidateCache();
        }

        public ISession Session
        {
            get { return _contentClassFolder.Session; }
        }

        private List<IProject> GetSharedToProjects()
        {
            const string LOAD_SHARED = @"<SHAREDFOLDER action=""load"" guid=""{0}""/>";

            var xmlDoc = Project.ExecuteRQL(LOAD_SHARED.RQLFormat(_contentClassFolder), RqlType.SessionKeyInProject);
            var projectNodes = xmlDoc.SelectNodes("//PROJECT[@sharedrights=1]");

            //server manager can access all projects, others might only be able to see the name/guid
            if (Session.CurrentUser.ModuleAssignment.IsServerManager)
            {
                return
                    (from XmlElement curProject in projectNodes select Session.Projects.GetByGuid(curProject.GetGuid()))
                        .ToList();
            }

            return (from XmlElement curProject in projectNodes
                    select (IProject) new Project(Session, curProject.GetGuid()) {Name = curProject.GetName()}).ToList();
        }
    }

    public interface IContentClassFolder : IRedDotObject, IProjectObject
    {
        /// <summary>
        ///     All content classes contained in this folder, indexed by name. The list is cached by default.
        /// </summary>
        IIndexedRDList<string, IContentClass> ContentClasses { get; }

        bool IsBroken { get; }

        bool IsSharedToOtherProjects { get; }
        IContentClassFolder SharedFrom { get; }
        IContentClassFolderSharing SharedTo { get; }
    }

    /// <summary>
    ///     A folder containing content classes.
    /// </summary>
    internal sealed class ContentClassFolder : RedDotProjectObject, IContentClassFolder
    {
        private IProject _project;
        private Lazy<IContentClassFolder> _sharedFrom;

        internal ContentClassFolder(IProject project, Guid guid) : base(project, guid)
        {
            Init(project);
        }

        internal ContentClassFolder(IProject project, XmlElement xmlElement) : base(project, xmlElement)
        {
            Init(project);
        }

        /// <summary>
        ///     All content classes contained in this folder, indexed by name. The list is cached by default.
        /// </summary>
        public IIndexedRDList<string, IContentClass> ContentClasses { get; private set; }

        public bool IsBroken { get; internal set; }

        public bool IsSharedToOtherProjects
        {
            get { return SharedTo.Any(); }
        }

        public IContentClassFolder SharedFrom
        {
            get { return _sharedFrom.Value; }
        }

        public IContentClassFolderSharing SharedTo { get; private set; }

        private List<IContentClass> GetContentClasses()
        {
            // RQL for listing all content classes of a folder. 
            // One Parameter: Folder Guid: {0}
            const string LIST_CC_OF_FOLDER = @"<TEMPLATES folderguid=""{0}"" action=""list""/>";

            XMLDoc = _project.ExecuteRQL(string.Format(LIST_CC_OF_FOLDER, Guid.ToRQLString()));

            return (from XmlElement curNode in XMLDoc.GetElementsByTagName("TEMPLATE")
                    select (IContentClass) new ContentClass(_project, curNode)).ToList();
        }

        private IContentClassFolder GetSharedFrom()
        {
            const string LOAD_FOLDER = @"<PROJECT><FOLDER action=""load"" guid=""{0}""/></PROJECT>";
            var xmlDoc = Project.ExecuteRQL(LOAD_FOLDER.RQLFormat(this));
            XmlElement = xmlDoc.GetSingleElement("FOLDER");
            Guid sharedProjectGuid, sharedFolderGuid;
            if (XmlElement.TryGetGuid("linkedprojectguid", out sharedProjectGuid) &&
                XmlElement.TryGetGuid("linkedfolderguid", out sharedFolderGuid))
            {
                if (IsBroken)
                {
                    throw new BrokenContentClassFolderSharingException(Session.ServerLogin, this, sharedProjectGuid,
                                                                       sharedFolderGuid);
                }
                if (Session.CurrentUser.ModuleAssignment.IsServerManager)
                {
                    IProject project = Session.Projects.GetByGuid(sharedProjectGuid);
                    return GetSharedFromFolder(project, sharedFolderGuid);
                }
                if (Session.ProjectsForCurrentUser.ContainsGuid(sharedProjectGuid))
                {
                    IProject project = Session.ProjectsForCurrentUser.GetByGuid(sharedProjectGuid);
                    return GetSharedFromFolder(project, sharedFolderGuid);
                }
                var sharedProject = new Project(Session, sharedProjectGuid);
                return new ContentClassFolder(sharedProject, sharedFolderGuid);
            }
            return null;
        }

        private static IContentClassFolder GetSharedFromFolder(IProject project, Guid sharedFolderGuid)
        {
            return
                project.ContentClassFolders.Union(project.ContentClassFolders.Broken)
                       .First(x => x.Guid == sharedFolderGuid);
        }

        private void Init(IProject project)
        {
            ContentClasses = new NameIndexedRDList<IContentClass>(GetContentClasses, Caching.Enabled);
            SharedTo = new ContentClassFolderSharing(this, Caching.Enabled);
            _project = project;
            _sharedFrom = new Lazy<IContentClassFolder>(GetSharedFrom);
        }
    }
}