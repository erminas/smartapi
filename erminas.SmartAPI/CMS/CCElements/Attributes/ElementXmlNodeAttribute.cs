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

namespace erminas.SmartAPI.CMS.CCElements.Attributes
{
    public class ElementXmlNodeAttribute : AbstractGuidXmlNodeAttribute<CCElement>
    {
        public ElementXmlNodeAttribute(IAttributeContainer parent, string name) : base((RedDotObject) parent, name)
        {
        }

        protected override string GetTypeDescription()
        {
            return "element";
        }

        protected override CCElement RetrieveByGuid(Guid elementGuid)
        {
            var parentCcElement = (CCElement) Parent;
            return
                parentCcElement.ContentClass.Elements[parentCcElement.LanguageVariant.Language].Elements.Find(
                    x => x.Guid.Equals(elementGuid));
        }

        protected override CCElement RetrieveByName(string name)
        {
            var parentCcElement = (CCElement) Parent;
            return
                parentCcElement.ContentClass.Elements[parentCcElement.LanguageVariant.Language].Elements.Find(
                    x => x.Name == name);
        }
    }
}