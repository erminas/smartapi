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

using System.Xml;
using erminas.SmartAPI.CMS.Converter;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface IText : IExtendedContentClassContentElement
    {
        [RedDot("eltdefaulttextguid", ConverterType = typeof (DefaultTextConverter))]
        string DefaultValue { get; set; }

        [RedDot("eltcrlftobr")]
        bool IsCrlfConvertedToBr { get; set; }

        [RedDot("eltdirectedit")]
        bool IsDirectEditActivated { get; set; }

        [RedDot("eltdragdrop")]
        bool IsDragAndDropActivated { get; set; }

        [RedDot("eltdeactivatetextfilter")]
        bool IsTextFilterDeactivated { get; set; }

        [RedDot("eltwholetext")]
        bool IsUsingEntireTextIfNoMatchingTagsCanBeFound { get; set; }

        [RedDot("eltmaxsize")]
        int? MaxCharacterCount { get; set; }

        [RedDot("eltrdexampleguid", ConverterType = typeof (SampleTextConverter))]
        string SampleValue { get; set; }
    }

    internal abstract class Text : ExtendedContentClassContentElement, IText
    {
        protected Text(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
        }

        public string DefaultValue
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public bool IsCrlfConvertedToBr
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        public bool IsDirectEditActivated
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        public bool IsDragAndDropActivated
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        public bool IsTextFilterDeactivated
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        public bool IsUsingEntireTextIfNoMatchingTagsCanBeFound
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        public int? MaxCharacterCount
        {
            get { return GetAttributeValue<int?>(); }
            set { SetAttributeValue(value); }
        }

        public string SampleValue
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }
    }
}