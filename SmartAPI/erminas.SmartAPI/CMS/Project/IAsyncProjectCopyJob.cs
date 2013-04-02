using System;
using System.Linq;
using System.Security;
using erminas.SmartAPI.CMS.Administration;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project
{
    public interface IAsyncProjectCopyJob : IAsyncProjectJob
    {
        string DatabaseName { get; set; }
        DatabaseServer DatabaseServer { get; set; }
        bool IsCopyingArchive { get; set; }
        bool IsLoggingOffActiveUsersInProject { get; set; }
        string NewProjectName { get; }
        NewProjectType ProjectType { get; set; }
    }

    internal sealed class AsyncProjectCopyJob : AbstractAsyncProjectJob, IAsyncProjectCopyJob
    {
        private readonly string _newProjectName;

        internal AsyncProjectCopyJob(Project sourceProject, string newProjectName) : base(sourceProject)
        {
            _newProjectName = newProjectName;
            DatabaseName = _newProjectName;
            IsLoggingOffActiveUsersInProject = true;
            ProjectType = NewProjectType.TestProject;
            EmailSubject = String.Format("Finished copying project ({0})", sourceProject.Name);
            EmailMessage = String.Format("Finished copying project. ({0})", sourceProject.Name);
            DatabaseServer dbServer;
            if (!Session.DatabaseServers.TryGetByName("localhost", out dbServer))
            {
                dbServer = Session.DatabaseServers.First(server => server.IsCreateAllowed);
            }
            DatabaseServer = dbServer;
        }

        public string DatabaseName { get; set; }
        public DatabaseServer DatabaseServer { get; set; }
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

            string query = COPY_PROJECT.RQLFormat(Session.CurrentUser, Project,
                                                  SecurityElement.Escape(NewProjectName),
                                                  SecurityElement.Escape(DatabaseName),
                                                  IsCopyingArchive, IsLoggingOffActiveUsersInProject,
                                                  DatabaseServer, Server,
                                                  IsSendingEmailOnCompletion, EmailReceipient,
                                                  SecurityElement.Escape(EmailSubject),
                                                  SecurityElement.Escape(EmailMessage));
            Session.ExecuteRQL(query);
        }

        public override void RunSync(TimeSpan maxWait)
        {
            RunAsync();
            Session.WaitForAsyncProcess(maxWait, p => p.Type == AsynchronousProcessType.CopyProject && Project.Equals(p.Project));
        }
    }
}