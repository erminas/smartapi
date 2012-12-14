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
using System.Text;

namespace erminas.SmartAPI.CMS.CCElements.Attributes
{
    public class EditorOptionsAttribute : RDXmlNodeAttribute
    {
        private EditorSettings _value;

        public EditorOptionsAttribute(IAttributeContainer parent, string name) : base(parent, name, true)
        {
        }

        public EditorSettings Value
        {
            get { return _value; }
            set
            {
                _value = value;
                SetValue(_value == EditorSettings.NotSet ? null : ((int) _value).ToString(CultureInfo.InvariantCulture));
            }
        }

        public override object DisplayObject
        {
            get
            {
                if (Value == EditorSettings.NotSet)
                {
                    return null; //todo better return ""?
                }
                var builder = new StringBuilder();
                string[] names = Enum.GetNames(typeof (EditorSettings));
                Array values = Enum.GetValues(typeof (EditorSettings));
                for (int i = 0; i < names.Length; ++i)
                {
                    if (((EditorSettings) values.GetValue(i) & Value) != 0)
                    {
                        builder.Append(names[i] + ", ");
                    }
                }
                return builder.ToString();
            }
        }

        public override bool IsAssignableFrom(IRDAttribute o, out string reason)
        {
            reason = string.Empty;
            return o is EditorOptionsAttribute;
        }

        public override void Assign(IRDAttribute o)
        {
            var other = (EditorOptionsAttribute) o;
            SetValue(other.GetXmlNodeValue());
        }

        protected override void UpdateValue(string value)
        {
            _value = String.IsNullOrEmpty(value) ? EditorSettings.NotSet : (EditorSettings) int.Parse(value);
        }
    }
}