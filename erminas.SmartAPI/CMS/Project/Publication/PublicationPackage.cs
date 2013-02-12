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
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Publication
{
    public class PublicationPackage : PartialRedDotObject
    {
        public PublicationPackage(Project project, Guid guid) : base(guid)
        {
            ExportSettings = new CachedList<PublicationSetting>(LoadExportSettings, Caching.Enabled);
            Project = project;
        }

        public CachedList<PublicationSetting> ExportSettings { get; private set; }
        public Project Project { get; set; }

        protected override void LoadWholeObject()
        {
        }

        protected override XmlElement RetrieveWholeObject()
        {
            const string LOAD_PUBLICATION_PACKAGE = @"<PROJECT><EXPORTPACKET action=""load"" guid=""{0}""/></PROJECT>";
            XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(LOAD_PUBLICATION_PACKAGE, Guid.ToRQLString()));
            return (XmlElement) xmlDoc.GetElementsByTagName("EXPORTPACKET")[0];
        }

        private List<PublicationSetting> LoadExportSettings()
        {
            const string LOAD_PUBLICATION_PACKAGE =
                @"<PROJECT><EXPORTPACKET action=""loadpacket"" guid=""{0}"" /></PROJECT>";
            XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(LOAD_PUBLICATION_PACKAGE, Guid.ToRQLString()));

            return (from XmlElement curSetting in xmlDoc.GetElementsByTagName("EXPORTSETTING")
                    select new PublicationSetting(this, curSetting)).ToList();
        }
    }
}