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
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    public class Locale
    {
        public readonly string Country;
        public readonly string Id;
        public readonly bool IsStandardLanguage;
        public readonly int LCID;
        public readonly string Language;
        public readonly string RFCLanguageId;
        public readonly Session Session;

        public Locale(Session session, XmlElement xmlElement)
        {
            Session = session;
            Id = xmlElement.GetAttributeValue("id");
            Country = xmlElement.GetAttributeValue("country");
            Language = xmlElement.GetAttributeValue("language");
            IsStandardLanguage = xmlElement.GetAttributeValue("id") == "1";
            LCID = int.Parse(xmlElement.GetAttributeValue("lcid"));
            RFCLanguageId = xmlElement.GetAttributeValue("rfclanguageid");
            DateTimeFormats = new IndexedCachedList<int, DateTimeFormat>(GetFormats, x => x.TypeId, Caching.Enabled);
        }

        /// <summary>
        ///     All Date/Time/DateTime formats of this locale, indexed by their format type id. This list is cached by default.
        /// </summary>
        public IndexedCachedList<int, DateTimeFormat> DateTimeFormats { get; private set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((Locale) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return Country + " (" + Language + ")";
        }

        private bool Equals(Locale other)
        {
            return string.Equals(Id, other.Id);
        }

        private List<DateTimeFormat> GetFormats()
        {
            List<DateTimeFormat> dateEntries =
                (from XmlElement curEntry in GetFormatsOfSingleType(DateTimeFormatTypes.Date)
                 select new DateTimeFormat(DateTimeFormatTypes.Date | DateTimeFormatTypes.DateTime, curEntry)).ToList();

            IEnumerable<DateTimeFormat> timeEntries =
                from XmlElement curEntry in GetFormatsOfSingleType(DateTimeFormatTypes.Time)
                select new DateTimeFormat(DateTimeFormatTypes.Time, curEntry);

            IEnumerable<DateTimeFormat> dateTimeEntries =
                from XmlElement curEntry in GetFormatsOfSingleType(DateTimeFormatTypes.DateTime)
                let entry = new DateTimeFormat(DateTimeFormatTypes.DateTime, curEntry)
                where dateEntries.All(x => x.TypeId != entry.TypeId)
                select entry;

            return dateEntries.Union(timeEntries).Union(dateTimeEntries).ToList();
        }

        private XmlNodeList GetFormatsOfSingleType(DateTimeFormatTypes types)
        {
            const string LOAD_TIME_FORMATS =
                @"<TEMPLATE><ELEMENT action=""load"" ><{0}FORMATS action=""list"" lcid=""{1}""/></ELEMENT></TEMPLATE>";
            string formatTypeString = types.ToString().ToUpper();
            XmlDocument result = Session.ExecuteRQL(string.Format(LOAD_TIME_FORMATS, formatTypeString, LCID));

            var timeformats = result.GetElementsByTagName(formatTypeString + "FORMATS")[0] as XmlElement;
            if (timeformats == null)
            {
                var e = new Exception("could not load timeformats for lcid '" + LCID + "'");
                e.Data.Add("result", result);
                throw e;
            }

            string answerElementsName = types == DateTimeFormatTypes.Time ? "TIMEFORMAT" : "DATEFORMAT";
            return timeformats.GetElementsByTagName(answerElementsName);
        }
    }
}