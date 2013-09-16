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
    public interface IProjectContent : IContentClassElement
    {
        bool IsHitList { get; set; }

        bool IsNotVisibleOnPublishedPage { get; set; }

        bool IsReferenceField { get; set; }

        IContentClassElement ReferencedElement { get; set; }
    }

    internal class ProjectContent : ContentClassElement, IProjectContent
    {
        internal ProjectContent(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
        }

        public override sealed ContentClassCategory Category
        {
            get { return ContentClassCategory.Content; }
        }

        [RedDot("eltislistentry")]
        public bool IsHitList
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltinvisibleinpage")]
        public bool IsNotVisibleOnPublishedPage
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltisreffield")]
        public bool IsReferenceField
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("__elementreference", ConverterType = typeof (ElementReferenceConverter))]
        public IContentClassElement ReferencedElement
        {
            get { return GetAttributeValue<IContentClassElement>(); }
            set { SetAttributeValue(value); }
        }
    }
}