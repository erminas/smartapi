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
using System.Web;
using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.Publication
{
    /// <summary>
    ///     TODO trennen in ftp/ds
    /// </summary>
    public interface IPublicationFolder : IPartialRedDotObject, IProjectObject, IDeletable
    {
        void Commit();
        string ContentGroup { get; set; }

        new string Name { get; set; }
        PublicationFolderContentType ContentTypeValue { get; set; }
        PublicationFolderContextInfoPreparationType ContextInfoPreparation { get; set; }
        string ContextTags { get; set; }
        IPublicationFolder CreateInProject(IProject project, Guid parentFolderGuid);
        bool DoCreateLog { get; set; }
        bool DoIndexForFulltextSearch { get; set; }
        bool DoReleaseAfterImport { get; set; }
        bool DoReplaceExistingContent { get; set; }
        bool DoReplaceGroupAssignment { get; set; }
        bool IsPublishedPagesFolder { get; }
        string RealName { get; set; }
        string RealVirtualName { get; set; }
        PublicationFolderType Type { get; set; }
        string VirtualName { get; set; }
    }

    internal class PublicationFolder : PartialRedDotProjectObject, IPublicationFolder
    {
        public static readonly Guid ROOT_LEVEL_GUID = Guid.Parse("9BBF210F7923406291BE7AE47B4CA571");
        public static readonly Guid PUBLISHED_PAGES_GUID;
        private static readonly XmlElement PUBLISHED_PAGES_NODE;
        private string _contentgroup;
        private PublicationFolderContentType _contenttype;
        private PublicationFolderContextInfoPreparationType _contextInfoPreparationType;
        private string _contexttags;
        private bool _doArchivePreviousVersion;
        private bool _doCreateLog;
        private bool _doIgnoreMetadata;
        private bool _doIndexing;
        private bool _doOverwriteContent;
        private bool _doOverwriteGroupAssignment;
        private bool _doReleasePublishedFiles;
        private string _inlineFunctionName;
        private string _prefix;
        private string _realName;
        private string _realVirtualName;
        private string _script;
        private PublicationFolderType _type;
        private bool _usePrefix;
        private string _virtualName;

        static PublicationFolder()
        {
            var doc = new XmlDocument();
            PUBLISHED_PAGES_NODE = doc.CreateElement("EXPORTFOLDER");
            const string PUBLISHED_PAGES_GUID_STRING = "00000000000000000000000000000100";
            PUBLISHED_PAGES_GUID = Guid.Parse(PUBLISHED_PAGES_GUID_STRING);
            PUBLISHED_PAGES_NODE.SetAttributeValue("name", "Published pages");
            PUBLISHED_PAGES_NODE.SetAttributeValue("guid", PUBLISHED_PAGES_GUID_STRING);
        }

        public new string Name { get { return base.Name; } set { base.Name = value; } }

        public PublicationFolder(string name, PublicationFolderType type) : base(null)
        {
            _contextInfoPreparationType = PublicationFolderContextInfoPreparationType.None;
            Name = name;
            _type = type;
        }

        public PublicationFolder(IProject project, Guid guid) : base(project, guid)
        {
            _contextInfoPreparationType = PublicationFolderContextInfoPreparationType.None;
        }

        public void Commit()
        {
            const string SAVE_STRING =
                @"<PROJECT><EXPORTFOLDER action=""save"" guid=""{0}"" type=""{1}"" name=""{2}"" {3} /></PROJECT>";

            XmlDocument reply =
                Project.ExecuteRQL(string.Format(SAVE_STRING, Guid.ToRQLString(), ((int) _type), Name, SaveParameters()));

            if (reply.GetElementsByTagName("EXPORTFOLDER").Count != 1)
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not save publication folder {0}", this));
            }
        }

        public string ContentGroup
        {
            get { return LazyLoad(ref _contentgroup); }
            set { _contentgroup = value; }
        }

        public PublicationFolderContentType ContentTypeValue
        {
            get { return LazyLoad(ref _contenttype); }
            set { _contenttype = value; }
        }

        public PublicationFolderContextInfoPreparationType ContextInfoPreparation
        {
            get { return LazyLoad(ref _contextInfoPreparationType); }
            set { _contextInfoPreparationType = value; }
        }

        public string ContextTags
        {
            get { return LazyLoad(ref _contexttags); }
            set { _contexttags = value; }
        }

        public IPublicationFolder CreateInProject(IProject project, Guid parentFolderGuid)
        {
            const string CREATE_STRING =
                @"<PROJECT><EXPORTFOLDER action=""assign"" guid=""{0}""><EXPORTFOLDER action=""addnew"" name=""{1}"" type=""{2}"" {3} /></EXPORTFOLDER></PROJECT>";

            XmlDocument reply =
                project.ExecuteRQL(string.Format(CREATE_STRING, parentFolderGuid.ToRQLString(), Name, ((int) _type),
                                                 SaveParameters()));

            var newFolderElement = (XmlElement)reply.GetElementsByTagName("EXPORTFOLDER")[0];
            return new PublicationFolder(project, newFolderElement.GetGuid());
        }

        public void Delete()
        {
            const string DELETE = @"<PROJECT><EXPORTFOLDER action=""delete"" guid=""{0}""/> </PROJECT>";
            Project.ExecuteRQL(string.Format(DELETE, Guid.ToRQLString()));
            //TODO check result
        }

        public bool DoCreateLog
        {
            get { return LazyLoad(ref _doCreateLog); }
            set { _doCreateLog = value; }
        }

        public bool DoIndexForFulltextSearch
        {
            get { return LazyLoad(ref _doIndexing); }
            set { _doIndexing = value; }
        }

        public bool DoReleaseAfterImport
        {
            get { return LazyLoad(ref _doReleasePublishedFiles); }
            set { _doReleasePublishedFiles = value; }
        }

        public bool DoReplaceExistingContent
        {
            get { return LazyLoad(ref _doOverwriteContent); }
            set { _doOverwriteContent = value; }
        }

        public bool DoReplaceGroupAssignment
        {
            get { return LazyLoad(ref _doOverwriteGroupAssignment); }
            set { _doOverwriteGroupAssignment = value; }
        }

        public bool IsPublishedPagesFolder
        {
            get { return Guid == PUBLISHED_PAGES_GUID; }
        }

        public string RealName
        {
            get { return LazyLoad(ref _realName); }
            set { _realName = value; }
        }

        public string RealVirtualName
        {
            get { return LazyLoad(ref _realVirtualName); }
            set { _realVirtualName = value; }
        }

        public override ISession Session
        {
            get { return Project != null ? Project.Session : null; }
        }

        public PublicationFolderType Type
        {
            get { return LazyLoad(ref _type); }
            set { _type = value; }
        }

        public string VirtualName
        {
            get { return LazyLoad(ref _virtualName); }
            set { _virtualName = value; }
        }

        protected override void LoadWholeObject()
        {
            LoadXml();
        }

        protected override XmlElement RetrieveWholeObject()
        {
            if (Guid != PUBLISHED_PAGES_GUID)
            {
                const string LOAD_PUBLICATION_FOLDER =
                    @"<PROJECT><EXPORTFOLDER action=""load"" guid=""{0}""/></PROJECT>";

                return
                    (XmlElement)
                    Project.ExecuteRQL(string.Format(LOAD_PUBLICATION_FOLDER, Guid.ToRQLString()))
                           .GetElementsByTagName("EXPORTFOLDER")[0];
            }

            return PUBLISHED_PAGES_NODE;
        }

        private static string BoolToString(bool value)
        {
            return value ? "1" : "0";
        }

        private void LoadXml()
        {
            if (Guid == PUBLISHED_PAGES_GUID)
            {
                return;
            }
            EnsuredInit(ref _realName, "realname", x => x ?? "");
            EnsuredInit(ref _type, "type", x => (PublicationFolderType) int.Parse(x));

            InitIfPresent(ref _realVirtualName, "realvirtualname", x => x ?? "");
            InitIfPresent(ref _virtualName, "virtualname", x => x ?? "");
            if (_type == PublicationFolderType.DeliveryServer)
            {
                InitIfPresent(ref _contentgroup, "contentgroup", x => x);
                EnsuredInit(ref _contenttype, "contenttype",
                            x => (PublicationFolderContentType) Enum.Parse(typeof (PublicationFolderContentType), x));
                InitIfPresent(ref _contexttags, "contexttags", x => x);
                InitIfPresent(ref _contextInfoPreparationType, "contextinfo",
                              x =>
                              (PublicationFolderContextInfoPreparationType)
                              Enum.Parse(typeof (PublicationFolderContextInfoPreparationType), x));
                EnsuredInit(ref _doArchivePreviousVersion, "flag_archive_prev_version", BoolConvert);
                InitIfPresent(ref _doCreateLog, "flag_create_log", BoolConvert);
                InitIfPresent(ref _doIndexing, "flag_indexing", BoolConvert);
                InitIfPresent(ref _doOverwriteContent, "flag_overwrite_content", BoolConvert);
                InitIfPresent(ref _doOverwriteGroupAssignment, "flag_overwrite_group_assignment", BoolConvert);
                InitIfPresent(ref _doReleasePublishedFiles, "flag_set_final", BoolConvert);
                InitIfPresent(ref _doIgnoreMetadata, "ignore_metadata", BoolConvert);
                InitIfPresent(ref _inlineFunctionName, "inlinefunctionname", x => x);
                InitIfPresent(ref _prefix, "prefix", x => x);
                InitIfPresent(ref _script, "script", x => x);
                InitIfPresent(ref _usePrefix, "useprefix", BoolConvert);
            }
        }

        private string SaveParameters()
        {
            string optionalParameters = String.IsNullOrEmpty(_virtualName)
                                            ? ""
                                            : "virtualname=\"" + HttpUtility.HtmlEncode(_virtualName) + "\" ";
            if (Type == PublicationFolderType.DeliveryServer)
            {
                if (!String.IsNullOrEmpty(_contentgroup))
                {
                    optionalParameters += "contentgroup=\"" + HttpUtility.HtmlEncode(_contentgroup) + "\" ";
                }
                optionalParameters += "contenttype=\"" + _contenttype + "\" ";

                if (!String.IsNullOrEmpty(_contexttags))
                {
                    optionalParameters += "contexttags=\"" + HttpUtility.HtmlEncode(_contexttags) + "\" ";
                }

                if (_contextInfoPreparationType != PublicationFolderContextInfoPreparationType.None)
                {
                    optionalParameters += "contextinfo=\"" + HttpUtility.HtmlEncode(_contextInfoPreparationType) + "\" ";
                }

                if (_doArchivePreviousVersion)
                {
                    optionalParameters += "flag_archive_prev_version=\"1\" ";
                }

                if (_doCreateLog)
                {
                    optionalParameters += "flag_create_log=\"1\" ";
                }

                if (_doIndexing)
                {
                    optionalParameters += "flag_indexing=\"1\" ";
                }

                if (_doOverwriteContent)
                {
                    optionalParameters += "flag_overwrite_content=\"1\" ";
                }

                if (_doOverwriteGroupAssignment)
                {
                    optionalParameters += "flag_overwrite_group_assignment=\"1\" ";
                }

                if (_doReleasePublishedFiles)
                {
                    optionalParameters += "flag_set_final=\"1\" ";
                }

                optionalParameters += "ignore_metadata=\"" + BoolToString(_doIgnoreMetadata) + "\" ";

                if (!String.IsNullOrEmpty(_inlineFunctionName))
                {
                    optionalParameters += "inlinefunctionname=\"" + HttpUtility.HtmlEncode(_inlineFunctionName) + "\" ";
                }

                if (!String.IsNullOrEmpty(_prefix))
                {
                    optionalParameters += "prefix=\"" + HttpUtility.HtmlEncode(_prefix) + "\" ";
                }

                if (!String.IsNullOrEmpty(_script))
                {
                    optionalParameters += "script=\"" + HttpUtility.HtmlEncode(_script) + "\" ";
                }

                if (_usePrefix)
                {
                    optionalParameters += "useprefix=\"1\" ";
                }
            }
            return optionalParameters;
        }
    }

    public enum PublicationFolderType
    {
        FtpUncLocal = 0,
        DeliveryServer = 1
    }

    public enum PublicationFolderContextInfoPreparationType
    {
        None = -1,
        DoNotPrepare = 0,
        BeginningOfText = 1,
        TextForHighlighting = 2,
        ContextTags = 3
    }

    public enum PublicationFolderContentType
    {
        HTML = 0,
        XML,
        XSL,
        SCRIPT,
        BLOB
    }
}