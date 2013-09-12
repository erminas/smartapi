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

using System.Xml;
using erminas.SmartAPI.CMS.Administration.Language;
using erminas.SmartAPI.CMS.Converter;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface IStandardFieldTime : IStandardFieldNonDate
    {
        bool IsUserDefinedTimeFormat { get; }

        [RedDot("eltlcid", ConverterType = typeof (LocaleConverter))]
        ISystemLocale Locale { get; set; }

        [RedDot("eltformatno", ConverterType = typeof (DateTimeFormatConverter))]
        IDateTimeFormat TimeFormat { get; set; }

        [RedDot("eltformatting")]
        string UserDefinedTimeFormat { get; set; }
    }

    internal class StandardFieldTime : StandardFieldNonDate, IStandardFieldTime
    {
        internal StandardFieldTime(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
        }

        public bool IsUserDefinedTimeFormat
        {
            get { return TimeFormat == DateTimeFormat.USER_DEFINED_TIME_FORMAT; }
        }

        public ISystemLocale Locale
        {
            get { return GetAttributeValue<ISystemLocale>(); }
            set { SetAttributeValue(value); }
        }

        public IDateTimeFormat TimeFormat
        {
            get { return GetAttributeValue<IDateTimeFormat>() ?? DateTimeFormat.USER_DEFINED_TIME_FORMAT; }
            set { SetAttributeValue(value); }
        }

        public string UserDefinedTimeFormat
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }
    }
}