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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using erminas.SmartAPI.CMS.PageElements;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    /// <summary>
    ///   Wrapper for the RedDot Page object. If status changes occur, you have to call <see cref="PartialRedDotObject.Refresh" /> to see them reflected in the status field,
    /// </summary>
    public class Page : PartialRedDotObject, IPage
    {
        // Flags are defined in the RQL manual.

        #region PageFlags enum

        [Flags]
        public enum PageFlags
        {
            NotSet = 0,
            NotForBreadcrumb = 4,
            Workflow = 64,
            WaitingForTranslation = 1024,
            Unlinked = 8192,
            WaitingForCorrection = 131072,
            Draft = 262144,
            Released = 524288,
            BreadCrumbStaringPoint = 2097152,
            ContainsExternalReference = 8388608,
            OwnPageWaitingForRelease = 134217728,
            Locked = 268435456,
            Null = -1
        }

        #endregion

        #region PageReleaseStatus enum

        [Flags]
        public enum PageReleaseStatus
        {
            Draft = 65536,
            WorkFlow = 32768,
            Released = 4096,
            NotSet = 0,
            Rejected = 16384
        };

        #endregion

        #region PageState enum

        public enum PageState
        {
            NotSet = 0,
            IsReleased = 1,
            WaitsForRelease = 2,
            WaitsForCorrection = 3,
            SavedAsDraft = 4,
            NotAvailableInLanguage = 5,

            /// <summary>
            ///   From RQL docs: 6= Page has never been released in the selected language variant, in which it was created for the first time.
            /// </summary>
            NeverHasBeenReleasedInOriginalLanguage = 6,
            IsInRecycleBin = 10,
            WillBeArchived = 50,
            WillBeRemovedCompletely = 99
        }

        #endregion

        #region PageType enum

        public enum PageType
        {
            All = 0,
            Released = 1,
            Unlinked = 8192,
            Draft = 262144
        };

        #endregion

        private Guid _ccGuid;
        private DateTime _checkinDate;
        private ContentClass _contentClass;
        private string _headline;
        private int _id;
        private LanguageVariant _lang;
        private PageElement _mainLinkElement;
        private Guid _mainLinkGuid;
        private PageFlags _pageFlags = PageFlags.Null;

        private PageState _pageState;
        private Page _parentPage;
        private DateTime _releaseDate;
        private PageReleaseStatus _releaseStatus;

        public Page(Project project, XmlElement xmlElement) : base(xmlElement)
        {
            Project = project;
            Init(xmlElement);
            //reset isinitialized, because other information can still be retrieved
            //TODO find a clean solution for the various partial initialization states the page can be in
            IsInitialized = false;

            InitProperties();
        }


        public Page(Project project, Guid guid)
            : base(guid)
        {
            Project = project;
            InitProperties();
        }

        #region IPage Members

        /// <summary>
        ///   All keywords associated with this page.
        /// </summary>
        public NameIndexedRDList<Keyword> Keywords { get; private set; }

        /// <summary>
        ///   All link elements of this page.
        /// </summary>
        public NameIndexedRDList<PageElement> LinkElements { get; private set; }

        /// <summary>
        ///   ReleaseStatus of the page.
        /// </summary>
        public PageState Status
        {
            get { return LazyLoad(ref _pageState); }
            set { _pageState = value; }
        }

        /// <summary>
        ///   Language variant of this page instance.
        /// </summary>
        public LanguageVariant LanguageVariant
        {
            get { return LazyLoad(ref _lang); }
        }

        /// <summary>
        ///   Page filename. Same as Name.
        /// </summary>
        public string Filename
        {
            get { return Name; }
            set { Name = value; }
        }

        public Project Project { get; private set; }

        /// <summary>
        ///   Content class of the page
        /// </summary>
        public ContentClass ContentClass
        {
            get { return _contentClass ?? (_contentClass = Project.ContentClasses.GetByGuid(LazyLoad(ref _ccGuid))); }
        }

        /// <summary>
        ///   Headline of the page
        /// </summary>
        public string Headline
        {
            get { return LazyLoad(ref _headline); }
            set { _headline = value; }
        }

        /// <summary>
        ///   Date of the release.
        /// </summary>
        /// TODO last or initial release?
        public DateTime ReleaseDate
        {
            get { return LazyLoad(ref _releaseDate); }
        }

        public DateTime CheckinDate
        {
            get { return LazyLoad(ref _checkinDate); }
        }

        /// <summary>
        ///   The element this page has as mainlink.
        /// </summary>
        public PageElement MainLinkElement
        {
            get
            {
                if (_mainLinkElement != null)
                {
                    return _mainLinkElement;
                }
                if (LazyLoad(ref _mainLinkGuid).Equals(Guid.Empty))
                {
                    return null;
                }
                _mainLinkElement = PageElement.CreateElement(Project, _mainLinkGuid);
                return _mainLinkElement;
            }
        }

        /// <summary>
        ///   Parent page (the page containing this page's main link).
        /// </summary>
        public Page Parent
        {
            get { return _parentPage ?? (_parentPage = MainLinkElement != null ? MainLinkElement.Page : null); }
        }

        /// <summary>
        ///   Page Id.
        /// </summary>
        public int Id
        {
            get { return LazyLoad(ref _id); }
        }

        /// <summary>
        ///   The current release status of this page. Setting it will change it on the server.
        /// </summary>
        public PageReleaseStatus ReleaseStatus
        {
            get { return LazyLoad(ref _releaseStatus); }
            set
            {
                const string SET_RELEASE_STATUS = @"<PAGE action=""save"" guid=""{0}"" actionflag=""{1}""/>";
                XmlDocument xmlDoc =
                    Project.ExecuteRQL(string.Format(SET_RELEASE_STATUS, Guid.ToRQLString(), (int) value));
                XmlNodeList pageElements = xmlDoc.GetElementsByTagName("PAGE");
                if (pageElements.Count != 1 ||
                    (int.Parse(((XmlElement) pageElements[0]).GetAttributeValue("actionflag")) & (int) value) !=
                    (int) value)
                {
                    throw new Exception("Could not set release status to " + value);
                }
                IsInitialized = false;
                _releaseStatus = value;
                Status = PageState.NotSet;
            }
        }

        /// <summary>
        ///   Returns the Workflow this page adheres to.
        /// </summary>
        public Workflow Workflow
        {
            get { return new Workflow(Project, ((XmlElement) XmlNode.SelectSingleNode("descendant::WORKFLOW")).GetGuid()); }
        }

        /// <summary>
        ///   All content elements of this page.
        /// </summary>
        public NameIndexedRDList<PageElement> ContentElements { get; private set; }

        /// <summary>
        ///   Get a content/link element of this page with a specific name.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown, if no element with the expected name could be found.</exception>
        public PageElement this[string elementName]
        {
            get
            {
                PageElement result;
                return ContentElements.TryGetByName(elementName, out result)
                           ? result
                           : LinkElements.GetByName(elementName);
            }
        }

        /// <summary>
        ///   Remove a keyword from this page.
        /// </summary>
        public void DeleteKeyword(Keyword keyword)
        {
            const string DELETE_KEYWORD =
                @"<PROJECT><PAGE guid=""{0}"" action=""unlink""><KEYWORD guid=""{1}"" /></PAGE></PROJECT>";
            Project.ExecuteRQL(string.Format(DELETE_KEYWORD, Guid.ToRQLString(), keyword.Guid.ToRQLString()));
        }

        /// <summary>
        ///   Save changes to headline/filename to the server.
        /// </summary>
        public void Commit()
        {
            const string SAVE_PAGE = @"<PAGE action=""save"" guid=""{0}"" headline=""{1}"" name=""{2}"" />";
            XmlDocument xmlDoc =
                Project.ExecuteRQL(string.Format(SAVE_PAGE, Guid.ToRQLString(), HttpUtility.HtmlEncode(Headline),
                                                 HttpUtility.HtmlEncode(Filename)));
            if (xmlDoc.GetElementsByTagName("PAGE").Count != 1)
            {
                throw new Exception(string.Format("Could not save changes to page {0}", Guid.ToRQLString()));
            }
        }

        /// <summary>
        ///   Submit the page to workflow.
        /// </summary>
        public void SubmitToWorkflow()
        {
            ReleaseStatus = PageReleaseStatus.WorkFlow;
        }

        /// <summary>
        ///   Released the page.
        /// </summary>
        public void Release()
        {
            ReleaseStatus = PageReleaseStatus.Released;
        }

        /// <summary>
        ///   Disconnects the page from its parent (main link).
        /// </summary>
        public void DisconnectFromParent()
        {
            var link = MainLinkElement as ILinkElement;
            if (link != null)
            {
                link.DisconnectPage(this);
            }
        }

        /// <summary>
        ///   Push the page through workflow. Afterwards the (release) status of this page object no longer reflects the real status. To update it, call <see
        ///    cref="PartialRedDotObject.Refresh" /> . The object ist not automaticall updated to not incurr unnecessary overhead, if that information isn't needed anyway.
        /// </summary>
        public void SkipWorkflow()
        {
            const string SKIP_WORKFLOW =
                @"<PAGE action=""save"" guid=""{0}"" globalsave=""0"" skip=""1"" actionflag=""{1}"" />";

            Project.ExecuteRQL(string.Format(SKIP_WORKFLOW, Guid.ToRQLString(), (int) PageReleaseStatus.WorkFlow));
        }

        /// <summary>
        ///   Imitates the RedDot Undo page function. If the page has no previous state it is deleted. See the RedDot documentation for more details.
        /// </summary>
        public void Undo()
        {
            const string UNDO = @"<PAGE action=""rejecttempsaved"" guid=""{0}"" />";
            Project.ExecuteRQL(string.Format(UNDO, Guid.ToRQLString()));
        }

        /// <summary>
        ///   Move the page to the recycle bin, if page has been released yet. Otherwise the page will be deleted from CMS server completely.
        /// </summary>
        public void Delete()
        {
            const string DELETE_PAGE = @"<PAGE action=""delete"" guid=""{0}""/>";
            XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(DELETE_PAGE, Guid.ToRQLString()));
            if (xmlDoc.InnerText == "ok")
            {
                throw new Exception("Could not delete page {" + Guid.ToRQLString() + "}");
            }
            IsInitialized = false;
            _releaseStatus = PageReleaseStatus.NotSet;
            Status = PageState.NotSet;
        }

        /// <summary>
        ///   Delete the page from the recycle bin
        /// </summary>
        public void DeleteFromRecycleBin()
        {
            const string DELETE_FINALLY =
                @"<PAGE action=""deletefinally"" guid=""{0}"" alllanguages="""" forcedelete2910="""" forcedelete2911=""""/>";
            Project.ExecuteRQL(string.Format(DELETE_FINALLY, Guid.ToRQLString()));
            IsInitialized = false;
            _releaseStatus = PageReleaseStatus.NotSet;
            Status = PageState.NotSet;
        }

        /// <summary>
        ///   Restore page from recycle bin
        /// </summary>
        public void Restore()
        {
            const string RESTORE_PAGE =
                @"<PAGE action=""restore"" guid=""{0}"" alllanguages="""" forcedelete2910="""" forcedelete2911=""""/>";
            Project.ExecuteRQL(string.Format(RESTORE_PAGE, Guid.ToRQLString()));
            IsInitialized = false;
            ReleaseStatus = PageReleaseStatus.NotSet;
            Status = PageState.NotSet;
        }

        /// <summary>
        ///   Rejects the page from the current level of workflow.
        /// </summary>
        public void Reject()
        {
            ReleaseStatus = PageReleaseStatus.Rejected;
        }

        /// <summary>
        ///   Reset the page to draft status.
        /// </summary>
        public void ResetToDraft()
        {
            ReleaseStatus = PageReleaseStatus.Draft;
        }

        #endregion

        private void InitProperties()
        {
            LinkElements = new NameIndexedRDList<PageElement>(GetLinks, Caching.Enabled);
            ContentElements = new NameIndexedRDList<PageElement>(GetContentElements, Caching.Enabled);
            Keywords = new NameIndexedRDList<Keyword>(GetKeywords, Caching.Enabled);
        }

        private void Init(XmlElement xmlElement)
        {
            LoadXml(xmlElement);
        }

        private PageElement TryCreateElement(XmlElement xmlElement)
        {
            try
            {
                return PageElement.CreateElement(Project, xmlElement);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private List<PageElement> ToElementList(XmlNodeList elementNodes)
        {
            return (from XmlElement curNode in elementNodes
                    let element = TryCreateElement(curNode)
                    where element != null
                    select element).ToList();
        }


        protected override void LoadXml(XmlElement node)
        {
            //TODO schoenere loesung fuer partielles nachladen von pages wegen unterschiedlicher anfragen fuer unterschiedliche infos
            InitIfPresent(ref _id, "id", int.Parse);
            InitIfPresent(ref _lang, "languagevariantid", x => Project.LanguageVariants[x]);
            InitIfPresent(ref _parentPage, "parentguid", x => new Page(Project, GuidConvert(x)));
            InitIfPresent(ref _headline, "headline", HttpUtility.HtmlDecode);
            InitIfPresent(ref _pageFlags, "flags", x => (PageFlags) int.Parse(x));
            Name = HttpUtility.HtmlDecode(node.GetAttributeValue("name"));
            InitIfPresent(ref _ccGuid, "templateguid", Guid.Parse);
            InitIfPresent(ref _pageState, "status", x => (PageState) int.Parse(x));

            _releaseStatus = ReleaseStatusFromFlags();

            string xmlDate = XmlNode.GetAttributeValue("checkindate");

            _checkinDate = string.IsNullOrEmpty(xmlDate)
                               ? DateTime.MinValue
                               : DateTimeConvert(xmlDate);

            InitIfPresent(ref _mainLinkGuid, "mainlinkguid", GuidConvert);
            InitIfPresent(ref _releaseDate, "releasedate", DateTimeConvert);
        }

        private PageReleaseStatus ReleaseStatusFromFlags()
        {
            if (_pageFlags == PageFlags.Null)
            {
                return default(PageReleaseStatus);
            }
            if ((_pageFlags & PageFlags.Draft) == PageFlags.Draft)
            {
                return PageReleaseStatus.Draft;
            }
            if ((_pageFlags & PageFlags.Workflow) == PageFlags.Workflow)
            {
                return PageReleaseStatus.WorkFlow;
            }
            if ((_pageFlags & PageFlags.WaitingForCorrection) == PageFlags.WaitingForCorrection)
            {
                return PageReleaseStatus.Rejected;
            }

            return default(PageReleaseStatus);
        }

        protected override XmlElement RetrieveWholeObject()
        {
            const string REQUEST_PAGE = @"<PAGE action=""load"" guid=""{0}""/>";

            XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(REQUEST_PAGE, Guid.ToRQLString()));
            XmlNodeList pages = xmlDoc.GetElementsByTagName("PAGE");
            if (pages.Count != 1)
            {
                throw new Exception(string.Format("Could not load page with guid {0}", Guid.ToRQLString()));
            }
            return (XmlElement) pages[0];
        }

        private List<PageElement> GetLinks()
        {
            const string LOAD_LINKS = @"<PAGE guid=""{0}""><LINKS action=""load"" /></PAGE>";
            XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(LOAD_LINKS, Guid.ToRQLString()));
            return (from XmlElement curNode in xmlDoc.GetElementsByTagName("LINK")
                    select PageElement.CreateElement(Project, curNode)).ToList();
        }

        private List<PageElement> GetContentElements()
        {
            const string LOAD_PAGE_ELEMENTS =
                @"<PROJECT><PAGE guid=""{0}""><ELEMENTS action=""load""/></PAGE></PROJECT>";
            XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(LOAD_PAGE_ELEMENTS, Guid.ToRQLString()));
            return ToElementList(xmlDoc.GetElementsByTagName("ELEMENT"));
        }


        private List<Keyword> GetKeywords()
        {
            const string LOAD_KEYWORDS = @"<PROJECT><PAGE guid=""{0}""><KEYWORDS action=""load"" /></PAGE></PROJECT>";
            var xmlDoc = Project.ExecuteRQL(string.Format(LOAD_KEYWORDS, Guid.ToRQLString()));
            return
                (from XmlElement curNode in xmlDoc.GetElementsByTagName("KEYWORD") select new Keyword(Project, curNode))
                    .
                    ToList();
        }
    }
}