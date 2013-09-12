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
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Converter
{
    public class EditorSettingsConverter : IAttributeConverter<EditorSettings>
    {
        public EditorSettings ConvertFrom(IProjectObject parent, XmlElement element, RedDotAttribute attribute)
        {
            ConverterHelper.EnsureValidProjectObject(parent);

            if (!element.IsAttributeSet(parent, attribute.ElementName))
            {
                return EditorSettings.NotSet;
            }

            var intValue = element.GetIntAttributeValue(attribute.ElementName);
            return (EditorSettings) (intValue ?? (int) EditorSettings.NotSet);
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void WriteTo(IProjectObject parent, XmlElement element, RedDotAttribute attribute, EditorSettings value)
        {
            ConverterHelper.EnsureValidProjectObject(parent);

            var attributeValue = value == EditorSettings.NotSet
                                     ? null
                                     : ((int) value).ToString(CultureInfo.InvariantCulture);

            element.SetAttributeValue(attribute.ElementName, attributeValue);
        }
    }
}