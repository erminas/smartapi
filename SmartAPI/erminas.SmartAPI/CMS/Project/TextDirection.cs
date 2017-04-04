using System;
using erminas.SmartAPI.CMS.Converter;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements;

namespace erminas.SmartAPI.CMS.Project
{
    public enum TextDirection
    {
        LeftToRight,
        RightToLeft
    }

    [EnumConversionHelper]
    public static class TextDirectionUtils
    {
        public static string ToRQLString(this TextDirection value)
        {
            switch (value)
            {
                case TextDirection.LeftToRight:
                    return "ltr";
                case TextDirection.RightToLeft:
                    return "rtl";
                default:
                    throw new ArgumentException(string.Format("Unknown {0} value: {1}", typeof(TextDirection).Name,
                        value));
            }
        }

        public static TextDirection ToTextDirection(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException(string.Format("Cannot convert empty string value to {0}",
                    typeof(TextDirection).Name));
            }
            switch (value.ToLowerInvariant())
            {
                case "ltr":
                    return TextDirection.LeftToRight;
                case "rtl":
                    return TextDirection.RightToLeft;
               
                default:
                    throw new ArgumentException(string.Format("Cannot convert string value {1} to {0}",
                        typeof(Shape).Name, value));
            }
        }
    }
}