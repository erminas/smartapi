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
    public class StandardField : ExtendedContentClassContentElement
    {
        protected StandardField(ContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltrdexample", /*"eltparentelementname",*/ "eltparentelementguid");
// ReSharper disable ObjectCreationAsStatement
            new StringXmlNodeAttribute(this, "eltdefaultvalue");
// ReSharper restore ObjectCreationAsStatement
        }

        public ContentClassElement ChildElementOf
        {
            get { return ((ElementXmlNodeAttribute) GetAttribute("eltparentelementguid")).Value; }
            set { ((ElementXmlNodeAttribute) GetAttribute("eltparentelementguid")).Value = value; }
        }

        public string DefaultValue
        {
            get { return GetAttributeValue<string>("eltdefaultvalue"); }
            set { SetAttributeValue("eltdefaultvalue", value); }
        }

        public string Sample
        {
            get { return GetAttributeValue<string>("eltrdexample"); }
            set { SetAttributeValue("eltrdexample", value); }
        }
    }
}