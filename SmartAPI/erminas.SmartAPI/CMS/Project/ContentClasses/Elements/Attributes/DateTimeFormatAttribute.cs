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

using System.Globalization;
using erminas.SmartAPI.CMS.Administration.Language;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes
{
    internal class DateTimeFormatAttribute : RDXmlNodeAttribute
    {
        private int? _type;

        public DateTimeFormatAttribute(IContentClassElement parent, string name) : base(parent, name, true)
        {
        }

        public override void Assign(IRDAttribute o)
        {
            var attr = (DateTimeFormatAttribute) o;
            SetValue(attr.GetXmlNodeValue());
        }

        public override object DisplayObject
        {
            get
            {
                if (_type == null)
                {
                    return null;
                }
                if (_type.Value < 0)
                {
                    return "user defined";
                }
                IDateTimeFormat value = GetDateTimeFormat();
                if (value == null)
                {
                    return null;
                }
                return value.Name + " (" + value.Example + ")";
            }
        }

        public override bool IsAssignableFrom(IRDAttribute o, out string reason)
        {
            reason = string.Empty;
            return o is DateTimeFormatAttribute;
        }

        public IDateTimeFormat Value
        {
            get
            {
                if (!_type.HasValue || _type.Value < 0)
                {
                    return null;
                }

                return GetDateTimeFormat();
            }

            set { SetValue(value.TypeId.ToString(CultureInfo.InvariantCulture)); }
        }

        protected override void UpdateValue(string value)
        {
            _type = string.IsNullOrEmpty(value) || value == "#" + Parent.Session.SessionKey ||
                    value.Contains("EmptyBuffer")
                        ? (int?) null
                        : int.Parse(value);
        }

        private IDateTimeFormat GetDateTimeFormat()
        {
            int lcid;
            bool valid = int.TryParse(Parent.XmlElement.GetAttributeValue("eltlcid"), out lcid);
            if (!valid || _type == null)
            {
                return null;
            }

            return
                ((IContentClassElement) Parent).ContentClass.Project.Session.Locales[lcid].DateTimeFormats[_type.Value];
        }
    }
}