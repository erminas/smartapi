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
    public class Headline : CCElement
    {
        public Headline(ContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltignoreworkflow", "eltlanguageindependent", "eltdonothtmlencode", "elthideinform",
                             "eltrddescription", "eltdirectedit", "eltdragdrop");
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Content; }
        }

        [VersionIsLessThan(9, 0, 0, 41, VersionName = "Version 9 Hotfix 5")]
        public bool IsNotRelevantForWorklow
        {
            get
            {
                ContentClass.Project.Session.EnsureVersion();
                return ((BoolXmlNodeAttribute) GetAttribute("eltignoreworkflow")).Value;
            }
            set
            {
                ContentClass.Project.Session.EnsureVersion();
                ((BoolXmlNodeAttribute) GetAttribute("eltignoreworkflow")).Value = value;
            }
        }

        [VersionIsLessThan(9, 0, 0, 41, VersionName = "Version 9 Hotfix 5")]
        public bool IsLanguageIndependent
        {
            get
            {
                ContentClass.Project.Session.EnsureVersion();
                return ((BoolXmlNodeAttribute) GetAttribute("eltlanguageindependent")).Value;
            }
            set
            {
                ContentClass.Project.Session.EnsureVersion();
                ((BoolXmlNodeAttribute) GetAttribute("eltlanguageindependent")).Value = value;
            }
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

        public string Description
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltrddescription")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltrddescription")).Value = value; }
        }

        public bool IsDirectEditActivated
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltdirectedit")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltdirectedit")).Value = value; }
        }

        public bool IsDragAndDropActivated
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltdragdrop")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltdragdrop")).Value = value; }
        }
    }
}