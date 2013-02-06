// Smart API - .Net programatical access to RedDot servers
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
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.CCElements.Attributes
{
    public class CategoryXmlNodeAttribute : AbstractGuidXmlNodeAttribute<Category>
    {
        public CategoryXmlNodeAttribute(ContentClass parent, string name) : base(parent, name)
        {
        }

        private bool IsArbitraryCategory
        {
            get
            {
                string value = ((ContentClass) Parent).XmlElement.GetAttributeValue(Name);
                return value == "-1" || String.IsNullOrEmpty(value);
            }
        }

        protected override Category RetrieveByGuid(Guid guid)
        {
            return ((ContentClass) Parent).Project.Categories.GetByGuid(guid);
        }

        protected override Category RetrieveByName(string name)
        {
            return ((ContentClass) Parent).Project.Categories.GetByName(name);
        }

        public void SetUseArbitraryCategory()
        {
            SetXmlNodeValue("-1");
        }

        protected override void UpdateValue(string value)
        {
            string updateValue = IsArbitraryCategoryValue(value) ? null : value;

            base.UpdateValue(updateValue);
        }

        private static bool IsArbitraryCategoryValue(string value)
        {
            return String.IsNullOrEmpty(value) || value == "-1";
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

        protected override void SetXmlNodeValue(string value)
        {
            Parent.XmlElement.SetAttributeValue(Name, string.IsNullOrEmpty(value) ? "-1" : value);
        }

        protected override string GetTypeDescription()
        {
            return "category";
        }
    }
}