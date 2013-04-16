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
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes;
using erminas.SmartAPI.CMS.Project.Filesystem;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
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
        public static BrowseAlignment ToBrowseAlignment(this string value)
        {
            return string.IsNullOrEmpty(value)
                       ? BrowseAlignment.NotSet
                       : (BrowseAlignment) Enum.Parse(typeof (BrowseAlignment), value);
        }

        public static string ToRQLString(this BrowseAlignment align)
        {
            return align == BrowseAlignment.NotSet ? "" : align.ToString();
        }
    }

    public interface IBrowse : IContentClassElement
    {
        BrowseAlignment Align { get; set; }
        string AltText { get; set; }
        Appearance Appearance { get; set; }
        string Border { get; set; }
        string DefaultValue { get; set; }
        string Description { get; set; }
        Direction Direction { get; set; }
        string HSpace { get; set; }
        string Height { get; set; }
        bool IsAltPreassignedAutomatically { get; set; }
        bool IsLanguageIndependent { get; set; }
        bool IsOnlyPathAndFilenameInserted { get; set; }
        bool IsSyntaxConformingToXHtml { get; set; }
        IFile SampleImageFile { get; set; }
        IFile SrcFile { get; set; }
        string Supplement { get; set; }
        string Usemap { get; set; }
        string VSpace { get; set; }
        string Width { get; set; }
    }

    internal class Browse : ContentClassElement, IBrowse
    {
        internal Browse(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltlanguageindependent", "eltwidth", "eltheight", "eltborder", "eltvspace", "elthspace",
                             "eltusermap", "eltsupplement", "eltonlyhrefvalue", "eltxhtmlcompliant", "eltsrc", "eltalt",
                             "eltsrcsubdirguid", "eltrddescription", "eltrdexample");
// ReSharper disable ObjectCreationAsStatement
            new EnumXmlNodeAttribute<Direction>(this, "eltdirection");
            new EnumXmlNodeAttribute<Appearance>(this, "eltnextpagetype");
            new StringXmlNodeAttribute(this, "eltdefaultvalue");
            new BoolXmlNodeAttribute(this, "eltpresetalt");
            new StringEnumXmlNodeAttribute<BrowseAlignment>(this, "eltalign", BrowseAlignmentUtils.ToRQLString,
                                                            BrowseAlignmentUtils.ToBrowseAlignment);
// ReSharper restore ObjectCreationAsStatement
        }

        public BrowseAlignment Align
        {
            get { return ((StringEnumXmlNodeAttribute<BrowseAlignment>) GetAttribute("eltalign")).Value; }
            set { ((StringEnumXmlNodeAttribute<BrowseAlignment>) GetAttribute("eltalign")).Value = value; }
        }

        public string AltText
        {
            get { return GetAttributeValue<string>("eltalt"); }
            set { SetAttributeValue("eltalt", value); }
        }

        public Appearance Appearance
        {
            get { return ((EnumXmlNodeAttribute<Appearance>) GetAttribute("eltnextpagetype")).Value; }
            set { ((EnumXmlNodeAttribute<Appearance>) GetAttribute("eltnextpagetype")).Value = value; }
        }

        public string Border
        {
            get { return GetAttributeValue<string>("eltborder"); }
            set { SetAttributeValue("eltborder", value); }
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Structural; }
        }

        public string DefaultValue
        {
            get { return GetAttributeValue<string>("eltdefaultvalue"); }
            set { SetAttributeValue("eltdefaultvalue", value); }
        }

        public string Description
        {
            get { return GetAttributeValue<string>("eltrddescription"); }
            set { SetAttributeValue("eltrddescription", value); }
        }

        public Direction Direction
        {
            get { return ((EnumXmlNodeAttribute<Direction>) GetAttribute("eltdirection")).Value; }
            set { ((EnumXmlNodeAttribute<Direction>) GetAttribute("eltdirection")).Value = value; }
        }

        public string HSpace
        {
            get { return GetAttributeValue<string>("elthspace"); }
            set { SetAttributeValue("elthspace", value); }
        }

        public string Height
        {
            get { return GetAttributeValue<string>("eltheight"); }
            set { SetAttributeValue("eltheight", value); }
        }

        public bool IsAltPreassignedAutomatically
        {
            get { return GetAttributeValue<bool>("eltpresetalt"); }
            set { SetAttributeValue("eltpresetalt", value); }
        }

        public bool IsLanguageIndependent
        {
            get { return GetAttributeValue<bool>("eltlanguageindependent"); }
            set { SetAttributeValue("eltlanguageindependent", value); }
        }

        public bool IsOnlyPathAndFilenameInserted
        {
            get { return GetAttributeValue<bool>("eltonlyhrefvalue"); }
            set { SetAttributeValue("eltonlyhrefvalue", value); }
        }

        public bool IsSyntaxConformingToXHtml
        {
            get { return GetAttributeValue<bool>("eltxhtmlcompliant"); }
            set { SetAttributeValue("eltxhtmlcompliant", value); }
        }

        public IFile SampleImageFile
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
                ((StringXmlNodeAttribute) GetAttribute("eltsrc")).Value = value != null ? value.Name : "";
                if (value != null)
                {
                    ((FolderXmlNodeAttribute) GetAttribute("eltsrcsubdirguid")).Value = value.Folder;
                }
            }
        }

        public string Supplement
        {
            get { return GetAttributeValue<string>("eltsupplement"); }
            set { SetAttributeValue("eltsupplement", value); }
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

        public string Width
        {
            get { return GetAttributeValue<string>("eltwidth"); }
            set { SetAttributeValue("eltwidth", value); }
        }
    }
}