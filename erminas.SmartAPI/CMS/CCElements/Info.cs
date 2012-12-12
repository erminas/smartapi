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

using System.Xml;
using erminas.SmartAPI.CMS.CCElements.Attributes;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.CCElements
{
    public class Info : CCElement
    {
        public Info(ContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltsubtype", "eltevalcalledpage", "eltuserfc3066", "eltkeywordseparator", "eltformatno",
                             "eltlcid", "eltformatting", "eltdonothtmlencode", "eltusemainlink", "eltprojectvariantguid",
                             "eltlanguagevariantguid");
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Meta; }
        }

        public Locale Locale
        {
            get { return ((LocaleXmlNodeAttribute) GetAttribute("eltlcid")).Value; }
            set { ((LocaleXmlNodeAttribute) GetAttribute("eltlcid")).Value = value; }
        }

        public DateTimeFormat DateFormat
        {
            get { return ((DateTimeFormatAttribute) GetAttribute("eltformatno")).Value; }
            set { ((DateTimeFormatAttribute) GetAttribute("eltformatno")).Value = value; }
        }

        public bool IsUsingRfc3066
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltuserfc3066")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltuserfc3066")).Value = value; }
        }

        /// <summary>
        ///   A list of all possible values for the Content property (=Project.InfoAttributes)
        /// </summary>
        public IIndexedCachedList<int, InfoAttribute> AllContentAttributes
        {
            get { return ContentClass.Project.InfoAttributes; }
        }

        public InfoAttribute Content
        {
            get { return ((InfoElementAttribute) GetAttribute("eltsubtype")).Value; }
            set { ((InfoElementAttribute) GetAttribute("eltsubtype")).Value = value; }
        }

        public bool IsUsingDataOfPageInTargetContainer
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltevalcalledpage")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltevalcalledpage")).Value = value; }
        }

        public string Separator
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltkeywordseparator")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltkeywordseparator")).Value = value; }
        }

        public string UserDefinedDateFormat
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltformatting")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltformatting")).Value = value; }
        }

        public bool IsNotConvertingCharactersToHtml
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltdonothtmlencode")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltdonothtmlencode")).Value = value; }
        }

        public bool IsUsingMainLink
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltusemainlink")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltusemainlink")).Value = value; }
        }

        public ProjectVariant ProjectVariant
        {
            get { return ((ProjectVariantAttribute) GetAttribute("eltprojectvariantguid")).Value; }
            set { ((ProjectVariantAttribute) GetAttribute("eltprojectvariantguid")).Value = value; }
        }
    }
}