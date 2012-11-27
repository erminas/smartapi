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
    public class List : CCElement
    {
        public List(ContentClass contentClass, XmlNode xmlNode) : base(contentClass, xmlNode)
        {
            CreateAttributes("eltextendedlist", "eltfontclass", "eltfontsize", "eltfontbold",
                             "eltonlyhrefvalue", "eltxhtmlcompliant",
                             "eltfontface", "eltfontcolor");
        }

        public override sealed ContentClassCategory Category
        {
            get { return ContentClassCategory.Structural; }
        }

        public bool IsOnlyPathAndFilenameInserted
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltonlyhrefvalue")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltonlyhrefvalue")).Value = value; }
        }

        public bool IsSyntaxConformingToXHtml
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltxhtmlcompliant")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltxhtmlcompliant")).Value = value; }
        }

        public string FontClass
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltfontclass")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltfontclass")).Value = value; }
        }

        public string FontSize
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltfontsize")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltfontsize")).Value = value; }
        }

        public bool IsFontBold
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltfontbold")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltfontbold")).Value = value; }
        }

        public string FontFace
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltfontface")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltfontface")).Value = value; }
        }

        public string FontColor
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltfontcolor")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltfontcolor")).Value = value; }
        }
    }
}