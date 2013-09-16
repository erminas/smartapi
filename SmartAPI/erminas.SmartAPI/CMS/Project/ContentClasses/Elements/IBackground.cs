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
    public interface IBackground : IContentClassElement
    {
        IFolder Folder { get; set; }

        bool IsDragAndDropActivated { get; set; }

        bool IsHiddenInProjectStructure { get; set; }

        bool IsLanguageIndependent { get; set; }

        bool IsNotRelevantForWorklow { get; set; }

        bool IsNotUsedInForm { get; set; }

        IFile SrcFile { get; set; }
    }

    internal class Background : ContentClassElement, IBackground
    {
        internal Background(IContentClass cc, XmlElement xmlElement) : base(cc, xmlElement)
        {
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Content; }
        }

        [RedDot("eltfolderguid", ConverterType = typeof (FolderConverter))]
        public IFolder Folder
        {
            get { return GetAttributeValue<IFolder>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltdragdrop")]
        public bool IsDragAndDropActivated
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltinvisibleinclient")]
        public bool IsHiddenInProjectStructure
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltlanguageindependent")]
        public bool IsLanguageIndependent
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltignoreworkflow")]
        public bool IsNotRelevantForWorklow
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("elthideinform")]
        public bool IsNotUsedInForm
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("__file", ConverterType = typeof (SrcFileConverter))]
        public IFile SrcFile
        {
            get { return GetAttributeValue<IFile>(); }

            set { SetAttributeValue(value); }
        }
    }
}