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

namespace erminas.SmartAPI.CMS.Project.Publication
{
    public interface IPublicationFolderSetting : IPartialRedDotObject, IProjectObject
    {
        void Commit();
        IPublicationFolder PublicationFolder { get; set; }
        IPublicationSetting PublicationSetting { get; set; }
    }

    internal class PublicationFolderSetting : PartialRedDotProjectObject, IPublicationFolderSetting
    {
        private IPublicationFolder _publicationFolder;

        public PublicationFolderSetting(IPublicationSetting parent, Guid guid) : base(parent.Project, guid)
        {
            PublicationSetting = parent;
        }

        internal PublicationFolderSetting(IPublicationSetting parent, XmlElement element)
            : base(parent.Project, element)
        {
            PublicationSetting = parent;

            LoadXml();
        }

        public void Commit()
        {
            const string SAVE_SETTING =
                @"<PROJECT><EXPORTSETTING action=""save"" guid=""{0}""><FOLDEREXPORTSETTING folderguid=""{1}"" guid=""{2}""/></EXPORTSETTING></PROJECT>";

            XmlDocument xmlDoc =
                Project.ExecuteRQL(string.Format(SAVE_SETTING, PublicationSetting.Guid.ToRQLString(),
                                                 _publicationFolder.Guid.ToRQLString(), Guid.ToRQLString()));

            if (!xmlDoc.IsContainingOk())
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not change publication folder setting for {0}", this));
            }
        }

        public IPublicationFolder PublicationFolder
        {
            get { return LazyLoad(ref _publicationFolder); }
            set { _publicationFolder = value; }
        }

        public IPublicationSetting PublicationSetting { get; set; }

        protected override void LoadWholeObject()
        {
            LoadXml();
        }

        protected override XmlElement RetrieveWholeObject()
        {
            const string LOAD_FOLDER_SETTING =
                @"<PROJECT><EXPORTSETTING guid=""{0}"" action=""load"" ><FOLDEREXPORTSETTING guid=""{1}""/></EXPORTSETTING></PROJECT>";

            XmlDocument xmlDoc =
                PublicationSetting.PublicationPackage.Project.ExecuteRQL(String.Format(LOAD_FOLDER_SETTING,
                                                                                       PublicationSetting.Guid
                                                                                                         .ToRQLString(),
                                                                                       Guid.ToRQLString()));

            return (XmlElement) xmlDoc.GetElementsByTagName("FOLDEREXPORTSETTING")[0];
        }

        private void LoadXml()
        {
            const string FOLDER_GUID = "folderguid";
            Guid tmpGuid;
            _publicationFolder = XmlElement.TryGetGuid(FOLDER_GUID, out tmpGuid)
                                     ? new PublicationFolder(PublicationSetting.PublicationPackage.Project, tmpGuid)
                                     : null;
        }
    }
}