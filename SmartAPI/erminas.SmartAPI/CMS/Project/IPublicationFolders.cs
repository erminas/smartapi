using System;
using erminas.SmartAPI.CMS.Project.Publication;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project
{
    public interface IPublicationFolders : IRDList<IPublicationFolder>
    {
        [Obsolete("Only for testing purposes, API part of parent folder will change before real release")]
        IPublicationFolder Create(string name, PublicationFolderType type, Guid parentFolderGuid);
    }
}