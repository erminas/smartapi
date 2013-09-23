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

using System;
using System.Xml;
using erminas.SmartAPI.CMS.Converter;
using erminas.SmartAPI.CMS.Project.Folder;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface IDatabaseContent : IContentClassElement
    {
        BasicAlignment Align { get; set; }

        /// <summary>
        ///     The alt text. While for RedDot Versions 9/10/11 the alt text doesn't get set through the smart tree (alt always remains empty), this method actually works.
        /// </summary>
        string AltText { get; set; }

        string Border { get; set; }
        string DataFieldForBinayData { get; set; }
        string DataFieldName { get; set; }
        IDatabaseConnection DatabaseConnection { get; set; }
        string HSpace { get; set; }
        bool IsListEntry { get; set; }
        ListType ListType { get; set; }
        IFolder PublicationFolder { get; set; }
        SpecialDataFieldFormat SpecialDataFieldFormat { get; set; }
        string Supplement { get; set; }
        string TableName { get; set; }
        string UserDefinedFormat { get; set; }
        string VSpace { get; set; }
    }

    internal class DatabaseContent : ContentClassElement, IDatabaseContent
    {
        internal DatabaseContent(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            //We need to add eltsrc with sessionkey, because otherwise eltalt won't get stored (setting alt through the smart tree doesn't work for that reason).
            XmlElement.SetAttributeValue("eltsrc", RQL.SESSIONKEY_PLACEHOLDER);
        }

        [RedDot("eltalign", ConverterType = typeof (StringEnumConverter<BasicAlignment>))]
        public BasicAlignment Align
        {
            get { return GetAttributeValue<BasicAlignment>(); }
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

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Content; }
        }

        [RedDot("eltbincolumnname")]
        public string DataFieldForBinayData
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltcolumnname")]
        public string DataFieldName
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltdatabasename", ConverterType = typeof (DatabaseConnectionConverter))]
        public IDatabaseConnection DatabaseConnection
        {
            get { return GetAttributeValue<IDatabaseConnection>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("elthspace")]
        public string HSpace
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltislistentry")]
        public bool IsListEntry
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltlisttype", ConverterType = typeof (StringEnumConverter<ListType>))]
        public ListType ListType
        {
            get { return GetAttributeValue<ListType>(); }
            set
            {
                if (value == ListType.None)
                {
                    throw new ArgumentException(
                        string.Format("It is not possible to reset the {0} to {1}, once it was set.",
                                      RedDotAttributeDescription.GetDescriptionForElement("eltlisttype"), ListType.None));
                }
                SetAttributeValue(value);
            }
        }

        [RedDot("eltrelatedfolderguid", ConverterType = typeof (FolderConverter))]
        public IFolder PublicationFolder
        {
            get { return GetAttributeValue<IFolder>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltcolumniotype", ConverterType = typeof (StringEnumConverter<SpecialDataFieldFormat>))]
        public SpecialDataFieldFormat SpecialDataFieldFormat
        {
            get { return GetAttributeValue<SpecialDataFieldFormat>(); }
            set
            {
                SpecialDataFieldFormatUtils.EnsureValueIsSupportedByServerVersion(this, value);
                SetAttributeValue(value);
            }
        }

        [RedDot("eltsupplement")]
        public string Supplement
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("elttablename")]
        public string TableName
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltformatting")]
        public string UserDefinedFormat
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

        protected override XmlElement RetrieveWholeObject()
        {
            var element = base.RetrieveWholeObject();
            //We need to add eltsrc with sessionkey, because otherwise eltalt won't get stored (setting alt through the smart tree doesn't work for that reason).
            element.SetAttributeValue("eltsrc", RQL.SESSIONKEY_PLACEHOLDER);

            return element;
        }
    }
}