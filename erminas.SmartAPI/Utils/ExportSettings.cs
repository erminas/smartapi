/*
 * Smart API - .Net programatical access to RedDot servers
 * Copyright (C) 2012  erminas GbR 
 *
 * This program is free software: you can redistribute it and/or modify it 
 * under the terms of the GNU General Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details. 
 *
 * You should have received a copy of the GNU General Public License along with this program.
 * If not, see <http://www.gnu.org/licenses/>. 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using erminas.SmartAPI.CMS;

namespace erminas.SmartAPI.Utils
{
    public class ExportSettings
    {
        private readonly string _targetPath;
        private readonly bool _createFolderForEachExport;
        private readonly bool _includeAdminData;
        private readonly bool _includeArchive;
        private readonly bool _logoutUsers;
        private readonly CMSServer _editorialServer;
        private readonly CMSServer _redDotServer;
        private readonly bool _emailNotification;
        private readonly User _sendTo;
        private readonly string _subject;
        private readonly string _message;

        #region Properties

        public string TargetPath
        {
            get { return _targetPath; }
        }

        public string CreateFolderForEachExport
        {
            get { return _createFolderForEachExport ? "1" : "0"; }
        }

        public string IncludeAdminData
        {
            get { return _includeAdminData ? "1" : "0"; }
        }

        public string IncludeArchive
        {
            get { return _includeArchive ? "1" : "0"; }
        }

        public string EditorialServerGuid
        {
            get { return _editorialServer != null ? _editorialServer.Guid.ToRQLString() : null; }
        }

        public string LogoutUsers
        {
            get { return _logoutUsers ? "1" : "0"; }
        }

        public string RedDotServerGuid
        {
            get { return _redDotServer != null ? _redDotServer.Guid.ToRQLString() : null; }
        }

        public string EmailNotification
        {
            get { return _emailNotification ? "1" : "0"; }
        }

        public string SendTo
        {
            get { return _sendTo != null ? _sendTo.Guid.ToRQLString() : null; }
        }

        public string Subject
        {
            get { return _subject; }
        }

        public string Message
        {
            get { return _message; }
        }
        #endregion

        /// <summary>
        ///   Create an Exportsettings Object that stores parameters for project export
        /// </summary>
        /// <param name="targetPath"> Path to a folder in which the project export will be stored. Can be a path or UNC path </param>
        /// <param name="createFolderForEachExport"> Determine if export path will be extended with the subdirectory "Date_Time_ProjectName"</param>
        /// <param name="includeAdminData"> Determine if admin data should be included in the export</param>
        /// <param name="includeArchive"> Determine if archive should be exported</param>
        /// <param name="logoutUsers"> Determine if active users are logged of the project</param>
        /// <param name="editorialServer"> CMSServer that sends the e-mail. If no separate server is specified, the application server where this RQL is executed is used.  </param>
        /// <param name="reddotServer">  CMSServer where the export is carried out. If no separate server is specified, the application server where this RQL is executed is used for the export.   </param>
        /// <param name="emailNotification"> Determine if an e-mail will be sent when export is complete</param>
        /// <param name="sendTo"> Determine the user who should receive the email</param>
        /// <param name="subject"> Determine the subject of the email</param>
        /// <param name="message"> Determine the message of the email</param>     
        /// <returns> ExportSettings Object containing all possible values in this RQL query </returns>
        public ExportSettings(string targetPath = "", bool createFolderForEachExport = false, bool includeAdminData = false, bool includeArchive = false, bool logoutUsers = false, CMSServer editorialServer = null, CMSServer reddotServer = null, bool emailNotification = false, User sendTo = null, string subject = "", string message = "")
        {
            _targetPath = targetPath;
            _createFolderForEachExport = createFolderForEachExport;
            _includeAdminData = includeAdminData;
            _includeArchive = includeArchive;
            _logoutUsers = logoutUsers;
            _editorialServer = editorialServer;
            _redDotServer = reddotServer;
            _emailNotification = emailNotification;

            if (emailNotification && sendTo != null)
            {
                _sendTo = sendTo;
                _subject = subject;
                _message = message;
            }
        }

        public string ToRQLString()
        {
            string exportSettings = "";

            if (!string.IsNullOrEmpty(_targetPath))
            {
                exportSettings += string.Format(@"targetpath=""{0}"" ", _targetPath);
            }

            exportSettings += string.Format(@"createfolderforeachexport=""{0}"" ", CreateFolderForEachExport);
            exportSettings += string.Format(@"includeAdminData=""{0}"" ", IncludeAdminData);
            exportSettings += string.Format(@"logoutUsers=""{0}"" ", LogoutUsers);
            if (EditorialServerGuid != null)
            {
                exportSettings += string.Format(@"editorialServer=""{0}"" ", EditorialServerGuid);
            }

            if (RedDotServerGuid != null)
            {
                exportSettings += string.Format(@"reddotserverguid=""{0}"" ", RedDotServerGuid);
            }

            if (EmailNotification == "1" && SendTo != null)
            {
                exportSettings += string.Format(@"emailnotification=""{0}"" ", EmailNotification);
                exportSettings += string.Format(@"to=""{0}"" ", SendTo);
                exportSettings += string.Format(@"subject=""{0}"" ", Subject);
                exportSettings += string.Format(@"message=""{0}"" ", Message);
            }
            else
            {
                exportSettings += @"emailnotification=""0"" ";
            }
            return exportSettings;
        }
    }
}
