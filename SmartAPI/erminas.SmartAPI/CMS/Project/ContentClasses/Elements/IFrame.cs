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
using erminas.SmartAPI.CMS.Converter;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface IFrame : IWorkflowAssignments, IContentClassPreassignable
    {
        string FrameName { get; set; }
        Frameborder Frameborder { get; set; }
        bool IsNotResizing { get; set; }
        bool IsSyntaxConformingToXHtml { get; set; }
        string MarginHeight { get; set; }
        string MarginWidth { get; set; }
        Scrolling Scrolling { get; set; }
        string Src { get; set; }
        string Supplement { get; set; }
    }

    internal class Frame : AbstractWorkflowAssignments, IFrame
    {
        internal Frame(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            PreassignedContentClasses = new PreassignedContentClassesAndPageDefinitions(this);
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Structural; }
        }

        [RedDot("eltframename")]
        public string FrameName
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltframeborder", ConverterType = typeof (StringEnumConverter<Frameborder>))]
        public Frameborder Frameborder
        {
            get { return GetAttributeValue<Frameborder>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltnoresize")]
        public bool IsNotResizing
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltxhtmlcompliant")]
        public bool IsSyntaxConformingToXHtml
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltmarginheight")]
        public string MarginHeight
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltmarginwidth")]
        public string MarginWidth
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public PreassignedContentClassesAndPageDefinitions PreassignedContentClasses { get; private set; }

        [RedDot("eltscrolling", ConverterType = typeof (StringEnumConverter<Scrolling>))]
        public Scrolling Scrolling
        {
            get { return GetAttributeValue<Scrolling>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltsrc")]
        public string Src
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltsupplement")]
        public string Supplement
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }
    }
}