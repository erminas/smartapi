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
        ILanguageDependentValue<string> DefaultValue { get; }

        bool IsCrlfConvertedToBr { get; set; }

        bool IsDirectEditActivated { get; set; }

        bool IsDragAndDropActivated { get; set; }

        bool IsTextFilterDeactivated { get; set; }

        bool IsUsingEntireTextIfNoMatchingTagsCanBeFound { get; set; }

        int? MaxCharacterCount { get; set; }

        ILanguageDependentValue<string> SampleValue { get; }
    }

    internal abstract class Text : ExtendedContentClassContentElement, IText
    {
        protected Text(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
        }

        [RedDot("eltdefaulttextguid", ConverterType = typeof (DefaultTextConverter))]
        public ILanguageDependentValue<string> DefaultValue
        {
            get { return GetAttributeValue<ILanguageDependentValue<string>>(); }
        }

        [RedDot("eltcrlftobr")]
        public bool IsCrlfConvertedToBr
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltdirectedit")]
        public bool IsDirectEditActivated
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltdragdrop")]
        public bool IsDragAndDropActivated
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltdeactivatetextfilter")]
        public bool IsTextFilterDeactivated
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltwholetext")]
        public bool IsUsingEntireTextIfNoMatchingTagsCanBeFound
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltmaxsize")]
        public int? MaxCharacterCount
        {
            get { return GetAttributeValue<int?>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltrdexampleguid", ConverterType = typeof (SampleTextConverter))]
        public ILanguageDependentValue<string> SampleValue
        {
            get { return GetAttributeValue<ILanguageDependentValue<string>>(); }
        }
    }
}