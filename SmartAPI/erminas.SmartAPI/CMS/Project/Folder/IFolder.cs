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
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Folder
{
    public enum FolderType
    {
        FileFolder,
        AssetManager,
        DotNet,
        StyleSheet,
        ExternalApplication
    }

    public interface IFolder : IPartialRedDotObject, IProjectObject, IDeletable
    {
        IFiles Files { get; }
        bool IsAssetManager { get; }
        bool IsFileFolder { get; }
        IFolder LinkedFolder { get; }
        FolderType Type { get; }
    }

    internal static class InternalFolderFactory
    {
        public static bool HasSupportedFolderType(XmlElement element)
        {
            var folderType = element.GetIntAttributeValue("foldertype");
            if (!folderType.HasValue)
            {
                return false;
            }

            switch (folderType)
            {
                case 0:
                    return true;
                default:
                    return false;
            }
        }

        internal static IFolder CreateFolder(IProject project, XmlElement element)
        {
            var folderType = element.GetIntAttributeValue("foldertype");
            if (!folderType.HasValue)
            {
                throw new SmartAPIException(project.Session.ServerLogin, "Could not load folder information");
            }
            switch (folderType.Value)
            {
                case 0:
                    var isAssetManagerFolder = element.GetBoolAttributeValue("catalog").GetValueOrDefault();
                    if (!isAssetManagerFolder)
                    {
                        return new FileFolder(project, element);
                    }
                    return new AssetManagerFolder(project, element);
                default:
                    throw new SmartAPIInternalException(string.Format("Unsupported folder type: {0}", folderType));
            }
        }
    }

    internal abstract class BaseFolder : PartialRedDotProjectObject, IFolder
    {
        private IFolder _linkedFolder;

        internal BaseFolder(IProject project, XmlElement xmlElement) : base(project, xmlElement)
        {
            Files = new Files(this, Caching.Enabled);
            LoadXml();
        }

        internal BaseFolder(IProject project, Guid guid) : base(project, guid)
        {
            Files = new Files(this, Caching.Enabled);
        }

        public void Delete()
        {
            const string DELETE = @"<PROJECT><FOLDER action=""delete"" guid=""{0}""/></PROJECT>";
            Project.ExecuteRQL(DELETE.RQLFormat(this));
        }

        public IFiles Files { get; protected set; }

        public virtual bool IsAssetManager
        {
            get { return false; }
        }

        public bool IsFileFolder
        {
            get { return Type == FolderType.FileFolder; }
        }

        public IFolder LinkedFolder
        {
            get { return LazyLoad(ref _linkedFolder); }
        }

        public abstract FolderType Type { get; }

        protected override void LoadWholeObject()
        {
            LoadXml();
        }

        protected override XmlElement RetrieveWholeObject()
        {
            const string LOAD_FOLDER = @"<PROJECT><FOLDER action=""load"" guid=""{0}""/></PROJECT>";
            return Project.ExecuteRQL(LOAD_FOLDER.RQLFormat(this)).GetSingleElement("FOLDER");
        }

        private void LoadXml()
        {
            Guid linkedProjectGuid;
            if (_xmlElement.TryGetGuid("linkedprojectguid", out linkedProjectGuid))
            {
                var project = Project.Session.ServerManager.Projects.GetByGuid(linkedProjectGuid);

                // project could be null if the linked project is not available (broken folder)
                // in that case do not try to set the linked folder
                if (project != null)
                {
                    var linkedFolderGuid = _xmlElement.GetGuid("linkedfolderguid");
                    _linkedFolder = project.Folders.AllIncludingSubFolders.GetByGuid(linkedFolderGuid);
                }
            }
        }
    }

    internal class FileFolder : BaseFolder
    {
        public FileFolder(IProject project, XmlElement xmlElement) : base(project, xmlElement)
        {
        }

        public FileFolder(IProject project, Guid guid) : base(project, guid)
        {
        }

        public override FolderType Type
        {
            get { return FolderType.FileFolder; }
        }
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
        public readonly string Directory;
        public readonly string Filename;

        public FileSource(string filename, string directory)
        {
            Filename = filename;
            Directory = directory.EndsWith("\\") ? directory : directory + "\\";
        }
    }
}