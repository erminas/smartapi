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

namespace erminas.SmartAPI.CMS.CCElements
{
    public abstract class CCContentElement : CCElement
    {
        protected CCContentElement(ContentClass contentClass, XmlNode xmlNode) : base(contentClass, xmlNode)
        {
            CreateAttributes("eltignoreworkflow", "eltlanguageindependent", "eltrequired",
                             "eltinvisibleinclient", "eltinvisibleinpage", "elthideinform",
                             "eltdonothtmlencode");
        }

        public override sealed ContentClassCategory Category
        {
            get { return ContentClassCategory.Content; }
        }

        public bool IsNotRelevantForWorklow
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltignoreworkflow")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltignoreworkflow")).Value = value; }
        }

        public bool IsLanguageIndependent
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltlanguageindependent")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltlanguageindependent")).Value = value; }
        }

        public bool IsEditingMandatory
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltrequired")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltrequired")).Value = value; }
        }

        public bool IsHiddenInProjectStructure
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltinvisibleinclient")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltinvisibleinclient")).Value = value; }
        }

        public bool IsNotVisibleOnPublishedPage
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltinvisibleinpage")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltinvisibleinpage")).Value = value; }
        }

        public bool IsNotUsedInForm
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("elthideinform")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("elthideinform")).Value = value; }
        }

        public bool IsNotConvertingCharactersToHtml
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltdonothtmlencode")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltdonothtmlencode")).Value = value; }
        }
    }
}