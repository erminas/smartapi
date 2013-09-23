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

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    [EnumConversionHelper]
    public static class ShapeUtils
    {
        public static string ToRQLString(this Shape value)
        {
            switch (value)
            {
                case Shape.None:
                    return string.Empty;
                case Shape.Rect:
                    return "rect";
                case Shape.Polygon:
                    return "polygon";
                case Shape.Circle:
                    return "circle";
                default:
                    throw new ArgumentException(string.Format("Unknown {0} value: {1}", typeof (Shape).Name, value));
            }
        }

        public static Shape ToShape(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return Shape.None;
            }
            switch (value.ToLowerInvariant())
            {
                case "rect":
                    return Shape.Rect;
                case "circle":
                    return Shape.Circle;
                case "polygon":
                    return Shape.Polygon;
                default:
                    throw new ArgumentException(string.Format("Cannot convert string value {1} to {0}",
                                                              typeof (Shape).Name, value));
            }
        }
    }

    public enum Shape
    {
        None = 0,
        Rect,
        Circle,
        Polygon
    }
}