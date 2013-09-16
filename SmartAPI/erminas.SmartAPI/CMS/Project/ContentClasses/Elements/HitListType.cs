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