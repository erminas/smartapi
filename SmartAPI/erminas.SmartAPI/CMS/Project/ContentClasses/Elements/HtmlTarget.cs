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
    public static class HtmlTargetUtils
    {
        public static HtmlTarget ToHtmlTarget(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return HtmlTarget.None;
            }
            switch (value.ToUpperInvariant())
            {
                case "_BLANK":
                    return HtmlTarget.Blank;
                case "_PARENT":
                    return HtmlTarget.Parent;
                case "_TOP":
                    return HtmlTarget.Top;
                case "_SELF":
                    return HtmlTarget.Self;
                default:
                    throw new ArgumentException(string.Format("Cannot convert string value {1} to {0}",
                                                              typeof (HtmlTarget).Name, value));
            }
        }

        public static string ToRQLString(this HtmlTarget value)
        {
            switch (value)
            {
                case HtmlTarget.None:
                    return string.Empty;
                case HtmlTarget.Blank:
                    return "_blank";
                case HtmlTarget.Parent:
                    return "_parent";
                case HtmlTarget.Top:
                    return "_top";
                case HtmlTarget.Self:
                    return "_self";
                default:
                    throw new ArgumentException(string.Format("Unknown {0} value: {1}", typeof (HtmlTarget).Name, value));
            }
        }
    }

    public enum HtmlTarget
    {
        None = 0,
        Blank,
        Parent,
        Top,
        Self
    }
}