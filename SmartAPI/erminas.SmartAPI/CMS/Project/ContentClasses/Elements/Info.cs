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

using System.Xml;
using erminas.SmartAPI.CMS.Administration.Language;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface IInfo : IContentClassElement
    {
        /// <summary>
        ///     A list of all possible values for the Content property (=Project.InfoAttributes)
        /// </summary>
        IIndexedCachedList<int, InfoAttribute> AllContentAttributes { get; }

        InfoAttribute Content { get; set; }
        DateTimeFormat DateFormat { get; set; }
        bool IsNotConvertingCharactersToHtml { get; set; }
        bool IsUsingDataOfPageInTargetContainer { get; set; }
        bool IsUsingMainLink { get; set; }
        bool IsUsingRfc3066 { get; set; }
        ISystemLocale Locale { get; set; }
        IProjectVariant ProjectVariant { get; set; }
        string Separator { get; set; }
        string UserDefinedDateFormat { get; set; }
    }

    internal class Info : ContentClassElement, IInfo
    {
        internal Info(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltsubtype", "eltevalcalledpage", "eltuserfc3066", "eltkeywordseparator", "eltformatno",
                             "eltlcid", "eltformatting", "eltdonothtmlencode", "eltusemainlink", "eltprojectvariantguid",
                             "eltlanguagevariantguid");
        }

        /// <summary>
        ///     A list of all possible values for the Content property (=Project.InfoAttributes)
        /// </summary>
        public IIndexedCachedList<int, InfoAttribute> AllContentAttributes
        {
            get { return ContentClass.Project.InfoAttributes; }
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Meta; }
        }

        public InfoAttribute Content
        {
            get { return ((InfoElementAttribute) GetAttribute("eltsubtype")).Value; }
            set { ((InfoElementAttribute) GetAttribute("eltsubtype")).Value = value; }
        }

        public DateTimeFormat DateFormat
        {
            get { return ((DateTimeFormatAttribute) GetAttribute("eltformatno")).Value; }
            set { ((DateTimeFormatAttribute) GetAttribute("eltformatno")).Value = value; }
        }

        public bool IsNotConvertingCharactersToHtml
        {
            get { return GetAttributeValue<bool>("eltdonothtmlencode"); }
            set { SetAttributeValue("eltdonothtmlencode", value); }
        }

        public bool IsUsingDataOfPageInTargetContainer
        {
            get { return GetAttributeValue<bool>("eltevalcalledpage"); }
            set { SetAttributeValue("eltevalcalledpage", value); }
        }

        public bool IsUsingMainLink
        {
            get { return GetAttributeValue<bool>("eltusemainlink"); }
            set { SetAttributeValue("eltusemainlink", value); }
        }

        public bool IsUsingRfc3066
        {
            get { return GetAttributeValue<bool>("eltuserfc3066"); }
            set { SetAttributeValue("eltuserfc3066", value); }
        }

        public ISystemLocale Locale
        {
            get { return ((LocaleXmlNodeAttribute) GetAttribute("eltlcid")).Value; }
            set { ((LocaleXmlNodeAttribute) GetAttribute("eltlcid")).Value = value; }
        }

        public IProjectVariant ProjectVariant
        {
            get { return ((ProjectVariantAttribute) GetAttribute("eltprojectvariantguid")).Value; }
            set { ((ProjectVariantAttribute) GetAttribute("eltprojectvariantguid")).Value = value; }
        }

        public string Separator
        {
            get { return GetAttributeValue<string>("eltkeywordseparator"); }
            set { SetAttributeValue("eltkeywordseparator", value); }
        }

        public string UserDefinedDateFormat
        {
            get { return GetAttributeValue<string>("eltformatting"); }
            set { SetAttributeValue("eltformatting", value); }
        }
    }
}