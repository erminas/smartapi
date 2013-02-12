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
using System.Web;
using System.Xml;
using erminas.SmartAPI.CMS.Administration;
using erminas.SmartAPI.CMS.Project.ContentClasses;
using erminas.SmartAPI.CMS.Project.Keywords;
using erminas.SmartAPI.CMS.Project.Pages.Elements;
using erminas.SmartAPI.CMS.Project.Workflows;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Pages
{
    /// <summary>
    ///     Wrapper for the RedDot Page object. If status changes occur, you have to call
    ///     <see
    ///         cref="PartialRedDotObject.Refresh" />
    ///     to see them reflected in the status field,
    /// </summary>
    public class Page : PartialRedDotObject, IPage
    {
        /// <summary>
        ///     Default value for <see cref="MaxWaitForDeletion" /> (1.25s).
        /// </summary>
        public static readonly TimeSpan DEFAULT_WAIT_FOR_DELETION = new TimeSpan(0, 0, 0, 1, 250);

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
            ///     From RQL docs: 6= Page has never been released in the selected language variant, in which it was created for the first time.
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

        // Flags are defined in the RQL manual.

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

        static Page()
        {
            MaxWaitForDeletion = DEFAULT_WAIT_FOR_DELETION;
        }

        internal Page(Project project, XmlElement xmlElement) : base(xmlElement)
        {
            Project = project;
            LoadXml();
            //reset isinitialized, because other information can still be retrieved
            //TODO find a clean solution for the various partial initialization states the page can be in
            IsInitialized = false;

            InitProperties();
        }

        public Page(Project project, Guid guid, LanguageVariant languageVariant) : base(guid)
        {
            Project = project;
            _lang = languageVariant;
            InitProperties();
        }

        public void AddKeyword(Keyword keyword)
        {
            if (Keywords.ContainsGuid(keyword.Guid))
            {
                return;
            }

            const string ADD_KEYWORD =
                @"<PAGE guid=""{0}"" action=""assign""><KEYWORDS><KEYWORD guid=""{1}"" changed=""1"" /></KEYWORDS></PAGE>";
            Project.ExecuteRQL(string.Format(ADD_KEYWORD, Guid.ToRQLString(), keyword.Guid.ToRQLString()),
                               Project.RqlType.SessionKeyInProject);
            //server sends empty reply

            Keywords.InvalidateCache();
        }

        /// <summary>
        ///     Move the page to the recycle bin, if page has been released yet. Otherwise the page will be deleted from CMS server completely.
        ///     If you want to make sure it is completly removed from the server, even if has been released,
        ///     use <see cref="DeleteIrrevocably" /> instead of calling this method and <see cref="DeleteFromRecycleBin" />.
        ///     Throws a PageDeletionException, if references still point to elements of this page or an element is assigned as target container to a link.
        /// </summary>
        /// <exception cref="PageDeletionException">Thrown, if page could not be deleted.</exception>
        public void DeleteIfNotReferenced()
        {
            DeleteImpl(forceDeletion: false);
        }

        /// <summary>
        ///     Delete the page Independant of the state the page is in (e.g released or already in recycle bin), the page will be removed from CMS and cannot be restored.
        ///     Forces the deletion, even if references still point to elements of this page or an element is assigned as target container to a link.
        ///     If the page was released, it will be moved to the recycle bin first.
        ///     Removing it from there leads to a race condition on the server: the page can be already marked as being in the recycle bin, but a call to remove it from there can still fail for some time.
        ///     For this reason a we try to delete it until the operation is successful or a timeout is reached. The timeout can be set with
        ///     <see
        ///         cref="MaxWaitForDeletion" />
        ///     If you want to delete multiple pages a call only to Delete() and a collective removal from the recycle bin afterwards is faster than a call
        ///     to DeleteIrrevocably on every single page.
        /// </summary>
        /// <exception cref="PageDeletionException">Thrown, if page could not be deleted.</exception>
        public void DeleteIrrevocably()
        {
            var start = DateTime.Now;

            if (!Exists)
            {
                return;
            }

            //status gets loaded lazily, and we need need to know status at this point (before Delete() gets called), so we store it in a local var.
            PageState curStatus = Status;

            bool isAlreadyDeleted = curStatus == PageState.IsInRecycleBin;
            if (!isAlreadyDeleted)
            {
                Delete();

                //pages in draft status don't get moved to recycle bin, but deleted completely from a normal delete call
                if (curStatus == PageState.SavedAsDraft)
                {
                    return;
                }

                //deletion is an asynchronous process on the server, so we have to wait until it is done
                WaitUntilPageIsInRecycleBin(MaxWaitForDeletion);
            }

            var alreadyElapsed = DateTime.Now - start;
            var maxWaitForDeletionFromRecycleBin = MaxWaitForDeletion - alreadyElapsed;

            WaitForDeletionFromRecycleBin(maxWaitForDeletionFromRecycleBin);
        }

        public override bool Equals(object other)
        {
            if (!(other is IPage) || !base.Equals(other))
            {
                return false;
            }

            return LanguageVariant.Equals(((IPage) other).LanguageVariant);
        }

        public bool Exists
        {
            get { return Status != PageState.WillBeRemovedCompletely && Status != PageState.NotSet; }
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode() + 3*LanguageVariant.GetHashCode();
        }

        /// <summary>
        ///     Maximum time span to wait for a successful deletion of a page, before it can be removed from the recycle bin.
        ///     This is needed because of a race condition with the red dot server where it wrongly shows the page to be in the recycle bin.
        ///     It is set to a default value of 1.25s which proved reliable in internal tests on our servers.
        /// </summary>
        public static TimeSpan MaxWaitForDeletion { get; set; }

        public void SetKeywords(List<Keyword> newKeywords)
        {
            const string SET_KEYWORDS = @"<PAGE guid=""{0}"" action=""assign""><KEYWORDS>{1}</KEYWORDS></PAGE>";
            const string REMOVE_SINGLE_KEYWORD = @"<KEYWORD guid=""{0}"" delete=""1"" changed=""1"" />";
            const string ADD_SINGLE_KEYWORD = @"<KEYWORD guid=""{0}"" changed=""1"" />";

            string toRemove = Keywords.Except(newKeywords)
                                      .Aggregate("",
                                                 (x, y) =>
                                                 x + string.Format(REMOVE_SINGLE_KEYWORD, y.Guid.ToRQLString()));

            string toAdd = newKeywords.Except(Keywords)
                                      .Aggregate("",
                                                 (x, y) => x + string.Format(ADD_SINGLE_KEYWORD, y.Guid.ToRQLString()));

            if (string.IsNullOrEmpty(toRemove) && string.IsNullOrEmpty(toAdd))
            {
                return;
            }

            Project.ExecuteRQL(string.Format(SET_KEYWORDS, Guid.ToRQLString(), toRemove + toAdd),
                               Project.RqlType.SessionKeyInProject);

            Keywords.InvalidateCache();
        }

        public override string ToString()
        {
            return string.Format("{0} (Id: {1} Guid: {2} Language: {3})", Headline, Id, Guid.ToRQLString(),
                                 LanguageVariant.Language);
        }

        protected override void LoadWholeObject()
        {
            LoadXml();
        }

        protected override XmlElement RetrieveWholeObject()
        {
            using (new LanguageContext(LanguageVariant))
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
        }

        private void CheckReleaseStatusSettingSuccess(PageReleaseStatus value, XmlDocument xmlDoc)
        {
            XmlNodeList pageElements = xmlDoc.GetElementsByTagName("PAGE");
            if (pageElements.Count != 1)
            {
                var missingKeywords = xmlDoc.SelectNodes("/IODATA/EMPTYELEMENTS/ELEMENT[@type='1002']");
                if (missingKeywords != null && missingKeywords.Count != 0)
                {
                    throw new MissingKeywordsException(this, GetNames(missingKeywords));
                }

                var missingElements = xmlDoc.SelectNodes("/IODATA/EMPTYELEMENTS/ELEMENT");
                if (missingElements != null && missingElements.Count > 0)
                {
                    throw new MissingElementValueException(this, GetNames(missingElements));
                }

                throw new PageStatusException(this, "Could not set release status to " + value);
            }
            var element = (XmlElement) pageElements[0];
            var flag = (PageReleaseStatus) element.GetIntAttributeValue("actionflag").GetValueOrDefault();

            if (!flag.HasFlag(value) && !IsReleasedIntoWorkflow(value, flag))
            {
                throw new PageStatusException(this, "Could not set release status to " + value);
            }
        }

        private void DeleteImpl(bool forceDeletion)
        {
            try
            {
                const string DELETE_PAGE =
                    @"<PAGE action=""delete"" guid=""{0}"" forcedelete2910=""{1}"" forcedelete2911=""{1}""><LANGUAGEVARIANTS><LANGUAGEVARIANT language=""{2}""/></LANGUAGEVARIANTS></PAGE>";
                XmlDocument xmlDoc =
                    Project.ExecuteRQL(DELETE_PAGE.RQLFormat(this, forceDeletion, LanguageVariant.Language));
                if (!xmlDoc.InnerText.Contains("ok"))
                {
                    throw new PageDeletionException(Project.Session.ServerLogin,
                                                    string.Format("Could not delete page {0}", this));
                }
            } catch (RQLException e)
            {
                throw new PageDeletionException(e);
            }
            IsInitialized = false;
            _releaseStatus = PageReleaseStatus.NotSet;
            Status = PageState.NotSet;
        }

        private List<PageElement> GetContentElements()
        {
            using (new LanguageContext(LanguageVariant))
            {
                const string LOAD_PAGE_ELEMENTS =
                    @"<PROJECT><PAGE guid=""{0}""><ELEMENTS action=""load""/></PAGE></PROJECT>";
                XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(LOAD_PAGE_ELEMENTS, Guid.ToRQLString()));
                return ToElementList(xmlDoc.GetElementsByTagName("ELEMENT"));
            }
        }

        private List<Keyword> GetKeywords()
        {
            const string LOAD_KEYWORDS = @"<PROJECT><PAGE guid=""{0}""><KEYWORDS action=""load"" /></PAGE></PROJECT>";
            XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(LOAD_KEYWORDS, Guid.ToRQLString()));
            return
                (from XmlElement curNode in xmlDoc.GetElementsByTagName("KEYWORD") select new Keyword(Project, curNode))
                    .ToList();
        }

        private List<ILinkElement> GetLinks()
        {
            using (new LanguageContext(LanguageVariant))
            {
                const string LOAD_LINKS = @"<PAGE guid=""{0}""><LINKS action=""load"" /></PAGE>";
                XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(LOAD_LINKS, Guid.ToRQLString()));
                return (from XmlElement curNode in xmlDoc.GetElementsByTagName("LINK")
                        select (ILinkElement) PageElement.CreateElement(Project, curNode)).ToList();
            }
        }

        private static IEnumerable<string> GetNames(XmlNodeList elements)
        {
            return elements.Cast<XmlElement>().Select(x => x.GetAttributeValue("name"));
        }

        private List<ILinkElement> GetReferencingLinks()
        {
            const string LIST_REFERENCES = @"<REFERENCE action=""list"" guid=""{0}"" />";
            XmlDocument xmlDoc = Project.ExecuteRQL(LIST_REFERENCES.RQLFormat(this), Project.RqlType.SessionKeyInProject);

            return (from XmlElement curLink in xmlDoc.GetElementsByTagName("LINK")
                    select (ILinkElement) PageElement.CreateElement(Project, curLink.GetGuid(), LanguageVariant)).ToList
                ();
        }

        private void InitProperties()
        {
            LinkElements = new RDList<ILinkElement>(GetLinks, Caching.Enabled);
            ContentElements = new NameIndexedRDList<PageElement>(GetContentElements, Caching.Enabled);
            Keywords = new RDList<Keyword>(GetKeywords, Caching.Enabled);
            ReferencedBy = new RDList<ILinkElement>(GetReferencingLinks, Caching.Enabled);
        }

        private static bool IsReleasedIntoWorkflow(PageReleaseStatus value, PageReleaseStatus flag)
        {
            return value == PageReleaseStatus.Released && flag.HasFlag(PageReleaseStatus.WorkFlow);
        }

        private void LoadXml()
        {
            //TODO schoenere loesung fuer partielles nachladen von pages wegen unterschiedlicher anfragen fuer unterschiedliche infos
            InitIfPresent(ref _id, "id", int.Parse);
            EnsuredInit(ref _lang, "languagevariantid", Project.LanguageVariants.Get);
            InitIfPresent(ref _parentPage, "parentguid", x => new Page(Project, GuidConvert(x), LanguageVariant));
            InitIfPresent(ref _headline, "headline", x => x);
            InitIfPresent(ref _pageFlags, "flags", x => (PageFlags) int.Parse(x));
            InitIfPresent(ref _ccGuid, "templateguid", Guid.Parse);
            InitIfPresent(ref _pageState, "status", x => (PageState) int.Parse(x));

            _releaseStatus = ReleaseStatusFromFlags();

            _checkinDate = XmlElement.GetOADate("checkindate").GetValueOrDefault();

            InitIfPresent(ref _mainLinkGuid, "mainlinkguid", GuidConvert);
            InitIfPresent(ref _releaseDate, "releasedate", XmlUtil.ToOADate);
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

        private List<PageElement> ToElementList(XmlNodeList elementNodes)
        {
            return
                (from XmlElement curNode in elementNodes
                 let element = TryCreateElement(curNode)
                 where element != null
                 select element).ToList();
        }

        private PageElement TryCreateElement(XmlElement xmlElement)
        {
            try
            {
                return PageElement.CreateElement(Project, xmlElement);
            } catch (ArgumentException)
            {
                return null;
            }
        }

        private void WaitForDeletionFromRecycleBin(TimeSpan maxWaitForDeletionFromRecycleBin)
        {
            //At this point we are at a race condition with the server.
            //It can happen that although the status is set to IsInRecycleBin, it can't be removed from there yet.
            //Therefor we have to try again, until it works (or a timeout is reached to avoid infinite loops on errors).
            var timeOutTracker = new TimeOutTracker(maxWaitForDeletionFromRecycleBin);
            do
            {
                DeleteFromRecycleBin();

                Refresh();
                if (!Exists)
                {
                    return;
                }
            } while (!timeOutTracker.HasTimedOut);

            throw new PageDeletionException(Project.Session.ServerLogin,
                                            string.Format(
                                                "Timeout while waiting for remove from recycle bin for page {0}", this));
        }

        private void WaitUntilPageIsInRecycleBin(TimeSpan maxWaitForDeletionInMs)
        {
            var timeoutTracker = new TimeOutTracker(maxWaitForDeletionInMs);
            do
            {
                Refresh();
                if (Status == PageState.IsInRecycleBin)
                {
                    return;
                }
            } while (!timeoutTracker.HasTimedOut);

            throw new PageDeletionException(Project.Session.ServerLogin,
                                            string.Format(
                                                "Timeout while waiting for the page {0} to move into the recycle bin",
                                                this));
        }

        #region IPage Members

        public DateTime CheckinDate
        {
            get { return LazyLoad(ref _checkinDate); }
        }

        /// <summary>
        ///     Save changes to headline/filename to the server.
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
        ///     Content class of the page
        /// </summary>
        public ContentClass ContentClass
        {
            get { return _contentClass ?? (_contentClass = Project.ContentClasses.GetByGuid(LazyLoad(ref _ccGuid))); }
        }

        /// <summary>
        ///     All content elements of this page.
        /// </summary>
        public NameIndexedRDList<PageElement> ContentElements { get; private set; }

        /// <summary>
        ///     Move the page to the recycle bin, if page has been released yet. Otherwise the page will be deleted from CMS server completely.
        ///     Forces the deletion, even if references still point to elements of this page or an element is assigned as target container to a link.
        ///     If you want to make sure it is completly removed from the server, even if has been released,
        ///     use <see cref="DeleteIrrevocably" /> instead of calling this method and <see cref="DeleteFromRecycleBin" />.
        /// </summary>
        /// <exception cref="PageDeletionException">Thrown, if page could not be deleted.</exception>
        public void Delete()
        {
            DeleteImpl(forceDeletion: true);
        }

        /// <summary>
        ///     Delete the page from the recycle bin.
        ///     This is an asynchronous process on the server so the removal may not be immediatly visible.
        /// </summary>
        public void DeleteFromRecycleBin()
        {
            using (new LanguageContext(LanguageVariant))
            {
                const string DELETE_FINALLY =
                    @"<PAGE action=""deletefinally"" guid=""{0}"" alllanguages="""" forcedelete2910="""" forcedelete2911=""""/>";
                Project.ExecuteRQL(string.Format(DELETE_FINALLY, Guid.ToRQLString()));

                const string MARK_DIRTY =
                    @"<PAGEBUILDER><PAGES sessionkey=""{0}"" action=""pagevaluesetdirty""><PAGE sessionkey=""{0}"" guid=""{1}"" languages=""{2}""/></PAGES></PAGEBUILDER>";
                Project.Session.ExecuteRql(
                    MARK_DIRTY.RQLFormat(Project.Session.SessionKey, this, LanguageVariant.Language),
                    Session.IODataFormat.SessionKeyOnly);

                const string LINKING =
                    @"<PAGEBUILDER><LINKING sessionkey=""{0}""><PAGES><PAGE sessionkey=""{0}"" guid=""{1}""/></PAGES></LINKING></PAGEBUILDER>";
                Project.Session.ExecuteRql(LINKING.RQLFormat(Project.Session.SessionKey, this),
                                           Session.IODataFormat.SessionKeyOnly);
            }
            IsInitialized = false;
            _releaseStatus = PageReleaseStatus.NotSet;
            Status = PageState.NotSet;
        }

        /// <summary>
        ///     Remove a keyword from this page.
        /// </summary>
        public void DeleteKeyword(Keyword keyword)
        {
            const string DELETE_KEYWORD =
                @"<PROJECT><PAGE guid=""{0}"" action=""unlink""><KEYWORD guid=""{1}"" /></PAGE></PROJECT>";
            Project.ExecuteRQL(string.Format(DELETE_KEYWORD, Guid.ToRQLString(), keyword.Guid.ToRQLString()));

            Keywords.InvalidateCache();
        }

        /// <summary>
        ///     Disconnects the page from its parent (main link).
        /// </summary>
        public void DisconnectFromParent()
        {
            var link = MainLinkElement as ILinkElement;
            if (link != null)
            {
                link.Disconnect(this);
            }
        }

        /// <summary>
        ///     Page filename. Same as Name.
        /// </summary>
        public string Filename
        {
            get { return Name; }
            set { Name = value; }
        }

        /// <summary>
        ///     Headline of the page
        /// </summary>
        public string Headline
        {
            get { return LazyLoad(ref _headline); }
            set { _headline = value; }
        }

        /// <summary>
        ///     Page Id.
        /// </summary>
        public int Id
        {
            get { return LazyLoad(ref _id); }
            set { _id = value; }
        }

        /// <summary>
        ///     Get a content/link element of this page with a specific name.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown, if no element with the expected name could be found.</exception>
        public IPageElement this[string elementName]
        {
            get
            {
                PageElement result;
                return ContentElements.TryGetByName(elementName, out result)
                           ? (IPageElement) result
                           : LinkElements.GetByName(elementName);
            }
        }

        /// <summary>
        ///     All newKeywords associated with this page.
        /// </summary>
        public RDList<Keyword> Keywords { get; private set; }

        /// <summary>
        ///     Language variant of this page instance.
        /// </summary>
        public LanguageVariant LanguageVariant
        {
            get { return _lang; }
        }

        /// <summary>
        ///     All link elements of this page.
        /// </summary>
        public IRDList<ILinkElement> LinkElements { get; private set; }

        /// <summary>
        ///     The element this page has as mainlink.
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
                _mainLinkElement = PageElement.CreateElement(Project, _mainLinkGuid, LanguageVariant);
                return _mainLinkElement;
            }
        }

        /// <summary>
        ///     Parent page (the page containing this page's main link).
        /// </summary>
        public Page Parent
        {
            get { return _parentPage ?? (_parentPage = MainLinkElement != null ? MainLinkElement.Page : null); }
        }

        public Project Project { get; private set; }
        public IRDList<ILinkElement> ReferencedBy { get; private set; }

        /// <summary>
        ///     Rejects the page from the current level of workflow.
        /// </summary>
        public void Reject()
        {
            ReleaseStatus = PageReleaseStatus.Rejected;
        }

        /// <summary>
        ///     Released the page.
        /// </summary>
        public void Release()
        {
            ReleaseStatus = PageReleaseStatus.Released;
        }

        /// <summary>
        ///     Date of the release.
        /// </summary>
        /// TODO last or initial release?
        public DateTime ReleaseDate
        {
            get { return LazyLoad(ref _releaseDate); }
        }

        /// <summary>
        ///     The current release status of this page. Setting it will change it on the server.
        /// </summary>
        public PageReleaseStatus ReleaseStatus
        {
            get { return LazyLoad(ref _releaseStatus); }
            set
            {
                using (new LanguageContext(LanguageVariant))
                {
                    SaveReleaseStatus(value);
                }
                ResetReleaseStatusTo(value);
            }
        }

        /// <summary>
        ///     Reset the page to draft status.
        /// </summary>
        public void ResetToDraft()
        {
            ReleaseStatus = PageReleaseStatus.Draft;
        }

        /// <summary>
        ///     Restore page from recycle bin
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
        ///     Push the page through workflow. Afterwards the (release) status of this page object no longer reflects the real status. To update it, call
        ///     <see
        ///         cref="PartialRedDotObject.Refresh" />
        ///     . The object ist not automaticall updated to not incurr unnecessary overhead, if that information isn't needed anyway.
        /// </summary>
        public void SkipWorkflow()
        {
            const string SKIP_WORKFLOW =
                @"<PAGE action=""save"" guid=""{0}"" globalsave=""0"" skip=""1"" actionflag=""{1}"" />";

            Project.ExecuteRQL(string.Format(SKIP_WORKFLOW, Guid.ToRQLString(), (int) PageReleaseStatus.WorkFlow));
        }

        /// <summary>
        ///     Status of the page, useually ReleaseStatus should be used instead.
        /// </summary>
        public PageState Status
        {
            get { return LazyLoad(ref _pageState); }
            set { _pageState = value; }
        }

        /// <summary>
        ///     Submit the page to workflow.
        /// </summary>
        public void SubmitToWorkflow()
        {
            ReleaseStatus = PageReleaseStatus.WorkFlow;
        }

        /// <summary>
        ///     Imitates the RedDot Undo page function. If the page has no previous state it is deleted. See the RedDot documentation for more details.
        /// </summary>
        public void Undo()
        {
            const string UNDO = @"<PAGE action=""rejecttempsaved"" guid=""{0}"" />";
            Project.ExecuteRQL(string.Format(UNDO, Guid.ToRQLString()));
        }

        /// <summary>
        ///     Returns the Workflow this page adheres to.
        /// </summary>
        public Workflow Workflow
        {
            get
            {
                return new Workflow(Project,
                                    ((XmlElement) XmlElement.SelectSingleNode("descendant::WORKFLOW")).GetGuid());
            }
        }

        private void ResetReleaseStatusTo(PageReleaseStatus value)
        {
            IsInitialized = false;
            _releaseStatus = value;
            Status = PageState.NotSet;
        }

        private void SaveReleaseStatus(PageReleaseStatus value)
        {
            const string SET_RELEASE_STATUS = @"<PAGE action=""save"" guid=""{0}"" actionflag=""{1}""/>";
            XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(SET_RELEASE_STATUS, Guid.ToRQLString(), (int) value));
            CheckReleaseStatusSettingSuccess(value, xmlDoc);
        }

        #endregion
    }
}