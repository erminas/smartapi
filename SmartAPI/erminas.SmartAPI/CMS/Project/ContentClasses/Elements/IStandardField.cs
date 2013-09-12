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
    public interface IStandardField : IExtendedContentClassContentElement
    {
        [RedDot("eltparentelementguid", ConverterType = typeof (ContentClassElementConverter))]
        IContentClassElement ChildElementOf { get; set; }

        [RedDot("eltdefaultvalue")]
        string DefaultValue { get; set; }

        [RedDot("eltrdexample")]
        string Sample { get; set; }
    }

    internal class StandardField : ExtendedContentClassContentElement, IStandardField
    {
        protected StandardField(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
        }

        public IContentClassElement ChildElementOf
        {
            get { return GetAttributeValue<IContentClassElement>(); }
            set { SetAttributeValue(value); }
        }

        public string DefaultValue
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public string Sample
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }
    }
}