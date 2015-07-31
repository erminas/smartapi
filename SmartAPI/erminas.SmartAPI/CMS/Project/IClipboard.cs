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
using System.Xml;
using erminas.SmartAPI.CMS.Project.ContentClasses;
using erminas.SmartAPI.CMS.Project.Folder;
using erminas.SmartAPI.CMS.Project.Pages;
using erminas.SmartAPI.CMS.Project.Pages.Elements;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project
{
    public interface IClipboard : ISessionObject
    {
        IEnumerable<ISessionObject> Content { get; }

        IEnumerable<IClipboardEntry> ClipboardEntries { get; }
    }

    public interface IProjectClipboard : IClipboard, IProjectObject
    {
        new IEnumerable<IProjectObject> Content { get; }

        IEnumerable<IPage> Pages { get; }

        IEnumerable<IFolder> Folders { get; }

        IEnumerable<IPageElement> PageElements { get; }

        void Add(params IPage[] page);

        void Add(params IFolder[] folder);

        void Add(params IContentClass[] contentClass);

        void Add(params IPageElement[] pageElement);

        void Remove(params IPage[] page);

        void Remove(params IFolder[] folder);

        void Remove(params IContentClass[] contentClass);

        void Remove(params IPageElement[] pageElement);
    }

    public interface IClipboardEntry
    {
        Guid Guid { get; }

        String Type { get; }
    }

    public class ClipboardEntry : IClipboardEntry
    {
        public Guid Guid { get; internal set; }

        public String Type { get; internal set; }
    }

    internal class ProjectClipboard : IProjectClipboard, ICached
    {
        private const string SINGLE_ENTRY = @"<DATA guid=""{0}"" type=""{1}"" />";
        private readonly IProject _project;
        private List<ClipboardEntry> _clipboardEntries;
        private IList<IProjectObject> _content;

        internal ProjectClipboard(IProject project)
        {
            _project = project;
        }

        public void InvalidateCache()
        {
            _clipboardEntries = null;
        }

        public void Refresh()
        {
            InvalidateCache();
            EnsureContentIsLoaded();
        }

        public ISession Session
        {
            get { return _project.Session; }
        }

        public IEnumerable<IProjectObject> Content
        {
            get
            {
                EnsureContentIsLoaded();
                return _content;
            }
        }

        public IEnumerable<IClipboardEntry> ClipboardEntries
        {
            get 
            {
                EnsureContentIsLoaded();
                return _clipboardEntries.AsReadOnly(); }
        }

        IEnumerable<ISessionObject> IClipboard.Content
        {
            get { return _content; }
        }

        public IProject Project
        {
            get { return _project; }
        }

        public IEnumerable<IPage> Pages
        {
            get
            {
                return Content.Where(x => x is IPage)
                    .Cast<IPage>();
            }
        }

        public IEnumerable<IFolder> Folders
        {
            get
            {
                return Content.Where(x => x is IFolder)
                    .Cast<IFolder>();
            }
        }

        public IEnumerable<IPageElement> PageElements
        {
            get
            {
                return Content.Where(x => x is IPageElement)
                    .Cast<IPageElement>();
            }
        }

        public void Add(params IPage[] pages)
        {
            GenericAdd(pages, "page");
        }

        public void Add(params IFolder[] folders)
        {
            GenericAdd(folders.Where(IS_SUB_FOLDER).ToArray(), "project.6055");
            GenericAdd(folders.Where(x => !IS_SUB_FOLDER(x)).ToArray(), "project.6050");
        }

        private static readonly Func<IFolder, bool> IS_SUB_FOLDER = x=>x.IsAssetManager && ((IAssetManagerFolder)x).IsSubFolder;
        

        public void Add(params IContentClass[] contentClasses)
        {
            GenericAdd(contentClasses, "app.4015");
        }

        public void Add(params IPageElement[] pageElements)
        {
            GenericAdd(pageElements, "pageelement");
        }

        public void Remove(params IPage[] pages)
        {
            GenericRemove(pages, "page");
        }

        public void Remove(params IFolder[] folders)
        {
            GenericRemove(folders.Where(IS_SUB_FOLDER).ToArray(), "project.6055");
            GenericRemove(folders.Where(x => !IS_SUB_FOLDER(x)).ToArray(), "project.6050");
        }

        public void Remove(params IContentClass[] contentClasses)
        {
            GenericRemove(contentClasses, "app.4015");
        }

        public void Remove(params IPageElement[] pageElements)
        {
            GenericRemove(pageElements, "pageelement");
        }

        private void EnsureContentIsLoaded()
        {
            if (_clipboardEntries != null)
            {
                return;
            }

            const string LOAD_CLIPBOARD =
                @"<ADMINISTRATION><USER guid=""{0}""><CLIPBOARDDATA action=""load"" projectguid=""{1}"" foraspx=""1"" /></USER></ADMINISTRATION>";

            var doc =
                Project.ExecuteRQL(LOAD_CLIPBOARD.RQLFormat(Session.CurrentUser, Project, Project.LanguageVariants.Current.Abbreviation));

            var entries = doc.GetElementsByTagName("DATA");
            _clipboardEntries = entries.Cast<XmlElement>().Select(x => new ClipboardEntry{Guid = x.GetGuid(), Type = x.GetAttributeValue("type")}).ToList();

            _content = _clipboardEntries.Select(CreateContentEntry)
                .Where(x => x != null)
                .ToList();
        }

        private IProjectObject CreateContentEntry(IClipboardEntry clipboardEntry)
        {
            switch (clipboardEntry.Type)
            {
                case "page":
                    return new Page(Project, clipboardEntry.Guid, Project.LanguageVariants.Current);
                case "pageelement":
                    return PageElementFactory.Instance.CreateElement(Project, clipboardEntry.Guid, Project.LanguageVariants.Current);
                case "app.4015":
                    return new ContentClass(Project, clipboardEntry.Guid);
                case "project.6050":
                    IFolder folder;
                    return Project.Folders.TryGetByGuid(clipboardEntry.Guid, out folder) ? folder : null;
                case "project.6055":
                    IFolder subFolder;
                    return Project.Folders.AllIncludingSubFolders.TryGetByGuid(clipboardEntry.Guid, out subFolder) ? subFolder : null;
                default:
                    return null;
            }
        }

        private void GenericAdd(IRedDotObject[] entries, string type)
        {
            if (entries == null || entries.Length == 0)
            {
                return;
            }

            const string ADD_ENTRY =
                @"<ADMINISTRATION><USER guid=""{0}""><CLIPBOARDDATA action=""add"" projectguid=""{1}"">{2}</CLIPBOARDDATA></USER></ADMINISTRATION>";

            var entryStr = string.Join("", entries.Select(x => SINGLE_ENTRY.RQLFormat(x, type)));

            var doc = Session.ExecuteRQL(ADD_ENTRY.RQLFormat(Session.CurrentUser, Project, entryStr), RQL.IODataFormat.SessionKeyAndLogonGuid);

            //if (!doc.InnerText.Contains("ok"))
            //{
            //    throw new SmartAPIException(Session.ServerLogin, "Error adding entries to clipboard");
            //}

            InvalidateCache();
        }

        private void GenericRemove(IProjectObject[] entries, string type)
        {
            if (entries == null || entries.Length == 0)
            {
                return;
            }

            const string REMOVE_ENTRY =
                @"<ADMINISTRATION><USER guid=""{0}""><CLIPBOARDDATA action=""remove"" projectguid=""{1}"">{2}</CLIPBOARDDATA></USER></ADMINISTRATION>";

            var entryStr = string.Join("", entries.Select(x => SINGLE_ENTRY.RQLFormat(x, type)));

            var doc = Project.ExecuteRQL(REMOVE_ENTRY.RQLFormat(Session.CurrentUser, Project, entryStr));

            if (!doc.InnerText.Contains("ok"))
            {
                throw new SmartAPIException(Session.ServerLogin, "Error removing entries from clipboard");
            }

            InvalidateCache();
        }
    }
}
