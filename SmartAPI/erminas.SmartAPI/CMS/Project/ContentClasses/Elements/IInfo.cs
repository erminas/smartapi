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
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface IInfo : IContentClassElement
    {
        /// <summary>
        ///     A list of all possible values for the Content property (=Project.InfoAttributes)
        /// </summary>
        IIndexedCachedList<int, IInfoAttribute> AllContentAttributes { get; }

        IInfoAttribute Content { get; set; }
        IDateTimeFormat DateFormat { get; set; }
        bool IsNotConvertingCharactersToHtml { get; set; }
        bool IsUsingDataOfPageInTargetContainer { get; set; }
        bool IsUsingMainLink { get; set; }
        bool IsUsingRfc3066 { get; set; }
        ILanguageVariant LanguageVariantForUrlOfPage { get; set; }
        ISystemLocale Locale { get; set; }
        IProjectVariant ProjectVariantForUrlOfPage { get; set; }
        string Separator { get; set; }
        string UserDefinedDateFormat { get; set; }
    }

    internal class Info : ContentClassElement, IInfo
    {
        internal Info(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
        }

        /// <summary>
        ///     A list of all possible values for the Content property (=Project.InfoAttributes)
        /// </summary>
        public IIndexedCachedList<int, IInfoAttribute> AllContentAttributes
        {
            get { return ContentClass.Project.InfoAttributes; }
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Meta; }
        }

        [RedDot("eltsubtype", ConverterType = typeof (InfoElementConverter))]
        public IInfoAttribute Content
        {
            get { return GetAttributeValue<IInfoAttribute>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltformatno", ConverterType = typeof (DateTimeFormatConverter))]
        public IDateTimeFormat DateFormat
        {
            get { return GetAttributeValue<IDateTimeFormat>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltdonothtmlencode")]
        public bool IsNotConvertingCharactersToHtml
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltevalcalledpage")]
        public bool IsUsingDataOfPageInTargetContainer
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltusemainlink")]
        public bool IsUsingMainLink
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltuserfc3066")]
        public bool IsUsingRfc3066
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltlanguagevariantguid")]
        public ILanguageVariant LanguageVariantForUrlOfPage
        {
            get { return GetAttributeValue<ILanguageVariant>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltlcid", ConverterType = typeof (LocaleConverter))]
        public ISystemLocale Locale
        {
            get { return GetAttributeValue<ISystemLocale>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltprojectvariantguid", ConverterType = typeof (ProjectVariantConverter))]
        public IProjectVariant ProjectVariantForUrlOfPage
        {
            get { return GetAttributeValue<IProjectVariant>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltkeywordseparator")]
        public string Separator
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltformatting")]
        public string UserDefinedDateFormat
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }
    }
}