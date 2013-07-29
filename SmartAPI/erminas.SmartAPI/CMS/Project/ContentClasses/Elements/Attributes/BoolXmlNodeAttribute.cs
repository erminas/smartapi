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

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes
{
    internal class BoolXmlNodeAttribute : RDXmlNodeAttribute
    {
        private bool _value;

        public BoolXmlNodeAttribute(ISessionObject parent, string name) : base(parent, name)
        {
        }

        public override void Assign(IRDAttribute o)
        {
            var boolAttribute = (BoolXmlNodeAttribute) o;
            SetValue(boolAttribute._value ? "1" : "0");
        }

        public override object DisplayObject
        {
            get { return _value ? "yes" : "no"; }
        }

        public override bool Equals(object o)
        {
            var attr = o as BoolXmlNodeAttribute;
            if (attr == null || !attr.Name.Equals(Name))
            {
                return false;
            }
            return attr._value == _value;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() + 7*_value.GetHashCode();
        }

        public override bool IsAssignableFrom(IRDAttribute o, out string reason)
        {
            reason = string.Empty;
            return o is BoolXmlNodeAttribute;
        }

        public bool Value
        {
            get { return _value; }
            set
            {
                _value = value;
                SetXmlNodeValue(_value ? "1" : "0");
            }
        }

        protected override void UpdateValue(string value)
        {
            if (string.IsNullOrEmpty(value) || value == "#" + Parent.Session.SessionKey)
            {
                _value = false;
                return;
            }
            switch (value.Trim())
            {
                case "0":
                    _value = false;
                    break;
                case "1":
                    _value = true;
                    break;
                default:
                    throw new ArgumentException("wrong boolean value: " + value);
            }
        }
    }
}