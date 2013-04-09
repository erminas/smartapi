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
using System.Xml;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes;
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

    public static class HitListTypeUtils
    {
        public static HitListType ToHitListType(string value)
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

    public class HitList : List
    {
        internal HitList(ContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("elthittype", "eltborder", "eltvspace", "elthspace", "eltusermap", "eltsupplement",
                             "eltalt");

// ReSharper disable ObjectCreationAsStatement
            new BoolXmlNodeAttribute(this, "eltpresetalt");
            new StringEnumXmlNodeAttribute<BasicAlignment>(this, "eltalign", BasicAlignmentUtils.ToRQLString,
                                                           BasicAlignmentUtils.ToBasicAlignment);
// ReSharper restore ObjectCreationAsStatement
        }

        public BasicAlignment Align
        {
            get { return ((StringEnumXmlNodeAttribute<BasicAlignment>) GetAttribute("eltalign")).Value; }
            set { ((StringEnumXmlNodeAttribute<BasicAlignment>) GetAttribute("eltalign")).Value = value; }
        }

        public string AltText
        {
            get { return GetAttributeValue<string>("eltalt"); }
            set { SetAttributeValue("eltalt", value); }
        }

        public string Border
        {
            get { return GetAttributeValue<string>("eltborder"); }
            set { SetAttributeValue("eltborder", value); }
        }

        public override void Commit()
        {
            using (new LanguageContext(ILanguageVariant))
            {
                //we need to have an eltsrc attribute with value sessionkey, otherwise eltalt won't get stored on the server oO
                XmlElement.SetAttributeValue("eltsrc", Session.SESSIONKEY_PLACEHOLDER);

                Project.ExecuteRQL("<TEMPLATE>" + GetSaveString(XmlElement) + "</TEMPLATE>",
                                   Project.RqlType.SessionKeyInProject);
            }
        }

        public string HSpace
        {
            get { return GetAttributeValue<string>("elthspace"); }
            set { SetAttributeValue("elthspace", value); }
        }

        public HitListType HitListType
        {
            get { return ((StringEnumXmlNodeAttribute<HitListType>) GetAttribute("elthittype")).Value; }
            set
            {
                if (value == HitListType.NotSet)
                {
                    throw new ArgumentException(string.Format("Hit list type cannot be set to {0} by the user",
                                                              HitListType.NotSet));
                }
                ((StringEnumXmlNodeAttribute<HitListType>) GetAttribute("elthittype")).Value = value;
            }
        }

        public bool IsAltPreassignedAutomatically
        {
            get { return GetAttributeValue<bool>("eltpresetalt"); }
            set { SetAttributeValue("eltpresetalt", value); }
        }

        public string Supplement
        {
            get { return GetAttributeValue<string>("eltsupplement"); }
            set { SetAttributeValue("eltsupplement", value); }
        }

        public string Usemap
        {
            get { return GetAttributeValue<string>("eltusermap"); }
            set { SetAttributeValue("eltusermap", value); }
        }

        public string VSpace
        {
            get { return GetAttributeValue<string>("eltvspace"); }
            set { SetAttributeValue("eltvspace", value); }
        }
    }
}