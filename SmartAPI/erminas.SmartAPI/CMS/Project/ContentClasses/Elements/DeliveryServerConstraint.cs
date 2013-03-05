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
    public class DeliveryServerConstraint : ContentClassElement
    {
        internal DeliveryServerConstraint(ContentClass contentClass, XmlElement xmlElement)
            : base(contentClass, xmlElement)
        {
            CreateAttributes("eltignoreworkflow", "eltlanguageindependent", "eltrequired", "eltinvisibleinclient",
                             "elthideinform");
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Content; }
        }

        public bool IsEditingMandatory
        {
            get { return GetAttributeValue<bool>("eltrequired"); }
            set { SetAttributeValue("eltrequired", value); }
        }

        public bool IsHiddenInProjectStructure
        {
            get { return GetAttributeValue<bool>("eltinvisibleinclient"); }
            set { SetAttributeValue("eltinvisibleinclient", value); }
        }

        public bool IsLanguageIndependent
        {
            get { return GetAttributeValue<bool>("eltlanguageindependent"); }
            set { SetAttributeValue("eltlanguageindependent", value); }
        }

        public bool IsNotRelevantForWorklow
        {
            get { return GetAttributeValue<bool>("eltignoreworkflow"); }
            set { SetAttributeValue("eltignoreworkflow", value); }
        }

        public bool IsNotUsedInForm
        {
            get { return GetAttributeValue<bool>("elthideinform"); }
            set { SetAttributeValue("elthideinform", value); }
        }
    }
}