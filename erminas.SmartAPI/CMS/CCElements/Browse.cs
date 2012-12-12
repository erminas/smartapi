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

using System;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.CCElements.Attributes;

namespace erminas.SmartAPI.CMS.CCElements
{
    public enum Direction
    {
        Forward = 0,
        Back = 1
    }

    public enum Appearance
    {
        StandardField = 0,
        Image = 1
    }

    public enum BrowseAlignment
    {
        NotSet = 0,
        // ReSharper disable InconsistentNaming
        top,
        bottom,
        middle
        // ReSharper restore InconsistentNaming
    }

    public static class BrowseAlignmentUtils
    {
        public static string ToRQLString(this BrowseAlignment align)
        {
            return align == BrowseAlignment.NotSet ? "" : align.ToString();
        }

        public static BrowseAlignment ToBrowseAlignment(this string value)
        {
            return string.IsNullOrEmpty(value)
                       ? BrowseAlignment.NotSet
                       : (BrowseAlignment) Enum.Parse(typeof (BrowseAlignment), value);
        }
    }

    public class Browse : CCElement
    {
        public Browse(ContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltlanguageindependent", "eltwidth", "eltheight", "eltborder", "eltvspace", "elthspace",
                             "eltusermap", "eltsupplement", "eltonlyhrefvalue", "eltxhtmlcompliant", "eltsrc", "eltalt",
                             "eltsrcsubdirguid", "eltrddescription", "eltrdexample");
            new EnumXmlNodeAttribute<Direction>(this, "eltdirection");
            new EnumXmlNodeAttribute<Appearance>(this, "eltnextpagetype");
            new StringXmlNodeAttribute(this, "eltdefaultvalue");
            new BoolXmlNodeAttribute(this, "eltpresetalt");
            new StringEnumXmlNodeAttribute<BrowseAlignment>(this, "eltalign", BrowseAlignmentUtils.ToRQLString,
                                                            BrowseAlignmentUtils.ToBrowseAlignment);
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Structural; }
        }

        public bool IsLanguageIndependent
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltlanguageindependent")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltlanguageindependent")).Value = value; }
        }

        public File SrcFile
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
                ((StringXmlNodeAttribute) GetAttribute("eltsrc")).Value = value != null ? value.Name : "";
                if (value != null)
                {
                    ((FolderXmlNodeAttribute) GetAttribute("eltsrcsubdirguid")).Value = value.Folder;
                }
            }
        }

        public File SampleImageFile
        {
            get
            {
                var folderAttr = (FolderXmlNodeAttribute) GetAttribute("eltsrcsubdirguid");
                string srcName = ((StringXmlNodeAttribute) GetAttribute("eltrdexample")).Value;
                if (folderAttr.Value == null || string.IsNullOrEmpty(srcName))
                {
                    return null;
                }
                return folderAttr.Value.GetFilesByNamePattern(srcName).First(x => x.Name == srcName);
            }

            set
            {
                ((StringXmlNodeAttribute) GetAttribute("eltrdexample")).Value = value != null ? value.Name : "";
                ((FolderXmlNodeAttribute) GetAttribute("eltsrcsubdirguid")).Value = value != null ? value.Folder : null;
            }
        }

        public string Supplement
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltsupplement")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltsupplement")).Value = value; }
        }

        public string Description
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltrddescription")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltrddescription")).Value = value; }
        }

        public string DefaultValue
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltdefaultvalue")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltdefaultvalue")).Value = value; }
        }

        public bool IsOnlyPathAndFilenameInserted
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltonlyhrefvalue")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltonlyhrefvalue")).Value = value; }
        }

        public bool IsSyntaxConformingToXHtml
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltxhtmlcompliant")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltxhtmlcompliant")).Value = value; }
        }

        public string Width
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltwidth")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltwidth")).Value = value; }
        }

        public string Height
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltheight")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltheight")).Value = value; }
        }

        public string Border
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltborder")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltborder")).Value = value; }
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

        public BrowseAlignment Align
        {
            get { return ((StringEnumXmlNodeAttribute<BrowseAlignment>) GetAttribute("eltalign")).Value; }
            set { ((StringEnumXmlNodeAttribute<BrowseAlignment>) GetAttribute("eltalign")).Value = value; }
        }

        public string Usemap
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltusermap")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltusermap")).Value = value; }
        }

        public bool IsAltPreassignedAutomatically
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltpresetalt")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltpresetalt")).Value = value; }
        }

        public string AltText
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltalt")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltalt")).Value = value; }
        }

        public Direction Direction
        {
            get { return ((EnumXmlNodeAttribute<Direction>) GetAttribute("eltdirection")).Value; }
            set { ((EnumXmlNodeAttribute<Direction>) GetAttribute("eltdirection")).Value = value; }
        }

        public Appearance Appearance
        {
            get { return ((EnumXmlNodeAttribute<Appearance>) GetAttribute("eltnextpagetype")).Value; }
            set { ((EnumXmlNodeAttribute<Appearance>) GetAttribute("eltnextpagetype")).Value = value; }
        }
    }
}