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
using System.Linq;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes
{
    public class ElementXmlNodeAttribute : AbstractGuidXmlNodeAttribute<ContentClassElement>
    {
        public ElementXmlNodeAttribute(ContentClassElement parent, string name)
            : base(parent.ContentClass.Project.Session, parent, name)
        {
        }

        protected override string GetTypeDescription()
        {
            return "element";
        }

        protected override ContentClassElement RetrieveByGuid(Guid elementGuid)
        {
            var parentCcElement = (ContentClassElement) Parent;
            return
                parentCcElement.ContentClass.Elements[parentCcElement.ILanguageVariant.Abbreviation].FirstOrDefault(
                    x => x.Guid.Equals(elementGuid));
        }

        protected override ContentClassElement RetrieveByName(string name)
        {
            var parentCcElement = (ContentClassElement) Parent;
            return
                parentCcElement.ContentClass.Elements[parentCcElement.ILanguageVariant.Abbreviation].FirstOrDefault(
                    x => x.Name == name);
        }
    }
}