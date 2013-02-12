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
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.CCElements.Attributes;

namespace erminas.SmartAPI.CMS.CCElements
{
    public abstract class Text : ExtendedContentClassContentElement
    {
        private readonly TextContentAttribute _defaultText;
        private readonly TextContentAttribute _exampleText;
        private readonly StringXmlNodeAttribute _maxSizeAttribute;

        protected Text(ContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltcrlftobr", "eltdeactivatetextfilter", "eltmaxsize", "eltwholetext", "eltdirectedit",
                             "eltdragdrop");

            _defaultText = new TextContentAttribute(this, TextContentAttribute.TextType.Default, "eltdefaulttextguid");
            _exampleText = new TextContentAttribute(this, TextContentAttribute.TextType.Sample, "eltrdexampleguid");

            _maxSizeAttribute = (StringXmlNodeAttribute) Attributes.First(x => x.Name == "eltmaxsize");
        }

        public override void Commit()
        {
            _defaultText.Commit();
            _exampleText.Commit();
            base.Commit();
        }

        public string DefaultText
        {
            get { return ((TextContentAttribute) GetAttribute("eltdefaulttextguid")).Text; }
            set { ((TextContentAttribute) GetAttribute("eltdefaulttextguid")).Text = value; }
        }

        public string ExampleText
        {
            get { return ((TextContentAttribute) GetAttribute("eltrdexampleguid")).Text; }
            set { ((TextContentAttribute) GetAttribute("eltrdexampleguid")).Text = value; }
        }

        public bool IsCrlfConvertedToBr
        {
            get { return GetAttributeValue<bool>("eltcrlftobr"); }
            set { SetAttributeValue("eltcrlftobr", value); }
        }

        public bool IsDirectEditActivated
        {
            get { return GetAttributeValue<bool>("eltdirectedit"); }
            set { SetAttributeValue("eltdirectedit", value); }
        }

        public bool IsDragAndDropActivated
        {
            get { return GetAttributeValue<bool>("eltdragdrop"); }
            set { SetAttributeValue("eltdragdrop", value); }
        }

        public bool IsTextFilterDeactivated
        {
            get { return GetAttributeValue<bool>("eltdeactivatetextfilter"); }
            set { SetAttributeValue("eltdeactivatetextfilter", value); }
        }

        public bool IsUsingEntireTextIfNoMatchingTagsCanBeFound
        {
            get { return GetAttributeValue<bool>("eltwholetext"); }
            set { SetAttributeValue("eltwholetext", value); }
        }

        public int? MaxCharacterCount
        {
            get { return string.IsNullOrEmpty(_maxSizeAttribute.Value) ? (int?) null : int.Parse(_maxSizeAttribute.Value); }
            set
            {
                if (value.HasValue)
                {
                    _maxSizeAttribute.Value = value.ToString();
                }
                else
                {
                    throw new ArgumentNullException("value");
                }
            }
        }
    }
}