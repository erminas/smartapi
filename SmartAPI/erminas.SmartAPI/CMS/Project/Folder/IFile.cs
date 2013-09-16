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
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.Folder
{
    public interface IFile : IProjectObject, IDeletable
    {
        FileAttributes Attributes { get; }

        /// <summary>
        ///     Folder the file is stored in
        /// </summary>
        IFolder Folder { get; }

        /// <summary>
        ///     Name of the file
        /// </summary>
        string Name { get; }

        int ReferenceCount();

        string ThumbnailPath { get; }
        Guid? ThumbnailGuid { get; }
    }

    internal class File : IFile
    {
        /// <summary>
        ///     Folder the file is stored in
        /// </summary>
        private readonly IFolder _folder;

        /// <summary>
        ///     Name of the file
        /// </summary>
        private readonly string _name;

        private readonly IProject _project;
        private readonly XmlElement _xmlElement;
        private readonly Guid? _thumbnailGuid;

        internal File(IProject project, XmlElement xmlElement)
        {
            _project = project;
            _xmlElement = xmlElement;
            _name = xmlElement.GetAttributeValue("name");
            var folderGuid = xmlElement.GetGuid("folderguid");
            _folder = project.Folders.AllIncludingSubFolders.GetByGuid(folderGuid);
            Guid guid;
            if (_xmlElement.TryGetGuid("thumbguid", out guid))
            {
                _thumbnailGuid = guid;
            }
            if (IsAssetWithThumbnail)
            {
                //older versions do not contain the thumbnailpath attribute, so it has to be constructed
                ThumbnailPath = xmlElement.GetAttributeValue("thumbnailpath") ?? CreateThumbnailPath();
            }
        }

        private bool IsAssetWithThumbnail
        {
            get { return _thumbnailGuid != null; }
        }

        private string CreateThumbnailPath()
        {
            return @"THUMBNAIL\{0}\{1}\{2}.JPG".RQLFormat(_project, _folder, _thumbnailGuid.Value);
        }

        public File(IFolder folder, string fileName)
        {
            _project = folder.Project;
            _folder = folder;
            _name = fileName;
        }

        public FileAttributes Attributes
        {
            get
            {
                const string LIST_FILE_ATTRIBUTES =
                    @"<MEDIA><FOLDER guid=""{0}""><FILE sourcename=""{1}""><FILEATTRIBUTES action=""list""/></FILE></FOLDER></MEDIA>";

                XmlDocument xmlDoc = Project.ExecuteRQL(LIST_FILE_ATTRIBUTES.RQLFormat(_folder, _name));

                var node = (XmlElement) xmlDoc.GetElementsByTagName("EXTERNALATTRIBUTES")[0];
                return new FileAttributes(_folder, node);
            }
        }

        public void Delete()
        {
            //TODO ist ENG okay, oder sollte man z.b. die standardsprache waehlen
            const string DELETE_FILE =
                "<MEDIA><FOLDER guid=\"{0}\" ><FILES action=\"deletefiles\"><FILE sourcename=\"{1}\" languagevariantid=\"ENG\" checkfolder=\"1\"/></FILES></FOLDER></MEDIA>";

            _project.ExecuteRQL(string.Format(DELETE_FILE, _folder.Guid.ToRQLString(), _name));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return obj.GetType() == GetType() && Equals((File) obj);
        }

        /// <summary>
        ///     Folder the file is stored in
        /// </summary>
        public IFolder Folder
        {
            get { return _folder; }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_folder != null ? _folder.GetHashCode() : 0)*397) ^ (_name != null ? _name.GetHashCode() : 0);
            }
        }

        /// <summary>
        ///     Name of the file
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        public IProject Project
        {
            get { return _project; }
        }

        public int ReferenceCount()
        {
            const string GET_REFERENCES =
                "<PROJECT><TRANSFER action=\"checkimage\" getreferences=\"1\" sync=\"1\" folderguid=\"{0}\" filename=\"{1}\" /></PROJECT>";

            XmlDocument xmlDoc = _project.ExecuteRQL(string.Format(GET_REFERENCES, _folder.Guid.ToRQLString(), _name));
            return xmlDoc.GetElementsByTagName("REFERENCE").Count;
        }
        public Guid? ThumbnailGuid { get { return _thumbnailGuid; } }
        public string ThumbnailPath { get; private set; }

        public ISession Session
        {
            get { return Project.Session; }
        }

        public override string ToString()
        {
            string path = "";
            if (_folder is IAssetManagerFolder)
            {
                var assetFolder = (IAssetManagerFolder) _folder;
                if (assetFolder.IsSubFolder)
                {
                    path = assetFolder.ParentFolder.Name + "/";
                }
            }
            return _folder != null ? path + _folder.Name + "/" + Name : "";
        }

        protected bool Equals(File other)
        {
            return Equals(_folder, other._folder) && string.Equals(_name, other._name);
        }
    }
}