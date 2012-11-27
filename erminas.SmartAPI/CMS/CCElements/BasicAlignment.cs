using System;

namespace erminas.SmartAPI.CMS.CCElements
{
    public enum BasicAlignment
    {
        NotSet = 0,
        // ReSharper disable InconsistentNaming
        left,
        right,
        center
        // ReSharper restore InconsistentNaming
    }

    public static class BasicAlignmentUtils
    {
        public static string ToRQLString(this BasicAlignment align)
        {
            return align == BasicAlignment.NotSet ? "" : align.ToString();
        }

        public static BasicAlignment ToBasicAlignment(this string value)
        {
            return string.IsNullOrEmpty(value)
                       ? BasicAlignment.NotSet
                       : (BasicAlignment) Enum.Parse(typeof (BasicAlignment), value);
        }
    }
}