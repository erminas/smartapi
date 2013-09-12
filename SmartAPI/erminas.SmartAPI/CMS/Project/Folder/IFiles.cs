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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Folder
{
    internal class Files : IndexedCachedList<string, IFile>, IFiles
    {
        private readonly IFolder _folder;

        internal Files(IFolder folder, Caching caching) : base(file => file.Name, caching)
        {
            _folder = folder;
            RetrieveFunc = GetFiles;
        }

        public void Add(string filename, string directory)
        {
            AddRange(new[] {new FileSource(filename, directory)});
        }

        public void AddRange(IEnumerable<FileSource> sources)
        {
            var fileSources = sources as IList<FileSource> ?? sources.ToList();
            if (!fileSources.Any())
            {
                return;
            }
            const string SINGLE_FILE = @"<FILE action=""save"" sourcename=""{0}"" sourcepath=""{1}""/>";
            var filesToSave =
                fileSources.Select(fileSource => SINGLE_FILE.SecureRQLFormat(fileSource.Filename, fileSource.Directory));

            const string SAVE_FILES = @"<PROJECT><FOLDER guid=""{0}"">{1}</FOLDER></PROJECT>";
            var xmlDoc = Project.ExecuteRQL(SAVE_FILES.RQLFormat(_folder, string.Join(string.Empty, filesToSave)));

            var responseText = xmlDoc.GetSingleElement("IODATA").InnerText;
            if (!string.IsNullOrWhiteSpace(responseText))
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not add files in folder {0}: {1}", _folder,
                                                          responseText));
            }
        }

        public IFolder Folder
        {
            get { return _folder; }
        }

        public IFile GetByName(string name)
        {
            return GetByNamePattern(name).First(file => file.Name == name);
        }

        public ReadOnlyCollection<IFile> GetByNamePattern(string searchText)
        {
            const string LIST_FILES_BY_NAME_PATTERN =
                @"<PROJECT><TEMPLATE><ELEMENT folderguid=""{0}""><FILES action=""list"" searchtext=""{1}"" /></ELEMENT></TEMPLATE></PROJECT>";
            return RetrieveFiles(LIST_FILES_BY_NAME_PATTERN.SecureRQLFormat(Folder, searchText)).AsReadOnly();
        }

        public IProject Project
        {
            get { return Folder.Project; }
        }

        public void Remove(string filename)
        {
            RemoveRange(new[] {filename});
        }

        public void RemoveForcibly(string filename)
        {
            RemoveRangeForcibly(new[] {filename});
        }

        public virtual void RemoveRange(IEnumerable<string> filenames)
        {
            RemoveRangeForcibly(filenames);
        }

        public virtual void RemoveRangeForcibly(IEnumerable<string> filenames)
        {
            var filenameList = filenames as IList<string> ?? filenames.ToList();
            if (!filenameList.Any())
            {
                return;
            }
            
            var files = string.Join(string.Empty, filenameList.Select(s => GetSingleFilenameTemplate().SecureRQLFormat(s)));
            var deleteFiles = GetDeleteFilesStatement(files);

            var xmlDoc = Project.ExecuteRQL(deleteFiles);
            if (!xmlDoc.IsContainingOk())
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format(
                                                "In folder {0}, could not delete one ore more files out of: {1}",
                                                _folder, string.Join(", ", filenameList)));
            }
        }

        protected virtual string GetSingleFilenameTemplate()
        {
            const string SINGLE_FILE = @"<FILE sourcename=""{0}"" currendirectory="""" checkfolder=""1""/>";
            return SINGLE_FILE;
        }

        protected virtual string GetDeleteFilesStatement(string files)
        {
            const string DELETE_FILES =
                @"<MEDIA><FOLDER guid=""{0}""><FILES action=""deletefiles"">{1}</FILES></FOLDER></MEDIA>";
            return DELETE_FILES.RQLFormat(Folder, files);
        }
        
        public ISession Session
        {
            get { return Folder.Session; }
        }

        protected List<IFile> RetrieveFiles(string rqlString)
        {
            XmlDocument xmlDoc = Project.ExecuteRQL(rqlString);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("FILE");

            return (from XmlElement xmlNode in xmlNodes select (IFile) new File(Project, xmlNode)).ToList();
        }

        private List<IFile> GetFiles()
        {
            const string LIST_FILES =
                @"<PROJECT><TEMPLATE><ELEMENT folderguid=""{0}""><FILES action=""list"" /></ELEMENT></TEMPLATE></PROJECT>";

            return RetrieveFiles(LIST_FILES.RQLFormat(Folder));
        }
    }

    public interface IFiles : IIndexedCachedList<string, IFile>, IProjectObject
    {
        void Add(string filename, string directory);
        void AddRange(IEnumerable<FileSource> sources);
        IFolder Folder { get; }
        ReadOnlyCollection<IFile> GetByNamePattern(string searchText);

        void Remove(string filename);
        void RemoveForcibly(string filename);
        void RemoveRange(IEnumerable<string> filenames);
        void RemoveRangeForcibly(IEnumerable<string> filenames);
    }
}