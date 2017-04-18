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
using erminas.SmartAPI.CMS.Project.Authorizations;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Folder
{
    internal class AssetManagerFolder : BaseFolder, IAssetManagerFolder
    {
        private string _path;

        public AssetManagerFolder(IProject project, XmlElement xmlElement) : base(project, xmlElement)
        {
            ParentFolder = null;
            SubFolders = new SubFolders(this, Caching.Enabled);
            Files = new AssetManagerFiles(this, Caching.Enabled);
            LoadXml();
        }

        public AssetManagerFolder(IAssetManagerFolder parentFolder, XmlElement xmlElement)
            : base(parentFolder.Project, xmlElement)
        {
            ParentFolder = parentFolder;
            SubFolders = new EmptySubFolders(this);
            Files = new AssetManagerFiles(this, Caching.Enabled);
            LoadXml();
        }

        public void CreateSubfolder(string name, string description, string filesystemDirectoryName)
        {
            if (ParentFolder != null)
            {
                throw new SmartAPIException(Session.ServerLogin,
                    string.Format("Can not create subfolder in another subfolder ({0}).", this));
            }

            const string CREATE_SUBFOLDER =
                @"<FOLDER guid=""{0}""><SUBFOLDER action=""addnew"" name=""{1}"" dirname=""{2}"" description=""{3}"" /></FOLDER>";

            Project.ExecuteRQL(CREATE_SUBFOLDER.SecureRQLFormat(this, name, filesystemDirectoryName, description));
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

        public bool IsDatabaseFolder
        {
            get { return StorageType == AssetManagerFolderStorageType.Database; }
        }

        public bool IsFileSystemFolder
        {
            get { return StorageType == AssetManagerFolderStorageType.FileSystem; }
        }

        public bool IsSubFolder
        {
            get { return ParentFolder != null; }
        }

        public IAssetManagerFolder ParentFolder { get; }

        public AssetManagerFolderStorageType StorageType
        {
            get
            {
                return (AssetManagerFolderStorageType) XmlElement.GetIntAttributeValue("savetype").GetValueOrDefault();
            }
        }

        public ISubFolders SubFolders { get; }

        public string FilesystemPath
        {
            get { return _path; }
        }

        public override FolderType Type
        {
            get { return FolderType.AssetManager; }
        }

        private void LoadXml()
        {
            InitIfPresent(ref _path, "path", x => x);
        }

        protected override void LoadWholeObject()
        {
            base.LoadWholeObject();
            LoadXml();
        }

        public void SetFolderAuthorizationPackage(IAuthorizationPackage authorizationPackage)
        {
            if (authorizationPackage == null)
            {
                throw new NotImplementedException(
                    "Removing folder authorization packages is not supported at the moment");
            }

            const string ASSIGN_AUTH_PACKAGE =
                @"<AUTHORIZATION><FOLDER guid=""{0}""><AUTHORIZATIONPACKET action=""assign"" guid=""{1}"" /></FOLDER></AUTHORIZATION>";

            Project.ExecuteRQL(ASSIGN_AUTH_PACKAGE.RQLFormat(this, authorizationPackage));
        }
    }

    public enum AssetManagerFolderStorageType
    {
        Database = 0,
        FileSystem = 2,
        Asset = 6
    }

    public interface IAssetManagerFolder : IFolder
    {
        new IAssetManagerFiles Files { get; }
        bool IsDatabaseFolder { get; }
        bool IsFileSystemFolder { get; }
        bool IsSubFolder { get; }
        IAssetManagerFolder ParentFolder { get; }
        AssetManagerFolderStorageType StorageType { get; }
        ISubFolders SubFolders { get; }

        void SetFolderAuthorizationPackage(IAuthorizationPackage authorizationPackage);

        string FilesystemPath { get; }
        void CreateSubfolder(string name, string description, string filesystemDirectoryName);
    }
}