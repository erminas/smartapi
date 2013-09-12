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
using erminas.SmartAPI.CMS.Administration.Language;
using erminas.SmartAPI.CMS.Converter;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface IStandardFieldDate : IStandardField
    {
        [RedDot("eltformatno", ConverterType = typeof (DateTimeFormatConverter))]
        IDateTimeFormat DateFormat { get; set; }

        bool IsUserDefinedDateFormat { get; }

        [RedDot("eltlcid", ConverterType = typeof (LocaleConverter))]
        ISystemLocale Locale { get; set; }

        [RedDot("eltformatting")]
        string UserDefinedDateFormat { get; set; }
    }

    internal class StandardFieldDate : StandardField, IStandardFieldDate
    {
        internal StandardFieldDate(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
        }

        public IDateTimeFormat DateFormat
        {
            get { return GetAttributeValue<IDateTimeFormat>() ?? DateTimeFormat.USER_DEFINED_DATE_FORMAT; }
            set
            {
                if (!value.IsDateFormat)
                {
                    throw new ArgumentException(string.Format(
                        "DateTimeFormat {1} with type id {0} is not a date format", value.TypeId, value.Name));
                }
                SetAttributeValue(value);
            }
        }

        public bool IsUserDefinedDateFormat
        {
            get { return DateFormat == DateTimeFormat.USER_DEFINED_DATE_FORMAT; }
        }

        public ISystemLocale Locale
        {
            get { return GetAttributeValue<ISystemLocale>(); }
            set { SetAttributeValue(value); }
        }

        public string UserDefinedDateFormat
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }
    }
}