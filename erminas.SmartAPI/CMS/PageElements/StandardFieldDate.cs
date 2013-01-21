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
    /// <summary>
    /// Standard field for dates. Takes input for SetValueFromString in the format yyyy-MM-dd.
    /// </summary>
    [PageElementType(ElementType.StandardFieldDate)]
    public class StandardFieldDate : StandardField<DateTime>
    {
        private readonly DateTime BASE_DATE = new DateTime(1899, 12, 30);

        public StandardFieldDate(Project project, XmlElement xmlElement) : base(project, xmlElement)
        {
        }

        public StandardFieldDate(Project project, Guid guid, LanguageVariant languageVariant)
            : base(project, guid, languageVariant)
        {
        }

        protected override DateTime FromString(string value)
        {
            try
            {
                return DateTime.Parse(value, CultureInfo.InvariantCulture);
            } catch (FormatException e)
            {
                throw new ArgumentException(string.Format("Invalid date value: {0}", value), e);
            }
        }

        protected override string GetXmlNodeValue()
        {
            return Value == default(DateTime)
                       ? ""
                       : Value.Subtract(BASE_DATE).Days.ToString(CultureInfo.InvariantCulture);
        }

        protected override void LoadWholeStandardField()
        {
        }

        protected override DateTime FromXmlNodeValue(string value)
        {
            return BASE_DATE.AddDays(int.Parse(value));
        }

        public override void Commit()
        {
            //TODO testen gegen _value == null und ob das ergebnis mit htmlencode richtig ist
            Project.ExecuteRQL(string.Format(SAVE_VALUE, Guid.ToRQLString(), _value.Date.Subtract(BASE_DATE).Days,
                                             (int) ElementType));
            //TODO check guid
            //xml
        }
    }
}