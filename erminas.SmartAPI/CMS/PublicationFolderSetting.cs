/*
 * Smart API - .Net programatical access to RedDot servers
 * Copyright (C) 2012  erminas GbR 
 *
 * This program is free software: you can redistribute it and/or modify it 
 * under the terms of the GNU General Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details. 
 *
 * You should have received a copy of the GNU General Public License along with this program.
 * If not, see <http://www.gnu.org/licenses/>. 
 */

using System;
using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    public class PublicationFolderSetting : PartialRedDotObject
    {
        private PublicationFolder _publicationFolder;

        public PublicationFolderSetting(PublicationSetting parent, Guid guid)
            : base(guid)
        {
            PublicationSetting = parent;
        }

        public PublicationFolderSetting(PublicationSetting parent, XmlElement element)
            : base(element)
        {
            PublicationSetting = parent;

            LoadXml();
        }

        public PublicationSetting PublicationSetting { get; set; }
        
        public PublicationFolder PublicationFolder
        {
            get { return LazyLoad(ref _publicationFolder); }
            set { _publicationFolder = value; }
        }

        public void Commit()
        {
            const string SAVE_SETTING =
                @"<PROJECT><EXPORTSETTING action=""save"" guid=""{0}""><FOLDEREXPORTSETTING folderguid=""{1}"" guid=""{2}""/></EXPORTSETTING></PROJECT>";

            XmlDocument xmlDoc = PublicationSetting.PublicationPackage.Project.ExecuteRQL(string.Format(SAVE_SETTING,
                                                                                                        PublicationSetting
                                                                                                            .Guid.
                                                                                                            ToRQLString(),
                                                                                                        _publicationFolder
                                                                                                            .Guid.
                                                                                                            ToRQLString(),
                                                                                                        Guid.ToRQLString
                                                                                                            ()));

            if (!xmlDoc.InnerText.Contains("ok"))
            {
                throw new Exception("Could not change folderexportsetting for " + Guid.ToRQLString());
            }
        }

        private void LoadXml()
        {
            const string FOLDER_GUID = "folderguid";
            Guid tmpGuid;
            _publicationFolder = XmlNode.TryGetGuid(FOLDER_GUID, out tmpGuid)
                                     ? new PublicationFolder(PublicationSetting.PublicationPackage.Project,
                                                             tmpGuid)
                                     : null;
        }

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
                                                                                       PublicationSetting.Guid.
                                                                                           ToRQLString(),
                                                                                       Guid.ToRQLString()));

            return (XmlElement) xmlDoc.GetElementsByTagName("FOLDEREXPORTSETTING")[0];
        }
    }
}