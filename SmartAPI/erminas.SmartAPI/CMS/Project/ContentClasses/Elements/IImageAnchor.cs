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

using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes;
using erminas.SmartAPI.CMS.Project.Filesystem;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface IImageAnchor : IAnchor
    {
        ImageAlignment Align { get; set; }
        string AltText { get; set; }
        string HSpace { get; set; }
        string ImageLinkSupplement { get; set; }
        bool IsAltPreassignedAutomatically { get; set; }
        bool IsBorderAutomaticallyInsertedIntoPage { get; set; }
        bool IsHeightAutomaticallyInsertedIntoPage { get; set; }
        bool IsWidthAutomaticallyInsertedIntoPage { get; set; }
        IFile SrcFile { get; set; }
        string Usemap { get; set; }
        string VSpace { get; set; }
    }

    internal class ImageAnchor : Anchor, IImageAnchor
    {
        internal ImageAnchor(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltwidth", "eltheight", "eltborder", "eltvspace", "elthspace", "eltusermap",
                             "eltautoheight", "eltautowidth", "eltfolderguid", "eltautoborder", "eltsrcsubdirguid",
                             "eltimagesupplement", "eltsrc", "eltalt");
// ReSharper disable ObjectCreationAsStatement
            new BoolXmlNodeAttribute(this, "eltpresetalt");
            new StringEnumXmlNodeAttribute<ImageAlignment>(this, "eltalign", ImageAlignmentUtils.ToRQLString,
                                                           ImageAlignmentUtils.ToImageAlignment);
// ReSharper restore ObjectCreationAsStatement
        }

        public ImageAlignment Align
        {
            get { return ((StringEnumXmlNodeAttribute<ImageAlignment>) GetAttribute("eltalign")).Value; }
            set { ((StringEnumXmlNodeAttribute<ImageAlignment>) GetAttribute("eltalign")).Value = value; }
        }

        public string AltText
        {
            get { return GetAttributeValue<string>("eltalt"); }
            set { SetAttributeValue("eltalt", value); }
        }

        public string HSpace
        {
            get { return GetAttributeValue<string>("elthspace"); }
            set { SetAttributeValue("elthspace", value); }
        }

        public string ImageLinkSupplement
        {
            get { return GetAttributeValue<string>("eltimagesupplement"); }
            set { SetAttributeValue("eltimagesupplement", value); }
        }

        public bool IsAltPreassignedAutomatically
        {
            get { return GetAttributeValue<bool>("eltpresetalt"); }
            set { SetAttributeValue("eltpresetalt", value); }
        }

        public bool IsBorderAutomaticallyInsertedIntoPage
        {
            get { return GetAttributeValue<bool>("eltautoborder"); }
            set { SetAttributeValue("eltautoborder", value); }
        }

        public bool IsHeightAutomaticallyInsertedIntoPage
        {
            get { return GetAttributeValue<bool>("eltautoheight"); }
            set { SetAttributeValue("eltautoheight", value); }
        }

        public bool IsWidthAutomaticallyInsertedIntoPage
        {
            get { return GetAttributeValue<bool>("eltautowidth"); }
            set { SetAttributeValue("eltautowidth", value); }
        }

        public IFile SrcFile
        {
            get
            {
                var folderAttr = (FolderXmlNodeAttribute) GetAttribute("eltsrcsubdirguid");
                string srcName = ((StringXmlNodeAttribute) GetAttribute("eltsrc")).Value;
                if (folderAttr.Value == null || string.IsNullOrEmpty(srcName))
                {
                    return null;
                }
                return folderAttr.Value.GetFilesByNamePattern(srcName).First(x => x.Name == srcName);
            }

            set
            {
                ((StringXmlNodeAttribute) GetAttribute("eltsrc")).Value = value.Name;
                ((FolderXmlNodeAttribute) GetAttribute("eltsrcsubdirguid")).Value = value.Folder;
            }
        }

        public string Usemap
        {
            get { return GetAttributeValue<string>("eltusermap"); }
            set { SetAttributeValue("eltusermap", value); }
        }

        public string VSpace
        {
            get { return GetAttributeValue<string>("eltvspace"); }
            set { SetAttributeValue("eltvspace", value); }
        }
    }
}