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
using System.Linq;
using System.Reflection;
using System.Xml;
using erminas.SmartAPI.CMS.Project;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Converter
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class EnumConversionHelperAttribute : Attribute
    {
    }

    internal static class StringEnumConversionRegistry
    {
        private static readonly Dictionary<Type, MethodInfo> FROM_STRING_METHODS = new Dictionary<Type, MethodInfo>();
        private static readonly Dictionary<Type, MethodInfo> TO_STRING_METHODS = new Dictionary<Type, MethodInfo>();

        static StringEnumConversionRegistry()
        {
            var converters =
                typeof (StringEnumConversionRegistry).Assembly.GetTypes()
                                                     .Where(
                                                         type =>
                                                         type.GetCustomAttributes(
                                                             typeof (EnumConversionHelperAttribute), false).Any());

            foreach (var curConverter in converters)
            {
                try
                {
                    var toStringConversion = curConverter.GetMethod("ToRQLString",
                                                                    BindingFlags.Static | BindingFlags.Public);

                    var fromStringConversion =
                        curConverter.GetMethods(BindingFlags.Static | BindingFlags.Public)
                                    .Single(
                                        methodInfo =>
                                        methodInfo.GetParameters()
                                                  .SingleOrDefault(info => info.ParameterType == typeof (string)) !=
                                        null);

                    //used as an assertion
                    toStringConversion.GetParameters()
                                      .Single(info => info.ParameterType == fromStringConversion.ReturnType);

                    if (toStringConversion.ReturnType != typeof (string))
                    {
                        throw new Exception(string.Format("Illegal return type for ToRQLString method in class {0}",
                                                          curConverter.Name));
                    }

                    FROM_STRING_METHODS[fromStringConversion.ReturnType] = fromStringConversion;
                    TO_STRING_METHODS[fromStringConversion.ReturnType] = toStringConversion;
                } catch (Exception e)
                {
                    throw new SmartAPIInternalException(
                        string.Format("Class {0} is not a legal converter", curConverter.Name), e);
                }
            }
        }

        public static T ConvertFromString<T>(string value)
        {
            try
            {
                return (T) FROM_STRING_METHODS[typeof (T)].Invoke(null, new object[] {value});
            } catch (KeyNotFoundException)
            {
                throw new SmartAPIInternalException(string.Format("No converter registered for type {0}",
                                                                  typeof (T).Name));
            }
        }

        public static string ConvertToRQLString<T>(T value) where T : struct, IConvertible
        {
            try
            {
                return (string) TO_STRING_METHODS[typeof (T)].Invoke(null, new object[] {value});
            } catch (KeyNotFoundException)
            {
                throw new SmartAPIInternalException(string.Format("No converter registered for type {0}",
                                                                  typeof (T).Name));
            }
        }
    }

    internal class StringEnumConverter<T> : IAttributeConverter<T> where T : struct, IConvertible
    {
        public T ConvertFrom(IProjectObject parent, XmlElement element, RedDotAttribute attribute)
        {
            var strValue = element.GetAttributeValue(attribute.ElementName);
            return StringEnumConversionRegistry.ConvertFromString<T>(strValue);
        }

        public bool IsReadOnly { get; set; }

        public void WriteTo(IProjectObject parent, IXmlReadWriteWrapper writeElement, RedDotAttribute attribute, T value)
        {
            writeElement.SetAttributeValue(attribute.ElementName, StringEnumConversionRegistry.ConvertToRQLString(value));
        }
    }
}