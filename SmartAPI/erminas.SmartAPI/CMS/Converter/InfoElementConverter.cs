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

using System.Globalization;
using System.Xml;
using erminas.SmartAPI.CMS.Project;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Converter
{
    public class InfoElementConverter : IAttributeConverter<IInfoAttribute>
    {
        public IInfoAttribute ConvertFrom(IProjectObject parent, XmlElement element, RedDotAttribute attribute)
        {
            if (!element.IsAttributeSet(parent, attribute.ElementName))
            {
                return null;
            }

// ReSharper disable PossibleInvalidOperationException
            var id = element.GetIntAttributeValue(attribute.ElementName).Value;
// ReSharper restore PossibleInvalidOperationException
            return parent.Project.InfoAttributes[id];
        }

        public bool IsReadOnly { get; set; }

        public void WriteTo(IProjectObject parent, XmlElement element, RedDotAttribute attribute, IInfoAttribute value)
        {
            if (parent == null)
            {
                throw new SmartAPIInternalException("InfoElementConverter not called from a valid project object");
            }

            if (value == null)
            {
                element.SetAttributeValue(attribute.ElementName, null);
                return;
            }

            element.SetAttributeValue(attribute.ElementName, value.Id.ToString(CultureInfo.InvariantCulture));
        }
    }
}