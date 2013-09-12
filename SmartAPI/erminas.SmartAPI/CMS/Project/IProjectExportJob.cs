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
using System.Security;
using erminas.SmartAPI.CMS.Administration;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project
{
    public interface IProjectExportJob : IAsyncProjectJob
    {
        bool IsIncludingAdministrationSettings { get; set; }
        bool IsIncludingArchive { get; set; }
        bool IsLoggingOffActiveUsersInProject { get; set; }
        string TargetPath { get; set; }
    }

    internal sealed class ProjectExportJob : AbstractAsyncProjectJob, IProjectExportJob
    {
        internal ProjectExportJob(IProject project, string targetPath) : base(project)
        {
            TargetPath = targetPath;
            EmailSubject = String.Format("Project: {0}", project.Name);
            EmailMessage = "Project export finished";
            IsLoggingOffActiveUsersInProject = true;
            IsIncludingAdministrationSettings = true;
            IsIncludingArchive = false;
        }

        public bool IsIncludingAdministrationSettings { get; set; }
        public bool IsIncludingArchive { get; set; }
        public bool IsLoggingOffActiveUsersInProject { get; set; }

        public override void RunAsync()
        {
            const string EXPORT =
                @"<ADMINISTRATION><PROJECT action=""export"" projectguid=""{0}"" targetpath=""{1}"" emailnotification=""{2}""
editorialserver=""{3}"" includearchive=""{4}"" to=""{5}"" provider="""" subject=""{6}"" 
message=""{7}"" logoutusers=""{8}"" reddotserverguid=""{3}"" includeadmindata=""{9}"" />
</ADMINISTRATION>";
            var query = EXPORT.RQLFormat(Project, SecurityElement.Escape(TargetPath), IsSendingEmailOnCompletion, Server,
                                         IsIncludingArchive, EmailReceipient, SecurityElement.Escape(EmailSubject),
                                         SecurityElement.Escape(EmailMessage), IsLoggingOffActiveUsersInProject,
                                         IsIncludingAdministrationSettings);

            Session.ExecuteRQL(query, RQL.IODataFormat.LogonGuidOnly);
        }

        public override void RunSync(TimeSpan maxWait)
        {
            RunAsync();
            Session.WaitForAsyncProcess(maxWait,
                                        p => p.Type == AsynchronousProcessType.XMLExport && Project.Equals(p.Project));
        }

        public string TargetPath { get; set; }
    }
}