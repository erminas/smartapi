// Smart API - .Net programatical access to RedDot servers
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
using erminas.SmartAPI.CMS.CCElements.Attributes;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.CCElements
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
    }

    #endregion

    public class HitList : List
    {
        public HitList(ContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("elthittype", "eltborder", "eltvspace", "elthspace", "eltusermap", "eltsupplement",
                             "eltalt");

            new BoolXmlNodeAttribute(this, "eltpresetalt");
            new StringEnumXmlNodeAttribute<BasicAlignment>(this, "eltalign", BasicAlignmentUtils.ToRQLString,
                                                           BasicAlignmentUtils.ToBasicAlignment);
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

        public string Border
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltborder")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltborder")).Value = value; }
        }

        public string VSpace
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltvspace")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltvspace")).Value = value; }
        }

        public string HSpace
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("elthspace")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("elthspace")).Value = value; }
        }

        public string Usemap
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltusermap")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltusermap")).Value = value; }
        }

        public BasicAlignment Align
        {
            get { return ((StringEnumXmlNodeAttribute<BasicAlignment>) GetAttribute("eltalign")).Value; }
            set { ((StringEnumXmlNodeAttribute<BasicAlignment>) GetAttribute("eltalign")).Value = value; }
        }

        public bool IsAltPreassignedAutomatically
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltpresetalt")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltpresetalt")).Value = value; }
        }

        public string AltText
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltalt")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltalt")).Value = value; }
        }

        public string Supplement
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltsupplement")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltsupplement")).Value = value; }
        }

        public override void Commit()
        {
            using (new LanguageContext(LanguageVariant))
            {
                //we need to have an eltsrc attribute with value sessionkey, otherwise eltalt won't get stored on the server oO
                XmlElement.SetAttributeValue("eltsrc", Session.SESSIONKEY_PLACEHOLDER);

                ContentClass.Project.ExecuteRQL("<TEMPLATE>" + GetSaveString(XmlElement) + "</TEMPLATE>",
                                                Project.RqlType.SessionKeyInProject);
            }
        }
    }
}