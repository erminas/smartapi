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
using System.Xml;
using erminas.SmartAPI.CMS.Administration;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes;
using erminas.SmartAPI.CMS.Project.Filesystem;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public enum ListType
    {
        None = 0,
        Supplement,
        DisplayAsLink
    }

    public static class ListTypeUtils
    {
        public static ListType ToListType(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return ListType.None;
            }
            switch (value.ToUpperInvariant())
            {
                case "ISSUPPLEMENT":
                    return ListType.Supplement;
                case "LINKSINTEXT":
                    return ListType.DisplayAsLink;
                default:
                    throw new ArgumentException(string.Format("Cannot convert string value {1} to {0}",
                                                              typeof (ListType).Name, value));
            }
        }

        public static string ToRQLString(this ListType type)
        {
            switch (type)
            {
                case ListType.None:
                    return "";
                case ListType.Supplement:
                    return "issupplement";
                case ListType.DisplayAsLink:
                    return "linksintext";
                default:
                    throw new ArgumentException(string.Format("Unknown {0} value: {1}", typeof (ListType).Name, type));
            }
        }
    }

    public class DatabaseContent : ContentClassElement
    {
        #region StaticAttributeInit

        static DatabaseContent()
        {
            AttributeFactory.AddFactory("eltdatabasename", new DatabaseConnectionAttributeFactory());
            AttributeFactory.AddFactory("eltcolumniotype", new StringAttributeFactory());
        }

        private class DatabaseConnectionAttributeFactory : AttributeFactory
        {
            protected override RDXmlNodeAttribute CreateAttributeInternal(IAttributeContainer element, string name)
            {
                return new DatabaseConnectionXmlNodeAttribute((ContentClassElement) element, name);
            }
        }

        #endregion

        internal DatabaseContent(ContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltislistentry", "eltlisttype", "eltdatabasename", "elttablename", "eltcolumnname",
                             "eltcolumniotype", "eltrelatedfolderguid", "eltformatting", "eltbincolumnname", "eltborder",
                             "eltvspace", "elthspace", "eltsupplement", "eltalt");
// ReSharper disable ObjectCreationAsStatement
            new StringEnumXmlNodeAttribute<BasicAlignment>(this, "eltalign", BasicAlignmentUtils.ToRQLString,
                                                           // ReSharper restore ObjectCreationAsStatement
                                                           BasicAlignmentUtils.ToBasicAlignment);

            //We need to add eltsrc with sessionkey, because otherwise eltalt won't get stored (setting alt through the smart tree doesn't work for that reason).
            XmlElement.SetAttributeValue("eltsrc", Session.SESSIONKEY_PLACEHOLDER);
        }

        public BasicAlignment Align
        {
            get { return ((StringEnumXmlNodeAttribute<BasicAlignment>) GetAttribute("eltalign")).Value; }
            set { ((StringEnumXmlNodeAttribute<BasicAlignment>) GetAttribute("eltalign")).Value = value; }
        }

        /// <summary>
        ///     The alt text. While for RedDot Versions 9/10/11 the alt text doesn't get set through the smart tree (alt always remains empty), this method actually works.
        /// </summary>
        public string AltText
        {
            get { return GetAttributeValue<string>("eltalt"); }
            set
            {
                SetAttributeValue("eltalt", value);
            }
        }

        public string Border
        {
            get { return GetAttributeValue<string>("eltborder"); }
            set
            {
                SetAttributeValue("eltborder", value);
            }
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Content; }
        }

        public string DataFieldForBinayData
        {
            get { return GetAttributeValue<string>("eltbincolumnname"); }
            set
            {
                SetAttributeValue("eltbincolumnname", value);
            }
        }

        public string DataFieldName
        {
            get { return GetAttributeValue<string>("eltcolumnname"); }
            set
            {
                SetAttributeValue("eltcolumnname", value);
            }
        }

        //todo implement as enum/special type
        public string DataFieldType
        {
            get { return GetAttributeValue<string>("eltcolumniotype"); }
            set
            {
                SetAttributeValue("eltcolumniotype", value);
            }
        }

        public DatabaseConnection DatabaseConnection
        {
            get { return ((DatabaseConnectionXmlNodeAttribute) GetAttribute("eltdatabasename")).Value; }
            set { ((DatabaseConnectionXmlNodeAttribute) GetAttribute("eltdatabasename")).Value = value; }
        }

        public string HSpace
        {
            get { return GetAttributeValue<string>("elthspace"); }
            set
            {
                SetAttributeValue("elthspace", value);
            }
        }

        public bool IsListEntry
        {
            get { return GetAttributeValue<bool>("eltislistentry"); }
            set { SetAttributeValue("eltislistentry", value); }
        }

        public ListType ListType
        {
            get { return ((StringEnumXmlNodeAttribute<ListType>) GetAttribute("eltlisttype")).Value; }
            set
            {
                if (value == ListType.None)
                {
                    throw new ArgumentException(
                        string.Format("It is not possible to reset the {0} to {1}, once it was set.",
                                      RDXmlNodeAttribute.ELEMENT_DESCRIPTION["eltlisttype"], ListType.None));
                }
                ((StringEnumXmlNodeAttribute<ListType>) GetAttribute("eltlisttype")).Value = value;
            }
        }

        public Folder PublicationFolder
        {
            get { return GetAttributeValue<Folder>("eltrelatedfolderguid"); }
            set { SetAttributeValue("eltrelatedfolderguid", value); }
        }

        public string Supplement
        {
            get { return GetAttributeValue<string>("eltsupplement"); }
            set
            {
                SetAttributeValue("eltsupplement", value);
            }
        }

        public string TableName
        {
            get { return GetAttributeValue<string>("elttablename"); }
            set
            {
                SetAttributeValue("elttablename", value);
            }
        }

        public string UserDefinedFormat
        {
            get { return GetAttributeValue<string>("eltformatting"); }
            set
            {
                SetAttributeValue("eltformatting", value);
            }
        }

        public string VSpace
        {
            get { return GetAttributeValue<string>("eltvspace"); }
            set
            {
                SetAttributeValue("eltvspace", value);
            }
        }
    }
}