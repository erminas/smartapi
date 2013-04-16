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
    internal class LanguageVariantAttribute : AbstractGuidXmlNodeAttribute<ILanguageVariant>
    {
        public LanguageVariantAttribute(IContentClassElement parent, string name)
            : base(parent.ContentClass.Project.Session, parent, name)
        {
        }

        public override void Assign(IRDAttribute o)
        {
            ILanguageVariant value = ((LanguageVariantAttribute) o).Value;
            SetValue(value == null ? null : value.Abbreviation);
        }

        public override bool Equals(object o)
        {
            var xmlNodeAttribute = o as LanguageVariantAttribute;
            return xmlNodeAttribute != null && Name == xmlNodeAttribute.Name &&
                   (Value == null
                        ? xmlNodeAttribute.Value == null
                        : Equals(Value.Abbreviation, xmlNodeAttribute.Value.Abbreviation));
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() + 13*(Value != null ? (Value.Name != null ? Value.Name.GetHashCode() : 0) : 0);
        }

        protected override string GetTypeDescription()
        {
            return "language variant";
        }

        protected override ILanguageVariant RetrieveByGuid(Guid guid)
        {
            return ((IContentClassElement) Parent).ContentClass.Project.LanguageVariants.GetByGuid(guid);
        }

        protected override ILanguageVariant RetrieveByName(string name)
        {
            return ((IContentClassElement) Parent).ContentClass.Project.LanguageVariants[name];
        }
    }
}