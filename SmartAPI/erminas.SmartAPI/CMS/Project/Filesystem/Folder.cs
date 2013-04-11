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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Filesystem
{
    public interface IFolder : IPartialRedDotObject, IProjectObject
    {
        ICachedList<File> AllFiles { get; }
        bool IsAssetManagerFolder { get; }
        bool IsSubFolder { get; }
        IFolder LinkedFolder { get; }
        IFolder ParentFolder { get; }
        IEnumerable<IFolder> Subfolders { get; }
        void DeleteFiles(IEnumerable<string> filenames, bool forceDelete);

        /// <summary>
        ///     Returns List of files that match a predicate on an attribute
        /// </summary>
        /// <param name="attribute"> Attribute which values get checked in the predicate </param>
        /// <param name="operator"> Opreator e.g. "le" (less equal), "ge" (greater equal), "lt"(less than), "gt" (greater than) or "eq" (equal) </param>
        /// <param name="value"> Value e.g. 50 pixel/ 24 bit, etc. </param>
        /// <returns> </returns>
        [VersionIsGreaterThanOrEqual(10, VersionName = "Version 10")]
        IEnumerable<File> GetFilesByAttributeComparison(FileComparisonAttribute attribute, FileComparisonOperator @operator,
                                                                        int value);

        IEnumerable<File> GetFilesByAuthor(Guid authorGuid);
        IEnumerable<File> GetFilesByLastModifier(Guid lastModifierGuid);
        List<File> GetFilesByNamePattern(string searchText);
        List<File> GetSubListOfFiles(int startCount, int fileCount);
        void SaveFiles(IEnumerable<FileSource> sources);
        void UpdateFiles(IEnumerable<FileSource> files);
    }

    internal class Folder : PartialRedDotProjectObject, IFolder
    {
        #region ComparisonFileAttribute enum

        #endregion

        #region ComparisonOperator enum

        #endregion

        /// <summary>
        ///     RQL for listing files for the folder with guid {0}. No parameters
        /// </summary>
        private const string LIST_FILES_IN_FOLDER =
            @"<PROJECT><MEDIA><FOLDER guid=""{0}"" subdirguid=""{0}""><FILES action=""list"" view=""thumbnail"" orderby=""name"" maxcount=""1000"" attributeguid="""" searchtext=""*"" /></FOLDER></MEDIA></PROJECT>";

        /// <summary>
        ///     RQL for listing files for the folder with guid {0}. No parameters
        /// </summary>
        private const string LIST_FILES_IN_FOLDER_PARTIAL =
            @"<PROJECT><MEDIA><FOLDER guid=""{0}"" subdirguid=""{0}""><FILES action=""list"" view=""thumbnail"" orderby=""name"" maxcount=""1000""  searchtext=""*"" pattern="""" startcount=""{1}"" sectioncount=""{2}""/></FOLDER></MEDIA></PROJECT>";

        /// <summary>
        ///     RQL for listing files for the folder with guid {0} and the filtertext {1}. No parameters
        /// </summary>
        private const string FILTER_FILES_BY_TEXT =
            @"<MEDIA><FOLDER  guid=""{0}"" subdirguid=""{0}""><FILES action=""list"" view=""thumbnail"" maxfilesize=""0""  searchtext=""{1}"" pattern="""" startcount=""1"" orderby=""name""/></FOLDER></MEDIA>";

        /// <summary>
        ///     RQL for listing files for the folder with guid {0} by the creator with guid {1}. No parameters
        /// </summary>
        private const string FILTER_FILES_BY_CREATOR =
            @"<MEDIA><FOLDER  guid=""{0}"" subdirguid=""{0}""><FILES action=""list"" view=""thumbnail"" maxfilesize=""0"" createguid=""{1}"" pattern="""" startcount=""1"" orderby=""name""/></FOLDER></MEDIA>";

        /// <summary>
        ///     RQL for listing files for the folder with guid {0} changed by a user with guid {1}. No parameters
        /// </summary>
        private const string FILTER_FILES_BY_CHANGEAUTHOR =
            @"<MEDIA><FOLDER  guid=""{0}"" subdirguid=""{0}""><FILES action=""list"" view=""thumbnail"" maxfilesize=""0"" changeguid=""{1}"" pattern="""" startcount=""1"" orderby=""name""/></FOLDER></MEDIA>";

        /// <summary>
        ///     RQL for listing files for the folder with guid {0} which match the command {1} with the operator {2} and value {3}. No parameters
        /// </summary>
        private const string FILTER_FILES_BY_COMMAND =
            @"<MEDIA><FOLDER  guid=""{0}"" subdirguid=""{0}""><FILES action=""list"" view=""thumbnail"" sectioncount=""30"" maxfilesize=""0""  command=""{1}"" op=""{2}"" value=""{3}""  startcount=""1"" orderby=""name""/></FOLDER></MEDIA>";

        /// <summary>
        ///     RQL for saving a file {1} in a folder {0}. IMPORTANT: For {1} Create a File by using String FILE_TO_SAVE to insert 1...n files and fill in required values No parameters
        /// </summary>
        private const string SAVE_FILES_IN_FOLDER = @"<MEDIA><FOLDER guid=""{0}"">{1}</FOLDER></MEDIA>";

        /// <summary>
        ///     RQL for a file to be saved. Has to be inserted in SAVE_FILES_IN_FOLDER. No parameters
        /// </summary>
        private const string FILE_TO_SAVE = @"<FILE action=""save"" sourcename=""{0}"" sourcepath=""{1}""/>";

        /// <summary>
        ///     RQL for updating files {0} from source in a folder. No parameters
        /// </summary>
        private const string UPDATE_FILES_IN_FOLDER = @"<MEDIA><FOLDER guid=""{0}"">{1}</FOLDER></MEDIA>";

        /// <summary>
        ///     RQL for a file to be updated. Has to be inserted in UPDATE_FILES_IN_FOLDER No parameters
        /// </summary>
        private const string FILE_TO_UPDATE = @"<FILE action=""update"" sourcename=""{0}""/>";

        /// <summary>
        ///     RQL for deleting files for the folder with guid {0}. {1} List of Files to be deleted. Can contain mor than one FILE element.
        /// </summary>
        private const string DELETE_FILES =
            @"<MEDIA><FOLDER guid=""{0}""><FILES action=""deletefiles"">{1}</FILES></FOLDER></MEDIA>";

        /// <summary>
        ///     RQL for a files for the folder with the sourcename {0} to be inserted in e.g. DELETE_FILES deletereal=0: Prior to deleting, a message is sent back if the file is already being used. (Default setting).
        /// </summary>
        private const string FILE_TO_DELETE_IF_UNUSED = @"<FILE deletereal=""0"" sourcename=""{0}""/>";

        /// <summary>
        ///     RQL for a files for the folder with the sourcename {0} to be inserted in e.g. DELETE_FILES deletereal=1: The file is deleted regardless of whether it is being used in a project or not.
        /// </summary>
        private const string FORCE_FILE_TO_BE_DELETED = @"<FILE deletereal=""1"" sourcename=""{0}""/>";

        private bool _isAssetManagerFolder;
        private bool? _isSubFolder;
        private IFolder _linkedFolder;
        private IFolder _parentFolder;
        private List<IFolder> _subfolders;

        internal Folder(IProject project, XmlElement xmlElement) : base(project, xmlElement)
        {
            var subfolders = XmlElement.GetElementsByTagName("SUBFOLDER");
            _isSubFolder = false;
            _subfolders = (from XmlElement curFolder in subfolders select (IFolder)new Folder(Project, this, curFolder)).ToList();
            Init();
        }

        public Folder(IProject project, Guid guid) : base(project, guid)
        {
            Init();
        }

        private Folder(IProject project, IFolder parentFolder, XmlElement element) : base(project, element)
        {
            LoadXml();
            Init();
            _parentFolder = parentFolder;
        }

        public ICachedList<File> AllFiles { get; private set; }

        public static string AttributeToString(FileComparisonAttribute attribute)
        {
            switch (attribute)
            {
                case FileComparisonAttribute.Width:
                    return "width";
                case FileComparisonAttribute.Heigth:
                    return "height";
                case FileComparisonAttribute.Size:
                    return "size";
                case FileComparisonAttribute.Depth:
                    return "depth";
                default:
                    throw new ArgumentException(string.Format("Unknown file attribute: {0}", attribute));
            }
        }

        public void DeleteFiles(IEnumerable<string> filenames, bool forceDelete)
        {
            // Add 1..n file update Strings in UPDATE_FILES_IN_FOLDER string and execute RQL-Query
            string fileDeletionTemplate = forceDelete ? FORCE_FILE_TO_BE_DELETED : FILE_TO_DELETE_IF_UNUSED;
            List<string> filesToDelete =
                filenames.Select(
                    filename =>
                    string.Format(fileDeletionTemplate, filename)).ToList();

            XmlDocument xmlDoc =
                Project.ExecuteRQL(string.Format(DELETE_FILES, Guid.ToRQLString(),
                                                 string.Join(string.Empty, filesToDelete)));
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("IODATA");
            if (xmlNodes.Count == 0)
            {
                throw new ArgumentException("Could not delete Files.");
            }
        }

        /// <summary>
        ///     Returns List of files that match a predicate on an attribute
        /// </summary>
        /// <param name="attribute"> Attribute which values get checked in the predicate </param>
        /// <param name="operator"> Opreator e.g. "le" (less equal), "ge" (greater equal), "lt"(less than), "gt" (greater than) or "eq" (equal) </param>
        /// <param name="value"> Value e.g. 50 pixel/ 24 bit, etc. </param>
        /// <returns> </returns>
        [VersionIsGreaterThanOrEqual(10, VersionName = "Version 10")]
        public IEnumerable<File> GetFilesByAttributeComparison(FileComparisonAttribute attribute, FileComparisonOperator @operator,
                                                        int value)
        {
            Session.EnsureVersion();
            string rqlString = String.Format(FILTER_FILES_BY_COMMAND, Guid.ToRQLString(), AttributeToString(attribute),
                                             ComparisonOperatorToString(@operator), value);
            return RetrieveFiles(rqlString);
        }

        public IEnumerable<File> GetFilesByAuthor(Guid authorGuid)
        {
            string rqlString = String.Format(FILTER_FILES_BY_CREATOR, Guid.ToRQLString(), authorGuid.ToRQLString());
            return RetrieveFiles(rqlString);
        }

        public IEnumerable<File> GetFilesByLastModifier(Guid lastModifierGuid)
        {
            string rqlString = String.Format(FILTER_FILES_BY_CHANGEAUTHOR, Guid.ToRQLString(),
                                             lastModifierGuid.ToRQLString());
            return RetrieveFiles(rqlString);
        }

        public List<File> GetFilesByNamePattern(string searchText)
        {
            string rqlString = String.Format(FILTER_FILES_BY_TEXT, Guid.ToRQLString(), searchText);
            return RetrieveFiles(rqlString);
        }

        public List<File> GetSubListOfFiles(int startCount, int fileCount)
        {
            string rqlString = String.Format(LIST_FILES_IN_FOLDER_PARTIAL, Guid.ToRQLString(), startCount, fileCount);

            return RetrieveFiles(rqlString);
        }

        public bool IsAssetManagerFolder
        {
            get { return LazyLoad(ref _isAssetManagerFolder); }
        }

        public bool IsSubFolder
        {
            get
            {
                EnsureSubFolderInitialization();
                return _isSubFolder.GetValueOrDefault();
            }
        }

        public IFolder LinkedFolder
        {
            get { return LazyLoad(ref _linkedFolder); }
        }

        public IFolder ParentFolder
        {
            get
            {
                EnsureSubFolderInitialization();
                return _parentFolder;
            }
        }

        public void SaveFiles(IEnumerable<FileSource> sources)
        {
            List<string> filesToSave =
                sources.Select(fileSource => string.Format(FILE_TO_SAVE, fileSource.Sourcename, fileSource.Sourcepath))
                       .ToList();

            XmlDocument xmlDoc =
                Project.ExecuteRQL(String.Format(SAVE_FILES_IN_FOLDER, Guid.ToRQLString(),
                                                 string.Join(string.Empty, filesToSave)));
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("FILE");
            if (xmlNodes.Count == 0)
            {
                throw new SmartAPIException(Session.ServerLogin, "Could not save Files.");
            }
        }

        public IEnumerable<IFolder> Subfolders
        {
            get
            {
                EnsureSubFolderInitialization();
                return _subfolders;
            }
        }

        public void UpdateFiles(IEnumerable<FileSource> files)
        {
            // Add 1..n file update Strings in UPDATE_FILES_IN_FOLDER string and execute RQL-Query
            List<string> filesToUpdate = files.Select(file => string.Format(FILE_TO_UPDATE, file.Sourcename)).ToList();

            XmlDocument xmlDoc =
                Project.ExecuteRQL(string.Format(UPDATE_FILES_IN_FOLDER, Guid.ToRQLString(),
                                                 string.Join(string.Empty, filesToUpdate)));
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("FILE");
            if (xmlNodes.Count == 0)
            {
                throw new SmartAPIException(Session.ServerLogin, "Could not update Files.");
            }
        }

        protected override void LoadWholeObject()
        {
            LoadXml();
        }

        protected override XmlElement RetrieveWholeObject()
        {
            const string LOAD_FOLDER = @"<PROJECT><FOLDER action=""load"" guid=""{0}""/></PROJECT>";

            XmlDocument xmlDoc = Project.ExecuteRQL(String.Format(LOAD_FOLDER, Guid.ToRQLString()));
            XmlNodeList folders = xmlDoc.GetElementsByTagName("FOLDER");
            if (folders.Count != 1)
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            String.Format("No folder with guid {0} found.", Guid.ToRQLString()));
            }
            return (XmlElement) folders[0];
        }

        private static string ComparisonOperatorToString(FileComparisonOperator @operator)
        {
            switch (@operator)
            {
                case FileComparisonOperator.Greater:
                    return "gt";
                case FileComparisonOperator.Less:
                    return "lt";
                case FileComparisonOperator.LessEqual:
                    return "le";
                case FileComparisonOperator.GreaterEqual:
                    return "ge";
                case FileComparisonOperator.Equal:
                    return "eq";
                default:
                    throw new ArgumentException(string.Format("Unknown comparison operator: {0}", @operator));
            }
        }

        private void EnsureSubFolderInitialization()
        {
            if (_isSubFolder.HasValue)
            {
                return;
            }

            IFolder folder;
            if (Project.Folders.TryGetByGuid(Guid, out folder))
            {
                _subfolders = folder.Subfolders.ToList();
                _isSubFolder = false;
            }
            else
            {
                foreach (var curFolder in Project.Folders)
                {
                    var subfolder = curFolder.Subfolders.FirstOrDefault(folder1 => folder1.Guid == Guid);
                    if (subfolder != null)
                    {
                        _parentFolder = curFolder;
                        _subfolders = new List<IFolder>();
                        _isSubFolder = true;
                        return;
                    }
                }
            }
            throw new Exception(string.Format("Could not find folder with Guid {0} in project {1}", Guid.ToRQLString(),
                                              Project));
        }

        // New Version to delete Files

        private List<File> GetAllFiles()
        {
            string rqlString = String.Format(LIST_FILES_IN_FOLDER, Guid.ToRQLString());

            return RetrieveFiles(rqlString);
        }

        private void Init()
        {
            AllFiles = new CachedList<File>(GetAllFiles, Caching.Enabled);
        }

        private void LoadXml()
        {
            InitIfPresent(ref _isAssetManagerFolder, "catalog", BoolConvert);

            Guid linkedProjectGuid;
            if (XmlElement.TryGetGuid("linkedprojectguid", out linkedProjectGuid))
            {
                _linkedFolder = new Folder(Project.Session.Projects.GetByGuid(linkedProjectGuid),
                                           XmlElement.GetGuid("linkedfolderguid"));
            }
        }

        private List<File> RetrieveFiles(string rqlString)
        {
            XmlDocument xmlDoc = Project.ExecuteRQL(rqlString);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("FILE");

            return (from XmlElement xmlNode in xmlNodes select new File(Project, xmlNode)).ToList();
        }

        #region Nested type: FileSource

        #endregion
    }

    public enum FileComparisonOperator
    {
        Equal,
        Less,
        Greater,
        LessEqual,
        GreaterEqual
    }

    public enum FileComparisonAttribute
    {
        Width,
        Heigth,
        Depth,
        Size
    }

    public class FileSource
    {
        public readonly string Sourcename;
        public readonly string Sourcepath;

        public FileSource(string sourcename, string sourcepath)
        {
            Sourcename = sourcename;
            Sourcepath = sourcepath;
        }
    }
}