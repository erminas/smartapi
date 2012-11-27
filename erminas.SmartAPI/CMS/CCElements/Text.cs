/*
 * Smart API - .Net programatical access to RedDot servers
 * Copyright (C) 2012  erminas GbR 
 *
 * This program is free software: you can redistribute it and/or modify it 
 * under the terms of the GNU General Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details. 
 *
 * You should have received a copy of the GNU General Public License along with this program.
 * If not, see <http://www.gnu.org/licenses/>. 
 */

using System;
using System.Xml;
using erminas.SmartAPI.CMS.CCElements.Attributes;

namespace erminas.SmartAPI.CMS.CCElements
{
    public abstract class Text : CCExtendedContentElement
    {
        private readonly TextContentAttribute _defaultText;
        private readonly TextContentAttribute _exampleText;
        private readonly StringXmlNodeAttribute _maxSizeAttribute;

        protected Text(ContentClass contentClass, XmlNode xmlNode)
            : base(contentClass, xmlNode)
        {
            CreateAttributes("eltcrlftobr", "eltdeactivatetextfilter", "eltmaxsize",
                             "eltwholetext", "eltdirectedit", "eltdragdrop");

            _defaultText = new TextContentAttribute(this, TextContentAttribute.TextType.Default, "eltdefaulttextguid");
            _exampleText = new TextContentAttribute(this, TextContentAttribute.TextType.Sample, "eltrdexampleguid");

            _maxSizeAttribute = (StringXmlNodeAttribute) Attributes.Find(x => x.Name == "eltmaxsize");
        }

        public bool IsCrlfConvertedToBr
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltcrlftobr")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltcrlftobr")).Value = value; }
        }

        public bool IsTextFilterDeactivated
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltdeactivatetextfilter")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltdeactivatetextfilter")).Value = value; }
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

        public bool IsUsingEntireTextIfNoMatchingTagsCanBeFound
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltwholetext")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltwholetext")).Value = value; }
        }

        public bool IsDirectEditActivated
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltdirectedit")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltdirectedit")).Value = value; }
        }

        public bool IsDragAndDropActivated
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltdragdrop")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltdragdrop")).Value = value; }
        }

        public string DefaultText
        {
            get { return (GetAttribute("eltdefaulttextguid") as TextContentAttribute).Text; }
            set { (GetAttribute("eltdefaulttextguid") as TextContentAttribute).Text = value; }
        }

        public string ExampleText
        {
            get { return (GetAttribute("eltrdexampleguid") as TextContentAttribute).Text; }
            set { (GetAttribute("eltrdexampleguid") as TextContentAttribute).Text = value; }
        }

        public override void Commit()
        {
            _defaultText.Commit();
            _exampleText.Commit();
            base.Commit();
        }
    }
}