// Smart API - .Net programmatic access to RedDot servers
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
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Administration.Language
{
    public interface IDateTimeFormat
    {
        /// <summary>
        ///     Example of the format
        /// </summary>
        string Example { get; }

        bool IsDateFormat { get; }
        bool IsDateTimeFormat { get; }
        bool IsTimeFormat { get; }

        /// <summary>
        ///     Name of the format
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Format types id in RedDot
        /// </summary>
        int TypeId { get; }
    }

    /// <summary>
    ///     A date/time format used in locales.
    /// </summary>
    public class DateTimeFormat : IDateTimeFormat
    {
        public static readonly IDateTimeFormat USER_DEFINED_DATE_FORMAT = new DateTimeFormat(DateTimeFormatTypes.Date);
        public static readonly IDateTimeFormat USER_DEFINED_TIME_FORMAT = new DateTimeFormat(DateTimeFormatTypes.Time);

        public static readonly IDateTimeFormat USER_DEFINED_DATE_TIME_FORMAT =
            new DateTimeFormat(DateTimeFormatTypes.DateTime);

        private readonly DateTimeFormatTypes _formatTypes;

        internal DateTimeFormat(DateTimeFormatTypes dateTimeFormatTypes, XmlElement xmlElement)
        {
            Name = xmlElement.GetAttributeValue("name");
            TypeId = int.Parse(xmlElement.GetAttributeValue("type"));
            Example = xmlElement.GetAttributeValue("example");
            _formatTypes = dateTimeFormatTypes;
        }

        private DateTimeFormat(DateTimeFormatTypes dateTimeFormatTypes)
        {
            Name = "user defined";
            TypeId = -1;
            Example = "";
            _formatTypes = dateTimeFormatTypes;
        }

        /// <summary>
        ///     Example of the format
        /// </summary>
        public string Example { get; private set; }

        public bool IsDateFormat
        {
            get { return (_formatTypes & DateTimeFormatTypes.Date) == DateTimeFormatTypes.Date; }
        }

        public bool IsDateTimeFormat
        {
            get { return (_formatTypes & DateTimeFormatTypes.DateTime) == DateTimeFormatTypes.DateTime; }
        }

        public bool IsTimeFormat
        {
            get
            {
                //Nothing can be combined with time formats.
                return _formatTypes == DateTimeFormatTypes.Time;
            }
        }

        /// <summary>
        ///     Name of the format
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     Format types id in RedDot
        /// </summary>
        public int TypeId { get; private set; }
    }

    [Flags]
    public enum DateTimeFormatTypes
    {
        Date = 1,
        Time = 2,
        DateTime = 4
    }
}