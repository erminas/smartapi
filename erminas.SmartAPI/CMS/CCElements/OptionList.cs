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

using System.Xml;
using erminas.SmartAPI.CMS.CCElements.Attributes;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.CCElements
{
    public enum SortMode
    {
        Ascending = 2,
        Descending = 3
    }

    //defaultvalue gets automatically handled by optionlistselectionattribute
    //if a value is assigend for optionlistdata, the defaultvalue will get set, too, from the same
    //source
    public class OptionList : CCContentElement
    {
        private const string ELTDEFAULTVALUE = "eltdefaultvalue";

        public OptionList(ContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltlanguagedependentvalue", "eltlanguagedependentname", "eltuserdefinedallowed",
                             "eltrdexample", "eltrddescription", "eltorderby",
                             /*"eltparentelementname",*/ "eltparentelementguid");
            new OptionListSelectionAttribute(this, "eltoptionlistdata", xmlElement);
        }

        public string DefaultValueString
        {
            get { return XmlNode.GetAttributeValue(ELTDEFAULTVALUE); }

            set { XmlNode.SetAttributeValue(ELTDEFAULTVALUE, value); }
        }

        public bool HasLanguageDependendValues
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltlanguagedependentvalue")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltlanguagedependentvalue")).Value = value; }
        }

        public bool HasLanguageDependendNames
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltlanguagedependentname")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltlanguagedependentname")).Value = value; }
        }

        public bool IsAllowingOtherValues
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltuserdefinedallowed")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltuserdefinedallowed")).Value = value; }
        }

        public string SampleText
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltrdexample")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltrdexample")).Value = value; }
        }

        public string Description
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltrddescription")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltrddescription")).Value = value; }
        }

        public SortMode SortMode
        {
            get { return ((EnumXmlNodeAttribute<SortMode>) GetAttribute("eltorderby")).Value; }
            set { ((EnumXmlNodeAttribute<SortMode>) GetAttribute("eltorderby")).Value = value; }
        }

        public CCElement ChildElementOf
        {
            get { return ((ElementXmlNodeAttribute) GetAttribute("eltparentelementguid")).Value; }
            set { ((ElementXmlNodeAttribute) GetAttribute("eltparentelementguid")).Value = value; }
        }

        //todo use optionlist classes
        public string Entries
        {
            get { return XmlNode.GetAttributeValue("eltoptionlistdata"); }
            set { XmlNode.SetAttributeValue("eltoptionlistdata", value); }
        }
    }
}