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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Converter;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements;
using erminas.SmartAPI.CMS.Project.Folder;
using erminas.SmartAPI.CMS.Project.Keywords;
using erminas.SmartAPI.CMS.Project.Pages;
using erminas.SmartAPI.CMS.ServerManagement;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.ContentClasses
{
    public interface IContentClass : IPartialRedDotObject, IProjectObject, IDeletable, ISessionObject
    {
        /// <summary>
        ///     Commit changes on attributes to the server.
        /// </summary>
        void Commit();

        /// <summary>
        ///     Copy selected elements from this content class to another target content class.
        /// </summary>
        /// <param name="targetCC"> Target content class to copy the elements to </param>
        /// <param name="elementNames"> Names of the elements to copy </param>
        void CopyElementsToContentClass(IContentClass targetCC, params string[] elementNames);

        /// <summary>
        ///     Copy this content class to another project
        /// </summary>
        /// <param name="project"> The target project to copy the content class to </param>
        /// <param name="targetFolderGuid"> Guid of the target content class folder in the target project </param>
        void CopyToProject(IProject project, Guid targetFolderGuid);

        /// <summary>
        ///     Description
        /// </summary>
        string DescriptionInCurrentDisplayLanguage { get; set; }

        /// <summary>
        ///     EditableAreaSettings of the content class The settings get cached. To refresh the settings call <see cref="Refresh" />
        /// </summary>
        IContentClassEditableAreaSettings EditableAreaSettings { get; }

        IContentClassElements Elements { get; }

        ICollection<IPage> Pages { get; }

        /// <summary>
        ///     Folder that contains the content class.
        /// </summary>
        IContentClassFolder Folder { get; }

        bool IsAvailableViaTheShortcutMenuInSmartEdit { get; set; }

        [VersionIsGreaterThanOrEqual(9, 0, 0, 41, VersionName = "Version 9 Hotfix 5")]
        bool IsChangingHeadlineEffectiveForAllLanguageVariants { get; set; }

        bool IsKeywordRequired { get; set; }

        bool IsNotRelevantForGlobalContentWorkflow { get; set; }

        IPageDefinitions PageDefinitions { get; }

        /// <summary>
        ///     List of the preassigned keywords of this content class, indexed by name. This list is cached by default.
        /// </summary>
        IPreassignedKeywords PreassignedKeywords { get; }

        /// <summary>
        ///     Default prefix for pages.
        /// </summary>
        ISyllable Prefix { get; }

        IProjectVariantAssignments ProjectVariantAssignments { get; }

        ICategory RequiredKeywordCategory { get; set; }

        /// <summary>
        ///     Default suffix for pages.
        /// </summary>
        ISyllable Suffix { get; }

        ITemplateVariants TemplateVariants { get; }

        /// <summary>
        ///     Versioning information for the content class. List is cached by default.
        /// </summary>
        IContentClassVersions Versions { get; }
    }

    /// <summary>
    ///     Represents a content class in RedDot.
    /// </summary>
    internal class ContentClass : PartialRedDotProjectObject, IContentClass
    {
        internal ContentClass(IProject project, XmlElement xmlElement) : base(project, xmlElement)
        {
            Init();
            //TODO sharedrights = 1 bei ccs von anderen projekten
        }

        internal ContentClass(IProject project, Guid guid) : base(project, guid)
        {
            Init();
            //TODO sharedrights = 1 bei ccs von anderen projekten
        }

        /// <summary>
        ///     Commit changes on attributes to the server.
        /// </summary>
        public void Commit()
        {
            //if it isn't even initialized, nothing was changed
            if (!IsInitialized)
            {
                return;
            }

            var query = GetSaveString(_readWriteWrapper.MergedElement);
            Project.ExecuteRQL(query, RqlType.SessionKeyInProject);
        }

        /// <summary>
        ///     Copy selected elements from this content class to another target content class.
        /// </summary>
        /// <param name="targetCC"> Target content class to copy the elements to </param>
        /// <param name="elementNames"> Names of the elements to copy </param>
        public void CopyElementsToContentClass(IContentClass targetCC, params string[] elementNames)
        {
            if (elementNames == null || elementNames.Length == 0)
            {
                return;
            }

            var createdElements = new Dictionary<string, IContentClassElement>();
            using (new LanguageContext(Project))
            {
                var assign = new AttributeAssignment();
                foreach (var languageVariant in Project.LanguageVariants)
                {
                    ILanguageVariant targetLanguageVariant =
                        targetCC.Project.LanguageVariants[languageVariant.Abbreviation];
                    foreach (var curElementName in elementNames)
                    {
                        IContentClassElement curTargetContentClassElement;
                        languageVariant.Select();
                        var curSourceContentClassElement = Elements[curElementName];
                        if (createdElements.TryGetValue(curElementName, out curTargetContentClassElement))
                        {
                            targetLanguageVariant.Select();
                            assign.AssignAllRedDotAttributesForLanguage(curSourceContentClassElement,
                                                                        curTargetContentClassElement,
                                                                        targetLanguageVariant.Abbreviation);
                            curTargetContentClassElement.CommitInCurrentLanguage();
                        }
                        else
                        {
                            targetLanguageVariant.Select();
                            curTargetContentClassElement = curSourceContentClassElement.CopyToContentClass(targetCC);
                            createdElements.Add(curElementName, curTargetContentClassElement);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Copy this content class to another project
        /// </summary>
        /// <param name="project"> The target project to copy the content class to </param>
        /// <param name="targetFolderGuid"> Guid of the target content class folder in the target project </param>
        public void CopyToProject(IProject project, Guid targetFolderGuid)
        {
            ContentClass targetCC = CreateCopyInProject(project, targetFolderGuid);

            CopyProjectVariantAssignmentToCC(targetCC);

            CopyAttributesToCC(targetCC);

            CopyAllElementsToCC(targetCC);

            CopyPreassignedKeywordsToCC(targetCC);
        }

        private void CopyProjectVariantAssignmentToCC(ContentClass targetCC)
        {
            var assignment = ProjectVariantAssignments.ToLookup(x=>targetCC.TemplateVariants.GetByName(x.TemplateVariant.Name), x=>targetCC.Project.ProjectVariants.GetByName(x.ProjectVariant.Name));
            targetCC.ProjectVariantAssignments.Assign(assignment);
        }

        /// <summary>
        ///     Delete this content class
        /// </summary>
        public void Delete()
        {
            const string DELETE_CC = @"<TEMPLATE action=""delete"" guid=""{0}""/>";
            XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(DELETE_CC, Guid.ToRQLString()));
            var template = (XmlElement) xmlDoc.GetElementsByTagName("TEMPLATE")[0];
            Guid guid;
            if (template == null || !template.TryGetGuid(out guid) || guid != Guid)
            {
                var msgNode = (XmlElement) xmlDoc.GetElementsByTagName("MESSAGE")[0];
                string msg = "could not delete content class: " + ToString();
                if (msgNode != null)
                {
                    msg += "; Reason: " + msgNode.GetAttributeValue("value");
                }
                throw new SmartAPIException(Session.ServerLogin, msg);
            }
        }

        /// <summary>
        ///     Description
        /// </summary>
        [RedDot("description")]
        public string DescriptionInCurrentDisplayLanguage
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        /// <summary>
        ///     EditableAreaSettings of the content class The settings get cached. To refresh the settings call EditableAreaSettings.
        ///     <see
        ///         cref="Refresh" />
        /// </summary>
        public IContentClassEditableAreaSettings EditableAreaSettings { get; private set; }

        public IContentClassElements Elements { get; private set; }

        public ICollection<IPage> Pages
        {
            get { return Project.Pages.Search(x => x.ContentClass = this).ToList(); }
        }

        /// <summary>
        ///     Folder that contains the content class.
        /// </summary>
        [RedDot("folderguid", ConverterType = typeof (ContentClassFolderConverter), Description = "Content Class Folder"
            )]
        public IContentClassFolder Folder
        {
            get { return GetAttributeValue<IContentClassFolder>(); }
        }

        [RedDot("selectinnewpage")]
        public bool IsAvailableViaTheShortcutMenuInSmartEdit
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [VersionIsGreaterThanOrEqual(9, 0, 0, 41, VersionName = "Version 9 Hotfix 5")]
        [RedDot("adoptheadlinetoalllanguages")]
        public bool IsChangingHeadlineEffectiveForAllLanguageVariants
        {
            get
            {
                VersionVerifier.EnsureVersion(Project.Session);
                return GetAttributeValue<bool>();
            }
            set
            {
                VersionVerifier.EnsureVersion(Project.Session);
                SetAttributeValue(value);
            }
        }

        [RedDot("keywordrequired")]
        public bool IsKeywordRequired
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("ignoreglobalworkflow")]
        public bool IsNotRelevantForGlobalContentWorkflow
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        public IPageDefinitions PageDefinitions { get; private set; }

        /// <summary>
        ///     List of the preassigned keywords of this content class, indexed by name. This list is cached by default.
        /// </summary>
        public IPreassignedKeywords PreassignedKeywords { get; private set; }

        /// <summary>
        ///     Default prefix for pages.
        /// </summary>
        [RedDot("praefixguid", ConverterType = typeof (SyllableConverter))]
        public ISyllable Prefix
        {
            get { return GetAttributeValue<ISyllable>(); }
        }

        public IProjectVariantAssignments ProjectVariantAssignments { get; private set; }

        [RedDot("requiredcategory", ConverterType = typeof (CategoryConverter))]
        public ICategory RequiredKeywordCategory
        {
            get { return GetAttributeValue<ICategory>(); }
            set
            {
                EnsureInitialization();
                if (value != null && !IsKeywordRequired)
                {
                    IsKeywordRequired = true;
                }

                SetAttributeValue(value ?? ArbitraryCategory.INSTANCE);
            }
        }

        /// <summary>
        ///     Default suffix for pages.
        /// </summary>
        [RedDot("suffixguid", ConverterType = typeof (SyllableConverter))]
        public ISyllable Suffix
        {
            get { return GetAttributeValue<ISyllable>(); }
        }

        public ITemplateVariants TemplateVariants { get; private set; }

        public override string ToString()
        {
            return Name + " (" + Guid.ToRQLString() + ")";
        }

        /// <summary>
        ///     Versioning information for the content class. List is cached by default.
        /// </summary>
        public IContentClassVersions Versions { get; private set; }

        protected override void LoadWholeObject()
        {
        }

        protected override XmlElement RetrieveWholeObject()
        {
            const string LOAD_CC = @"<PROJECT><TEMPLATE action=""load"" guid=""{0}""/></PROJECT>";
            XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(LOAD_CC, Guid.ToRQLString()));
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("TEMPLATE");
            if (xmlNodes.Count != 1)
            {
                throw new ArgumentException(String.Format("Could not find content class with guid {0}.",
                                                          Guid.ToRQLString()));
            }
            return (XmlElement) xmlNodes[0];
        }

        private void AddProjectVariants(IProject project, XmlElement template)
        {
            XmlElement projectVariants = template.AddElement("PROJECTVARIANTS");
            projectVariants.AddAttribute("action", "assign");
            foreach (IProjectVariant curVariant in Project.ProjectVariants)
            {
                XmlElement projectVariant = projectVariants.AddElement("PROJECTVARIANT");
                IProjectVariant otherVariant;
                if (!project.ProjectVariants.TryGetByName(curVariant.Name, out otherVariant))
                {
                    throw new SmartAPIException(Session.ServerLogin,
                                                string.Format("Could not find project variant {0} in project {1}",
                                                              curVariant.Name, project.Name));
                }
                projectVariant.AddAttribute("guid", otherVariant.Guid.ToRQLString());
            }
        }

        private void AddTemplateDescriptions(IProject project, XmlElement template)
        {
            XmlElement templateDescriptions = template.AddElement("TEMPLATEDESCRIPTIONS");
            foreach (ILanguageVariant languageVariant in project.LanguageVariants)
            {
                XmlElement templateDescription = templateDescriptions.AddElement("TEMPLATEDESCRIPTION");
                templateDescription.AddAttribute("dialoglanguageid", languageVariant.Abbreviation);
                templateDescription.AddAttribute("name", Name);
                if (!string.IsNullOrEmpty(DescriptionInCurrentDisplayLanguage))
                {
                    templateDescription.AddAttribute("description", DescriptionInCurrentDisplayLanguage);
                }
            }
        }

        private void AddTemplateVariants(XmlElement template)
        {
            XmlDocument ownerDocument = template.OwnerDocument;

            XmlElement templateVariants = template.AddElement("TEMPLATEVARIANTS");
            foreach (ITemplateVariant curTemplateVariant in TemplateVariants)
            {
                XmlElement templateVariant = templateVariants.AddElement("TEMPLATEVARIANT");
                templateVariant.AddAttribute("name", curTemplateVariant.Name);
                // ReSharper disable PossibleNullReferenceException
                XmlText textNode = ownerDocument.CreateTextNode(curTemplateVariant.Data);
                // ReSharper restore PossibleNullReferenceException
                templateVariant.AppendChild(textNode);
            }
        }

        private void CopyAllElementsToCC(IContentClass targetCC)
        {
            CopyElementsToContentClass(targetCC, Elements.Select(element => element.Name).ToArray());
        }

        private void CopyAttributesToCC(IContentClass targetCC)
        {
            var assignment = new AttributeAssignment();
            assignment.AssignAllLanguageIndependentRedDotAttributes(EditableAreaSettings, targetCC.EditableAreaSettings);

            targetCC.EditableAreaSettings.Commit();
            targetCC.Refresh();
            try
            {
                assignment.AssignAllLanguageIndependentRedDotAttributes(this, targetCC);
            } catch (AttributeChangeException e)
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format(
                                                "Unable to assign attribute {0} in content class {1} of project {2} to content class {3} of project {4}",
                                                e.AttributeName, Name, Project.Name, targetCC.Name,
                                                targetCC.Project.Name), e);
            }
            targetCC.Commit();
        }

        private void CopyPreassignedKeywordsToCC(IContentClass targetCC)
        {
            try
            {
                List<IKeyword> keywordsToAssign =
                    PreassignedKeywords.Select(
                        x => targetCC.Project.Categories.GetByName(x.Category.Name).Keywords.GetByName(x.Name)).ToList();
                targetCC.PreassignedKeywords.Set(keywordsToAssign);
            } catch (Exception e)
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not copy preassigned keywords for content class {0}",
                                                          Name), e);
            }
        }

        private ContentClass CreateContentClass(IProject project, XmlElement template)
        {
            XmlDocument creationResultNode = project.ExecuteRQL(template.NodeToString());
            XmlNode guidTextNode = creationResultNode.FirstChild;
            Guid newCCGuid;
            if (guidTextNode == null || guidTextNode.NodeType != XmlNodeType.Element || guidTextNode.FirstChild == null ||
                !Guid.TryParse(guidTextNode.FirstChild.Value.Trim(), out newCCGuid))
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not create content class '{0}'", Name));
            }

            var targetCC = new ContentClass(project, newCCGuid);
            return targetCC;
        }

        private ContentClass CreateCopyInProject(IProject project, Guid targetFolderGuid)
        {
            IContentClassFolder folder = project.ContentClassFolders.GetByGuid(targetFolderGuid);
            if (folder == null)
            {
                throw new ArgumentException("no such content class folder '" + targetFolderGuid.ToRQLString() +
                                            "' in project " + Name);
            }

            var xmlDoc = new XmlDocument();
            XmlElement template = CreateTemplateXmlElement(xmlDoc, folder);

            AddTemplateDescriptions(project, template);

            AddTemplateVariants(template);

            //AddProjectVariants(project, template);

            return CreateContentClass(project, template);
        }

        private static XmlElement CreateTemplateXmlElement(XmlDocument xmlDoc, IContentClassFolder folder)
        {
            XmlElement template = xmlDoc.AddElement("TEMPLATE");
            template.AddAttribute("action", "addnew");
            template.AddAttribute("folderguid", folder.Guid.ToRQLString());
            return template;
        }

        private void Init()
        {
            Versions = new ContentClassVersions(this, Caching.Enabled);
            PreassignedKeywords = new PreassignedKeywords(this, Caching.Enabled);
            PageDefinitions = new PageDefinitions(this, Caching.Enabled);
            TemplateVariants = new TemplateVariants(this, Caching.Enabled);
            Elements = new ContentClassElements(this, Caching.Enabled);
            ProjectVariantAssignments = new ProjectVariantAssignments(this, Caching.Enabled);
            EditableAreaSettings = new CCEditableAreaSettings(this);
        }

        /// <summary>
        ///     Represents version information on a specific content class version.
        /// </summary>
        internal class ContentClassVersion : RedDotProjectObject, IContentClassVersion
        {
            private DateTime? _date;
            private IContentClassFolder _folder;
            private IUser _user;

            internal ContentClassVersion(IContentClass parent, XmlElement xmlElement) : base(parent.Project, xmlElement)
            {
                ContentClass = parent;
            }

            public IContentClass ContentClass { get; private set; }

            public ContentClassVersionType CreationType
            {
                get { return XmlElement.GetAttributeValue("type").ToEnum<ContentClassVersionType>(); }
            }

            /// <summary>
            ///     Time the version was created
            /// </summary>
            public DateTime Date
            {
                get { return _date ?? (_date = XmlElement.GetOADate()).GetValueOrDefault(); }
            }

            /// <summary>
            ///     Description text
            /// </summary>
            public string DescriptionInCurrentDisplayLanguage
            {
                get { return XmlElement.GetAttributeValue("description"); }
            }

            public IContentClassFolder Folder
            {
                get { return _folder ?? (_folder = Project.ContentClassFolders.GetByGuid(XmlElement.GetGuid("folderguid"))); }
            }

            public IUser User
            {
                get
                {
                    if (_user != null)
                    {
                        return _user;
                    }
                    string userGuid = XmlElement.GetAttributeValue("userguid");
                    return string.IsNullOrEmpty(userGuid)
                               ? null
                               : _user =
                                 ContentClass.Project.Session.ServerManager.Users.GetByGuid(GuidConvert(userGuid));
                }
            }

            public string Username
            {
                get { return XmlElement.GetAttributeValue("username"); }
            }
        }

        public interface ILanguageDependentPartialRedDotObject : IPartialRedDotObject,
                                                                 IProjectObject,
                                                                 ILanguageDependentXmlBasedObject
        {
        }
    }

    public enum ContentClassVersionType
    {
        AutomaticallyCreated = 1,
        ManuallyCreate = 2,
        Temporary = 3
    }
}