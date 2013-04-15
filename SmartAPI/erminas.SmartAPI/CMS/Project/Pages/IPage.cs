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
using erminas.SmartAPI.CMS.Project.ContentClasses;
using erminas.SmartAPI.CMS.Project.Keywords;
using erminas.SmartAPI.CMS.Project.Pages.Elements;
using erminas.SmartAPI.CMS.Project.Workflows;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Pages
{
    public interface IPage : ILinkTarget, IPartialRedDotObject
    {
        DateTime CheckinDate { get; }

        /// <summary>
        ///     Save changes to headline/filename to the server.
        /// </summary>
        void Commit();

        /// <summary>
        ///     Content class of the page
        /// </summary>
        ContentClass ContentClass { get; }

        /// <summary>
        ///     All content elements of this page.
        /// </summary>
        NameIndexedRDList<IPageElement> ContentElements { get; }

        /// <summary>
        ///     Move the page to the recycle bin, if page has been released yet. Otherwise the page will be deleted from CMS server completely.
        /// </summary>
        void Delete();

        /// <summary>
        ///     Delete the page from the recycle bin
        /// </summary>
        void DeleteFromRecycleBin();

        /// <summary>
        ///     Remove a keyword from this page.
        /// </summary>
        void DeleteKeyword(Keyword keyword);

        /// <summary>
        ///     Disconnects the page from its parent (main link).
        /// </summary>
        void DisconnectFromParent();

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

        /// <summary>
        ///     All keywords associated with this page.
        /// </summary>
        RDList<Keyword> Keywords { get; }

        /// <summary>
        ///     Language variant of this page instance.
        /// </summary>
        LanguageVariant LanguageVariant { get; }

        /// <summary>
        ///     All link elements of this page.
        /// </summary>
        IRDList<ILinkElement> LinkElements { get; }

        /// <summary>
        ///     The element this page has as mainlink.
        /// </summary>
        PageElement MainLinkElement { get; }

        /// <summary>
        ///     Parent page (the page containing this page's main link).
        /// </summary>
        Page Parent { get; }

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
        Page.PageReleaseStatus ReleaseStatus { get; set; }

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
        Page.PageState Status { get; set; }

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
        Workflow Workflow { get; }

        IRDList<ILinkElement> LinkedFrom { get; }
    }
}