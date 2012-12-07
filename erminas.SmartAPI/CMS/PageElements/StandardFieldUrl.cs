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
using System.Xml;

namespace erminas.SmartAPI.CMS.PageElements
{
    [PageElementType(ElementType.StandardFieldUrl)]
    public class StandardFieldUrl : StandardField<Uri>
    {
        public StandardFieldUrl(Project project, XmlElement xmlElement) : base(project, xmlElement)
        {
        }

        public StandardFieldUrl(Project project, Guid guid) : base(project, guid)
        {
        }

        protected override Uri FromString(string value)
        {
            try
            {
                return string.IsNullOrEmpty(value) ? null : new Uri(value);
            } catch (UriFormatException e)
            {
                throw new ArgumentException(string.Format("Invalid URL: {0}", value), e);
            }
        }

        protected override string GetXmlNodeValue()
        {
            return Value != null ? Value.ToString() : "";
        }

        protected override void LoadWholeStandardField()
        {
        }
    }
}