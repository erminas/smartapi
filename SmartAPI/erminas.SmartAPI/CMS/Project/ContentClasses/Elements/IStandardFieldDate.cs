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
using erminas.SmartAPI.CMS.Converter;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface IStandardFieldDate : IStandardField
    {
        ILanguageDependentValue<IDateTimeFormat> DateFormat { get; }

        ILanguageDependentReadValue<bool> IsUserDefinedDateFormat { get; }

        ILanguageDependentValue<ISystemLocale> Locale { get; }

        ILanguageDependentValue<string> UserDefinedDateFormat { get; set; }
    }

    internal class IsUserDefinedDateFromatValue : AbstractLanguageDependendReadValue<bool>
    {
        public IsUserDefinedDateFromatValue(IStandardFieldDate date) : base(date)
        {
        }

        public override bool this[string languageAbbreviation]
        {
            get
            {
                return ((IStandardFieldDate) Parent).DateFormat[languageAbbreviation] ==
                       DateTimeFormat.USER_DEFINED_DATE_FORMAT;
            }
        }
    }

    internal class LanguageDependendDateFormat : LanguageDependentValue<IDateTimeFormat>
    {
        public LanguageDependendDateFormat(IPartialRedDotProjectObject parent, RedDotAttribute attribute)
            : base(parent, attribute)
        {
        }

        public override IDateTimeFormat this[string languageAbbreviation]
        {
            get { return base[languageAbbreviation] ?? DateTimeFormat.USER_DEFINED_DATE_FORMAT; }
            set
            {
                if (!value.IsDateFormat)
                {
                    throw new ArgumentException(string.Format(
                        "DateTimeFormat {1} with type id {0} is not a date format", value.TypeId, value.Name));
                }
                base[languageAbbreviation] = value;
            }
        }
    }

    internal class StandardFieldDate : StandardField, IStandardFieldDate
    {
        internal StandardFieldDate(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
        }

        [RedDot("eltformatno", ConverterType = typeof (DateTimeFormatConverter))]
        public ILanguageDependentValue<IDateTimeFormat> DateFormat
        {
            get
            {
                return new LanguageDependendDateFormat(this,
                                                       (RedDotAttribute)
                                                       GetType()
                                                           .GetProperty("DateFormat")
                                                           .GetCustomAttributes(typeof (RedDotAttribute), false)
                                                           .Single());
            }
        }

        public ILanguageDependentReadValue<bool> IsUserDefinedDateFormat
        {
            get { return new IsUserDefinedDateFromatValue(this); }
        }

        [RedDot("eltlcid", ConverterType = typeof (LocaleConverter))]
        public ILanguageDependentValue<ISystemLocale> Locale
        {
            get { return GetAttributeValue<ILanguageDependentValue<ISystemLocale>>(); }
        }

        [RedDot("eltformatting")]
        public ILanguageDependentValue<string> UserDefinedDateFormat
        {
            get { return GetAttributeValue<ILanguageDependentValue<string>>(); }
            set { SetAttributeValue(value); }
        }
    }
}