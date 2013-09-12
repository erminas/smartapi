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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Xml;
using erminas.SmartAPI.CMS.Converter;
using erminas.SmartAPI.Utils;

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

        IEnumerable<IOptionListSelection> Entries { get; }

        /// <summary>
        ///     The entries as xml string as provided by RQL.
        ///     In a future version a Entries property giving access to an object model of the entries might get added.
        /// </summary>
        string EntriesAsString { get; set; }

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
        IEnumerable<IOptionListEntry> Entries { get; }
        Guid Guid { get; }
        IOptionList OptionList { get; }
    }

    internal sealed class OptionListSelection : IOptionListSelection
    {
        private readonly IOptionList _optionList;

        public OptionListSelection(IOptionList optionList, XmlElement element)
        {
            _optionList = optionList;
            Guid = element.GetGuid();
            Entries = GetEntries(element);
        }

        public IEnumerable<IOptionListEntry> Entries { get; private set; }
        public Guid Guid { get; private set; }

        public IOptionList OptionList
        {
            get { return _optionList; }
        }

        public IProject Project
        {
            get { return OptionList.Project; }
        }

        public ISession Session
        {
            get { return OptionList.Session; }
        }

        private ReadOnlyCollection<OptionListEntry> GetEntries(XmlElement element)
        {
            return (from XmlElement curEntry in element.GetElementsByTagName("ITEM")
                    select
                        new OptionListEntry
                            {
                                LanguageVariant = Project.LanguageVariants[curEntry.GetAttributeValue("languageid")],
                                Name = curEntry.GetName(),
                                Value = curEntry.InnerText
                            }).ToList().AsReadOnly();
        }
    }

    internal class OptionList : ContentClassContentElement, IOptionList
    {
        private const string ELTDEFAULTVALUE = "eltdefaultvalue";

        internal OptionList(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
          
// ReSharper disable ObjectCreationAsStatement
            //TODO neu implementieren:
            //new OptionListSelectionAttribute(this, "eltoptionlistdata", xmlElement);
// ReSharper restore ObjectCreationAsStatement
        }

        [RedDot("eltparentelementguid", ConverterType = typeof (ContentClassElementConverter))]
        public IContentClassElement ChildElementOf
        {
            get { return GetAttributeValue<IContentClassElement>(); }
            set { SetAttributeValue(value); }
        }

        public string DefaultValueString
        {
            get { return XmlElement.GetAttributeValue(ELTDEFAULTVALUE); }

            set { XmlElement.SetAttributeValue(ELTDEFAULTVALUE, value); }
        }

        [RedDot("eltrddescription")]
        public string Description
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
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

        [RedDot("eltlanguagedependentname")]
        public bool HasLanguageDependendNames
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltlanguagedependentvalue")]
        public bool HasLanguageDependendValues
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltuserdefinedallowed")]
        public bool IsAllowingOtherValues
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltrdexample")]
        public string SampleText
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltorderby", ConverterType = typeof (EnumConverter<SortMode>))]
        public SortMode SortMode
        {
            get { return GetAttributeValue<SortMode>(); }
            set { SetAttributeValue(value); }
        }
    }
}