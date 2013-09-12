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
    public class LocaleConverter : IAttributeConverter<ISystemLocale>
    {
        public ISystemLocale ConvertFrom(IProjectObject parent, XmlElement element, RedDotAttribute attribute)
        {
            string value = element.GetAttributeValue(attribute.ElementName);
            var lcid = element.IsAttributeSet(parent, attribute.ElementName) && !value.Contains("EmptyBuffer")
                           ? int.Parse(value)
                           : (int?) null;

            return lcid == null
                       ? null
                       : ((IContentClassElement) parent).ContentClass.Project.Session.Locales[lcid.Value];
        }

        public bool IsReadOnly { get; private set; }

        public void WriteTo(IProjectObject parent, XmlElement element, RedDotAttribute attribute, ISystemLocale value)
        {
            element.SetAttributeValue(attribute.ElementName,
                                      value == null ? null : value.LCID.ToString(CultureInfo.InvariantCulture));
        }
    }
}