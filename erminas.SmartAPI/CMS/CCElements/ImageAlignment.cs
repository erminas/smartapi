using System;

namespace erminas.SmartAPI.CMS.CCElements
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
        public static string ToRQLString(this ImageAlignment align)
        {
            return align == ImageAlignment.NotSet ? "" : align.ToString();
        }

        public static ImageAlignment ToImageAlignment(this string value)
        {
            return string.IsNullOrEmpty(value)
                       ? ImageAlignment.NotSet
                       : (ImageAlignment) Enum.Parse(typeof (ImageAlignment), value);
        }
    }
}