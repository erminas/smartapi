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
        [RedDot("eltfolderguid", ConverterType = typeof (FolderConverter))]
        IFolder Folder { get; set; }

        [RedDot("eltdragdrop")]
        bool IsDragAndDropActivated { get; set; }

        [RedDot("eltinvisibleinclient")]
        bool IsHiddenInProjectStructure { get; set; }

        [RedDot("eltlanguageindependent")]
        bool IsLanguageIndependent { get; set; }

        [RedDot("eltignoreworkflow")]
        bool IsNotRelevantForWorklow { get; set; }

        [RedDot("elthideinform")]
        bool IsNotUsedInForm { get; set; }

        [RedDot("__file", ConverterType = typeof (SrcFileConverter))]
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

        public IFolder Folder
        {
            get { return GetAttributeValue<IFolder>(); }
            set { SetAttributeValue(value); }
        }

        public bool IsDragAndDropActivated
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        public bool IsHiddenInProjectStructure
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        public bool IsLanguageIndependent
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        public bool IsNotRelevantForWorklow
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        public bool IsNotUsedInForm
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        public IFile SrcFile
        {
            get { return GetAttributeValue<IFile>(); }

            set { SetAttributeValue(value); }
        }
    }
}