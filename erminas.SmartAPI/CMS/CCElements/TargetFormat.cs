// Smart API - .Net programatical access to RedDot servers
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

namespace erminas.SmartAPI.CMS.CCElements
{
    public enum TargetFormat
    {
        SameAsOriginalFormat = 0,
        Jpeg,
        Gif,
        Png
    }

    public static class TargetFormatUtils
    {
        public static string ToRQLString(this TargetFormat format)
        {
            switch (format)
            {
                case TargetFormat.Jpeg:
                    return "jpg";
                case TargetFormat.Gif:
                    return "gif";
                case TargetFormat.Png:
                    return "png";
                case TargetFormat.SameAsOriginalFormat:
                    return "source";
                default:
                    throw new ArgumentException(string.Format("Unknown {0} value: {1}", typeof (TargetFormat).Name,
                                                              format));
            }
        }

        public static TargetFormat ToTargetFormat(string value)
        {
            switch (value.ToUpperInvariant())
            {
                case "JPG":
                    return TargetFormat.Jpeg;
                case "GIF":
                    return TargetFormat.Gif;
                case "PNG":
                    return TargetFormat.Png;
                case "SOURCE":
                    return TargetFormat.SameAsOriginalFormat;
                default:
                    throw new ArgumentException(string.Format("Cannot convert string value {1} to {0}",
                                                              typeof (TargetFormat).Name, value));
            }
        }
    }
}