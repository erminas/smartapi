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

namespace erminas.SmartAPI.CMS.CCElements.Attributes
{
    public class IntXmlNodeAttribute : RDXmlNodeAttribute
    {
        private int? _value;

        public IntXmlNodeAttribute(IAttributeContainer parent, string name) : base(parent, name)
        {
        }

        public override object DisplayObject
        {
            get { return _value.ToString(); }
        }

        public int? Value
        {
            get { return _value; }
            set
            {
                _value = value;
                SetXmlNodeValue(value == null ? null : value.ToString());
            }
        }

        protected override void UpdateValue(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                _value = null;
            }
            else
            {
                _value = int.Parse(value);
            }
        }

        public override void Assign(IRDAttribute o)
        {
            SetValue(((IntXmlNodeAttribute) o).GetXmlNodeValue());
        }

        public override bool IsAssignableFrom(IRDAttribute o, out string reason)
        {
            reason = string.Empty;
            return o is IntXmlNodeAttribute;
        }
    }
}