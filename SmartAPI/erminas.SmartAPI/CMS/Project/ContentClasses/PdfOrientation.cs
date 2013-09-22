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

namespace erminas.SmartAPI.CMS.Project.ContentClasses
{
    [EnumConversionHelper]
    public static class PdfOrientationUtils
    {
        public static PdfOrientation ToPdfOrientation(this string value)
        {
            switch (value.ToUpperInvariant())
            {
                case "DEFAULT":
                    return PdfOrientation.Default;
                case "PORTRAIT":
                    return PdfOrientation.Portrait;
                case "LANDSCAPE":
                    return PdfOrientation.Landscape;
                default:
                    throw new ArgumentException(string.Format("Cannot convert string value {1} to {0}",
                                                              typeof (PdfOrientation).Name, value));
            }
        }

        public static string ToRQLString(this PdfOrientation value)
        {
            switch (value)
            {
                case PdfOrientation.Default:
                    return "default";
                case PdfOrientation.Portrait:
                    return "portrait";
                case PdfOrientation.Landscape:
                    return "landscape";
                default:
                    throw new ArgumentException(string.Format("Unknown {0} value: {1}",
                                                              typeof (PdfOrientationUtils).Name, value));
            }
        }
    }

    public enum PdfOrientation
    {
        Default = 0,
        Portrait,
        Landscape
    }
}