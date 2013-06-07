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
using System.Diagnostics;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Administration;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes;
using erminas.SmartAPI.CMS.Project.Folder;
using erminas.SmartAPI.CMS.Project.Keywords;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.ContentClasses
{
    public interface IContentClassVersions : IRDList<IContentClassVersion>, IProjectObject
    {
        IContentClassVersion Current { get; }
    }

    internal class ContentClassVersions : RDList<IContentClassVersion>, IContentClassVersions
    {
        private readonly IContentClass _contentClass;

        internal ContentClassVersions(IContentClass contentClass, Caching caching) : base(caching)
        {
            _contentClass = contentClass;
            RetrieveFunc = GetVersions;
        }

        public IContentClass ContentClass
        {
            get { return _contentClass; }
        }

        /// <summary>
        ///     Versioning information for the latest version of the content class.
        /// </summary>
        public IContentClassVersion Current
        {
            get { return this.FirstOrDefault(); }
        }

        public IProject Project
        {
            get { return _contentClass.Project; }
        }

        public ISession Session
        {
            get { return _contentClass.Session; }
        }

        private List<IContentClassVersion> GetVersions()
        {
            const string LIST_VERSIONS =
                @"<PROJECT><TEMPLATE guid=""{0}""><ARCHIVE action=""list""/></TEMPLATE></PROJECT>";

            var xmlDoc = Project.ExecuteRQL(LIST_VERSIONS.RQLFormat(_contentClass));
            var versionNodes = xmlDoc.GetElementsByTagName("VERSION");

            return (from XmlElement curVersion in versionNodes
                    let cc = (IContentClassVersion) new ContentClass.ContentClassVersion(_contentClass, curVersion)
                    orderby cc.Date descending
                    select cc).ToList();
        }
    }

    public interface IContentClass : IPartialRedDotObject, IProjectObject, IDeletable, IAttributeContainer
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
        string Description { get; set; }

        /// <summary>
        ///     EditableAreaSettings of the content class The settings get cached. To refresh the settings call <see cref="Refresh" />
        /// </summary>
        IContentClassEditableAreaSettings EditableAreaSettings { get; set; }

        IContentClassElements Elements { get; }

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
        private IContentClassEditableAreaSettings _editableAreaSettings;
        private IContentClassFolder _folder;
        private Syllable _prefix;
        private Syllable _suffix;

        internal ContentClass(IProject project, XmlElement xmlElement) : base(project, xmlElement)
        {
            Init();
            LoadXml();
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
            var doc = new XmlDocument();
            XmlElement templateElement = doc.CreateElement("TEMPLATE");
            foreach (IRDAttribute attribute in Attributes)
            {
                XmlAttribute curAttribute = doc.CreateAttribute(attribute.Name);
                curAttribute.Value = ((RDXmlNodeAttribute) attribute).GetXmlNodeValue();
                templateElement.Attributes.Append(curAttribute);
            }

            XmlAttribute guidAttr = doc.CreateAttribute("guid");
            guidAttr.Value = Guid.ToRQLString();
            templateElement.Attributes.Append(guidAttr);

            Project.ExecuteRQL(GetSaveString(templateElement), RqlType.SessionKeyInProject);
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
                foreach (var languageVariant in Project.LanguageVariants)
                {
                    ILanguageVariant targetLanguageVariant =
                        targetCC.Project.LanguageVariants[languageVariant.Abbreviation];
                    foreach (var curElementName in elementNames)
                    {
                        IContentClassElement curTargetContentClassElement;
                        languageVariant.Select();
                        var curSourceContentClassElement = this[languageVariant.Abbreviation, curElementName];
                        if (createdElements.TryGetValue(curElementName, out curTargetContentClassElement))
                        {
                            IContentClassElement tmpTargetContentClassElement =
                                ContentClassElement.CreateElement(targetCC, curTargetContentClassElement.XmlElement);
                            tmpTargetContentClassElement.AssignAttributes(curSourceContentClassElement.Attributes);
                            targetLanguageVariant.Select();
                            tmpTargetContentClassElement.Commit();
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

            CopyAttributesToCC(targetCC);

            CopyAllElementsToCC(targetCC);

            CopyPreassignedKeywordsToCC(targetCC);
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
        public string Description
        {
            get { return GetAttributeValue<string>("description"); }
            set { SetAttributeValue("description", value); }
        }

        /// <summary>
        ///     EditableAreaSettings of the content class The settings get cached. To refresh the settings call <see cref="Refresh" />
        /// </summary>
        public IContentClassEditableAreaSettings EditableAreaSettings
        {
            get
            {
                if (_editableAreaSettings == null)
                {
                    const string LOAD_CC_SETTINGS = @"<TEMPLATE guid=""{0}""><SETTINGS action=""load""/></TEMPLATE>";
                    XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(LOAD_CC_SETTINGS, Guid.ToRQLString()));
                    var node = (XmlElement) xmlDoc.GetElementsByTagName("SETTINGS")[0];
                    if (node == null)
                    {
                        throw new SmartAPIException(Session.ServerLogin,
                                                    string.Format("Could not load settings for content class {0}", this));
                    }
                    _editableAreaSettings = new CCEditableAreaSettings(this, node);
                }
                return _editableAreaSettings;
            }
            set { _editableAreaSettings = value; }
        }

        public IContentClassElements Elements { get; private set; }

        /// <summary>
        ///     Folder that contains the content class.
        /// </summary>
        public IContentClassFolder Folder
        {
            get
            {
                EnsureInitialization();
                return _folder;
            }
        }

        public bool IsAvailableViaTheShortcutMenuInSmartEdit
        {
            get { return GetAttributeValue<bool>("selectinnewpage"); }
            set { SetAttributeValue("selectinnewpage", value); }
        }

        [VersionIsGreaterThanOrEqual(9, 0, 0, 41, VersionName = "Version 9 Hotfix 5")]
        public bool IsChangingHeadlineEffectiveForAllLanguageVariants
        {
            get
            {
                VersionVerifier.EnsureVersion(Project.Session);
                EnsureInitialization();
                return GetAttributeValue<bool>("adoptheadlinetoalllanguages");
            }
            set
            {
                VersionVerifier.EnsureVersion(Project.Session);
                EnsureInitialization(); //TODO eigentlich muessen nur die attribute fuers schreiben vorhanden sein
                SetAttributeValue("adoptheadlinetoalllanguages", value);
            }
        }

        public bool IsKeywordRequired
        {
            get
            {
                EnsureInitialization();
                return GetAttributeValue<bool>("keywordrequired");
            }
            set
            {
                EnsureInitialization();
                SetAttributeValue("keywordrequired", value);
            }
        }

        public bool IsNotRelevantForGlobalContentWorkflow
        {
            get { return GetAttributeValue<bool>("ignoreglobalworkflow"); }
            set { SetAttributeValue("ignoreglobalworkflow", value); }
        }

        /// <summary>
        ///     Get an element by language/element name
        /// </summary>
        /// <param name="language"> Language Id of language variant </param>
        /// <param name="elementName"> Name of the element </param>
        public IContentClassElement this[string language, string elementName]
        {
            get { return Elements[language][elementName]; }
        }

        public IPageDefinitions PageDefinitions { get; private set; }

        /// <summary>
        ///     List of the preassigned keywords of this content class, indexed by name. This list is cached by default.
        /// </summary>
        public IPreassignedKeywords PreassignedKeywords { get; private set; }

        /// <summary>
        ///     Default prefix for pages.
        /// </summary>
        public ISyllable Prefix
        {
            get { return LazyLoad(ref _prefix); }
        }

        public IProjectVariantAssignments ProjectVariantAssignments { get; private set; }

        public override void Refresh()
        {
            base.Refresh();
            _editableAreaSettings = null;
        }

        public ICategory RequiredKeywordCategory
        {
            get
            {
                EnsureInitialization();
                return ((CategoryXmlNodeAttribute) GetAttribute("requiredcategory")).Value;
            }
            set
            {
                EnsureInitialization();
                if (value != null && !IsKeywordRequired)
                {
                    IsKeywordRequired = true;
                }

                var categoryXmlNodeAttribute = ((CategoryXmlNodeAttribute) GetAttribute("requiredcategory"));

                if (value == null)
                {
                    categoryXmlNodeAttribute.SetUseArbitraryCategory();
                }
                else
                {
                    categoryXmlNodeAttribute.Value = value;
                }
            }
        }

        /// <summary>
        ///     Default suffix for pages.
        /// </summary>
        public ISyllable Suffix
        {
            get { return LazyLoad(ref _suffix); }
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
            LoadXml();
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
                if (!string.IsNullOrEmpty(Description))
                {
                    templateDescription.AddAttribute("description", Description);
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
            CopyElementsToContentClass(targetCC, Elements.Names.ToArray());
        }

        private void CopyAttributesToCC(IContentClass targetCC)
        {
            foreach (var curAttribute in EditableAreaSettings.Attributes)
            {
                targetCC.EditableAreaSettings.GetAttribute(curAttribute.Name).Assign(curAttribute);
            }
            targetCC.EditableAreaSettings.Commit();
            targetCC.Refresh();
            foreach (var curAttribute in Attributes)
            {
                try
                {
                    targetCC.GetAttribute(curAttribute.Name).Assign(curAttribute);
                } catch (Exception e)
                {
                    throw new SmartAPIException(Session.ServerLogin,
                                                string.Format(
                                                    "Unable to assign attribute {0} in content class {1} of project {2}",
                                                    curAttribute.Name, Name, Project.Name), e);
                }
            }
            targetCC.Commit();
        }

        private void CopyPreassignedKeywordsToCC(IContentClass targetCC)
        {
            try
            {
                List<IKeyword> keywordsToAssign =
                    PreassignedKeywords.Select(
                        x => targetCC.Project.Categories.GetByName(x.Category.Name).CategoryKeywords.GetByName(x.Name))
                                       .ToList();
                targetCC.PreassignedKeywords.Set(keywordsToAssign);
            } catch (Exception e)
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not copy preassigned keywords for content class {0}",
                                                          Name), e);
            }
        }

        private void CreateBaseAttributes()
        {
            CreateAttributes("approverequired", "description", "framesetafterlist", "name", "praefixguid", "suffixguid",
                             "adoptheadlinetoalllanguages", "keywordrequired", "requiredcategory", "selectinnewpage",
                             "ignoreglobalworkflow");
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

            AddProjectVariants(project, template);

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
            Elements = new ContentClassElements(this);
            ProjectVariantAssignments = new ProjectVariantAssignments(this, Caching.Enabled);
        }

        private void LoadXml()
        {
            if (!Attributes.Any())
            {
                CreateBaseAttributes();
            }
            var settingsNode = (XmlElement) XmlElement.GetElementsByTagName("SETTINGS")[0];
            if (settingsNode != null)
            {
                _editableAreaSettings = new CCEditableAreaSettings(this, settingsNode);
            }

            //InitIfPresent(ref _languageVariant, "languagevariantid", x => Project.LanguageVariants[x]);
            InitIfPresent(ref _prefix, "praefixguid", x => new Syllable(Project, GuidConvert(x)));
            InitIfPresent(ref _suffix, "suffixguid", x => new Syllable(Project, GuidConvert(x)));
            InitIfPresent(ref _folder, "folderguid",
                          x =>
                          Project.ContentClassFolders.Union(Project.ContentClassFolders.Broken)
                                 .First(folder => folder.Guid == Guid.Parse(x)));
        }

        #region Nested type: CCEditableAreaSettings

        /// <summary>
        ///     Represents editable area configuration of a content class.
        /// </summary>
        private class CCEditableAreaSettings : AbstractAttributeContainer, IContentClassEditableAreaSettings
        {
            private readonly IContentClass _parent;

            internal CCEditableAreaSettings(IContentClass parent, XmlElement xmlElement)
                : base(parent.Session, xmlElement)
            {
                Debug.Assert(xmlElement != null);
                Project = parent.Project;
                _parent = parent;
                InitAttributes();
            }

            public string BorderColor
            {
                get { return GetAttributeValue<string>("bordercolor"); }
                set { SetAttributeValue("bordercolor", value); }
            }

            public string BorderStyle
            {
                get { return GetAttributeValue<string>("borderstyle"); }
                set { SetAttributeValue("borderstyle", value); }
            }

            public string BorderWidth
            {
                get { return GetAttributeValue<string>("borderwidth"); }
                set { SetAttributeValue("borderwidth", value); }
            }

            public void Commit()
            {
                const string SAVE_CC_SETTINGS = @"<TEMPLATE guid=""{0}"">{1}</TEMPLATE>";
                XmlDocument result =
                    _parent.Project.ExecuteRQL(
                        string.Format(SAVE_CC_SETTINGS, _parent.Guid.ToRQLString(),
                                      GetSaveString((XmlElement) XmlElement.Clone())), RqlType.SessionKeyInProject);

                if (result.GetElementsByTagName("SETTINGS").Count != 1)
                {
                    throw new SmartAPIException(Session.ServerLogin,
                                                string.Format("Could not save settings for content class {0}", _parent));
                }
            }

            public bool IsUsingBorderDefinitionFromProjectSetting
            {
                get { return GetAttributeValue<bool>("usedefaultrangesettings"); }
                set { SetAttributeValue("usedefaultrangesettings", value); }
            }

            public bool IsUsingBordersToHighlightPages
            {
                get { return GetAttributeValue<bool>("showpagerange"); }
                set { SetAttributeValue("showpagerange", value); }
            }

            public IProject Project { get; private set; }

            private void InitAttributes()
            {
                CreateAttributes("bordercolor", "borderstyle", "borderwidth", "showpagerange", "usedefaultrangesettings");
            }
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
            public string Description
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
                               : _user = ContentClass.Project.Session.GetUser(GuidConvert(userGuid));
                }
            }

            public string Username
            {
                get { return XmlElement.GetAttributeValue("username"); }
            }
        }

        #endregion
    }

    public interface IContentClassEditableAreaSettings : IAttributeContainer, IProjectObject
    {
        string BorderColor { get; set; }
        string BorderStyle { get; set; }
        string BorderWidth { get; set; }
        void Commit();
        bool IsUsingBorderDefinitionFromProjectSetting { get; set; }
        bool IsUsingBordersToHighlightPages { get; set; }
    }

    public enum ContentClassVersionType
    {
        AutomaticallyCreated = 1,
        ManuallyCreate = 2,
        Temporary = 3
    }

    public interface IContentClassVersion : IRedDotObject, IProjectObject
    {
        IContentClass ContentClass { get; }
        ContentClassVersionType CreationType { get; }

        /// <summary>
        ///     Time the version was created
        /// </summary>
        DateTime Date { get; }

        /// <summary>
        ///     Description text
        /// </summary>
        string Description { get; }

        IContentClassFolder Folder { get; }
        IUser User { get; }
        string Username { get; }
    }
}