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
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Administration.Language;
using erminas.SmartAPI.CMS.Converter;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface IStandardFieldTime : IStandardFieldNonDate
    {
        bool IsUserDefinedTimeFormat(string languageAbbreviation);

        ILanguageDependentValue<ISystemLocale> Locale { get; }

        ILanguageDependentValue<IDateTimeFormat> TimeFormat { get; }

        ILanguageDependentValue<string> UserDefinedTimeFormat { get; }
    }

    internal class LanguageDependendTimeFormat : LanguageDependentValue<IDateTimeFormat>
    {
        public LanguageDependendTimeFormat(IPartialRedDotProjectObject parent, RedDotAttribute attribute)
            : base(parent, attribute)
        {
        }

        public override IDateTimeFormat this[string languageAbbreviation]
        {
            get { return base[languageAbbreviation] ?? DateTimeFormat.USER_DEFINED_TIME_FORMAT; }
            set
            {
                if (!value.IsTimeFormat)
                {
                    throw new ArgumentException(string.Format(
                        "DateTimeFormat {1} with type id {0} is not a time format", value.TypeId, value.Name));
                }
                base[languageAbbreviation] = value;
            }
        }
    }

    internal class StandardFieldTime : StandardFieldNonDate, IStandardFieldTime
    {
        internal StandardFieldTime(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
        }

        public bool IsUserDefinedTimeFormat(string languageAbbreviation)
        {
            return TimeFormat[languageAbbreviation] == DateTimeFormat.USER_DEFINED_TIME_FORMAT;
        }

        [RedDot("eltlcid", ConverterType = typeof (LocaleConverter))]
        public ILanguageDependentValue<ISystemLocale> Locale
        {
            get { return GetAttributeValue<ILanguageDependentValue<ISystemLocale>>(); }
        }

        [RedDot("eltformatno", ConverterType = typeof (DateTimeFormatConverter))]
        public ILanguageDependentValue<IDateTimeFormat> TimeFormat
        {
            get
            {
                var redDotAttribute =
                    (RedDotAttribute)
                    GetType().GetProperty("TimeFormat").GetCustomAttributes(typeof (RedDotAttribute), false).Single();
                return new LanguageDependendTimeFormat(this, redDotAttribute);
            }
        }

        [RedDot("eltformatting")]
        public ILanguageDependentValue<string> UserDefinedTimeFormat
        {
            get { return GetAttributeValue<ILanguageDependentValue<string>>(); }
        }
    }
}