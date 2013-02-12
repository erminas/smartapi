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

namespace erminas.SmartAPI.CMS.CCElements.Attributes
{
    public class InfoElementAttribute : RDXmlNodeAttribute
    {
        private int? _id;

        public InfoElementAttribute(ContentClassElement parent, string name) : base(parent, name, true)
        {
        }

        public override void Assign(IRDAttribute o)
        {
            var attr = (InfoElementAttribute) o;
            SetValue(attr.GetXmlNodeValue());
        }

        public override object DisplayObject
        {
            get
            {
                return _id == null
                           ? null
                           : ((ContentClassElement) Parent).ContentClass.Project.InfoAttributes[_id.Value].Name;
            }
        }

        public override bool IsAssignableFrom(IRDAttribute o, out string reason)
        {
            reason = "";
            return o is InfoElementAttribute;
        }

        public InfoAttribute Value
        {
            get { return _id == null ? null : ((ContentClassElement) Parent).ContentClass.Project.InfoAttributes[_id.Value]; }
            set { SetValue(value == null ? null : value.Id.ToString(CultureInfo.InvariantCulture)); }
        }

        protected override void UpdateValue(string value)
        {
            _id = string.IsNullOrEmpty(value) ? (int?) null : int.Parse(value);
        }
    }
}