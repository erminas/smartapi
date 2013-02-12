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

namespace erminas.SmartAPI.CMS.CCElements.Attributes
{
    public class EnumWithCustomValuesXmlNodeAttribute<T> : EnumXmlNodeAttribute<T> where T : struct, IConvertible
    {
        private String _customValue;

        public EnumWithCustomValuesXmlNodeAttribute(ContentClassElement parent, string name) : base(parent, name)
        {
        }

        public EnumWithCustomValuesXmlNodeAttribute(IAttributeContainer parent, string name,
                                                    Dictionary<T, string> displayStrings)
            : base(parent, name, displayStrings)
        {
        }

        public override object DisplayObject
        {
            get { return _customValue ?? base.DisplayObject; }
        }

        public override bool Equals(object o)
        {
            var other = o as EnumWithCustomValuesXmlNodeAttribute<T>;
            return other != null &&
                   (_customValue == null ? Equals(Value, other.Value) : _customValue.Equals(other._customValue));
        }

        public override int GetHashCode()
        {
            return _customValue == null ? Value.GetHashCode() : _customValue.GetHashCode();
        }

        protected override void UpdateValue(string value)
        {
            try
            {
                base.UpdateValue(value);
                _customValue = null;
            } catch (ArgumentException)
            {
                _customValue = value;
                SetXmlNodeValue(value);
            }
        }
    }
}