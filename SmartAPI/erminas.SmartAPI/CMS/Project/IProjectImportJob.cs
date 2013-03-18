using System;
using System.Linq;
using System.Security;
using erminas.SmartAPI.CMS.Administration;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project
{

    public enum UserGroupAndAssignments
    {
        DoNotImport = 0,
        ImportWithoutReplacingExistingUsers = 1,
        ImportAndReplaceExistingUsers = 2
    }

    public interface IProjectImportJob : IProjectJob
    {
        NewProjectType ProjectType { get; set; }
        string ProjectName { get; }
        DatabaseServer DatabaseServer { get; set; }
        string DatabaseName { get; set; }
        string ImportFolder { get; set; }
        bool IsImportingArchive { get; set; }
        bool IsIncludingAdministratorSettings { get; set; }
        UserGroupAndAssignments UserGroupAndAssignmentsSettings { get; set; }
        ApplicationServer ImportServer { get; set; }
        bool IsImportingReleases { get; set; }
    }

    internal class ProjectProjectImportJob : AbstractProjectJob,  IProjectImportJob
    {
        private readonly string _newProjectName;

        public ProjectProjectImportJob(Project project, string newProjectName, string importFolder) : base(project)
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

        public override void RunAsync()
        {
            const string IMPORT_PROJECT = @"<ADMINISTRATION>
                                                <PROJECT action=""import"" userguid=""{0}"" databaseserver=""{1}"" editorialserver=""{2}"" databasename=""{3}"" 
                                                emailnotification=""{4}"" includearchive=""{5}"" projectname=""{6}"" xmlpath=""{7}"" schema="""" schemapassword="""" 
                                                reddotserverguid=""{8}"" testproject=""{9}"" useoldguid=""0"" importusers=""{10}"" importobjectrelease=""{11}"" 
                                                projectguid="""" to=""{12}"" provider="""" subject=""{13}"" message=""{14}""/>
                                            </ADMINISTRATION>";
            var query = IMPORT_PROJECT.RQLFormat(Session.CurrentUser, DatabaseServer, Server, SecurityElement.Escape(DatabaseName), IsSendingEmailOnCompletion,
                IsImportingArchive, SecurityElement.Escape(ProjectName), ImportFolder, ImportServer, (int)ProjectType, (int)UserGroupAndAssignmentsSettings, 
                IsImportingReleases, EmailReceipient, SecurityElement.Escape(EmailSubject), SecurityElement.Escape(EmailMessage));

            Session.ExecuteRQL(query, Session.IODataFormat.LogonGuidOnly);
        }

        public override void RunSync(TimeSpan maxWait)
        {
            RunAsync();
            Session.WaitForAsyncProcess(maxWait, process => process.Type==AsynchronousProcessType.XMLImport && Project.Equals(process.Project));
        }

        public NewProjectType ProjectType { get; set; }
        public string ProjectName { get { return _newProjectName; } }
        public DatabaseServer DatabaseServer { get; set; }
        public string DatabaseName { get; set; }
        public string ImportFolder { get; set; }
        public bool IsImportingArchive { get; set; }
        public bool IsIncludingAdministratorSettings { get; set; }
        public UserGroupAndAssignments UserGroupAndAssignmentsSettings { get; set; }
        public ApplicationServer ImportServer { get; set; }
        public bool IsImportingReleases { get; set; }
    }
}
