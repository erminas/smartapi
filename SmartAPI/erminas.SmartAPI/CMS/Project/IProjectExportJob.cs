using System;
using System.Security;
using erminas.SmartAPI.CMS.Administration;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project
{
    public interface IProjectExportJob : IProjectJob
    {
        bool IsIncludingArchive { get; set; }
        bool IsIncludingAdministrationSettings { get; set; }
        bool IsLoggingOffActiveUsersInProject { get; set; }
        string TargetPath { get; set; }
    }

    internal sealed  class ProjectExportJob : AbstractProjectJob, IProjectExportJob
    {

        internal ProjectExportJob(Project project, string targetPath) : base(project)
        {
            TargetPath = targetPath;
            EmailSubject = String.Format("Project: {0}" + project.Name);
            EmailMessage = "Project export finished";
            IsLoggingOffActiveUsersInProject = true;
            IsIncludingAdministrationSettings = true;
            IsIncludingArchive = false;
        }

        public bool IsIncludingArchive { get; set; }
        public bool IsIncludingAdministrationSettings { get; set; }
        public bool IsLoggingOffActiveUsersInProject { get; set; }
        public string TargetPath { get; set; }

        public override void RunAsync()
        {
            const string EXPORT = @"<ADMINISTRATION>
                                            <PROJECT action=""export"" projectguid=""{0}"" targetpath=""{1}"" emailnotification=""{2}"" editorialserver=""{3}""
                                                includearchive=""{4}"" to=""{5}"" provider="" subject=""{6}"" message=""{7}""
                                                logoutusers=""{8}"" reddotserverguid=""{3}"" includeadmindata=""{9}"" />
                                        </ADMINISTRATION>";
            var query = EXPORT.RQLFormat(Project, SecurityElement.Escape(TargetPath), IsSendingEmailOnCompletion,
                                         Server, IsIncludingArchive, EmailReceipient,
                                         SecurityElement.Escape(EmailSubject),
                                         SecurityElement.Escape(EmailMessage), IsLoggingOffActiveUsersInProject,
                                         IsIncludingAdministrationSettings);

            Session.ExecuteRQL(query, Session.IODataFormat.LogonGuidOnly);
        }

        public override void RunSync(TimeSpan maxWait)
        {
            RunAsync();
            Session.WaitForAsyncProcess(maxWait, p => p.Type == AsynchronousProcessType.XMLExport && Project.Equals(p.Project));
        }
    }
}