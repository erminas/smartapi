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

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface ITextAnchor : IAnchor
    {
        [RedDot("eltfontclass")]
        string FontClass { get; set; }

        [RedDot("eltfontcolor")]
        string FontColor { get; set; }

        [RedDot("eltfontface")]
        string FontFace { get; set; }

        [RedDot("eltfontsize")]
        string FontSize { get; set; }

        [RedDot("eltfontbold")]
        bool IsFontBold { get; set; }
    }

    internal class TextAnchor : Anchor, ITextAnchor
    {
        internal TextAnchor(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
        }

        public string FontClass
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public string FontColor
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public string FontFace
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public string FontSize
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public bool IsFontBold
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }
    }
}