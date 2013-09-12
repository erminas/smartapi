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
using erminas.SmartAPI.CMS.Project.Folder;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.Exceptions
{
    public class BrokenContentClassFolderSharingException : SmartAPIException
    {
        private readonly Guid _sharedFromFolderGuid;
        private readonly Guid _sharedFromProjectGuid;

        public BrokenContentClassFolderSharingException(ServerLogin login, IContentClassFolder folder,
                                                        Guid sharedFromProjectGuid, Guid sharedFromFolderGuid)
            : base(
                login,
                string.Format(
                    "Cannot load project/folder information on broken content class folder {0}. Missing project/folder: {1}/{2}",
                    folder, sharedFromProjectGuid, sharedFromFolderGuid))
        {
            _sharedFromProjectGuid = sharedFromProjectGuid;
            _sharedFromFolderGuid = sharedFromFolderGuid;
        }

        public Guid SharedFromFolderGuid
        {
            get { return _sharedFromFolderGuid; }
        }

        public Guid SharedFromProjectGuid
        {
            get { return _sharedFromProjectGuid; }
        }
    }
}