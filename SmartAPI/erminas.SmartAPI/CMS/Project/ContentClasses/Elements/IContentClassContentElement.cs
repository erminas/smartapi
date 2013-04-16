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
    public interface IContentClassContentElement : IContentClassElement, ICanBeRequiredForEditing
    {
        bool IsHiddenInProjectStructure { get; set; }
        bool IsLanguageIndependent { get; set; }
        bool IsNotConvertingCharactersToHtml { get; set; }
        bool IsNotRelevantForWorklow { get; set; }
        bool IsNotUsedInForm { get; set; }
        bool IsNotVisibleOnPublishedPage { get; set; }
        new void Commit();
    }

    internal abstract class ContentClassContentElement : ContentClassElement, IContentClassContentElement
    {
        protected ContentClassContentElement(IContentClass contentClass, XmlElement xmlElement)
            : base(contentClass, xmlElement)
        {
            CreateAttributes("eltignoreworkflow", "eltlanguageindependent", "eltrequired", "eltinvisibleinclient",
                             "eltinvisibleinpage", "elthideinform", "eltdonothtmlencode");
        }

        public override sealed ContentClassCategory Category
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

        public bool IsNotConvertingCharactersToHtml
        {
            get { return GetAttributeValue<bool>("eltdonothtmlencode"); }
            set { SetAttributeValue("eltdonothtmlencode", value); }
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

        public bool IsNotVisibleOnPublishedPage
        {
            get { return GetAttributeValue<bool>("eltinvisibleinpage"); }
            set { SetAttributeValue("eltinvisibleinpage", value); }
        }
    }
}