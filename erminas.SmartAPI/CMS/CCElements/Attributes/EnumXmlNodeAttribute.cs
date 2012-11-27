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
using System.Collections.Generic;
using System.Globalization;

namespace erminas.SmartAPI.CMS.CCElements.Attributes
{
    public class EnumXmlNodeAttribute<T> : RDXmlNodeAttribute where T : struct, IConvertible
    {
        private readonly Dictionary<T, string> _displayStrings;
        protected T _value;

        public EnumXmlNodeAttribute(CCElement parent, string name, bool initValue = true)
            : base(parent, name, initValue)
        {
        }

        public EnumXmlNodeAttribute(IAttributeContainer Parent, string name, Dictionary<T, string> displayStrings,
                                    bool initValue = true)
            : base(Parent, name, initValue)
        {
            _displayStrings = displayStrings;
        }

        public override object DisplayObject
        {
            get { return _displayStrings != null ? _displayStrings[Value] : Value.ToString(); }
        }

        public virtual T Value
        {
            get { return _value; }
            set { SetValue(value.ToInt32(CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture)); }
        }

        protected override void UpdateValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                _value = default(T);
            }
            else
            {
                _value = (T) Enum.Parse(typeof (T), value, true);
            }
        }

        public override bool IsAssignableFrom(IRDAttribute o, out string reason)
        {
            reason = string.Empty;
            return o is EnumXmlNodeAttribute<T>;
        }

        public override void Assign(IRDAttribute o)
        {
            SetValue(((EnumXmlNodeAttribute<T>) o).GetXmlNodeValue());
        }

        public override bool Equals(object o)
        {
            var other = o as EnumXmlNodeAttribute<T>;
            return other != null && Equals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}