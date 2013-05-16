using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Folder
{
    internal class SubFolders : NameIndexedRDList<IAssetManagerFolder>, ISubFolders
    {
        private readonly IAssetManagerFolder _folder;

        internal SubFolders(IAssetManagerFolder folder, Caching caching) : base(caching)
        {
            _folder = folder;
            RetrieveFunc = GetSubFolders;
        }

        public IAssetManagerFolder ParentFolder
        {
            get { return _folder; }
        }

        public IProject Project
        {
            get { return _folder.Project; }
        }

        public ISession Session
        {
            get { return _folder.Session; }
        }

        private List<IAssetManagerFolder> GetSubFolders()
        {
            const string LOAD_FOLDERS =
                @"<PROJECT><FOLDERS action=""list"" foldertype=""0"" withsubfolders=""1""/></PROJECT>";
            var xmlDoc = Project.ExecuteRQL(LOAD_FOLDERS);
            var parentFolderXPath = "//FOLDERS/FOLDER[@guid='{0}']".RQLFormat(ParentFolder);
            var parentNode = xmlDoc.SelectSingleNode(parentFolderXPath);
            if (parentNode == null)
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not load subfolders of {0}", _folder));
            }

            var subFolders = parentNode.SelectNodes("//SUBFOLDER");
            return
                (from XmlElement curSubNode in subFolders
                 select (IAssetManagerFolder) new AssetManagerFolder(_folder, curSubNode)).ToList();
        }
    }
}