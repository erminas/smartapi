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
using System.Xml;
using erminas.SmartAPI.CMS.CCElements.Attributes;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.CCElements
{
    public enum ListType
    {
        None = 0,
        Supplement,
        DisplayAsLink
    }

    public static class ListTypeUtils
    {
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
                    throw new ArgumentException(string.Format("Unknown {0} value: {1}",
                                                              typeof (ListType).Name, type));
            }
        }

        public static ListType ToListType(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return ListType.None;
            }
            switch (value.ToLowerInvariant())
            {
                case "issupplement":
                    return ListType.Supplement;
                case "linksintext":
                    return ListType.DisplayAsLink;
                default:
                    throw new ArgumentException(string.Format("Cannot convert string value {1} to {0}",
                                                              typeof (ListType).Name, value));
            }
        }
    }

    public class DatabaseContent : CCElement
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
                return new DatabaseConnectionXmlNodeAttribute((CCElement) element, name);
            }
        }

        #endregion

        public DatabaseContent(ContentClass cc, XmlNode node)
            : base(cc, node)
        {
            CreateAttributes("eltislistentry", "eltlisttype", "eltdatabasename",
                             "elttablename", "eltcolumnname", "eltcolumniotype",
                             "eltrelatedfolderguid", "eltformatting", "eltbincolumnname",
                             "eltborder", "eltvspace", "elthspace", "eltsupplement", "eltalt");
            new StringEnumXmlNodeAttribute<BasicAlignment>(this, "eltalign", BasicAlignmentUtils.ToRQLString,
                                                           BasicAlignmentUtils.ToBasicAlignment);

            //We need to add eltsrc with sessionkey, because otherwise eltalt won't get stored (setting alt through the smart tree doesn't work for that reason).
            XmlNode.SetAttributeValue("eltsrc", Session.SESSIONKEY_PLACEHOLDER);
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Content; }
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

        public BasicAlignment Align
        {
            get { return ((StringEnumXmlNodeAttribute<BasicAlignment>) GetAttribute("eltalign")).Value; }
            set { ((StringEnumXmlNodeAttribute<BasicAlignment>) GetAttribute("eltalign")).Value = value; }
        }

        public string Supplement
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltsupplement")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltsupplement")).Value = value; }
        }

        /// <summary>
        ///   The alt text. While for RedDot Versions 9/10/11 the alt text doesn't get set through the smart tree (alt always remains empty), this method actually works.
        /// </summary>
        public string AltText
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltalt")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltalt")).Value = value; }
        }

        public bool IsListEntry
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltislistentry")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltislistentry")).Value = value; }
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
                                      RDXmlNodeAttribute.ElementDescription["eltlisttype"], ListType.None));
                }
                ((StringEnumXmlNodeAttribute<ListType>) GetAttribute("eltlisttype")).Value = value;
            }
        }

        public DatabaseConnection DatabaseConnection
        {
            get { return ((DatabaseConnectionXmlNodeAttribute) GetAttribute("eltdatabasename")).Value; }
            set { ((DatabaseConnectionXmlNodeAttribute) GetAttribute("eltdatabasename")).Value = value; }
        }

        public string TableName
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("elttablename")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("elttablename")).Value = value; }
        }

        public string DataFieldName
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltcolumnname")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltcolumnname")).Value = value; }
        }

        public string DataFieldForBinayData
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltbincolumnname")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltbincolumnname")).Value = value; }
        }

        //todo implement as enum/special type
        public string DataFieldType
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltcolumniotype")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltcolumniotype")).Value = value; }
        }

        public Folder PublicationFolder
        {
            get { return ((FolderXmlNodeAttribute) GetAttribute("eltrelatedfolderguid")).Value; }
            set { ((FolderXmlNodeAttribute) GetAttribute("eltrelatedfolderguid")).Value = value; }
        }

        public string UserDefinedFormat
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltformatting")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltformatting")).Value = value; }
        }
    }
}