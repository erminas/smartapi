// SmartAPI - .Net programmatic access to RedDot servers
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
using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    internal interface IXmlReadWriteWrapper
    {
        string GetAttributeValue(string attributeName);
        bool? GetBoolAttributeValue(string attributeName);
        Guid GetGuid(string attributeName);
        int? GetIntAttributeValue(string attributeName);
        bool IsAttributeSet(ISessionObject session, string attributeName);
        XmlElement MergedElement { get; }
        void SetAttributeValue(string attributeName, string value);
        bool TryGetGuid(string attributeName, out Guid guid);
    }

    internal class XmlReadWriteWrapper : IXmlReadWriteWrapper
    {
        private readonly IDictionary<string, string> _basisOverrides;
        private readonly XmlElement _readElement;
        private readonly IDictionary<string, string> _writtenValues = new Dictionary<string, string>();

        public XmlReadWriteWrapper(XmlElement readElement, IDictionary<string, string> basicOverrides)
        {
            _readElement = readElement;
            _basisOverrides = basicOverrides;
        }

        public string GetAttributeValue(string attributeName)
        {
            string value;
            if (_writtenValues.TryGetValue(attributeName, out value) ||
                _basisOverrides.TryGetValue(attributeName, out value))
            {
                return value;
            }

            return _readElement.GetAttributeValue(attributeName);
        }

        public bool? GetBoolAttributeValue(string attributeName)
        {
            var value = GetIntAttributeValue(attributeName);
            if (value == null)
            {
                return null;
            }
            if (value == 1)
            {
                return true;
            }
            if (value == 0)
            {
                return false;
            }
            throw new SmartAPIException((ServerLogin) null,
                                        string.Format(
                                            "Could not convert value '{0}' of attribute '{1}' to a boolean value", value,
                                            attributeName));
        }

        public Guid GetGuid(string attributeName)
        {
            var strValue = GetAttributeValue(attributeName);
            return Guid.Parse(strValue);
        }

        public int? GetIntAttributeValue(string attributeName)
        {
            var strValue = GetAttributeValue(attributeName);
            return string.IsNullOrEmpty(strValue) ? null : (int?) int.Parse(strValue);
        }

        public bool IsAttributeSet(ISessionObject session, string attributeName)
        {
            var strValue = GetAttributeValue(attributeName);

            return !string.IsNullOrEmpty(strValue) && strValue != ("#" + session.Session.SessionKey);
        }

        public XmlElement MergedElement
        {
            get
            {
                var mergedElement = (XmlElement) _readElement.Clone();
                foreach (var curEntry in _basisOverrides)
                {
                    mergedElement.SetAttributeValue(curEntry.Key, curEntry.Value);
                }
                foreach (var curEntry in _writtenValues)
                {
                    mergedElement.SetAttributeValue(curEntry.Key, curEntry.Value);
                }

                return mergedElement;
            }
        }

        public void SetAttributeValue(string attributeName, string value)
        {
            _writtenValues[attributeName] = value;
        }

        public bool TryGetGuid(string attributeName, out Guid guid)
        {
            var strValue = GetAttributeValue(attributeName);
            return Guid.TryParse(strValue, out guid);
        }

        public IDictionary<string, string> WrittenValues
        {
            get { return _writtenValues; }
        }
    }
}