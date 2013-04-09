// Smart API - .Net programmatic access to RedDot servers
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

using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.Filesystem
{
    public class File : IProjectObject,IDeletable
    {
        /// <summary>
        ///     Folder the file is stored in
        /// </summary>
        private readonly IFolder _folder;

        /// <summary>
        ///     Name of the file
        /// </summary>
        private readonly string _name;

        private readonly Project _project;

        internal File(Project project, XmlElement xmlElement)
        {
            _project = project;

            _name = xmlElement.GetAttributeValue("name");
            _folder = new Folder(project, xmlElement.GetGuid("folderguid"));
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

        public Project Project
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

        public Session Session
        {
            get { return Project.Session; }
        }

        public override string ToString()
        {
            return _folder != null ? _folder.Name + "/" + Name : "";
        }

        protected bool Equals(File other)
        {
            return Equals(_folder, other._folder) && string.Equals(_name, other._name);
        }
    }
}