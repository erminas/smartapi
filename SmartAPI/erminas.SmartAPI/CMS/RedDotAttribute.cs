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
using System.Globalization;
using System.Xml;
using erminas.SmartAPI.CMS.Project;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class RedDotAttribute : Attribute
    {
        public readonly string ElementName;
        private IAttributeConvertBase _converterInstance;
        private Type _converterType;
        private string _description;
        private bool _isReadOnly;
        private Type _targetType;

        public RedDotAttribute(string elementName)
        {
            ElementName = elementName;
        }

        public Type ConverterType
        {
            get { return _converterType; }
            set
            {
                if (!IsConverterType(value))
                {
                    throw new SmartAPIInternalException(string.Format("Invalid converter type '{0}' for element {1}",
                                                                      value.Name, ElementName));
                }
                try
                {
                    _targetType =
                        _converterType.GetInterface(typeof (IAttributeConverter<object>).Name).GetGenericArguments()[0];
                    _converterInstance = (IAttributeConvertBase) value.GetConstructor(new Type[0]).Invoke(new object[0]);
                    _converterType = value;
                } catch (Exception e)
                {
                    throw new SmartAPIInternalException(
                        string.Format("Invalid converter type '{0}' for element {1}", value.Name, ElementName), e);
                }
            }
        }

        public string DependsOn { get; set; }

        public string Description
        {
            get { return _description ?? RedDotAttributeDescription.GetDescriptionForElement(ElementName); }
            set { _description = value; }
        }

        public bool IsReadOnly
        {
            get { return _converterInstance != null ? _converterInstance.IsReadOnly || _isReadOnly : _isReadOnly; }
            set { _isReadOnly = value; }
        }

        public T ReadFrom<T>(IProjectObject sourceProject, XmlElement element)
        {
            Type type = typeof (T);
            return _converterInstance != null
                       ? GetCustomConversion<T>(sourceProject, element, type)
                       : GetDefaultConversion<T>(element, type);
        }

        public void WriteTo<T>(IProjectObject targetProject, XmlElement element, T value)
        {
            if (IsReadOnly)
            {
                throw new SmartAPIException((string) null,
                                            string.Format("Cannot write to read only attribute {0}", Description));
            }
            if (_converterInstance != null)
            {
                SetWithCustomConversion(targetProject, element, value);
            }
            else
            {
                SetWithDefaultConversion(element, value);
            }
        }

        private T GetCustomConversion<T>(IProjectObject sourceProject, XmlElement element, Type type)
        {
            if (_targetType != type)
            {
                throw new SmartAPIInternalException(
                    string.Format("Converter type does not match Convert<T> call for element {0}", ElementName));
            }

            return ((IAttributeConverter<T>) _converterInstance).ConvertFrom(sourceProject, element, this);
        }

        private T GetDefaultConversion<T>(XmlElement element, Type type)
        {
            if (type == typeof (string))
            {
                return (T) (object) element.GetAttributeValue(ElementName);
            }
            if (type == typeof (bool))
            {
                return (T) (object) element.GetBoolAttributeValue(ElementName).GetValueOrDefault();
            }
            if (type == typeof (int?))
            {
                return (T) (object) element.GetIntAttributeValue(ElementName);
            }
            throw new SmartAPIInternalException(string.Format("No matching conversion for element {0} to type {1}",
                                                              ElementName, type.Name));
        }

        private bool IsConverterType(Type value)
        {
            return value.GetInterface(typeof (IAttributeConverter<object>).Name) == null;
        }

        private void SetWithCustomConversion<T>(IProjectObject targetProject, XmlElement element, T value)
        {
            if (typeof (T) != _targetType)
            {
                throw new SmartAPIInternalException(
                    string.Format("Converter type {0} does not match Set<T> call for element {1} with type {2}",
                                  _targetType.Name, ElementName, typeof (T).Name));
            }

            ((IAttributeConverter<T>) _converterInstance).WriteTo(targetProject, element, this, value);
        }

        private void SetWithDefaultConversion<T>(XmlElement element, T value)
        {
            Type type = typeof (T);
            if (type == typeof (string))
            {
                element.SetAttributeValue(ElementName, (string) (object) value);
                return;
            }

            if (type == typeof (bool))
            {
                element.SetAttributeValue(ElementName, (bool) (object) value ? "1" : "0");
                return;
            }
            if (type == typeof (int?))
            {
                var i = ((int?) (object) value);
                element.SetAttributeValue(ElementName, i != null ? i.Value.ToString(CultureInfo.InvariantCulture) : null);
                return;
            }
            throw new SmartAPIInternalException(string.Format("No matching conversion for element {0} to type {1}",
                                                              ElementName, type.Name));
        }
    }
}