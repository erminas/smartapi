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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public enum SortMode
    {
        Ascending = 2,
        Descending = 3
    }

    public interface IOptionList : IContentClassContentElement
    {
        IContentClassElement ChildElementOf { get; set; }
        string DefaultValueString { get; set; }
        string Description { get; set; }

        /// <summary>
        ///     The entries as xml string as provided by RQL.
        ///     In a future version a Entries property giving access to an object model of the entries might get added.
        /// </summary>
        string EntriesAsString { get; set; }

        IEnumerable<IOptionListSelection> Entries { get; }

        bool HasLanguageDependendNames { get; set; }
        bool HasLanguageDependendValues { get; set; }
        bool IsAllowingOtherValues { get; set; }
        string SampleText { get; set; }
        SortMode SortMode { get; set; }
    }

    public interface IOptionListEntry
    {
        ILanguageVariant LanguageVariant { get; }
        string Name { get; }
        string Value { get; }
    }

    internal class OptionListEntry : IOptionListEntry
    {
        public ILanguageVariant LanguageVariant { get; internal set; }
        public string Name { get; internal set; }
        public string Value { get; internal set; }
    }

    public interface IOptionListSelection : IProjectObject
    {
        IOptionList OptionList { get; }
        Guid Guid { get; }
        IEnumerable<IOptionListEntry> Entries { get; }
    }

    internal sealed class OptionListSelection : IOptionListSelection
    {
        private readonly IOptionList _optionList;

        public OptionListSelection(IOptionList optionList, XmlElement element)
        {
            _optionList = optionList;
            Guid = element.GetGuid();
            Entries = (from XmlElement curEntry in element.GetElementsByTagName("ITEM")
                       select
                           new OptionListEntry()
                               {
                                   LanguageVariant = Project.LanguageVariants[curEntry.GetAttributeValue("languageid")],
                                   Name = curEntry.GetName(),
                                   Value = curEntry.InnerText
                               }).ToList().AsReadOnly();
        }

        public IOptionList OptionList
        {
            get { return _optionList; }
        }

        public Guid Guid { get; private set; }
        public IEnumerable<IOptionListEntry> Entries { get; private set; }

        public IProject Project
        {
            get { return OptionList.Project; }
        }

        public ISession Session { get { return OptionList.Session; } }
    }

    internal class OptionList : ContentClassContentElement, IOptionList
    {
        private const string ELTDEFAULTVALUE = "eltdefaultvalue";

        internal OptionList(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltlanguagedependentvalue", "eltlanguagedependentname", "eltuserdefinedallowed",
                             "eltrdexample", "eltrddescription", "eltorderby", /*"eltparentelementname",*/
                             "eltparentelementguid");
// ReSharper disable ObjectCreationAsStatement
            new OptionListSelectionAttribute(this, "eltoptionlistdata", xmlElement);
// ReSharper restore ObjectCreationAsStatement
        }

        public IContentClassElement ChildElementOf
        {
            get { return ((ElementXmlNodeAttribute) GetAttribute("eltparentelementguid")).Value; }
            set { ((ElementXmlNodeAttribute) GetAttribute("eltparentelementguid")).Value = value; }
        }

        public string DefaultValueString
        {
            get { return XmlElement.GetAttributeValue(ELTDEFAULTVALUE); }

            set { XmlElement.SetAttributeValue(ELTDEFAULTVALUE, value); }
        }

        public string Description
        {
            get { return GetAttributeValue<string>("eltrddescription"); }
            set { SetAttributeValue("eltrddescription", value); }
        }

        public IEnumerable<IOptionListSelection> Entries
        {
            get
            {
                var doc = new XmlDocument();
                doc.LoadXml(HttpUtility.HtmlDecode(EntriesAsString));
                return (from XmlElement curEntry in doc.GetElementsByTagName("SELECTION")
                        select (IOptionListSelection) new OptionListSelection(this, curEntry)).ToList().AsReadOnly();
            }
        }

        public string EntriesAsString
        {
            get { return XmlElement.GetAttributeValue("eltoptionlistdata"); }
            set { XmlElement.SetAttributeValue("eltoptionlistdata", value); }
        }

        public bool HasLanguageDependendNames
        {
            get { return GetAttributeValue<bool>("eltlanguagedependentname"); }
            set { SetAttributeValue("eltlanguagedependentname", value); }
        }

        public bool HasLanguageDependendValues
        {
            get { return GetAttributeValue<bool>("eltlanguagedependentvalue"); }
            set { SetAttributeValue("eltlanguagedependentvalue", value); }
        }

        public bool IsAllowingOtherValues
        {
            get { return GetAttributeValue<bool>("eltuserdefinedallowed"); }
            set { SetAttributeValue("eltuserdefinedallowed", value); }
        }

        public string SampleText
        {
            get { return GetAttributeValue<string>("eltrdexample"); }
            set { SetAttributeValue("eltrdexample", value); }
        }

        public SortMode SortMode
        {
            get { return ((EnumXmlNodeAttribute<SortMode>) GetAttribute("eltorderby")).Value; }
            set { ((EnumXmlNodeAttribute<SortMode>) GetAttribute("eltorderby")).Value = value; }
        }
    }
}