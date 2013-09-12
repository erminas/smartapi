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
using System.Globalization;
using System.Xml;
using erminas.SmartAPI.CMS.Project;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Converter
{
    public class EnumConverter<T> : IAttributeConverter<T> where T : struct, IConvertible
    {
        public T ConvertFrom(IProjectObject parent, XmlElement element, RedDotAttribute attribute)
        {
            if (parent == null)
            {
                throw new SmartAPIInternalException(string.Format("EnumConverter can only be used on project objects"));
            }

            var strValue = element.GetAttributeValue(attribute.ElementName);
            if (string.IsNullOrEmpty(strValue) || strValue == "#" + parent.Session.SessionKey)
            {
                return default(T);
            }

            const bool IGNORE_CASE = true;
            return (T) Enum.Parse(typeof (T), strValue, IGNORE_CASE);
        }

        public bool IsReadOnly { get; set; }

        public void WriteTo(IProjectObject parent, XmlElement element, RedDotAttribute attribute, T value)
        {
            element.SetAttributeValue(attribute.ElementName, value.ToString(CultureInfo.InvariantCulture));
        }
    }
}