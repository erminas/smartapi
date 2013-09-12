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
    public interface IImageAnchor : IAnchor
    {
        ImageAlignment Align { get; set; }

        string AltText { get; set; }

        string Border { get; set; }

        IFolder Folder { get; set; }

        string HSpace { get; set; }

        string Height { get; set; }

        string ImageLinkSupplement { get; set; }

        bool IsAltPreassignedAutomatically { get; set; }

        bool IsBorderAutomaticallyInsertedIntoPage { get; set; }

        bool IsHeightAutomaticallyInsertedIntoPage { get; set; }

        bool IsWidthAutomaticallyInsertedIntoPage { get; set; }

        IFile SrcFile { get; set; }

        string Usemap { get; set; }

        string VSpace { get; set; }

        string Width { get; set; }
    }

    internal class ImageAnchor : Anchor, IImageAnchor
    {
        internal ImageAnchor(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
        }

        [RedDot("eltalign", ConverterType = typeof (StringEnumConverter<ImageAlignment>))]
        public ImageAlignment Align
        {
            get { return GetAttributeValue<ImageAlignment>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltalt")]
        public string AltText
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltborder")]
        public string Border
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

        [RedDot("elthspace")]
        public string HSpace
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltheight")]
        public string Height
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltimagesupplement")]
        public string ImageLinkSupplement
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltpresetalt")]
        public bool IsAltPreassignedAutomatically
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltautoborder")]
        public bool IsBorderAutomaticallyInsertedIntoPage
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltautoheight")]
        public bool IsHeightAutomaticallyInsertedIntoPage
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltautowidth")]
        public bool IsWidthAutomaticallyInsertedIntoPage
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("__srcfile", ConverterType = typeof (SrcFileConverter))]
        public IFile SrcFile
        {
            get { return GetAttributeValue<IFile>(); }

            set { SetAttributeValue(value); }
        }

        [RedDot("eltusermap")]
        public string Usemap
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltvspace")]
        public string VSpace
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltwidth")]
        public string Width
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }
    }
}