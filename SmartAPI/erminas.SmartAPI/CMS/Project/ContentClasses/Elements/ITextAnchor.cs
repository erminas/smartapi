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

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface ITextAnchor : IAnchor
    {
        string FontClass { get; set; }
        string FontColor { get; set; }
        string FontFace { get; set; }
        string FontSize { get; set; }
        bool IsFontBold { get; set; }
    }

    internal class TextAnchor : Anchor, ITextAnchor
    {
        internal TextAnchor(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltfontclass", "eltfontsize", "eltfontbold", "eltfontface", "eltfontcolor");
        }

        public string FontClass
        {
            get { return GetAttributeValue<string>("eltfontclass"); }
            set { SetAttributeValue("eltfontclass", value); }
        }

        public string FontColor
        {
            get { return GetAttributeValue<string>("eltfontcolor"); }
            set { SetAttributeValue("eltfontcolor", value); }
        }

        public string FontFace
        {
            get { return GetAttributeValue<string>("eltfontface"); }
            set { SetAttributeValue("eltfontface", value); }
        }

        public string FontSize
        {
            get { return GetAttributeValue<string>("eltfontsize"); }
            set { SetAttributeValue("eltfontsize", value); }
        }

        public bool IsFontBold
        {
            get { return GetAttributeValue<bool>("eltfontbold"); }
            set { SetAttributeValue("eltfontbold", value); }
        }
    }
}