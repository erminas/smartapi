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
using System.Xml;
using erminas.SmartAPI.CMS.Administration;
using erminas.SmartAPI.CMS.Project;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.ServerManagement
{
    public enum AsynchronousProcessType
    {
        Publication = 0,
        CleanupLiveServer = 1,
        EscalationProcedure = 2,
        XMLExport = 3,
        XMLImport = 4,
        Import3To4 = 5,
        CopyProject = 6,
        InheritPublicationPackage = 7,
        CheckURLs = 8,
        RedDotDatabaseBackup = 9,
        ContentClassReplacement = 10,
        UploadMediaElement = 11,
        CopyTreesegment = 12,
        PageForwarding = 13,
        ScheduledJob = 14,
        PublishingQueue = 15,
        DeletePagesViaFTP = 16,
        FTPTransfer = 17,
        ExportInstances = 18,
        StartUserdefinedJob = 19,
        XCMSProjectNotifications = 20,
        CheckSpelling = 21,
        ValidatePage = 22,
        FindAndReplace = 23,
        ProjectReport = 24,
        CheckReferencesToOtherProjects = 25,
        DeletePagesViaFTPInheritPublicationPackage = 26,
    }

    public interface IAsynchronousProcess : IRedDotObject
    {
        IProject Project { get; }
        AsynchronousProcessType Type { get; }
        IUser User { get; }
    }

    internal class AsynchronousProcess : RedDotObject, IAsynchronousProcess
    {
        private AsynchronousProcessType _type;

        internal AsynchronousProcess(ISession session, XmlElement xmlElement) : base(session, xmlElement)
        {
            LoadXml();
        }

        public IProject Project { get; private set; }

        public AsynchronousProcessType Type
        {
            get { return _type; }
        }

        public IUser User { get; private set; }

        private void LoadXml()
        {
            Guid userGuid;
            if (_xmlElement.TryGetGuid("user", out userGuid))
            {
                User = new User(Session, userGuid) {Name = XmlElement.GetAttributeValue("username")};
            }

            Guid projectGuid;
            if (_xmlElement.TryGetGuid("project", out projectGuid))
            {
                Project = new Project.Project(Session, projectGuid) {Name = XmlElement.GetAttributeValue("projectname")};
            }

            EnsuredInit(ref _type, "category", s => (AsynchronousProcessType) int.Parse(s));
        }
    }
}