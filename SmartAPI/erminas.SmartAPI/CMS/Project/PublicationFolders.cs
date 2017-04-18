using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Project.Publication;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project
{
    internal class PublicationFolders : RDList<IPublicationFolder>, IPublicationFolders
    {
        private readonly IProject _project;

        public PublicationFolders(IProject project) : base(Caching.Enabled)
        {
            _project = project;
            RetrieveFunc = GetPublicationFolders;
        }

        public IPublicationFolder Create(string name, PublicationFolderType type, Guid parentFolderGuid)
        {
            var folder = new PublicationFolder(name, type);
            var resultFolder = folder.CreateInProject(_project, parentFolderGuid);
            InvalidateCache();

            return resultFolder;
        }

        private List<IPublicationFolder> GetPublicationFolders()
        {
            const string LIST_PUBLICATION_FOLDERS = @"<PROJECT><EXPORTFOLDERS action=""list"" /></PROJECT>";

            var xmlDoc = _project.ExecuteRQL(LIST_PUBLICATION_FOLDERS);
            if (xmlDoc.GetElementsByTagName("EXPORTFOLDERS")
                    .Count != 1)
            {
                throw new SmartAPIException(
                    _project.Session.ServerLogin,
                    string.Format("Could not retrieve publication folders of project {0}", this));
            }
            return (from XmlElement curFolder in xmlDoc.GetElementsByTagName("EXPORTFOLDER")
                select (IPublicationFolder) new PublicationFolder(_project, curFolder.GetGuid())).ToList();
        }
    }
}