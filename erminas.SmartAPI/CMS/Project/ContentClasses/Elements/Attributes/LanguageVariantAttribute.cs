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

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes
{
    public class LanguageVariantAttribute : AbstractGuidXmlNodeAttribute<LanguageVariant>
    {
        public LanguageVariantAttribute(ContentClassElement parent, string name)
            : base(parent.ContentClass.Project.Session, parent, name)
        {
        }

        public override void Assign(IRDAttribute o)
        {
            LanguageVariant value = ((LanguageVariantAttribute) o).Value;
            SetValue(value == null ? null : value.Language);
        }

        public override bool Equals(object o)
        {
            var xmlNodeAttribute = o as LanguageVariantAttribute;
            return xmlNodeAttribute != null && Name == xmlNodeAttribute.Name &&
                   (Value == null
                        ? xmlNodeAttribute.Value == null
                        : Equals(Value.Language, xmlNodeAttribute.Value.Language));
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() + 13*(Value != null ? (Value.Name != null ? Value.Name.GetHashCode() : 0) : 0);
        }

        protected override string GetTypeDescription()
        {
            return "language variant";
        }

        protected override LanguageVariant RetrieveByGuid(Guid guid)
        {
            return ((ContentClassElement) Parent).ContentClass.Project.LanguageVariants.GetByGuid(guid);
        }

        protected override LanguageVariant RetrieveByName(string name)
        {
            return ((ContentClassElement) Parent).ContentClass.Project.LanguageVariants[name];
        }
    }
}