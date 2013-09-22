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
using erminas.SmartAPI.CMS.Project;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.ServerManagement
{
    internal class Projects : NameIndexedRDList<IProject>, IProjects
    {
        private readonly ISession _session;

        public Projects(ISession session, Caching caching) : base(caching)
        {
            _session = session;
            RetrieveFunc = GetProjects;
            ForCurrentUser =
                new NameIndexedRDList<IProject>(() => ForUser(_session.ServerManager.Users.Current.Guid).ToList(),
                                                Caching.Enabled);
        }

        public IProjectImportJob CreateImportJob(string newProjectName, string importPath)
        {
            return new ProjectImportJob(_session, newProjectName, importPath);
        }

        public IProject CreateProjectMsSql(string projectName, IApplicationServer appServer, IDatabaseServer dbServer,
                                           string databaseName, ISystemLocale language, CreatedProjectType type,
                                           UseVersioning useVersioning, IUser user)
        {
            const string CREATE_PROJECT =
                @"<ADMINISTRATION><PROJECT action=""addnew"" projectname=""{0}"" databaseserverguid=""{1}"" editorialserverguid=""{2}"" databasename=""{3}""
versioning=""{4}"" testproject=""{5}""><LANGUAGEVARIANTS><LANGUAGEVARIANT language=""{7}"" name=""{8}"" /></LANGUAGEVARIANTS><USERS><USER action=""assign"" guid=""{6}""/></USERS></PROJECT></ADMINISTRATION>";

            XmlDocument result = Session.ParseRQLResult(_session,
                                                        _session.ExecuteRQLRaw(
                                                            CREATE_PROJECT.RQLFormat(projectName, dbServer, appServer,
                                                                                     databaseName, (int) useVersioning,
                                                                                     (int) type, user,
                                                                                     language.LanguageAbbreviation,
                                                                                     language.Language),
                                                            RQL.IODataFormat.SessionKeyAndLogonGuid));

            string guidStr = result.InnerText;
            Guid projectGuid;
            if (!Guid.TryParse(guidStr, out projectGuid))
            {
                throw new SmartAPIException(_session.ServerLogin,
                                            string.Format("Could not create project {0}", projectName));
            }

            InvalidateCache();
            return new Project.Project(_session, projectGuid);
        }

        public IIndexedRDList<string, IProject> ForCurrentUser { get; private set; }

        /// <summary>
        ///     Get all projects a specific user has access to
        /// </summary>
        /// <param name="userGuid"> Guid of the user </param>
        /// <returns> All projects the user with Guid==userGuid has access to </returns>
        public IRDEnumerable<IProject> ForUser(Guid userGuid)
        {
            const string LIST_PROJECTS_FOR_USER =
                @"<ADMINISTRATION><USER guid=""{0}""><PROJECTS action=""list"" extendedinfo=""1""/></USER></ADMINISTRATION>";
            XmlDocument xmlDoc = _session.ExecuteRQL(String.Format(LIST_PROJECTS_FOR_USER, userGuid.ToRQLString()));
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("PROJECT");
            return
                (from XmlElement curNode in xmlNodes select (IProject) new Project.Project(_session, curNode))
                    .ToRDEnumerable();
        }

        private List<IProject> GetProjects()
        {
            const string LIST_PROJECTS = @"<ADMINISTRATION><PROJECTS action=""list""/></ADMINISTRATION>";
            XmlDocument xmlDoc = _session.ExecuteRQL(LIST_PROJECTS);
            XmlNodeList projectNodes = xmlDoc.GetElementsByTagName("PROJECT");
            return
                (from XmlElement curNode in projectNodes select (IProject) new Project.Project(_session, curNode))
                    .ToList();
        }
    }

    public interface IProjects : IIndexedRDList<string, IProject>
    {
        IProjectImportJob CreateImportJob(string newProjectName, string importPath);

        IProject CreateProjectMsSql(string projectName, IApplicationServer appServer, IDatabaseServer dbServer,
                                    string databaseName, ISystemLocale language, CreatedProjectType type,
                                    UseVersioning useVersioning, IUser user);

        IIndexedRDList<string, IProject> ForCurrentUser { get; }
        IRDEnumerable<IProject> ForUser(Guid userGuid);
    }
}