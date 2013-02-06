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
using System.Text;
using System.Web.Script.Serialization;
using System.Xml;
using erminas.SmartAPI.CMS.CCElements.Attributes;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    /// <summary>
    ///     Represents a content class in RedDot.
    /// </summary>
    public class ContentClass : PartialRedDotObject
    {
        private string _description;
        private CCEditableAreaSettings _editableAreaSettings;
        private Dictionary<string, CCElementList> _elements;
        private Folder _folder;
        private LanguageVariant _languageVariant;
        private Syllable _prefix;
        private Syllable _suffix;

        #region Properties

        /// <summary>
        ///     Description
        /// </summary>
        [ScriptIgnore]
        public string Description
        {
            get { return LazyLoad(ref _description); }
            set { _description = value; }
        }

        /// <summary>
        ///     Folder that contains the content class.
        /// </summary>
        [ScriptIgnore]
        public Folder Folder
        {
            get { return LazyLoad(ref _folder); }
        }

        [ScriptIgnore]
        public LanguageVariant LanguageVariant
        {
            get { return LazyLoad(ref _languageVariant); }
        }

        [ScriptIgnore]
        public RDList<IPageDefinition> PageDefinitions { get; private set; }

        /// <summary>
        ///     Default prefix for pages.
        /// </summary>
        [ScriptIgnore]
        public Syllable Prefix
        {
            get { return LazyLoad(ref _prefix); }
        }

        [ScriptIgnore]
        public Project Project { get; private set; }

        /// <summary>
        ///     Default suffix for pages.
        /// </summary>
        [ScriptIgnore]
        public Syllable Suffix
        {
            get { return LazyLoad(ref _suffix); }
        }

        #endregion

        public ContentClass(Project project, XmlElement xmlElement) : base(xmlElement)
        {
            Project = project;

            Init();
            LoadXml();
            //TODO sharedrights = 1 bei ccs von anderen projekten
        }

        public ContentClass(Project project, Guid guid) : base(guid)
        {
            Project = project;
            Init();
            //TODO sharedrights = 1 bei ccs von anderen projekten
        }

        /// <summary>
        ///     Assign project variants to template variants
        /// </summary>
        public void AssignProjectVariants(Dictionary<TemplateVariant, ProjectVariant> assignments)
        {
            const string ASSIGN_PROJECT_VARIANT =
                @"<TEMPLATE guid=""{0}""><TEMPLATEVARIANTS>{1}</TEMPLATEVARIANTS></TEMPLATE>";
            const string SINGLE_ASSIGNMENT =
                @"<TEMPLATEVARIANT guid=""{0}""><PROJECTVARIANTS action=""assign""><PROJECTVARIANT donotgenerate=""0"" donotusetidy=""0"" guid=""{1}"" /></PROJECTVARIANTS></TEMPLATEVARIANT>";

            var builder = new StringBuilder();
            foreach (var curEntry in assignments)
            {
                builder.Append(SINGLE_ASSIGNMENT.RQLFormat(curEntry.Key, curEntry.Value));
            }

            Project.ExecuteRQL(ASSIGN_PROJECT_VARIANT.RQLFormat(this, builder), Project.RqlType.SessionKeyInProject);
        }

        /// <summary>
        ///     Commit changes on attributes to the server.
        /// </summary>
        public void CommitAttributes()
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

            Project.ExecuteRQL(GetSaveString(templateElement), Project.RqlType.SessionKeyInProject);
        }

        /// <summary>
        ///     Copy selected elements from this content class to another target content class.
        /// </summary>
        /// <param name="targetCC"> HtmlTarget content class to copy the elements to </param>
        /// <param name="elementNames"> Names of the elements to copy </param>
        public void CopyElementsToContentClass(ContentClass targetCC, params string[] elementNames)
        {
            if (elementNames == null || elementNames.Length == 0)
            {
                return;
            }

            var createdElements = new Dictionary<string, CCElement>();
            using (new LanguageContext(Project))
            {
                foreach (LanguageVariant languageVariant in Project.LanguageVariants)
                {
                    LanguageVariant targetLanguageVariant = targetCC.Project.LanguageVariants[languageVariant.Language];
                    foreach (string curElementName in elementNames)
                    {
                        CCElement curTargetCcElement;
                        languageVariant.Select();
                        CCElement curSourceCcElement = this[languageVariant.Language, curElementName];
                        if (createdElements.TryGetValue(curElementName, out curTargetCcElement))
                        {
                            CCElement tmpTargetCcElement = CCElement.CreateElement(targetCC,
                                                                                   curTargetCcElement.XmlElement);
                            tmpTargetCcElement.AssignAttributes(curSourceCcElement.Attributes);
                            targetLanguageVariant.Select();
                            tmpTargetCcElement.Commit();
                        }
                        else
                        {
                            targetLanguageVariant.Select();
                            curTargetCcElement = curSourceCcElement.CopyToContentClass(targetCC);
                            createdElements.Add(curElementName, curTargetCcElement);
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
        public void CopyToProject(Project project, Guid targetFolderGuid)
        {
            ContentClass targetCC = CreateCopyInProject(project, targetFolderGuid);

            CopyAttributesToCC(targetCC);

            CopyElementsToCC(targetCC);

            CopyPreassignedKeywordsToCC(targetCC);
        }

        /// <summary>
        ///     Versioning information for the latest version of the content class.
        /// </summary>
        [ScriptIgnore]
        public CCVersion CurrentVersion
        {
            get { return Versions.FirstOrDefault(); }
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
                throw new Exception(msg);
            }
        }

        /// <summary>
        ///     Delete a template from this content class
        /// </summary>
        /// <param name="guid"> Guid of the template variant to delete </param>
        public void DeleteTemplateVariant(Guid guid)
        {
            const string DELETE_TEMPLATE =
                @"<TEMPLATE><TEMPLATEVARIANTS><TEMPLATEVARIANT action=""delete"" guid=""{0}""/></TEMPLATEVARIANTS></TEMPLATE>";
            try
            {
                Project.ExecuteRQL(string.Format(DELETE_TEMPLATE, guid.ToRQLString()),
                                   Project.RqlType.SessionKeyInProject);
            } catch (RQLException e)
            {
                throw new Exception("Could not delete template variant from content class: " + e.Message);
            }
        }

        /// <summary>
        ///     EditableAreaSettings of the content class The settings get cached. To refresh the settings call <see cref="Refresh" />
        /// </summary>
        [ScriptIgnore]
        public CCEditableAreaSettings EditableAreaSettings
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
                        throw new Exception("could not load settings for content class '" + Name + "' (" +
                                            Guid.ToRQLString() + ")");
                    }
                    _editableAreaSettings = new CCEditableAreaSettings(this, node);
                }
                return _editableAreaSettings;
            }
            set { _editableAreaSettings = value; }
        }

        /// <summary>
        ///     Mapping of Language Variant Id => Elements of content class. There is an extra instance of every element for every language variant. This dictionary is cached. To refresh the contents call
        ///     <see
        ///         cref="Refresh" />
        ///     .
        /// </summary>
        /// <example>
        ///     If there are are two language variants (ENG and GER) and two elements (Text
        /// </example>
        [ScriptIgnore]
        public Dictionary<string, CCElementList> Elements
        {
            get
            {
                if (_elements == null)
                {
                    _elements = new Dictionary<string, CCElementList>();
                    using (new LanguageContext(Project))
                    {
                        foreach (LanguageVariant curLanguage in Project.LanguageVariants)
                        {
                            curLanguage.Select();
                            const string LOAD_CC_ELEMENTS =
                                @"<PROJECT><TEMPLATE action=""load"" guid=""{0}""><ELEMENTS childnodesasattributes=""1"" action=""load""/><TEMPLATEVARIANTS action=""list""/></TEMPLATE></PROJECT>";
                            XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(LOAD_CC_ELEMENTS, Guid.ToRQLString()));
                            var xmlNode = (XmlElement) xmlDoc.GetElementsByTagName("ELEMENTS")[0];
                            var curElements = new CCElementList(this, xmlNode);
                            _elements.Add(curLanguage.Language, curElements);
                        }
                    }
                }
                return _elements;
            }
        }

        /// <summary>
        ///     Get all assignments of templates to project variants of this content class.
        /// </summary>
        public ILookup<ProjectVariant, TemplateVariant> GetProjectVariantAssignments()
        {
            const string PROJECT_VARIANT_ASSIGNMENT =
                @"<TEMPLATE guid=""{0}"" ><TEMPLATEVARIANTS withstylesheets=""1"" action=""projectvariantslist"" /></TEMPLATE>";
            bool cachingStatus = Project.ProjectVariants.IsCachingEnabled;
            try
            {
                XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(PROJECT_VARIANT_ASSIGNMENT, Guid.ToRQLString()),
                                                        Project.RqlType.SessionKeyInProject);
                Project.ProjectVariants.IsCachingEnabled = true;
                return (from XmlElement curElement in xmlDoc.GetElementsByTagName("TEMPLATEVARIANT")
                        select
                            new
                                {
                                    projectVariant =
                            Project.ProjectVariants.First(x => x.Guid == curElement.GetGuid("projectvariantguid")),
                                    templateVariant = new TemplateVariant(this, curElement.GetGuid())
                                }).ToLookup(
                                    key => key.projectVariant, value => value.templateVariant);
            } catch (RQLException e)
            {
                throw new Exception("Could not get project variant assignments", e);
            }
            finally
            {
                Project.ProjectVariants.IsCachingEnabled = cachingStatus;
            }
        }

        [VersionIsGreaterThanOrEqual(9, 0, 0, 41, VersionName = "Version 9 Hotfix 5")]
        public bool IsChangingHeadlineEffectiveForAllLanguageVariants
        {
            get
            {
                Project.Session.EnsureVersion();
                EnsureInitialization();
                return ((BoolXmlNodeAttribute) GetAttribute("adoptheadlinetoalllanguages")).Value;
            }
            set
            {
                Project.Session.EnsureVersion();
                EnsureInitialization(); //TODO eigentlich muessen nur die attribute fuers schreiben vorhanden sein
                ((BoolXmlNodeAttribute) GetAttribute("adoptheadlinetoalllanguages")).Value = value;
            }
        }

        public bool IsKeywordRequired
        {
            get
            {
                EnsureInitialization();
                return ((BoolXmlNodeAttribute) GetAttribute("keywordrequired")).Value;
            }
            set
            {
                EnsureInitialization();
                ((BoolXmlNodeAttribute) GetAttribute("keywordrequired")).Value = value;
            }
        }

        /// <summary>
        ///     Get an element by language/element name
        /// </summary>
        /// <param name="language"> Language Id of language variant </param>
        /// <param name="elementName"> Name of the element </param>
        public CCElement this[string language, string elementName]
        {
            get { return Elements[language][elementName]; }
        }

        /// <summary>
        ///     List of the preassigned keywords of this content class, indexed by name. This list is cached by default.
        /// </summary>
        public NameIndexedRDList<Keyword> PreassignedKeywords { get; private set; }

        public override void Refresh()
        {
            base.Refresh();
            _editableAreaSettings = null;
            _elements = null;
        }

        /// <summary>
        ///     Remove an element from this content class
        /// </summary>
        /// <param name="elementName"> Name of the element to remove </param>
        public void RemoveElement(string elementName)
        {
            foreach (CCElementList curElements in Elements.Values)
            {
                CCElement ccElementToRemove = curElements.Elements.Find(x => x.Name == elementName);
                if (ccElementToRemove == null)
                {
                    throw new ArgumentException("Element '" + elementName + "' could not be found in content class '" +
                                                Name + "'");
                }
                RemoveElement(ccElementToRemove.Guid);
                return;
            }
        }

        public Category RequiredKeywordCategory
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
        ///     Set the preassigned keywords for this content class on the server.
        ///     PreassignedKeywords contain the updated keywords afterwards.
        ///     Set to an empty IEnumerable or null, to remove all preassigned keywords.
        /// </summary>
        public void SetPreassignedKeywords(IEnumerable<Keyword> keywords)
        {
            if (keywords == null)
            {
                keywords = new List<Keyword>();
            }
            using (new CachingContext<Keyword>(PreassignedKeywords, Caching.Enabled))
            {
                List<Keyword> keywordsToAdd = keywords.Except(PreassignedKeywords).ToList();
                List<Keyword> keywordsToRemove = PreassignedKeywords.Except(keywords).ToList();

                if (!keywordsToRemove.Any() && !keywordsToAdd.Any())
                {
                    return;
                }

                PreassignedKeywords.InvalidateCache();

                AssignKeywords(keywordsToAdd);

                UnlinkKeywords(keywordsToRemove);
            }
        }

        public NameIndexedRDList<TemplateVariant> TemplateVariants { get; private set; }

        public override string ToString()
        {
            return Name + " (" + Guid.ToRQLString() + ")";
        }

        /// <summary>
        ///     Versioning information for the content class. List is cached by default.
        /// </summary>
        [ScriptIgnore]
        public IRDList<CCVersion> Versions { get; private set; }

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

        private void AddProjectVariants(Project project, XmlElement template)
        {
            XmlElement projectVariants = template.AddElement("PROJECTVARIANTS");
            projectVariants.AddAttribute("action", "assign");
            foreach (ProjectVariant curVariant in Project.ProjectVariants)
            {
                XmlElement projectVariant = projectVariants.AddElement("PROJECTVARIANT");
                ProjectVariant otherVariant;
                if (!project.ProjectVariants.TryGetByName(curVariant.Name, out otherVariant))
                {
                    throw new Exception(string.Format("Could not find project variant {0} in project {1}",
                                                      curVariant.Name, project.Name));
                }
                projectVariant.AddAttribute("guid", otherVariant.Guid.ToRQLString());
            }
        }

        private void AddTemplateDescriptions(Project project, XmlElement template)
        {
            XmlElement templateDescriptions = template.AddElement("TEMPLATEDESCRIPTIONS");
            foreach (LanguageVariant languageVariant in project.LanguageVariants)
            {
                XmlElement templateDescription = templateDescriptions.AddElement("TEMPLATEDESCRIPTION");
                templateDescription.AddAttribute("dialoglanguageid", languageVariant.Language);
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
            foreach (TemplateVariant curTemplateVariant in TemplateVariants)
            {
                XmlElement templateVariant = templateVariants.AddElement("TEMPLATEVARIANT");
                templateVariant.AddAttribute("name", curTemplateVariant.Name);
                // ReSharper disable PossibleNullReferenceException
                XmlText textNode = ownerDocument.CreateTextNode(curTemplateVariant.Data);
                // ReSharper restore PossibleNullReferenceException
                templateVariant.AppendChild(textNode);
            }
        }

        private void AssignKeywords(IEnumerable<Keyword> keywordsToAdd)
        {
            foreach (Keyword curKeyword in keywordsToAdd)
            {
                const string ASSIGN_KEYWORD =
                    @"<TEMPLATE action=""assign"" guid=""{0}""><CATEGORY guid=""{1}""/><KEYWORD guid=""{2}""/></TEMPLATE>";

                XmlDocument xmlDoc =
                    Project.ExecuteRQL(
                        string.Format(ASSIGN_KEYWORD, Guid.ToRQLString(), curKeyword.Category.Guid.ToRQLString(),
                                      curKeyword.Guid.ToRQLString()), Project.RqlType.SessionKeyInProject);

                if (!WasKeywordActionSuccessful(xmlDoc))
                {
                    throw new Exception(string.Format("Could not assign keyword {0} to content class {1}",
                                                      curKeyword.Name, Name));
                }
            }
        }

        private void CopyAttributesToCC(ContentClass targetCC)
        {
            foreach (IRDAttribute curAttribute in EditableAreaSettings.Attributes)
            {
                targetCC.EditableAreaSettings.GetAttribute(curAttribute.Name).Assign(curAttribute);
            }
            targetCC.EditableAreaSettings.Commit();
            targetCC.Refresh();
            foreach (IRDAttribute curAttribute in Attributes)
            {
                try
                {
                    targetCC.GetAttribute(curAttribute.Name).Assign(curAttribute);
                } catch (Exception e)
                {
                    throw new Exception(
                        string.Format("Unable to assign attribute {0} in content class {1} of project {2}",
                                      curAttribute.Name, Name, Project.Name), e);
                }
            }
            targetCC.CommitAttributes();
        }

        private void CopyElementsToCC(ContentClass targetCC)
        {
            Dictionary<string, CCElementList>.ValueCollection.Enumerator it = Elements.Values.GetEnumerator();
            if (!it.MoveNext())
            {
                return;
            }

            string[] elementNames = (from curElement in it.Current select curElement.Name).ToArray();
            CopyElementsToContentClass(targetCC, elementNames);
        }

        private void CopyPreassignedKeywordsToCC(ContentClass targetCC)
        {
            try
            {
                List<Keyword> keywordsToAssign =
                    PreassignedKeywords.Select(
                        x => targetCC.Project.Categories.GetByName(x.Category.Name).Keywords.GetByName(x.Name)).ToList();
                targetCC.SetPreassignedKeywords(keywordsToAssign);
            } catch (Exception e)
            {
                throw new Exception(string.Format("Could not copy preassigned keywords for content class {0}", Name), e);
            }
        }

        private void CreateBaseAttributes()
        {
            CreateAttributes("approverequired", "description", "framesetafterlist", "name", "praefixguid", "suffixguid",
                             "adoptheadlinetoalllanguages", "keywordrequired", "requiredcategory");
        }

        private ContentClass CreateContentClass(Project project, XmlElement template)
        {
            XmlDocument creationResultNode = project.ExecuteRQL(template.NodeToString());
            XmlNode guidTextNode = creationResultNode.FirstChild;
            Guid newCCGuid;
            if (guidTextNode == null || guidTextNode.NodeType != XmlNodeType.Element || guidTextNode.FirstChild == null ||
                !Guid.TryParse(guidTextNode.FirstChild.Value.Trim(), out newCCGuid))
            {
                throw new Exception("could not create content class '" + Name + "'");
            }

            var targetCC = new ContentClass(project, newCCGuid);
            return targetCC;
        }

        private ContentClass CreateCopyInProject(Project project, Guid targetFolderGuid)
        {
            ContentClassFolder folder = project.ContentClassFolders.GetByGuid(targetFolderGuid);
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

        private static XmlElement CreateTemplateXmlElement(XmlDocument xmlDoc, ContentClassFolder folder)
        {
            XmlElement template = xmlDoc.AddElement("TEMPLATE");
            template.AddAttribute("action", "addnew");
            template.AddAttribute("folderguid", folder.Guid.ToRQLString());
            return template;
        }

        private List<IPageDefinition> GetPageDefinitions()
        {
            const string LOAD_PREASSIGNMENT = @"<TEMPLATELIST action=""load"" withpagedefinitions=""1""/>";

            var xmlDoc = Project.ExecuteRQL(LOAD_PREASSIGNMENT);
            const string PAGE_DEFINITIONS_XPATH = "//TEMPLATE[@guid='{0}']/PAGEDEFINITIONS/PAGEDEFINITION";
            var pageDefs = xmlDoc.SelectNodes(PAGE_DEFINITIONS_XPATH.RQLFormat(this));

            return
                (from XmlElement curPageDef in pageDefs select new PageDefinition(this, curPageDef))
                    .Cast<IPageDefinition>().ToList();
        }

        private List<Keyword> GetPreassignedKeywords()
        {
            const string LOAD_PREASSIGNED_KEYWORDS = @"<TEMPLATE guid=""{0}""><KEYWORDS action=""load""/></TEMPLATE>";
            XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(LOAD_PREASSIGNED_KEYWORDS, Guid.ToRQLString()),
                                                    Project.RqlType.SessionKeyInProject);

            IEnumerable<Keyword> keywords = new List<Keyword>();
            foreach (XmlElement node in xmlDoc.GetElementsByTagName("CATEGORY"))
            {
                var curCategory = new Category(Project, node.GetGuid()) {Name = node.GetAttributeValue("value")};
                IEnumerable<Keyword> newKeywords =
                    from XmlElement curKeywordNode in xmlDoc.GetElementsByTagName("KEYWORD")
                    select
                        new Keyword(Project, curKeywordNode.GetGuid())
                            {
                                Name = curKeywordNode.GetAttributeValue("value"),
                                Category = curCategory
                            };
                keywords = keywords.Union(newKeywords);
            }
            return keywords.ToList();
        }

        private List<TemplateVariant> GetTemplateVariants()
        {
            const string LIST_CC_TEMPLATES =
                @"<PROJECT><TEMPLATE guid=""{0}""><TEMPLATEVARIANTS action=""list"" withstylesheets=""0""/></TEMPLATE></PROJECT>";
            XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(LIST_CC_TEMPLATES, Guid.ToRQLString()));
            var variants = xmlDoc.GetElementsByTagName("TEMPLATEVARIANT");
            return (from XmlElement curVariant in variants select new TemplateVariant(this, curVariant)).ToList();
        }

        private List<CCVersion> GetVersions()
        {
            const string LIST_VERSIONS =
                @"<PROJECT><TEMPLATE guid=""{0}""><ARCHIVE action=""list""/></TEMPLATE></PROJECT>";
            XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(LIST_VERSIONS, Guid.ToRQLString()));
            XmlNodeList versionNodes = xmlDoc.GetElementsByTagName("VERSION");
            return
                (from XmlElement curVersion in versionNodes
                 let cc = new CCVersion(this, curVersion)
                 orderby cc.Date descending
                 select cc).ToList();
        }

        private void Init()
        {
            Versions = new RDList<CCVersion>(GetVersions, Caching.Enabled);
            PreassignedKeywords = new NameIndexedRDList<Keyword>(GetPreassignedKeywords, Caching.Enabled);
            PageDefinitions = new RDList<IPageDefinition>(GetPageDefinitions, Caching.Enabled);
            TemplateVariants = new NameIndexedRDList<TemplateVariant>(GetTemplateVariants, Caching.Enabled);
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

            InitIfPresent(ref _languageVariant, "languagevariantid", x => Project.LanguageVariants[x]);
            InitIfPresent(ref _folder, "folderguid", x => new Folder(Project, GuidConvert(x)));
            InitIfPresent(ref _prefix, "praefixguid", x => new Syllable(Project, GuidConvert(x)));
            InitIfPresent(ref _suffix, "suffixguid", x => new Syllable(Project, GuidConvert(x)));
            InitIfPresent(ref _description, "description", x => x);
        }

        private void RemoveElement(Guid guid)
        {
            const string REMOVE_ELEMENT = @"<TEMPLATE><ELEMENT action=""delete"" guid=""{0}""/></TEMPLATE>";
            XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(REMOVE_ELEMENT, guid.ToRQLString()),
                                                    Project.RqlType.SessionKeyInProject);
            if (!xmlDoc.InnerText.Contains("ok"))
            {
                //TODO richtige exception
                throw new Exception("could not remove element: " + guid.ToRQLString());
            }
            foreach (CCElementList curList in Elements.Values)
            {
                curList.Elements.RemoveAll(x => x.Guid.Equals(guid));
            }
        }

        private void UnlinkKeywords(IEnumerable<Keyword> keywordsToRemove)
        {
            foreach (Keyword curKeyword in keywordsToRemove)
            {
                const string REMOVE_KEYWORD =
                    @"<TEMPLATE action=""unlink"" guid=""{0}""><KEYWORD guid=""{1}""/></TEMPLATE>";

                XmlDocument xmlDoc =
                    Project.ExecuteRQL(
                        string.Format(REMOVE_KEYWORD, Guid.ToRQLString(), curKeyword.Guid.ToRQLString()),
                        Project.RqlType.SessionKeyInProject);

                if (!WasKeywordActionSuccessful(xmlDoc))
                {
                    throw new Exception(string.Format("Could not unlink keyword {0} from content class {1}",
                                                      curKeyword.Name, Name));
                }
            }
        }

        private static bool WasKeywordActionSuccessful(XmlNode node)
        {
            return node.InnerText.Contains("ok");
        }

        #region Nested type: CCEditableAreaSettings

        /// <summary>
        ///     Represents editable area configuration of a content class.
        /// </summary>
        public class CCEditableAreaSettings : AbstractAttributeContainer
        {
            private readonly ContentClass _parent;

            public CCEditableAreaSettings(ContentClass parent, XmlElement xmlElement) : base(xmlElement)
            {
                Debug.Assert(xmlElement != null);
                _parent = parent;
                InitAttributes();
            }

            public string BorderColor
            {
                get { return ((StringXmlNodeAttribute) GetAttribute("bordercolor")).Value; }
                set { ((StringXmlNodeAttribute) GetAttribute("bordercolor")).Value = value; }
            }

            public string BorderStyle
            {
                get { return ((StringXmlNodeAttribute) GetAttribute("borderstyle")).Value; }
                set { ((StringXmlNodeAttribute) GetAttribute("borderstyle")).Value = value; }
            }

            public string BorderWidth
            {
                get { return ((StringXmlNodeAttribute) GetAttribute("borderwidth")).Value; }
                set { ((StringXmlNodeAttribute) GetAttribute("borderwidth")).Value = value; }
            }

            public void Commit()
            {
                const string SAVE_CC_SETTINGS = @"<TEMPLATE guid=""{0}"">{1}</TEMPLATE>";
                XmlDocument result =
                    _parent.Project.ExecuteRQL(
                        string.Format(SAVE_CC_SETTINGS, _parent.Guid.ToRQLString(),
                                      GetSaveString((XmlElement) XmlElement.Clone())),
                        Project.RqlType.SessionKeyInProject);

                if (result.GetElementsByTagName("SETTINGS").Count != 1)
                {
                    throw new Exception("could not save settings for content class '" + _parent.Name + "' (" +
                                        _parent.Guid.ToRQLString() + ")");
                }
            }

            public bool IsUsingBorderDefinitionFromProjectSetting
            {
                get { return ((BoolXmlNodeAttribute) GetAttribute("usedefaultrangesettings")).Value; }
                set { ((BoolXmlNodeAttribute) GetAttribute("usedefaultrangesettings")).Value = value; }
            }

            public bool IsUsingBordersToHighlightPages
            {
                get { return ((BoolXmlNodeAttribute) GetAttribute("showpagerange")).Value; }
                set { ((BoolXmlNodeAttribute) GetAttribute("showpagerange")).Value = value; }
            }

            private void InitAttributes()
            {
                CreateAttributes("bordercolor", "borderstyle", "borderwidth", "showpagerange", "usedefaultrangesettings");
            }
        }

        #endregion

        #region Nested type: CCVersion

        /// <summary>
        ///     Represents version information on a specific content class version.
        /// </summary>
        public class CCVersion : RedDotObject
        {
            #region Type enum

            public enum Type
            {
                AutomaticallyCreated = 1,
                ManuallyCreate = 2,
                Temporary = 3
            }

            #endregion

            private DateTime? _date;
            private Folder _folder;
            private User _user;

            public CCVersion(ContentClass parent, XmlElement xmlElement) : base(xmlElement)
            {
                ContentClass = parent;
            }

            public ContentClass ContentClass { get; private set; }

            public Type CreationType
            {
                get { return (Type) Enum.Parse(typeof (Type), XmlElement.GetAttributeValue("type")); }
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

            public Folder Folder
            {
                get
                {
                    return _folder ??
                           (_folder =
                            new Folder(ContentClass.Project, GuidConvert(XmlElement.GetAttributeValue("folderguid"))));
                }
            }

            public User User
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
}