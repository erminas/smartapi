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
                    throw new ArgumentException(string.Format("Unknown {0} value: {1}",
                                                              typeof (TargetFormat).Name, format));
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