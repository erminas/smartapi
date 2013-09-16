using System;
using erminas.SmartAPI.CMS.Converter;

namespace erminas.SmartAPI.CMS.Project.ContentClasses
{
    [EnumConversionHelper]
    public static class PdfOrientationUtils
    {
        public static PdfOrientation ToPdfOrientation(this string value)
        {
            switch (value.ToUpperInvariant())
            {
                case "DEFAULT":
                    return PdfOrientation.Default;
                case "PORTRAIT":
                    return PdfOrientation.Portrait;
                case "LANDSCAPE":
                    return PdfOrientation.Landscape;
                default:
                    throw new ArgumentException(string.Format("Cannot convert string value {1} to {0}",
                                                              typeof (PdfOrientation).Name, value));
            }
        }

        public static string ToRQLString(this PdfOrientation value)
        {
            switch (value)
            {
                case PdfOrientation.Default:
                    return "default";
                case PdfOrientation.Portrait:
                    return "portrait";
                case PdfOrientation.Landscape:
                    return "landscape";
                default:
                    throw new ArgumentException(string.Format("Unknown {0} value: {1}",
                                                              typeof (PdfOrientationUtils).Name, value));
            }
        }
    }

    public enum PdfOrientation
    {
        Default = 0,
        Portrait,
        Landscape
    }
}