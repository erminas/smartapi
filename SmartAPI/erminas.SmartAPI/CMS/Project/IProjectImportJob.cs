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
using System.Linq;
using System.Security;
using System.Xml;
using erminas.SmartAPI.CMS.Administration;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project
{
    public enum UserGroupAndAssignments
    {
        DoNotImport = 0,
        ImportWithoutReplacingExistingUsers = 1,
        ImportAndReplaceExistingUsers = 2
    }

    public interface IProjectImportJob : IAsyncJob
    {
        string DatabaseName { get; set; }
        DatabaseServer DatabaseServer { get; set; }
        string ImportFolder { get; set; }
        ApplicationServer ImportServer { get; set; }
        bool IsImportingArchive { get; set; }
        bool IsImportingReleases { get; set; }
        bool IsIncludingAdministratorSettings { get; set; }
        string ProjectName { get; }
        NewProjectType ProjectType { get; set; }
        UserGroupAndAssignments UserGroupAndAssignmentsSettings { get; set; }
    }

    internal class ProjectImportJob : AbstractAsyncJob, IProjectImportJob
    {
        private readonly string _newProjectName;

        internal ProjectImportJob(Session session, string newProjectName, string importFolder) : base(session)
        {
            _newProjectName = newProjectName;
            ProjectType = NewProjectType.TestProject;
            DatabaseServer = Session.DatabaseServers.First();
            DatabaseName = _newProjectName;
            ImportFolder = importFolder;
            ImportServer = Session.ApplicationServers.First();
            EmailSubject = "Project import completed";
            EmailMessage = "Project import completed";
        }

        public string DatabaseName { get; set; }
        public DatabaseServer DatabaseServer { get; set; }
        public string ImportFolder { get; set; }
        public ApplicationServer ImportServer { get; set; }
        public bool IsImportingArchive { get; set; }
        public bool IsImportingReleases { get; set; }
        public bool IsIncludingAdministratorSettings { get; set; }

        public string ProjectName
        {
            get { return _newProjectName; }
        }

        public NewProjectType ProjectType { get; set; }

        public override void RunAsync()
        {
            CheckImport();
            ExecuteImport();
        }

        public override void RunSync(TimeSpan maxWait)
        {
            RunAsync();
            var retryEverySecond = new TimeSpan(0, 0, 1);
            Predicate<IRDList<AsynchronousProcess>> hasImportProcess =
                list => list.Any(process => process.Type == AsynchronousProcessType.XMLImport);

            //wait for the async process to spawn first and then wait until it is done

            var start = DateTime.Now;

            Func<bool> hasImportProcessOrProjectIsAlreadyImported =
                () =>
                hasImportProcess(Session.AsynchronousProcesses.Refreshed()) ||
                Session.Projects.Refreshed().ContainsName(ProjectName);
            Wait.For(hasImportProcessOrProjectIsAlreadyImported, maxWait, retryEverySecond);

            if (Session.Projects.ContainsName(ProjectName))
            {
                return;
            }

            TimeSpan timeLeft = maxWait - (DateTime.Now - start);
            timeLeft = timeLeft.TotalMilliseconds > 0 ? timeLeft : new TimeSpan(0, 0, 0);

            Session.AsynchronousProcesses.WaitFor(list => !hasImportProcess(list), timeLeft, retryEverySecond);
        }

        public UserGroupAndAssignments UserGroupAndAssignmentsSettings { get; set; }

        private void CheckImport()
        {
            //older RedDot versions don't support the checkbeforeimport command
            if (Session.Version < new Version(10, 0))
            {
                return;
            }

            const string CHECK_IMPORT =
                @"<ADMINISTRATION><PROJECT  action=""checkbeforeimport"" xmlpath=""{0}"" /></ADMINISTRATION>";
            var xmlDoc = Session.ExecuteRQL(CHECK_IMPORT.RQLFormat(SecurityElement.Escape(ImportFolder)),
                                            Session.IODataFormat.LogonGuidOnly);
            Guid projectGuid;
            XmlElement projectElement = xmlDoc.GetSingleElement("PROJECT");
            if (projectElement == null || !projectElement.TryGetGuid(out projectGuid))
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format(
                                                "Could not import project from {0}, please check folder/share permissions.",
                                                ImportFolder));
            }
        }

        private void ExecuteImport()
        {
            const string IMPORT_PROJECT = @"<ADMINISTRATION>
                                                <PROJECT action=""import"" userguid=""{0}"" databaseserver=""{1}"" editorialserver=""{2}"" databasename=""{3}"" 
                                                emailnotification=""{4}"" includearchive=""{5}"" projectname=""{6}"" xmlpath=""{7}"" schema="""" schemapassword="""" 
                                                reddotserverguid=""{8}"" testproject=""{9}"" useoldguid=""0"" importusers=""{10}"" importobjectrelease=""{11}"" 
                                                projectguid="""" to=""{12}"" provider="""" subject=""{13}"" message=""{14}""/>
                                            </ADMINISTRATION>";
            var query = IMPORT_PROJECT.RQLFormat(Session.CurrentUser, DatabaseServer, Server,
                                                 SecurityElement.Escape(DatabaseName), IsSendingEmailOnCompletion,
                                                 IsImportingArchive, SecurityElement.Escape(ProjectName), ImportFolder,
                                                 ImportServer, (int) ProjectType, (int) UserGroupAndAssignmentsSettings,
                                                 IsImportingReleases, EmailReceipient,
                                                 SecurityElement.Escape(EmailSubject),
                                                 SecurityElement.Escape(EmailMessage));

            Session.ExecuteRQL(query, Session.IODataFormat.LogonGuidOnly);
        }
    }
}