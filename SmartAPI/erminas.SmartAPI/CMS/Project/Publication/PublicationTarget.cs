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
using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.Publication
{
    public class PublicationTarget : PartialRedDotProjectObject
    {
        #region TargetType enum

        public enum TargetType
        {
            None = 0,
            Ftp = 6205,
            Directory = 6206,
            LiveServer = 6207,
            Sftp = 6208
        };

        #endregion

        private TargetType _type;
        private string _urlPrefix;

        internal PublicationTarget(Project project, XmlElement xmlElement) : base(project, xmlElement)
        {
            LoadXml();
        }

        public PublicationTarget(Project project, Guid guid) : base(project, guid)
        {
        }

        public TargetType Type
        {
            get { return LazyLoad(ref _type); }
        }

        public string UrlPrefix
        {
            get { return LazyLoad(ref _urlPrefix); }
        }

        protected override void LoadWholeObject()
        {
            LoadXml();
        }

        protected override XmlElement RetrieveWholeObject()
        {
            const string LOAD_PUBLISHING_TARGET = @"<EXPORT guid=""{0}"" action=""load""/>";

            XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(LOAD_PUBLISHING_TARGET, Guid.ToRQLString()),
                                                    Project.RqlType.SessionKeyInProject);
            return (XmlElement) xmlDoc.GetElementsByTagName("EXPORT")[0];
        }

        private void LoadXml()
        {
            InitIfPresent(ref _urlPrefix, "urlprefix", x => x);
            EnsuredInit(ref _type, "type", x => (TargetType) int.Parse(x));
        }
    }
}