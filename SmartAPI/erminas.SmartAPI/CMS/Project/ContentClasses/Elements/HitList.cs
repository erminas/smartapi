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
using System.Xml;
using erminas.SmartAPI.CMS.Converter;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{

    #region HitListType

    public enum HitListType
    {
        NotSet = 0,
        MatchingTexts,
        MatchingImages
    }

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

    #endregion

    public interface IHitList : IList
    {
        [RedDot("eltalign", ConverterType = typeof (StringEnumConverter<BasicAlignment>))]
        BasicAlignment Align { get; set; }

        [RedDot("eltalt")]
        string AltText { get; set; }

        [RedDot("eltborder")]
        string Border { get; set; }

        [RedDot("elthspace")]
        string HSpace { get; set; }

        [RedDot("elthittype", ConverterType = typeof (StringEnumConverter<HitListType>))]
        HitListType HitListType { get; set; }

        [RedDot("eltpresetalt")]
        bool IsAltPreassignedAutomatically { get; set; }

        [RedDot("eltsupplement")]
        string Supplement { get; set; }

        [RedDot("eltusermap")]
        string Usemap { get; set; }

        [RedDot("eltvspace")]
        string VSpace { get; set; }
    }

    internal class HitList : List, IHitList
    {
        internal HitList(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
        }

        [RedDot("eltalign", ConverterType = typeof (StringEnumConverter<BasicAlignment>))]
        public BasicAlignment Align
        {
            get { return GetAttributeValue<BasicAlignment>(); }
            set { SetAttributeValue(value); }
        }

        public string AltText
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public string Border
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public override void CommitInCurrentLanguage()
        {
            using (new LanguageContext(LanguageVariant))
            {
                //we need to have an eltsrc attribute with value sessionkey, otherwise eltalt won't get stored on the server oO
                XmlElement.SetAttributeValue("eltsrc", RQL.SESSIONKEY_PLACEHOLDER);

                Project.ExecuteRQL("<TEMPLATE>" + GetSaveString(XmlElement) + "</TEMPLATE>", RqlType.SessionKeyInProject);
            }
        }

        public string HSpace
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public HitListType HitListType
        {
            get { return GetAttributeValue<HitListType>(); }
            set
            {
                if (value == HitListType.NotSet)
                {
                    throw new ArgumentException(string.Format("Hit list type cannot be set to {0} by the user",
                                                              HitListType.NotSet));
                }
                SetAttributeValue(value);
            }
        }

        public bool IsAltPreassignedAutomatically
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        public string Supplement
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public string Usemap
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public string VSpace
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }
    }
}