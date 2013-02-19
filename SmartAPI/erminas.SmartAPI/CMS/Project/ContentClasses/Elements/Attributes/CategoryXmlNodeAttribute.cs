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

using System;
using erminas.SmartAPI.CMS.Project.Keywords;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes
{
    public class CategoryXmlNodeAttribute : AbstractGuidXmlNodeAttribute<Category>
    {
        public CategoryXmlNodeAttribute(ContentClass parent, string name) : base(parent.Project.Session, parent, name)
        {
        }

        public override void Assign(IRDAttribute o)
        {
            var categoryXmlNodeAttribute = ((CategoryXmlNodeAttribute) o);
            Category value = categoryXmlNodeAttribute.Value;

            if (categoryXmlNodeAttribute.IsArbitraryCategory)
            {
                SetUseArbitraryCategory();
            }
            else
            {
                SetValue(value.Name);
            }
        }

        public void SetUseArbitraryCategory()
        {
            SetXmlNodeValue("-1");
        }

        protected override string GetTypeDescription()
        {
            return "category";
        }

        protected override Category RetrieveByGuid(Guid guid)
        {
            //after a deletion of a category, references to it can still be present in the system and
            //thus we can't throw an exception but have to handle it like no category is assigned (RedDot seems to handle it that way).
            Category category;
            ((ContentClass) Parent).Project.Categories.TryGetByGuid(guid, out category);
            return category;
        }

        protected override Category RetrieveByName(string name)
        {
            return ((ContentClass) Parent).Project.Categories.GetByName(name);
        }

        protected override void SetXmlNodeValue(string value)
        {
            Parent.XmlElement.SetAttributeValue(Name, string.IsNullOrEmpty(value) ? "-1" : value);
        }

        protected override void UpdateValue(string value)
        {
            string updateValue = IsArbitraryCategoryValue(value) ? null : value;

            base.UpdateValue(updateValue);
        }

        private bool IsArbitraryCategory
        {
            get
            {
                string value = ((ContentClass) Parent).XmlElement.GetAttributeValue(Name);
                return value == "-1" || String.IsNullOrEmpty(value);
            }
        }

        private static bool IsArbitraryCategoryValue(string value)
        {
            return String.IsNullOrEmpty(value) || value == "-1";
        }
    }
}