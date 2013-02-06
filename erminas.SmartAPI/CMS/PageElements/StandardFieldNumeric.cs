// Smart API - .Net programatical access to RedDot servers
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

namespace erminas.SmartAPI.CMS.PageElements
{
    [PageElementType(ElementType.StandardFieldNumeric)]
    public class StandardFieldNumeric : StandardField<string>
    {
        public StandardFieldNumeric(Project project, XmlElement xmlElement) : base(project, xmlElement)
        {
        }

        public StandardFieldNumeric(Project project, Guid guid, LanguageVariant languageVariant)
            : base(project, guid, languageVariant)
        {
        }

        public override string Value
        {
            set
            {
                if (!string.IsNullOrEmpty(value) && !StandardFieldUserDefined.NUMERIC_CHECK_REGEX.IsMatch(value))
                {
                    throw new ArgumentException(string.Format("'{0}' is not a valid numeric value", value));
                }
                _value = value;
            }
        }

        protected override string FromString(string value)
        {
            if (StandardFieldUserDefined.NUMERIC_CHECK_REGEX.IsMatch(value))
            {
                return value;
            }

            throw new ArgumentException(string.Format("Not a valid numerical value: {0}", value), value);
        }

        protected override void LoadWholeStandardField()
        {
        }
    }
}