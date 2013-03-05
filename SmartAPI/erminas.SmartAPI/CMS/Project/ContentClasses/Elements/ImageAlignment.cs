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

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public enum ImageAlignment
    {
        NotSet = 0,
        // ReSharper disable InconsistentNaming
        absbottom,
        absmiddle,
        baseline,
        bottom,
        left,
        middle,
        right,
        texttop,
        top
        // ReSharper restore InconsistentNaming
    }

    public static class ImageAlignmentUtils
    {
        public static ImageAlignment ToImageAlignment(this string value)
        {
            return string.IsNullOrEmpty(value)
                       ? ImageAlignment.NotSet
                       : (ImageAlignment) Enum.Parse(typeof (ImageAlignment), value);
        }

        public static string ToRQLString(this ImageAlignment align)
        {
            return align == ImageAlignment.NotSet ? "" : align.ToString();
        }
    }
}