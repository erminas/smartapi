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
    public static class HitListTypeUtils
    {
        public static HitListType ToHitListType(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return HitListType.NotSet;
            }
            switch (value.ToUpperInvariant())
            {
                case "GRAFIK":
                    return HitListType.MatchingImages;
                case "TEXT":
                    return HitListType.MatchingTexts;
                default:
                    throw new ArgumentException(string.Format("Cannot convert string value {1} to {0}",
                                                              typeof (HitListType).Name, value));
            }
        }

        public static string ToRQLString(this HitListType value)
        {
            switch (value)
            {
                case HitListType.NotSet:
                    return string.Empty;
                case HitListType.MatchingImages:
                    return "Grafik";
                case HitListType.MatchingTexts:
                    return "Text";
                default:
                    throw new ArgumentException(string.Format("Unknown {0} value: {1}", typeof (HitListType).Name, value));
            }
        }
    }

    public enum HitListType
    {
        NotSet = 0,
        MatchingTexts,
        MatchingImages
    }
}