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

namespace erminas.SmartAPI.CMS.CCElements.Attributes
{
    internal class DelegatingAttribute<T> : IRDAttribute
    {
        public readonly CCElement CcElement;
        private readonly string _description;
        private readonly Func<T> _getValue;
        private readonly Func<T, string> _getValueDisplayString;
        private readonly Action<T> _setValue;

        public DelegatingAttribute(CCElement ccElement, string name, Func<T> getFunction, Action<T> setFunction,
                                   Func<T, string> getValueDisplayString, string description)
        {
            CcElement = ccElement;
            Name = name;
            ccElement.RegisterAttribute(this);
            _getValue = getFunction;
            _setValue = setFunction;
            _getValueDisplayString = getValueDisplayString;
            _description = description;
        }

        #region IRDAttribute Members

        public void Assign(IRDAttribute o)
        {
            //don't set value, if element isn't initialized
            //needed for textelements, which need to set the text
            //after the element is created
            if (!CcElement.Guid.Equals((Guid.Empty)))
            {
                var da = (DelegatingAttribute<T>) o;
                _setValue(da._getValue());
            }
        }

        public string Description
        {
            get { return _description; }
        }

        public object DisplayObject
        {
            get { return _getValueDisplayString(_getValue()); }
        }

        public override bool Equals(object o)
        {
            var attr = o as DelegatingAttribute<T>;
            return attr != null && Name == attr.Name && Equals(_getValue(), attr._getValue());
        }

        public bool IsAssignableFrom(IRDAttribute o, out string reason)
        {
            reason = string.Empty;
            return o is DelegatingAttribute<T>;
        }

        public string Name { get; private set; }

        public void Refresh()
        {
        }

        #endregion

        public override int GetHashCode()
        {
            T value = _getValue();
            return Name.GetHashCode() + 13*(Equals(default(T), value) ? value.GetHashCode() : 0);
        }
    }
}