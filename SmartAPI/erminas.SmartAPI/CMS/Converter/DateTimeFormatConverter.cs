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
using erminas.SmartAPI.CMS.Administration.Language;
using erminas.SmartAPI.CMS.Project;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Converter
{
    internal class DateTimeFormatConverter : IAttributeConverter<IDateTimeFormat>
    {
        public IDateTimeFormat ConvertFrom(IProjectObject parent, XmlElement element, RedDotAttribute attribute)
        {
            string value = element.GetAttributeValue(attribute.ElementName);

            int? type = element.IsAttributeSet(parent, attribute.ElementName) && !value.Contains("EmptyBuffer")
                            ? element.GetIntAttributeValue(attribute.ElementName)
                            : null;

            int lcid;
            bool valid = int.TryParse(element.GetAttributeValue("eltlcid"), out lcid);
            if (!valid || type == null || IsCustomFormat(type))
            {
                return null;
            }

            return
                ((IContentClassElement) parent).ContentClass.Project.Session.Locales[lcid].DateTimeFormats[type.Value];
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void WriteTo(IProjectObject parent, IXmlReadWriteWrapper element, RedDotAttribute attribute,
                            IDateTimeFormat value)
        {
            element.SetAttributeValue(attribute.ElementName, value.TypeId.ToString(CultureInfo.InvariantCulture));
        }

        private static bool IsCustomFormat(int? type)
        {
            return type < 0;
        }
    }
}