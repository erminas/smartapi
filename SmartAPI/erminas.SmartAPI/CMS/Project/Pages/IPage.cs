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
using System.Globalization;
using System.Linq;
using erminas.SmartAPI.CMS.Project.ContentClasses;
using erminas.SmartAPI.CMS.Project.Pages.Elements;
using erminas.SmartAPI.CMS.Project.Workflows;
using erminas.SmartAPI.CMS.ServerManagement;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Pages
{
    public enum PageType
    {
        All = 0,
        Released = 1,
        Unlinked = 8192,
        Draft = 262144
    };

    public enum Replace
    {
        OnlyForThisPage,
        ForAllPagesOfContentClass
    }

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

    [Flags]
    public enum PageReleaseStatus
    {
        Draft = 65536,
        WorkFlow = 32768,
        Released = 4096,
        NotSet = 0,
        Rejected = 16384
    };

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

    public interface IPagePublishJob : IProjectObject
    {
        IPage Page { get; }
        
        bool IsPublishingAllFollowingPages { get; set; }
        bool IsPublishingRelatedPages { get; set; }
        bool IsSendingEmailOnCompletion { get; set; }

        IUser EMailReceipient { get; set; }
        IApplicationServer Server { get; set; }
        
        DateTime PublishAccordingTo { get; set; }
        DateTime PublishOn { get; set; }
        IEnumerable<ILanguageVariant> LanguageVariants{ get; set; }
        IEnumerable<IProjectVariant> ProjectVariants { get; set; }

        void RunAsync();
    }

    internal class PagePublishJob : IPagePublishJob
    {
        internal PagePublishJob(IPage page)
        {
            Page = page;
            LanguageVariants = new List<ILanguageVariant>();
            ProjectVariants = new List<IProjectVariant>();
        }

        public ISession Session { get { return Page.Session; } }
        public IProject Project { get { return Page.Project; } }
        public IPage Page { get; private set; }
        public bool IsPublishingAllFollowingPages { get; set; }
        public bool IsPublishingRelatedPages { get; set; }
        public bool IsSendingEmailOnCompletion { get; set; }
        public IUser EMailReceipient { get; set; }
        public IApplicationServer Server { get; set; }
        public DateTime PublishAccordingTo { get; set; }
        public DateTime PublishOn { get; set; }
        public IEnumerable<ILanguageVariant> LanguageVariants { get; set; }
        public IEnumerable<IProjectVariant> ProjectVariants { get; set; }
        public void RunAsync()
        {
            CheckValidity();

            const string PUBLISH = @"<PROJECT guid=""{0}"" sessionkey=""{10}""><PAGE guid=""{1}""><EXPORTJOB action=""save"" email=""{2}"" toppriority=""0"" generatenextpages=""{3}"" generaterelativepages=""{4}"" reddotserver=""{5}"" application="""" generatedate=""{6}"" startgenerationat=""{7}""><LANGUAGEVARIANTS action=""checkassigning"">{8}</LANGUAGEVARIANTS><PROJECTVARIANTS action=""checkassigning"">{9}</PROJECTVARIANTS></EXPORTJOB></PAGE></PROJECT>";
            
            var languageVariants = GetLanguageVariantsString();
            var projectVariants = GetProjectVariantsString();

            var publishAccordingToString = ToRQLString(PublishAccordingTo);
            var publishOnString = ToRQLString(PublishOn);

            var query = PUBLISH.RQLFormat(Project, Page, IsSendingEmailOnCompletion ? EMailReceipient : null,
                              IsPublishingAllFollowingPages, IsPublishingRelatedPages, Server, publishAccordingToString,
                              publishOnString, languageVariants, projectVariants, Session.SessionKey);

            Project.ExecuteRQL(query);
        }

        private void CheckValidity()
        {
            if (LanguageVariants == null || !LanguageVariants.Any())
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format(
                                                "Missing language variant assignment for the publication of page {0}", Page));
            }

            if (ProjectVariants == null || !ProjectVariants.Any())
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Missing project variant assignment for the publication of page {0}",
                                                          Page));
            }
        }

        private static string ToRQLString(DateTime dt)
        {
            return dt != default(DateTime)
                       ? dt.ToString("M/d/yyyy", CultureInfo.InvariantCulture)
                       : RQL.SESSIONKEY_PLACEHOLDER;
        }

        private string GetLanguageVariantsString()
        {
            const string SINGLE_LANGUAGE_VARIANT = @"<LANGUAGEVARIANT guid=""{0}"" checked=""{1}""/>";

            var checkedLanguageVariants = LanguageVariants.Aggregate("",
                                                                     (s, variant) =>
                                                                     s + SINGLE_LANGUAGE_VARIANT.RQLFormat(variant, true));
            var uncheckedLanguageVariants = Project.LanguageVariants.Except(LanguageVariants)
                                                   .Aggregate("",
                                                              (s, variant) =>
                                                              s + SINGLE_LANGUAGE_VARIANT.RQLFormat(variant, false));

            var languageVariants = checkedLanguageVariants + uncheckedLanguageVariants;
            return languageVariants;
        }

        private string GetProjectVariantsString()
        {
            const string SINGLE_PROJECT_VARIANT = @"<PROJECTVARIANT guid=""{0}"" checked=""{1}""/>";

            var checkedProjectVariants = ProjectVariants.Aggregate("",
                                                                   (s, variant) =>
                                                                   s + SINGLE_PROJECT_VARIANT.RQLFormat(variant, true));

            var uncheckedProjectVariants = Project.ProjectVariants.Except(ProjectVariants)
                                                  .Aggregate("",
                                                             (s, variant) =>
                                                             s + SINGLE_PROJECT_VARIANT.RQLFormat(variant, false));
            var projectVariants = checkedProjectVariants + uncheckedProjectVariants;
            return projectVariants;
        }
    }

    /// <summary>
    ///     Wrapper for the RedDot Page object. If status changes occur, you have to call
    ///     <see
    ///         cref="PartialRedDotObject.Refresh" />
    ///     to see them reflected in the status field,
    /// </summary>
    public interface IPage : ILinkTarget, IPartialRedDotObject, IKeywordAssignable, IDeletable, ISessionObject
    {
        DateTime CheckinDate { get; }

        /// <summary>
        ///     Save changes to headline/filename to the server.
        /// </summary>
        void Commit();

        /// <summary>
        ///     Content class of the page
        /// </summary>
        IContentClass ContentClass { get; }

        /// <summary>
        ///     All content elements of this page. Indexed by name and cached by default.
        /// </summary>
        IIndexedRDList<string, IPageElement> ContentElements { get; }

        /// <summary>
        ///     Move the page to the recycle bin, if page has been released yet. Otherwise the page will be deleted from CMS server completely.
        /// </summary>
        new void Delete();

        /// <summary>
        ///     Delete the page from the recycle bin
        /// </summary>
        void DeleteFromRecycleBin();

        /// <summary>
        ///     Move the page to the recycle bin, if page has been released yet. Otherwise the page will be deleted from CMS server completely.
        ///     If you want to make sure it is completly removed from the server, even if has been released,
        ///     use <see cref="Page.DeleteIrrevocably" /> instead of calling this method and <see cref="Page.DeleteFromRecycleBin" />.
        ///     Throws a PageDeletionException, if references still point to elements of this page or an element is assigned as target container to a link.
        /// </summary>
        /// <exception cref="PageDeletionException">Thrown, if page could not be deleted.</exception>
        void DeleteIfNotReferenced();

        /// <summary>
        ///     Delete the page Independant of the state the page is in (e.g released or already in recycle bin), the page will be removed from CMS and cannot be restored.
        ///     Forces the deletion, even if references still point to elements of this page or an element is assigned as target container to a link.
        ///     If the page was released, it will be moved to the recycle bin first.
        ///     Removing it from there leads to a race condition on the server: the page can be already marked as being in the recycle bin, but a call to remove it from there can still fail for some time.
        ///     For this reason a we try to delete it until the operation is successful or a timeout is reached.
        ///     If you want to delete multiple pages a call only to Delete() and a collective removal from the recycle bin afterwards is faster than a call
        ///     to DeleteIrrevocably on every single page.
        /// </summary>
        /// <param name="maxWaitForDeletionInMs">Maximum amount of time in miliseconds to wait for a successful deletion of the page. The default value of 1250ms proved reliable in internal tests.</param>
        /// <exception cref="PageDeletionException">Thrown, if page could not be deleted.</exception>
        void DeleteIrrevocably(int maxWaitForDeletionInMs = 1250);

        /// <summary>
        ///     Disconnects the page from its parent (main link).
        /// </summary>
        void DisconnectFromParent();

        bool Exists { get; }

        /// <summary>
        ///     Page filename. Same as Name.
        /// </summary>
        string Filename { get; set; }

        /// <summary>
        ///     Headline of the page
        /// </summary>
        string Headline { get; set; }

        /// <summary>
        ///     Page Id.
        /// </summary>
        int Id { get; }

        /// <summary>
        ///     Get a content/link element of this page with a specific name.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown, if no element with the expected name could be found.</exception>
        IPageElement this[string elementName] { get; }

        ILanguageVariant LanguageVariant { get; }

        /// <summary>
        ///     All link elements of this page.
        /// </summary>
        IRDList<ILinkElement> LinkElements { get; }

        IRDList<ILinkingAndAppearance> LinkedFrom { get; }

        /// <summary>
        ///     The element this page has as mainlink.
        /// </summary>
        IPageElement MainLinkElement { get; }

        new string Name { get; set; }

        /// <summary>
        ///     Parent page (the page containing this page's main link).
        /// </summary>
        IPage Parent { get; }

        IPage Refreshed();

        /// <summary>
        ///     Rejects the page from the current level of workflow.
        /// </summary>
        void Reject();

        /// <summary>
        ///     Released the page.
        /// </summary>
        void Release();

        /// <summary>
        ///     Date of the release.
        /// </summary>
        /// TODO last or initial release?
        DateTime ReleaseDate { get; }

        /// <summary>
        ///     The current release status of this page. Setting it will change it on the server.
        /// </summary>
        PageReleaseStatus ReleaseStatus { get; set; }

        void ReplaceContentClass(IContentClass replacement, IDictionary<string, string> oldToNewMapping, Replace replace);

        /// <summary>
        ///     Reset the page to draft status.
        /// </summary>
        void ResetToDraft();

        /// <summary>
        ///     Restore page from recycle bin
        /// </summary>
        void Restore();

        /// <summary>
        ///     Push the page through workflow. Afterwards the (release) status of this page object no longer reflects the real status. To update it, call
        ///     <see
        ///         cref="PartialRedDotObject.Refresh" />
        ///     . The object ist not automaticall updated to not incurr unnecessary overhead, if that information isn't needed anyway.
        /// </summary>
        void SkipWorkflow();

        /// <summary>
        ///     ReleaseStatus of the page.
        /// </summary>
        PageState Status { get; set; }

        /// <summary>
        ///     Submit the page to workflow.
        /// </summary>
        void SubmitToWorkflow();

        /// <summary>
        ///     Imitates the RedDot Undo page function. If the page has no previous state it is deleted. See the RedDot documentation for more details.
        /// </summary>
        void Undo();

        /// <summary>
        ///     Returns the Workflow this page adheres to.
        /// </summary>
        IWorkflow Workflow { get; }

        IPagePublishJob CreatePublishJob();
    }
}