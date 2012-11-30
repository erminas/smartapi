/*
 * Smart API - .Net programatical access to RedDot servers
 * Copyright (C) 2012  erminas GbR 
 *
 * This program is free software: you can redistribute it and/or modify it 
 * under the terms of the GNU General Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details. 
 *
 * You should have received a copy of the GNU General Public License along with this program.
 * If not, see <http://www.gnu.org/licenses/>. 
 */

using System;
using System.Globalization;
using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.PageElements
{
    [PageElementType(ElementType.StandardFieldTime)]
    public sealed class StandardFieldTime : StandardField<TimeSpan>
    {
        public StandardFieldTime(Project project, XmlElement xmlElement)
            : base(project, xmlElement)
        {
            LoadXml(xmlElement);
        }

        public StandardFieldTime(Project project, Guid guid)
            : base(project, guid)
        {
        }

        protected override TimeSpan FromString(string value)
        {
            const string FORMAT = "H:mm";
            return DateTime.ParseExact(value, FORMAT, CultureInfo.InvariantCulture).TimeOfDay;
        }

        protected override TimeSpan FromXmlNodeValue(string value)
        {
            return value.ToOADate().TimeOfDay;
        }

        public override void Commit()
        {
            //TODO testen gegen _value == null und ob das ergebnis mit htmlencode richtig ist
            Project.ExecuteRQL(string.Format(SAVE_VALUE, Guid.ToRQLString(),
                                             _value.Hours/24.0 + _value.Minutes/(24.0*60.0) +
                                             _value.Seconds/(24.0*60.0*60.0)));
            //TODO check guid
            //xml
        }
    }
}