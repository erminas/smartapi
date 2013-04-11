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
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes
{
    internal class StringEnumXmlNodeAttribute<T> : EnumXmlNodeAttribute<T> where T : struct, IConvertible
    {
        private readonly Func<string, T> _parseEnum;
        private readonly Func<T, string> _toStringValue;

        public StringEnumXmlNodeAttribute(IContentClassElement parent, string name,
                                          Func<T, string> toStringValueRepresentation, Func<string, T> parseEnum)
            : base(parent, name, false)
        {
            _toStringValue = toStringValueRepresentation;
            _parseEnum = parseEnum;
            UpdateValue(Parent.XmlElement.GetAttributeValue(Name));
        }

        public StringEnumXmlNodeAttribute(IAttributeContainer parent, string name, Dictionary<T, string> displayStrings,
                                          Func<T, string> toStringValueRepresentation, Func<string, T> parseEnum)
            : base(parent, name, displayStrings, false)
        {
            _toStringValue = toStringValueRepresentation;
            _parseEnum = parseEnum;
            UpdateValue(Parent.XmlElement.GetAttributeValue(Name));
        }

        public override T Value
        {
            get { return base.Value; }
            set { SetValue(_toStringValue(value)); }
        }

        protected override sealed void UpdateValue(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value) || value == Session.SESSIONKEY_PLACEHOLDER)
                {
                    _value = default(T);
                    return;
                }
                _value = _parseEnum(value);
            } catch (Exception e)
            {
                throw new SmartAPIException(Parent.Session.ServerLogin,
                                            string.Format("Could not convert value {0} to enum {1}", value,
                                                          typeof (T).Name), e);
            }
        }
    }
}