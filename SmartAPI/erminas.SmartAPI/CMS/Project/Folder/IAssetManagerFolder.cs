using System.Xml;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Folder
{
    internal class AssetManagerFolder : BaseFolder, IAssetManagerFolder
    {
        private readonly IAssetManagerFolder _parentFolder;

        public AssetManagerFolder(IProject project, XmlElement xmlElement) : base(project, xmlElement)
        {
            _parentFolder = null;
            SubFolders = new SubFolders(this, Caching.Enabled);
            Files = new AssetManagerFiles(this, Caching.Enabled);
        }

        public AssetManagerFolder(IAssetManagerFolder parentFolder, XmlElement xmlElement)
            : base(parentFolder.Project, xmlElement)
        {
            _parentFolder = parentFolder;
            SubFolders = new EmptySubFolders(this);
            Files = new AssetManagerFiles(this, Caching.Enabled);
        }

        public new IAssetManagerFiles Files
        {
            get { return (IAssetManagerFiles) base.Files; }
            private set { base.Files = value; }
        }

        public override bool IsAssetManager
        {
            get { return true; }
        }

        public bool IsSubFolder
        {
            get { return _parentFolder != null; }
        }

        public IAssetManagerFolder ParentFolder
        {
            get { return _parentFolder; }
        }

        public ISubFolders SubFolders { get; private set; }

        public override FolderType Type
        {
            get { return FolderType.AssetManager; }
        }
    }

    public interface IAssetManagerFolder : IFolder
    {
        new IAssetManagerFiles Files { get; }

        bool IsSubFolder { get; }
        IAssetManagerFolder ParentFolder { get; }
        ISubFolders SubFolders { get; }
    }
}