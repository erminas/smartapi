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
    public interface ISiteMap : IContentClassElement
    {
        string EndOfColumn { get; set; }

        string EndOfLine { get; set; }

        string EndOfTable { get; set; }

        SiteMapFormat Format { get; set; }

        bool IsSyntaxConformingToXHtml { get; set; }
        int? MaxErrorCount { get; set; }
        int? MaxSearchDepth { get; set; }
        int? NestingLevel { get; set; }

        string StartOfColumn { get; set; }

        string StartOfLine { get; set; }

        string StartOfTable { get; set; }
        IFile XslStyleSheet { get; set; }
    }

    internal class SiteMap : ContentClassElement, ISiteMap
    {
        internal SiteMap(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Structural; }
        }

        [RedDot("eltcolclose")]
        public string EndOfColumn
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltrowclose")]
        public string EndOfLine
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("elttableclose")]
        public string EndOfTable
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltformat", ConverterType = typeof (EnumConverter<SiteMapFormat>))]
        public SiteMapFormat Format
        {
            get { return GetAttributeValue<SiteMapFormat>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltxhtmlcompliant")]
        public bool IsSyntaxConformingToXHtml
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltdropouts")]
        public int? MaxErrorCount
        {
            get { return GetAttributeValue<int?>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltsearchdepth")]
        public int? MaxSearchDepth
        {
            get { return GetAttributeValue<int?>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltdepth")]
        public int? NestingLevel
        {
            get { return GetAttributeValue<int?>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltcolopen")]
        public string StartOfColumn
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltrowopen")]
        public string StartOfLine
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("elttableopen")]
        public string StartOfTable
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("__xslstylesheet", ConverterType = typeof (XslFileConverter))]
        public IFile XslStyleSheet
        {
            get { return GetAttributeValue<IFile>(); }
            set { SetAttributeValue(value); }
        }
    }

    public enum SiteMapFormat
    {
        HTMLCode = 0,
        XMLStructure = 1,
    }
}