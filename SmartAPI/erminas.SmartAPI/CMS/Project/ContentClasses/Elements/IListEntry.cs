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

using System.Xml;
using erminas.SmartAPI.CMS.Converter;
using erminas.SmartAPI.CMS.Project.Folder;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface IListEntry : IContentClassElement
    {
        string EndTagForAutomaticProcessing { get; set; }

        IFolder Folder { get; set; }

        bool IsNotUsedInForm { get; set; }

        bool IsNotVisibleOnPublishedPage { get; set; }

        bool IsUsingEntireTextIfNoMatchingTagsCanBeFound { get; set; }

        string StartTagForAutomaticProcessing { get; set; }
    }

    internal class ListEntry : ContentClassElement, IListEntry
    {
        internal ListEntry(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Content; }
        }

        [RedDot("eltendmark")]
        public string EndTagForAutomaticProcessing
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltfolderguid", ConverterType = typeof (FolderConverter))]
        public IFolder Folder
        {
            get { return GetAttributeValue<IFolder>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("elthideinform")]
        public bool IsNotUsedInForm
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltinvisibleinpage")]
        public bool IsNotVisibleOnPublishedPage
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltwholetext")]
        public bool IsUsingEntireTextIfNoMatchingTagsCanBeFound
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltbeginmark")]
        public string StartTagForAutomaticProcessing
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }
    }
}