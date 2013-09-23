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
    internal abstract class TextContentConverter : IAttributeConverter<string>
    {
        public string ConvertFrom(IProjectObject parent, XmlElement element, RedDotAttribute attribute)
        {
            ILanguageVariant lang = parent.Project.LanguageVariants.Current;
            if (!element.IsAttributeSet(parent, attribute.ElementName))
            {
                return string.Empty;
            }

            Guid guid = element.GetGuid(attribute.ElementName);
            return parent.Project.GetTextContent(guid, lang, ((int) Type).ToString(CultureInfo.InvariantCulture));
        }

        public bool IsReadOnly { get; private set; }

        public void WriteTo(IProjectObject parent, IXmlReadWriteWrapper element, RedDotAttribute attribute, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                element.SetAttributeValue(attribute.ElementName, null);
            }
            else
            {
                Guid guid = element.IsAttributeSet(parent, attribute.ElementName)
                                ? element.GetGuid(attribute.ElementName)
                                : Guid.Empty;

                var languageVariantName = element.GetAttributeValue("languagevariantid");
                var languageVariant = parent.Project.LanguageVariants[languageVariantName];
                try
                {
                    Guid textGuid = parent.Project.SetTextContent(guid, languageVariant,
                                                                  ((int) Type).ToString(CultureInfo.InvariantCulture),
                                                                  value);

                    element.SetAttributeValue(attribute.ElementName, textGuid.ToRQLString());
                } catch (Exception e)
                {
                    throw new SmartAPIException(parent.Session.ServerLogin,
                                                string.Format("Could not set {0} text for {1} in language {2}",
                                                              Type.ToString().ToLower(), parent, languageVariantName), e);
                }
            }
        }

        protected abstract TextType Type { get; }

        protected enum TextType
        {
            Default = 3,
            Sample = 10
        }
    }

    internal class DefaultTextConverter : TextContentConverter
    {
        protected override TextType Type
        {
            get { return TextType.Default; }
        }
    }

    internal class SampleTextConverter : TextContentConverter
    {
        protected override TextType Type
        {
            get { return TextType.Sample; }
        }
    }
}