using System;
using System.Collections.Generic;
using erminas.SmartAPI.CMS.PageElements;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    public interface IPage : ILinkTarget, IPartialRedDotObject
    {
        /// <summary>
        ///   ReleaseStatus of the page.
        /// </summary>
        Page.PageState Status { get; set; }

        /// <summary>
        ///   Language variant of this page instance.
        /// </summary>
        LanguageVariant LanguageVariant { get; }

        /// <summary>
        ///   Page filename. Same as Name.
        /// </summary>
        string Filename { get; set; }

        Project Project { get; }

        /// <summary>
        ///   Content class of the page
        /// </summary>
        ContentClass ContentClass { get; }

        /// <summary>
        ///   Headline of the page
        /// </summary>
        string Headline { get; set; }

        /// <summary>
        ///   Date of the release.
        /// </summary>
        /// TODO last or initial release?
        DateTime ReleaseDate { get; }

        DateTime CheckinDate { get; }

        /// <summary>
        ///   The element this page has as mainlink.
        /// </summary>
        PageElement MainLinkElement { get; }

        /// <summary>
        ///   Parent page (the page containing this page's main link).
        /// </summary>
        Page Parent { get; }

        /// <summary>
        ///   Page Id.
        /// </summary>
        int Id { get; }

        /// <summary>
        ///   The current release status of this page. Setting it will change it on the server.
        /// </summary>
        Page.PageReleaseStatus ReleaseStatus { get; set; }

        /// <summary>
        ///   Returns the Workflow this page adheres to.
        /// </summary>
        Workflow Workflow { get; }

        /// <summary>
        ///   All keywords associated with this page.
        /// </summary>
        NameIndexedRDList<Keyword> Keywords { get; }

        /// <summary>
        ///   All link elements of this page.
        /// </summary>
        IRDList<ILinkElement> LinkElements { get; }

        /// <summary>
        ///   All content elements of this page.
        /// </summary>
        NameIndexedRDList<PageElement> ContentElements { get; }

        /// <summary>
        ///   Get a content/link element of this page with a specific name.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown, if no element with the expected name could be found.</exception>
        IPageElement this[string elementName] { get; }

        /// <summary>
        ///   Remove a keyword from this page.
        /// </summary>
        void DeleteKeyword(Keyword keyword);

        /// <summary>
        ///   Save changes to headline/filename to the server.
        /// </summary>
        void Commit();

        /// <summary>
        ///   Submit the page to workflow.
        /// </summary>
        void SubmitToWorkflow();

        /// <summary>
        ///   Released the page.
        /// </summary>
        void Release();

        /// <summary>
        ///   Disconnects the page from its parent (main link).
        /// </summary>
        void DisconnectFromParent();

        /// <summary>
        ///   Push the page through workflow. Afterwards the (release) status of this page object no longer reflects the real status. To update it, call <see
        ///    cref="PartialRedDotObject.Refresh" /> . The object ist not automaticall updated to not incurr unnecessary overhead, if that information isn't needed anyway.
        /// </summary>
        void SkipWorkflow();

        /// <summary>
        ///   Imitates the RedDot Undo page function. If the page has no previous state it is deleted. See the RedDot documentation for more details.
        /// </summary>
        void Undo();

        /// <summary>
        ///   Move the page to the recycle bin, if page has been released yet. Otherwise the page will be deleted from CMS server completely.
        /// </summary>
        void Delete();

        /// <summary>
        ///   Delete the page from the recycle bin
        /// </summary>
        void DeleteFromRecycleBin();

        /// <summary>
        ///   Restore page from recycle bin
        /// </summary>
        void Restore();

        /// <summary>
        ///   Rejects the page from the current level of workflow.
        /// </summary>
        void Reject();

        /// <summary>
        ///   Reset the page to draft status.
        /// </summary>
        void ResetToDraft();
    }
}