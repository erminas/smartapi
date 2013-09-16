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
using erminas.SmartAPI.CMS.Converter;
using erminas.SmartAPI.Exceptions;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    [EnumConversionHelper]
    public static class SpecialDataFieldFormatUtils
    {
        public static string ToRQLString(this SpecialDataFieldFormat value)
        {
            switch (value)
            {
                case SpecialDataFieldFormat.DefaultUserDefined:
                    return "no";
                case SpecialDataFieldFormat.DefaultHTML:
                    return "HTML";
                case SpecialDataFieldFormat.DefaultImage:
                    return "Image";
                case SpecialDataFieldFormat.TextUserDefined:
                    return "1";
                case SpecialDataFieldFormat.DateUserDefined:
                    return "5";
                case SpecialDataFieldFormat.CurrencyUserDefined:
                    return "48";
                default:
                    throw new ArgumentException(string.Format("Unknown {0} value: {1}",
                                                              typeof (SpecialDataFieldFormat).Name, value));
            }
        }

        public static SpecialDataFieldFormat ToSpecialDataFieldFormat(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return SpecialDataFieldFormat.DefaultUserDefined;
            }
            switch (value.ToUpperInvariant())
            {
                case "DEFAULT":
                case "NO":
                    return SpecialDataFieldFormat.DefaultUserDefined;
                case "1":
                    return SpecialDataFieldFormat.TextUserDefined;
                case "5":
                    return SpecialDataFieldFormat.DateUserDefined;
                case "48":
                    return SpecialDataFieldFormat.CurrencyUserDefined;
                case "HTML":
                    return SpecialDataFieldFormat.DefaultHTML;
                case "IMAGE":
                    return SpecialDataFieldFormat.DefaultImage;
                default:
                    throw new ArgumentException(string.Format("Cannot convert string value {1} to {0}",
                                                              typeof (SpecialDataFieldFormat).Name, value));
            }
        }

        public static void EnsureValueIsSupportedByServerVersion(IProjectObject obj, SpecialDataFieldFormat value)
        {
            var serverVersion = obj.Session.ServerVersion;
            switch (value)
            {
                case SpecialDataFieldFormat.DefaultUserDefined:
                case SpecialDataFieldFormat.DefaultHTML:
                case SpecialDataFieldFormat.DefaultImage:
                    return;
                case SpecialDataFieldFormat.TextUserDefined:
                case SpecialDataFieldFormat.DateUserDefined:
                case SpecialDataFieldFormat.CurrencyUserDefined:
                    var version = new Version(11, 0);
                    if (serverVersion < version)
                    {
                        throw new SmartAPIException(obj.Session.ServerLogin, string.Format("Cannot set {0} to value {1} for server versions older than {2}", RedDotAttributeDescription.GetDescriptionForElement("eltcolumniotype"), value.ToString(), version));
                    }
                    return;
                default:
                    throw new ArgumentException(string.Format("Unknown {0} value: {1}",
                                                              typeof(SpecialDataFieldFormat).Name, value));
            }
        }
    }

    public enum SpecialDataFieldFormat
    {
        DefaultUserDefined,
        DefaultHTML,
        DefaultImage,
        DateUserDefined,
        CurrencyUserDefined,
        TextUserDefined
    }
}