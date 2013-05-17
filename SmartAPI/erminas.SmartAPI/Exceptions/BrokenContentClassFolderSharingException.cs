using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using erminas.SmartAPI.CMS.Project.Folder;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.Exceptions
{
    public class BrokenContentClassFolderSharingException : SmartAPIException 
    {
        private readonly Guid _sharedFromProjectGuid;
        private readonly Guid _sharedFromFolderGuid;

        public BrokenContentClassFolderSharingException(ServerLogin login, IContentClassFolder folder, Guid sharedFromProjectGuid, Guid sharedFromFolderGuid) : base(login, string.Format("Cannot load project/folder information on broken content class folder {0}. Missing project/folder: {1}/{2}", folder, sharedFromProjectGuid, sharedFromFolderGuid))
        {
            _sharedFromProjectGuid = sharedFromProjectGuid;
            _sharedFromFolderGuid = sharedFromFolderGuid;
        }

        public Guid SharedFromProjectGuid
        {
            get { return _sharedFromProjectGuid; }
        }

        public Guid SharedFromFolderGuid
        {
            get { return _sharedFromFolderGuid; }
        }
    }
}
