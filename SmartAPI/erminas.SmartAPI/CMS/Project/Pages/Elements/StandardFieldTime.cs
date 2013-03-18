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
using System.Globalization;
using System.Xml;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.Pages.Elements
{
    /// <summary>
    ///     Standard field for time. Takes input for SetValueFromString in the format "H:mm".
    /// </summary>
    [PageElementType(ElementType.StandardFieldTime)]
    public sealed class StandardFieldTime : StandardField<TimeSpan>
    {
        internal StandardFieldTime(Project project, XmlElement xmlElement) : base(project, xmlElement)
        {
        }

        public StandardFieldTime(Project project, Guid guid, LanguageVariant languageVariant)
            : base(project, guid, languageVariant)
        {
        }

        public override void Commit()
        {
            using (new LanguageContext(LanguageVariant))
            {
                //TODO testen gegen _value == null und ob das ergebnis mit htmlencode richtig ist
                Project.ExecuteRQL(string.Format(SAVE_VALUE, Guid.ToRQLString(),
                                                 _value.Hours/24.0 + _value.Minutes/(24.0*60.0) +
                                                 _value.Seconds/(24.0*60.0*60.0), (int) ElementType));
            }
            //TODO check guid
            //xml
        }

        protected override TimeSpan FromString(string value)
        {
            try
            {
                return DateTime.Parse(value, CultureInfo.InvariantCulture).TimeOfDay;
            } catch (FormatException e)
            {
                throw new ArgumentException(string.Format("Invalid time value: {0}", value), e);
            }
        }

        protected override TimeSpan FromXmlNodeValue(string value)
        {
            return value.ToOADate().TimeOfDay;
        }

        protected override string GetXmlNodeValue()
        {
            if (Value == default(TimeSpan))
            {
                return "";
            }
            var date = new DateTime(0, 0, Value.Days, Value.Hours, Value.Minutes, Value.Seconds);
            return date.ToOADate().ToString(CultureInfo.InvariantCulture);
        }

        protected override void LoadWholeStandardField()
        {
        }
    }
}