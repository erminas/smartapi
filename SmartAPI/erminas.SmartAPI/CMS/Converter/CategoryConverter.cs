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

using System;
using System.Xml;
using erminas.SmartAPI.CMS.Project;
using erminas.SmartAPI.CMS.Project.Keywords;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Converter
{
    public class CategoryConverter : AbstractGuidElementConverter<ICategory>
    {
        private const string ARBITRARY_CATEGORY_VALUE = "-1";

        public override ICategory ConvertFrom(IProjectObject parent, XmlElement element, RedDotAttribute attribute)
        {
            var stringValue = element.GetAttributeValue(attribute.ElementName);

            if (string.IsNullOrEmpty(stringValue))
            {
                return null;
            }

            return stringValue == "-1" ? ArbitraryCategory.INSTANCE : base.ConvertFrom(parent, element, attribute);
        }

        public override void WriteTo(IProjectObject parent, XmlElement element, RedDotAttribute attribute,
                                     ICategory value)
        {
            CheckReadOnly(parent.Project, attribute);

            if (value == ArbitraryCategory.INSTANCE)
            {
                element.SetAttributeValue(attribute.ElementName, ARBITRARY_CATEGORY_VALUE);
            }
            else
            {
                base.WriteTo(parent, element, attribute, value);
            }
        }

        protected override ICategory GetFromGuid(IProjectObject parent, XmlElement element, RedDotAttribute attribute,
                                                 Guid guid)
        {
            //after a deletion of a category, references to it can still be present in the system and
            //thus we can't throw an exception but have to handle it like no category is assigned (RedDot seems to handle it that way).
            ICategory result;
            return parent.Project.Categories.TryGetByGuid(guid, out result) ? result : null;
        }

        protected override ICategory GetFromName(IProjectObject parent, XmlElement element, RedDotAttribute attribute,
                                                 ICategory value)
        {
            ICategory category;
            if (!parent.Project.Categories.TryGetByName(value.Name, out category))
            {
                throw new SmartAPIException(parent.Session.ServerLogin,
                                            string.Format("Cannot find category {0} in project {1}", value.Name, parent));
            }
            return category;
        }
    }
}