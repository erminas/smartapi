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
using System.Linq;
using System.Security;
using erminas.SmartAPI.CMS.Administration;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project
{
    public interface IProjectCopyJob : IAsyncProjectJob
    {
        string DatabaseName { get; set; }
        IDatabaseServer DatabaseServer { get; set; }
        bool IsCopyingArchive { get; set; }
        bool IsLoggingOffActiveUsersInProject { get; set; }
        string NewProjectName { get; }
        NewProjectType ProjectType { get; set; }
    }

    internal sealed class ProjectCopyJob : AbstractAsyncProjectJob, IProjectCopyJob
    {
        private readonly string _newProjectName;

        internal ProjectCopyJob(IProject sourceProject, string newProjectName) : base(sourceProject)
        {
            _newProjectName = newProjectName;
            DatabaseName = _newProjectName;
            IsLoggingOffActiveUsersInProject = true;
            ProjectType = NewProjectType.TestProject;
            EmailSubject = String.Format("Finished copying project ({0})", sourceProject.Name);
            EmailMessage = String.Format("Finished copying project. ({0})", sourceProject.Name);
            IDatabaseServer dbServer;
            if (!Session.DatabaseServers.TryGetByName("localhost", out dbServer))
            {
                dbServer = Session.DatabaseServers.First(server => server.IsCreateAllowed);
            }
            DatabaseServer = dbServer;
        }

        public string DatabaseName { get; set; }
        public IDatabaseServer DatabaseServer { get; set; }
        public bool IsCopyingArchive { get; set; }
        public bool IsLoggingOffActiveUsersInProject { get; set; }

        public string NewProjectName
        {
            get { return _newProjectName; }
        }

        public NewProjectType ProjectType { get; set; }

        public override void RunAsync()
        {
            const string COPY_PROJECT =
                @"<ADMINISTRATION><PROJECT userguid=""{0}"" action=""copy"" guid=""{1}"" newprojectname=""{2}"" newdatabasename=""{3}"" includearchive=""{4}"" logoutusers=""{5}"" schema="""" testproject="""" schemapassword="""" databaseserver=""{6}"" editorialserver=""{7}"" reddotserverguid="""" emailnotification=""{8}"" to=""{9}"" provider="""" subject=""{10}"" message=""{11}"" /></ADMINISTRATION>";

            string query = COPY_PROJECT.RQLFormat(Session.CurrentUser, Project, SecurityElement.Escape(NewProjectName),
                                                  SecurityElement.Escape(DatabaseName), IsCopyingArchive,
                                                  IsLoggingOffActiveUsersInProject, DatabaseServer, Server,
                                                  IsSendingEmailOnCompletion, EmailReceipient,
                                                  SecurityElement.Escape(EmailSubject),
                                                  SecurityElement.Escape(EmailMessage));
            Session.ExecuteRQL(query);
        }

        public override void RunSync(TimeSpan maxWait)
        {
            RunAsync();
            if (Session.ServerVersion < new Version(11, 0))
            {
                //we wait for a copy process to appear and disappear
                //this doesn't work reliably in newer OpentText/RedDot versions,
                //because the process doesn't show up in the process list long enough.
                Session.WaitForAsyncProcess(maxWait,
                                            process =>
                                            process.Type == AsynchronousProcessType.CopyProject &&
                                            Project.Equals(process.Project));
            }
            else
            {
                var retryEverySecond = new TimeSpan(0, 0, 1);
                Session.Projects.WaitFor(list =>
                    {
                        IProject project;
                        return list.Refreshed().TryGetByName(NewProjectName, out project) &&
                               !project.Refreshed().IsLockedBySystem;
                    }, maxWait, retryEverySecond);
            }
        }
    }
}