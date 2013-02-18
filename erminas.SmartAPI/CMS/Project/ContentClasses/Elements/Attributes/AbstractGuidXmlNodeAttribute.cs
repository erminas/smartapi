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
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes
{
    public abstract class AbstractGuidXmlNodeAttribute<T> : RDXmlNodeAttribute where T : RedDotObject
    {
        private readonly Session _session;
        private T _value;

        protected AbstractGuidXmlNodeAttribute(Session session, RedDotObject parent, string name)
            : base(parent, name, false)
        {
            _session = session;
        }

        public override void Assign(IRDAttribute o)
        {
            T value = ((AbstractGuidXmlNodeAttribute<T>) o).Value;
            SetValue(value == null ? null : value.Name);
        }

        public override object DisplayObject
        {
            get { return _value == null ? string.Empty : _value.Name; }
        }

        public override bool Equals(object o)
        {
            var xmlNodeAttribute = o as AbstractGuidXmlNodeAttribute<T>;
            if (xmlNodeAttribute == null)
            {
                return false;
            }
            T oValue = xmlNodeAttribute.Value;
            if (Value == null)
            {
                return oValue == null;
            }
            if (oValue == null)
            {
                return false;
            }
            return oValue.Name == Value.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool IsAssignableFrom(IRDAttribute o, out string reason)
        {
            var t = (AbstractGuidXmlNodeAttribute<T>) o;
            if (t.Value == null || RetrieveByName(t.Value.Name) != null)
            {
                reason = string.Empty;
                return true;
            }

            reason = GetTypeDescription() + " named '" + t.Value.Name + "' not found in target!";
            return false;
        }

        public T Value
        {
            get
            {
                if (_value != null)
                {
                    return _value;
                }
                UpdateValue(GetXmlNodeValue());
                return _value;
            }

            set
            {
                _value = value;
                SetXmlNodeValue(value != null ? value.Guid.ToRQLString() : null);
            }
        }

        protected abstract string GetTypeDescription();

        protected abstract T RetrieveByGuid(Guid guid);
        protected abstract T RetrieveByName(string name);

        protected override void SetValue(string value)
        {
            UpdateValue(value);
            SetXmlNodeValue(_value == null ? null : _value.Guid.ToRQLString());
        }

        /// <param name="value"> accepts a guid or a name of an existing reddotobject </param>
        protected override void UpdateValue(string value)
        {
            if (String.IsNullOrEmpty(value) || value == Session.SESSIONKEY_PLACEHOLDER)
            {
                _value = null;
                return;
            }

            Guid elementGuid;
            _value = Guid.TryParse(value, out elementGuid)
                         ? RetrieveByGuidInternal(elementGuid)
                         : RetrieveByNameInternal(value);
        }

        private T RetrieveByGuidInternal(Guid guid)
        {
            try
            {
                return RetrieveByGuid(guid);
            } catch (InvalidOperationException e)
            {
                throw new SmartAPIException(_session.ServerLogin,
                                            string.Format("Could not retrieve {0} with GUID {1}", GetTypeDescription(),
                                                          guid.ToRQLString()), e);
            }
        }

        private T RetrieveByNameInternal(string name)
        {
            try
            {
                return RetrieveByName(name);
            } catch (InvalidOperationException e)
            {
                throw new SmartAPIException(_session.ServerLogin,
                                            string.Format("Could not retrieve {0} with name {1}", GetTypeDescription(),
                                                          name), e);
            }
        }
    }
}