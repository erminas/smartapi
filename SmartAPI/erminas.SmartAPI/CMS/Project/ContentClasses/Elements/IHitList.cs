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

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface IHitList : IList
    {
        BasicAlignment Align { get; set; }

        string AltText { get; set; }

        string Border { get; set; }

        string HSpace { get; set; }

        HitListType HitListType { get; set; }

        bool IsAltPreassignedAutomatically { get; set; }

        string Supplement { get; set; }

        string Usemap { get; set; }

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

        [RedDot("eltalt")]
        public string AltText
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltborder")]
        public string Border
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public override void CommitInLanguage(string abbreviation)
        {
            var element = GetElementForLanguage(abbreviation);
            //we need to have an eltsrc attribute with value sessionkey, otherwise eltalt won't get stored on the server oO
            element.SetAttributeValue("eltsrc", RQL.SESSIONKEY_PLACEHOLDER);

            base.CommitInLanguage(abbreviation);
        }

        [RedDot("elthspace")]
        public string HSpace
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("elthittype", ConverterType = typeof (StringEnumConverter<HitListType>))]
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

        [RedDot("eltpresetalt")]
        public bool IsAltPreassignedAutomatically
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltsupplement")]
        public string Supplement
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltusermap")]
        public string Usemap
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltvspace")]
        public string VSpace
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }
    }
}