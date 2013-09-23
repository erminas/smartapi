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
        IContentClassElement ChildElementOf { get; set; }

        ILanguageDependentValue<string> DefaultValue { get; }

        ILanguageDependentValue<string> Sample { get; }
    }

    internal class StandardField : ExtendedContentClassContentElement, IStandardField
    {
        protected StandardField(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
        }

        [RedDot("eltparentelementguid", ConverterType = typeof (ContentClassElementConverter))]
        public IContentClassElement ChildElementOf
        {
            get { return GetAttributeValue<IContentClassElement>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltdefaultvalue")]
        public ILanguageDependentValue<string> DefaultValue
        {
            get { return GetAttributeValue<ILanguageDependentValue<string>>(); }
        }

        [RedDot("eltrdexample")]
        public ILanguageDependentValue<string> Sample
        {
            get { return GetAttributeValue<ILanguageDependentValue<string>>(); }
        }
    }
}