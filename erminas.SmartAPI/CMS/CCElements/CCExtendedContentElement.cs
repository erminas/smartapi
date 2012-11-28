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
    public abstract class CCExtendedContentElement : CCContentElement
    {
        protected CCExtendedContentElement(ContentClass contentClass, XmlElement xmlElement)
            : base(contentClass, xmlElement)
        {
            CreateAttributes("eltbeginmark", "eltendmark", "eltrddescription");
        }

        public string StartTagForAutomaticProcessing
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltbeginmark")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltbeginmark")).Value = value; }
        }

        public string EndTagForAutomaticProcessing
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltendmark")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltendmark")).Value = value; }
        }

        public string Description
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltrddescription")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltrddescription")).Value = value; }
        }
    }
}