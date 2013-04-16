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

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Publication
{
    public interface IPublicationSetting : IRedDotObject, IProjectObject
    {
        IIndexedRDList<string, IPublicationFolderSetting> ExportFolderSettings { get; }
        IProjectVariant ProjectVariant { get; }
        IPublicationPackage PublicationPackage { get; set; }
        IEnumerable<IPublicationTarget> PublishingTargets { get; }
        void SetPublishingTargetsAndCommit(List<IPublicationTarget> newTargets);
        ILanguageVariant LanguageVariant { get; }
    }

    internal class PublicationSetting : RedDotProjectObject, IPublicationSetting
    {
        private readonly NameIndexedRDList<IPublicationFolderSetting> _exportFolderSettings;
        private List<IPublicationTarget> _publishingTargets;

        internal PublicationSetting(IPublicationPackage package, XmlElement xmlElement)
            : base(package.Project, xmlElement)
        {
            _exportFolderSettings = new NameIndexedRDList<IPublicationFolderSetting>(LoadExportFolderSettings,
                                                                                    Caching.Enabled);
            PublicationPackage = package;
            LoadXml();
        }

        public IIndexedRDList<string, IPublicationFolderSetting> ExportFolderSettings
        {
            get { return _exportFolderSettings; }
        }

        public ILanguageVariant LanguageVariant { get; private set; }
        public IProjectVariant ProjectVariant { get; private set; }

        public IPublicationPackage PublicationPackage { get; set; }

        public IEnumerable<IPublicationTarget> PublishingTargets
        {
            get { return _publishingTargets.ToList(); }
        }

        public void SetPublishingTargetsAndCommit(List<IPublicationTarget> newTargets)
        {
            const string SAVE_EXPORT_TARGETS =
                @"<PROJECT><EXPORTSETTING guid=""{0}""><EXPORTTARGETS action=""save"">{1}</EXPORTTARGETS></EXPORTSETTING></PROJECT>";
            const string SINGLE_EXPORT_TARGET = @"<EXPORTTARGET guid=""{0}"" selected=""{1}"" />";

            string targets = newTargets.Aggregate("",
                                                  (current, curTarget) =>
                                                  current +
                                                  string.Format(SINGLE_EXPORT_TARGET, curTarget.Guid.ToRQLString(), "1"));
            string removeTargets = _publishingTargets.Where(x => newTargets.All(y => y.Guid != x.Guid))
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
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not set publishing targets for {0}", this));
            }
            _exportFolderSettings.InvalidateCache();
        }

        private List<IPublicationFolderSetting> LoadExportFolderSettings()
        {
            const string LIST_EXPORT_FOLDER_SETTINGS =
                @"<TREESEGMENT type=""project.1710"" action=""load"" guid=""{0}"" descent=""project"" parentguid=""{1}""/>";
            XmlDocument xmlDoc =
                PublicationPackage.Project.ExecuteRQL(string.Format(LIST_EXPORT_FOLDER_SETTINGS, Guid.ToRQLString(),
                                                                    PublicationPackage.Guid.ToRQLString()));

            return (from XmlElement curSegment in xmlDoc.GetElementsByTagName("SEGMENT")
                    select
                        (IPublicationFolderSetting) new PublicationFolderSetting(this, curSegment.GetGuid())
                            {
                                Name = curSegment.GetAttributeValue("value")
                            }).ToList();
        }

        private void LoadXml()
        {
            ProjectVariant = ProjectVariantFactory.CreateFromGuid(PublicationPackage.Project, XmlElement.GetGuid("projectvariantguid"));

            _name = XmlElement.GetAttributeValue("projectvariantname") + "/" +
                   XmlElement.GetAttributeValue("languagevariantname");
            LanguageVariant =
                PublicationPackage.Project.LanguageVariants.GetByGuid(XmlElement.GetGuid("languagevariantguid"));
            XmlNodeList exportTargets = (XmlElement).GetElementsByTagName("EXPORTTARGET");
            _publishingTargets =
                (from XmlElement curTarget in exportTargets select (IPublicationTarget) new PublicationTarget(Project, curTarget.GetGuid()))
                    .ToList();
        }
    }
}