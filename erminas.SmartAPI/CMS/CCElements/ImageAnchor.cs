/*
 * Smart API - .Net programatical access to RedDot servers
 * Copyright (C) 2012  erminas GbR 
 *
 * This program is free software: you can redistribute it and/or modify it 
 * under the terms of the GNU General Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details. 
 *
 * You should have received a copy of the GNU General Public License along with this program.
 * If not, see <http://www.gnu.org/licenses/>. 
 */

using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.CCElements.Attributes;

namespace erminas.SmartAPI.CMS.CCElements
{
    public class ImageAnchor : Anchor
    {
        public ImageAnchor(ContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltwidth", "eltheight", "eltborder", "eltvspace",
                             "elthspace", "eltusermap", "eltautoheight",
                             "eltautowidth", "eltautoborder", "eltsrcsubdirguid",
                             "eltimagesupplement", "eltsrc", "eltalt");
            new BoolXmlNodeAttribute(this, "eltpresetalt");
            new StringEnumXmlNodeAttribute<ImageAlignment>(this, "eltalign", ImageAlignmentUtils.ToRQLString,
                                                           ImageAlignmentUtils.ToImageAlignment);
        }

        public File SrcFile
        {
            get
            {
                var folderAttr = (FolderXmlNodeAttribute) GetAttribute("eltsrcsubdirguid");
                var srcName = ((StringXmlNodeAttribute) GetAttribute("eltsrc")).Value;
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

        public string ImageLinkSupplement
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltimagesupplement")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltimagesupplement")).Value = value; }
        }

        public bool IsAltPreassignedAutomatically
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltpresetalt")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltpresetalt")).Value = value; }
        }

        public string VSpace
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltvspace")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltvspace")).Value = value; }
        }

        public string HSpace
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("elthspace")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("elthspace")).Value = value; }
        }

        public ImageAlignment Align
        {
            get { return ((StringEnumXmlNodeAttribute<ImageAlignment>) GetAttribute("eltalign")).Value; }
            set { ((StringEnumXmlNodeAttribute<ImageAlignment>) GetAttribute("eltalign")).Value = value; }
        }

        public string Usemap
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltusermap")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltusermap")).Value = value; }
        }

        public bool IsHeightAutomaticallyInsertedIntoPage
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltautoheight")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltautoheight")).Value = value; }
        }

        public bool IsWidthAutomaticallyInsertedIntoPage
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltautowidth")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltautowidth")).Value = value; }
        }

        public bool IsBorderAutomaticallyInsertedIntoPage
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltautoborder")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltautoborder")).Value = value; }
        }

        public string AltText
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltalt")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltalt")).Value = value; }
        }
    }
}