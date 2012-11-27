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
            switch (value.ToLowerInvariant())
            {
                case "jpg":
                    return TargetFormat.Jpeg;
                case "gif":
                    return TargetFormat.Gif;
                case "png":
                    return TargetFormat.Png;
                case "source":
                    return TargetFormat.SameAsOriginalFormat;
                default:
                    throw new ArgumentException(string.Format("Cannot convert string value {1} to {0}",
                                                              typeof (TargetFormat).Name, value));
            }
        }
    }
}