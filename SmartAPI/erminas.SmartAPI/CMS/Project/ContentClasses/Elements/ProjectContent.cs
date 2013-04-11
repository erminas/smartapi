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
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface IProjectContent : IContentClassElement
    {
        bool IsHitList { get; set; }
        bool IsNotVisibleOnPublishedPage { get; set; }
        bool IsReferenceField { get; set; }
        IContentClassElement ReferencedElement { get; set; }
    }

    internal class ProjectContent : ContentClassElement, IProjectContent
    {
        private readonly ElementReferenceAttribute _elementReference;

        internal ProjectContent(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltislistentry", "eltinvisibleinpage", "eltisreffield");
            _elementReference = new ElementReferenceAttribute(this);
        }

        public override sealed ContentClassCategory Category
        {
            get { return ContentClassCategory.Content; }
        }

        public bool IsHitList
        {
            get { return GetAttributeValue<bool>("eltislistentry"); }
            set { SetAttributeValue("eltislistentry", value); }
        }

        public bool IsNotVisibleOnPublishedPage
        {
            get { return GetAttributeValue<bool>("eltinvisibleinpage"); }
            set { SetAttributeValue("eltinvisibleinpage", value); }
        }

        public bool IsReferenceField
        {
            get { return GetAttributeValue<bool>("eltisreffield"); }
            set { SetAttributeValue("eltisreffield", value); }
        }

        public IContentClassElement ReferencedElement
        {
            get { return _elementReference.Value; }
            set { _elementReference.Value = value; }
        }
    }
}