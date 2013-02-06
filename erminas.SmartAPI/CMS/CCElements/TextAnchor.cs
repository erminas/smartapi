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
using erminas.SmartAPI.CMS.CCElements.Attributes;

namespace erminas.SmartAPI.CMS.CCElements
{
    public class TextAnchor : Anchor
    {
        public TextAnchor(ContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltfontclass", "eltfontsize", "eltfontbold", "eltfontface", "eltfontcolor");
        }

        public string FontClass
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltfontclass")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltfontclass")).Value = value; }
        }

        public string FontColor
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltfontcolor")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltfontcolor")).Value = value; }
        }

        public string FontFace
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltfontface")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltfontface")).Value = value; }
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
    }
}