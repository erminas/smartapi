// Smart API - .Net programatical access to RedDot servers
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

namespace erminas.SmartAPI.CMS
{
    public class PublicationSetting : RedDotObject
    {
        private readonly NameIndexedRDList<PublicationFolderSetting> _exportFolderSettings;
        private List<PublicationTarget> _publishingTargets;

        public PublicationSetting(PublicationPackage package, XmlElement xmlElement) : base(xmlElement)
        {
            _exportFolderSettings = new NameIndexedRDList<PublicationFolderSetting>(LoadExportFolderSettings,
                                                                                    Caching.Enabled);
            PublicationPackage = package;
            LoadXml();
        }

        public PublicationPackage PublicationPackage { get; set; }

        public IEnumerable<PublicationTarget> PublishingTargets
        {
            get { return _publishingTargets.ToList(); }
        }

        public ProjectVariant ProjectVariant { get; private set; }

        public LanguageVariant LanguageVariant { get; private set; }

        public NameIndexedRDList<PublicationFolderSetting> ExportFolderSettings
        {
            get { return _exportFolderSettings; }
        }

        private void LoadXml()
        {
            ProjectVariant = new ProjectVariant(PublicationPackage.Project, XmlElement.GetGuid("projectvariantguid"));

            Name = XmlElement.GetAttributeValue("projectvariantname") + "/" +
                   XmlElement.GetAttributeValue("languagevariantname");
            LanguageVariant =
                PublicationPackage.Project.LanguageVariants.GetByGuid(XmlElement.GetGuid("languagevariantguid"));
            XmlNodeList exportTargets = (XmlElement).GetElementsByTagName("EXPORTTARGET");
            _publishingTargets =
                (from XmlElement curTarget in exportTargets select new PublicationTarget(curTarget.GetGuid())).ToList();
        }

        public void SetPublishingTargetsAndCommit(List<PublicationTarget> newTargets)
        {
            const string SAVE_EXPORT_TARGETS =
                @"<PROJECT><EXPORTSETTING guid=""{0}""><EXPORTTARGETS action=""save"">{1}</EXPORTTARGETS></EXPORTSETTING></PROJECT>";
            const string SINGLE_EXPORT_TARGET = @"<EXPORTTARGET guid=""{0}"" selected=""{1}"" />";

            string targets = newTargets.Aggregate("",
                                                  (current, curTarget) =>
                                                  current +
                                                  string.Format(SINGLE_EXPORT_TARGET, curTarget.Guid.ToRQLString(), "1"));
            string removeTargets = _publishingTargets.Where(x => !newTargets.Any(y => y.Guid == x.Guid))
                                                     .Aggregate("",
                                                                (current, curTarget) =>
                                                                current +
                                                                string.Format(SINGLE_EXPORT_TARGET,
                                                                              curTarget.Guid.ToRQLString(), "0"));

            XmlDocument xmlDoc =
                PublicationPackage.Project.ExecuteRQL(string.Format(SAVE_EXPORT_TARGETS, Guid.ToRQLString(),
                                                                    targets + removeTargets));

            if (!xmlDoc.InnerXml.Contains("ok"))
            {
                throw new Exception("Could not set publishing targets for " + Guid.ToRQLString());
            }
            _exportFolderSettings.InvalidateCache();
        }

        private List<PublicationFolderSetting> LoadExportFolderSettings()
        {
            const string LIST_EXPORT_FOLDER_SETTINGS =
                @"<TREESEGMENT type=""project.1710"" action=""load"" guid=""{0}"" descent=""project"" parentguid=""{1}""/>";
            XmlDocument xmlDoc =
                PublicationPackage.Project.ExecuteRQL(string.Format(LIST_EXPORT_FOLDER_SETTINGS, Guid.ToRQLString(),
                                                                    PublicationPackage.Guid.ToRQLString()));

            return (from XmlElement curSegment in xmlDoc.GetElementsByTagName("SEGMENT")
                    select
                        new PublicationFolderSetting(this, curSegment.GetGuid())
                            {
                                Name = curSegment.GetAttributeValue("value")
                            }).ToList();
        }
    }
}