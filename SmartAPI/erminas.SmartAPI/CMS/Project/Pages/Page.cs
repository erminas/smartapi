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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using erminas.SmartAPI.CMS.Project.ContentClasses;
using erminas.SmartAPI.CMS.Project.Pages.Elements;
using erminas.SmartAPI.CMS.Project.Workflows;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Pages
{
    internal class Page : PartialRedDotProjectObject, IPage
    {
        private Guid _ccGuid;
        private DateTime _checkinDate;
        private IContentClass _contentClass;
        private string _headline;
        private int _id;
        private ILanguageVariant _lang;
        private IPageElement _mainLinkElement;
        private Guid _mainLinkGuid;
        private PageFlags _pageFlags = PageFlags.Null;

        private PageState _pageState;
        private IPage _parentPage;
        private DateTime _releaseDate;
        private PageReleaseStatus _releaseStatus;

        internal Page(IProject project, XmlElement xmlElement) : base(project, xmlElement)
        {
            LoadXml();
            IsInitialized = false;
            InitProperties();
        }

        internal Page(IProject project, Guid guid, ILanguageVariant languageVariant) : base(project, guid)
        {
            _lang = languageVariant;
            InitProperties();
        }

        public IAssignedKeywords AssignedKeywords { get; private set; }

        public DateTime CheckinDate
        {
            get { return LazyLoad(ref _checkinDate); }
        }

        public void Commit()
        {
            const string SAVE_PAGE = @"<PAGE action=""save"" guid=""{0}"" headline=""{1}"" name=""{2}"" />";
            XmlDocument xmlDoc =
                Project.ExecuteRQL(string.Format(SAVE_PAGE, Guid.ToRQLString(), HttpUtility.HtmlEncode(Headline),
                                                 HttpUtility.HtmlEncode(Filename)));
            if (xmlDoc.GetElementsByTagName("PAGE").Count != 1)
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not save changes to page {0}", this));
            }
        }

        public IContentClass ContentClass
        {
            get { return _contentClass ?? (_contentClass = Project.ContentClasses.GetByGuid(LazyLoad(ref _ccGuid))); }
        }

        public IIndexedRDList<string, IPageElement> ContentElements { get; private set; }

        public void Delete()
        {
            DeleteImpl(forceDeletion: true);
        }

        public void DeleteFromRecycleBin()
        {
            using (new LanguageContext(LanguageVariant))
            {
                const string DELETE_FINALLY =
                    @"<PAGE action=""deletefinally"" guid=""{0}"" alllanguages="""" forcedelete2910="""" forcedelete2911=""""/>";
                Project.ExecuteRQL(string.Format(DELETE_FINALLY, Guid.ToRQLString()));

                const string MARK_DIRTY =
                    @"<PAGEBUILDER><PAGES sessionkey=""{0}"" action=""pagevaluesetdirty""><PAGE sessionkey=""{0}"" guid=""{1}"" languages=""{2}""/></PAGES></PAGEBUILDER>";
                Project.Session.ExecuteRQLRaw(
                    MARK_DIRTY.RQLFormat(Project.Session.SessionKey, this, LanguageVariant.Abbreviation),
                    RQL.IODataFormat.SessionKeyOnly);

                const string LINKING =
                    @"<PAGEBUILDER><LINKING sessionkey=""{0}""><PAGES><PAGE sessionkey=""{0}"" guid=""{1}""/></PAGES></LINKING></PAGEBUILDER>";
                Project.Session.ExecuteRQLRaw(LINKING.RQLFormat(Project.Session.SessionKey, this),
                                              RQL.IODataFormat.SessionKeyOnly);
            }
            IsInitialized = false;
            _releaseStatus = PageReleaseStatus.NotSet;
            Status = PageState.NotSet;
        }

        public void DeleteIfNotReferenced()
        {
            DeleteImpl(forceDeletion: false);
        }

        public void DeleteIrrevocably(int maxWaitForDeletionInMs)
        {
            var start = DateTime.Now;
            var maxWaitForDeletion = new TimeSpan(0, 0, 0, 0, maxWaitForDeletionInMs);

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
                WaitUntilPageIsInRecycleBin(maxWaitForDeletion);
            }

            var alreadyElapsed = DateTime.Now - start;
            var maxWaitForDeletionFromRecycleBin = maxWaitForDeletion - alreadyElapsed;

            WaitForDeletionFromRecycleBin(maxWaitForDeletionFromRecycleBin);
        }

        public void DisconnectFromParent()
        {
            var link = MainLinkElement as ILinkElement;
            if (link != null)
            {
                link.Connections.Remove(this);
            }
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

        public string Filename
        {
            get { return Name; }
            set { Name = value; }
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode() + 3*LanguageVariant.GetHashCode();
        }

        public string Headline
        {
            get { return LazyLoad(ref _headline); }
            set
            {
                EnsureInitialization();
                _headline = value;
            }
        }

        public int Id
        {
            get { return LazyLoad(ref _id); }
            internal set { _id = value; }
        }

        public IPageElement this[string elementName]
        {
            get
            {
                IPageElement result;
                return ContentElements.TryGetByName(elementName, out result)
                           ? result
                           : LinkElements.GetByName(elementName);
            }
        }

        public ILanguageVariant LanguageVariant
        {
            get { return _lang; }
        }

        public IRDList<ILinkElement> LinkElements { get; private set; }
        public IRDList<ILinkingAndAppearance> LinkedFrom { get; private set; }

        public IPageElement MainLinkElement
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

        public new string Name
        {
            get { return base.Name; }
            set
            {
                EnsureInitialization();
                base.Name = value;
            }
        }

        /// <summary>
        ///     Parent page (the page containing this page's main link).
        /// </summary>
        public IPage Parent
        {
            get { return _parentPage ?? (_parentPage = MainLinkElement != null ? MainLinkElement.Page : null); }
        }

        public IRDList<ILinkElement> ReferencedFrom { get; private set; }

        public override void Refresh()
        {
            _contentClass = null;
            _ccGuid = default(Guid);
            base.Refresh();
        }

        public IPage Refreshed()
        {
            Refresh();
            return this;
        }

        public void Reject()
        {
            ReleaseStatus = PageReleaseStatus.Rejected;
        }

        public void Release()
        {
            ReleaseStatus = PageReleaseStatus.Released;
        }

        public DateTime ReleaseDate
        {
            get { return LazyLoad(ref _releaseDate); }
        }

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

        public void ReplaceContentClass(IContentClass replacement, IDictionary<string, string> oldToNewMapping,
                                        Replace replace)
        {
            const string REPLACE_CC =
                @"<PAGE action=""changetemplate"" guid=""{0}"" changeall=""{1}"" holdreferences=""1"" holdexportsettings=""1"" holdauthorizations=""1"" holdworkflow=""1""><TEMPLATE originalguid=""{2}"" changeguid=""{3}"">{4}</TEMPLATE></PAGE>";

            const string REPLACE_ELEMENT = @"<ELEMENT originalguid=""{0}"" changeguid=""{1}""/>";
            var oldElements = ContentClass.Elements;
            var newElements = replacement.Elements;

            var unmappedElements = oldElements.Where(element => !oldToNewMapping.ContainsKey(element.Name));
            var unmappedStr = unmappedElements.Aggregate("",
                                                         (s, element) =>
                                                         s +
                                                         REPLACE_ELEMENT.RQLFormat(element, RQL.SESSIONKEY_PLACEHOLDER));
            var mappedStr = string.Join("", from entry in oldToNewMapping
                                            let oldElement = oldElements[entry.Key]
                                            let newElement = newElements.GetByName(entry.Value)
                                            select REPLACE_ELEMENT.RQLFormat(oldElement, newElement));

            var isReplacingAll = replace == Replace.ForAllPagesOfContentClass;
            var query = REPLACE_CC.RQLFormat(this, isReplacingAll, ContentClass, replacement, mappedStr + unmappedStr);

            Project.ExecuteRQL(query, RqlType.SessionKeyInProject);

            _contentClass = null;
            _ccGuid = default(Guid);
        }

        public void ResetToDraft()
        {
            ReleaseStatus = PageReleaseStatus.Draft;
        }

        public void Restore()
        {
            const string RESTORE_PAGE =
                @"<PAGE action=""restore"" guid=""{0}"" alllanguages="""" forcedelete2910="""" forcedelete2911=""""/>";
            Project.ExecuteRQL(string.Format(RESTORE_PAGE, Guid.ToRQLString()));
            IsInitialized = false;
            ReleaseStatus = PageReleaseStatus.NotSet;
            Status = PageState.NotSet;
        }

        public void SkipWorkflow()
        {
            const string SKIP_WORKFLOW =
                @"<PAGE action=""save"" guid=""{0}"" globalsave=""0"" skip=""1"" actionflag=""{1}"" />";

            Project.ExecuteRQL(string.Format(SKIP_WORKFLOW, Guid.ToRQLString(), (int) PageReleaseStatus.WorkFlow));
        }

        public PageState Status
        {
            get { return LazyLoad(ref _pageState); }
            set { _pageState = value; }
        }

        public void SubmitToWorkflow()
        {
            ReleaseStatus = PageReleaseStatus.WorkFlow;
        }

        public override string ToString()
        {
            return string.Format("{0} (Id: {1} Guid: {2} Language: {3})", Headline, Id, Guid.ToRQLString(),
                                 LanguageVariant.Abbreviation);
        }

        public void Undo()
        {
            const string UNDO = @"<PAGE action=""rejecttempsaved"" guid=""{0}"" />";
            Project.ExecuteRQL(string.Format(UNDO, Guid.ToRQLString()));
        }

        public IWorkflow Workflow
        {
            get
            {
                return new Workflow(Project,
                                    ((XmlElement) XmlElement.SelectSingleNode("descendant::WORKFLOW")).GetGuid());
            }
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
                    throw new SmartAPIException(Session.ServerLogin,
                                                string.Format("Could not load page with guid {0}", Guid.ToRQLString()));
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
                    Project.ExecuteRQL(DELETE_PAGE.RQLFormat(this, forceDeletion, LanguageVariant.Abbreviation));
                if (!xmlDoc.IsContainingOk())
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

        private List<IPageElement> GetContentElements()
        {
            using (new LanguageContext(LanguageVariant))
            {
                const string LOAD_PAGE_ELEMENTS =
                    @"<PROJECT><PAGE guid=""{0}""><ELEMENTS action=""load""/></PAGE></PROJECT>";
                XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(LOAD_PAGE_ELEMENTS, Guid.ToRQLString()));
                return ToElementList(xmlDoc.GetElementsByTagName("ELEMENT"));
            }
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

        private List<ILinkingAndAppearance> GetLinksFrom()
        {
            const string @LOAD_LINKING = @"<PAGE guid=""{0}""><LINKSFROM action=""load"" /></PAGE>";

            var xmlDoc = Project.ExecuteRQL(LOAD_LINKING.RQLFormat(this));
            return (from XmlElement curLink in xmlDoc.GetElementsByTagName("LINK")
                    select (ILinkingAndAppearance) new LinkingAndAppearance(this, curLink)).ToList();
        }

        private static IEnumerable<string> GetNames(XmlNodeList elements)
        {
            return elements.Cast<XmlElement>().Select(x => x.GetAttributeValue("name"));
        }

        private List<ILinkElement> GetReferencingLinks()
        {
            const string LIST_REFERENCES = @"<REFERENCE action=""list"" guid=""{0}"" />";
            XmlDocument xmlDoc = Project.ExecuteRQL(LIST_REFERENCES.RQLFormat(this), RqlType.SessionKeyInProject);

            return (from XmlElement curLink in xmlDoc.GetElementsByTagName("LINK")
                    select (ILinkElement) PageElement.CreateElement(Project, curLink.GetGuid(), LanguageVariant)).ToList
                ();
        }

        private void InitProperties()
        {
            LinkElements = new RDList<ILinkElement>(GetLinks, Caching.Enabled);
            ContentElements = new NameIndexedRDList<IPageElement>(GetContentElements, Caching.Enabled);
            ReferencedFrom = new RDList<ILinkElement>(GetReferencingLinks, Caching.Enabled);
            AssignedKeywords = new PageAssignedKeywords(this, Caching.Enabled);
            LinkedFrom = new RDList<ILinkingAndAppearance>(GetLinksFrom, Caching.Enabled);
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
            //parentguid seems to be the mainlinkguid
            //InitIfPresent(ref _parentPage, "parentguid", x => new Page(Project, GuidConvert(x), LanguageVariant));
            InitIfPresent(ref _headline, "headline", x => x);
            InitIfPresent(ref _pageFlags, "flags", x => (PageFlags) int.Parse(x));
            InitIfPresent(ref _ccGuid, "templateguid", Guid.Parse);
            InitIfPresent(ref _pageState, "status", x => (PageState) int.Parse(x));

            _releaseStatus = ReleaseStatusFromFlags();

            _checkinDate = _xmlElement.GetOADate("checkindate").GetValueOrDefault();

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

        private List<IPageElement> ToElementList(XmlNodeList elementNodes)
        {
            return
                (from XmlElement curNode in elementNodes
                 let element = TryCreateElement(curNode)
                 where element != null
                 select element).Cast<IPageElement>().ToList();
        }

        private IPageElement TryCreateElement(XmlElement xmlElement)
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
                if (!Exists || Status == PageState.IsInRecycleBin)
                {
                    return;
                }
            } while (!timeoutTracker.HasTimedOut);

            throw new PageDeletionException(Project.Session.ServerLogin,
                                            string.Format(
                                                "Timeout while waiting for the page {0} to move into the recycle bin",
                                                this));
        }
    }
}