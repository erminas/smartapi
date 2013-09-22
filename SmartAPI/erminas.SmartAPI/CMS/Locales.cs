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
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Administration.Language;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS
{
    public interface ISystemLocale : ISessionObject
    {
        string Country { get; }

        /// <summary>
        ///     All Date/Time/DateTime formats of this locale, indexed by their format type id. This list is cached by default.
        /// </summary>
        IIndexedCachedList<int, IDateTimeFormat> DateTimeFormats { get; }

        bool Equals(ISystemLocale other);
        bool IsStandardLanguage { get; }
        int LCID { get; }
        string Language { get; }
        string LanguageAbbreviation { get; }
        string RFCLanguageId { get; }
    }

    public interface IDialogLocale : ISessionObject
    {
        string Country { get; }

        /// <summary>
        ///     All Date/Time/DateTime formats of this locale, indexed by their format type id. This list is cached by default.
        /// </summary>
        IIndexedCachedList<int, IDateTimeFormat> DateTimeFormats { get; }

        bool Equals(IDialogLocale other);

        bool IsStandardLanguage { get; }
        int LCID { get; }
        string Language { get; }
        string LanguageAbbreviation { get; }
        string RFCLanguageId { get; }
    }

    internal class SystemLocale : Locale, ISystemLocale
    {
        internal SystemLocale(ISession session, XmlElement xmlElement) : base(session, xmlElement)
        {
        }

        public bool Equals(ISystemLocale other)
        {
            return other.LCID == LCID;
        }
    }

    internal class DialogLocale : Locale, IDialogLocale
    {
        internal DialogLocale(ISession session, XmlElement xmlElement) : base(session, xmlElement)
        {
        }

        public bool Equals(IDialogLocale other)
        {
            return other.LCID == LCID;
        }
    }

    internal abstract class Locale
    {
        private readonly ISession _session;

        protected Locale(ISession session, XmlElement xmlElement)
        {
            _session = session;
            LanguageAbbreviation = xmlElement.GetAttributeValue("id");
            Country = xmlElement.GetAttributeValue("country");
            Language = xmlElement.GetAttributeValue("language");
            IsStandardLanguage = xmlElement.GetBoolAttributeValue("standardlanguage").GetValueOrDefault();
            LCID = xmlElement.GetIntAttributeValue("lcid").GetValueOrDefault();
            RFCLanguageId = xmlElement.GetAttributeValue("rfclanguageid");
            DateTimeFormats = new IndexedCachedList<int, IDateTimeFormat>(GetFormats, x => x.TypeId, Caching.Enabled);
        }

        public string Country { get; private set; }

        /// <summary>
        ///     All Date/Time/DateTime formats of this locale, indexed by their format type id. This list is cached by default.
        /// </summary>
        public IIndexedCachedList<int, IDateTimeFormat> DateTimeFormats { get; private set; }

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

        public bool Equals(Locale other)
        {
            return Equals(LCID, other.LCID);
        }

        public override int GetHashCode()
        {
            return LanguageAbbreviation.GetHashCode();
        }

        public bool IsStandardLanguage { get; private set; }
        public int LCID { get; private set; }
        public string Language { get; private set; }
        public string LanguageAbbreviation { get; private set; }
        public string RFCLanguageId { get; private set; }

        public ISession Session
        {
            get { return _session; }
        }

        public override string ToString()
        {
            return Country + " (" + Language + ")";
        }

        private List<IDateTimeFormat> GetFormats()
        {
            List<IDateTimeFormat> dateEntries =
                (from XmlElement curEntry in GetFormatsOfSingleType(DateTimeFormatTypes.Date)
                 select
                     (IDateTimeFormat)
                     new DateTimeFormat(DateTimeFormatTypes.Date | DateTimeFormatTypes.DateTime, curEntry)).ToList();

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
            XmlDocument result = _session.ExecuteRQL(string.Format(LOAD_TIME_FORMATS, formatTypeString, LCID),
                                                     RQL.IODataFormat.SessionKeyAndLogonGuid);

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