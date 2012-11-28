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
    public class Area : CCElement
    {
        public Area(ContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltxhtmlcompliant", "eltsupplement", "eltonlyhrefvalue", "eltshape", "elttarget",
                             "eltcoords");
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Structural; }
        }

        public bool IsSyntaxConformingToXHtml
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltxhtmlcompliant")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltxhtmlcompliant")).Value = value; }
        }

        public string Supplement
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltsupplement")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltsupplement")).Value = value; }
        }

        public bool IsOnlyPathAndFilenameInserted
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltonlyhrefvalue")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltonlyhrefvalue")).Value = value; }
        }

        //TODO use enum instead
        public string Shape
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltshape")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltshape")).Value = value; }
        }

        //TODO use enum instead
        public string Target
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("elttarget")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("elttarget")).Value = value; }
        }

        public string Coords
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltcoords")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltcoords")).Value = value; }
        }
    }
}