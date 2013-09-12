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
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Pages.Elements
{
    public interface IOptionList : IValueElement<IOptionListEntry>
    {
        bool HasDefaultValue { get; }
        IRDEnumerable<IOptionListEntry> PossibleValues { get; }
        string ValueString { get; }
    }

    public interface IOptionListEntry : IRedDotObject
    {
        bool IsDefault { get; }
        string Value { get; }
    }

    internal class OptionListEntry : IOptionListEntry
    {
        public OptionListEntry(Guid defaultGuid, XmlElement element)
        {
            Guid = element.GetGuid("guid");
            Name = element.GetAttributeValue("description");
            Value = element.GetAttributeValue("value");
            IsDefault = defaultGuid == Guid;
        }

        public Guid Guid { get; private set; }
        public bool IsDefault { get; private set; }
        public string Name { get; private set; }
        public string Value { get; private set; }
    }

    [PageElementType(ElementType.OptionList)]
    internal class OptionList : PageElement, IOptionList
    {
        private List<IOptionListEntry> _entries;
        private IOptionListEntry _value;

        public OptionList(IProject project, Guid guid, ILanguageVariant languageVariant)
            : base(project, guid, languageVariant)
        {
        }

        internal OptionList(IProject project, XmlElement xmlElement) : base(project, xmlElement)
        {
            if (xmlElement.GetElementsByTagName("SELECTIONS").Count != 1)
            {
                IsInitialized = false;
                XmlElement = null;
            }
            else
            {
                LoadXml();
            }
        }

        public void Commit()
        {
            const string SAVE_VALUE =
                @"<ELEMENTS translationmode=""0"" action=""save"" reddotcacheguid="""" ><ELT guid=""{0}"" extendedinfo="""" reddotcacheguid="""" value=""{1}"" ></ELT></ELEMENTS>";
            using (new LanguageContext(LanguageVariant))
            {
                string value = _value == null ? RQL.SESSIONKEY_PLACEHOLDER : _value.Guid.ToRQLString();
                string query = SAVE_VALUE.RQLFormat(this, value);
                Project.ExecuteRQL(query);
            }
        }

        public bool HasDefaultValue
        {
            get { return Value != null && string.IsNullOrEmpty(XmlElement.GetAttributeValue("value")); }
        }

        public IRDEnumerable<IOptionListEntry> PossibleValues
        {
            get { return LazyLoad(ref _entries).ToRDEnumerable(); }
        }

        public IOptionListEntry Value
        {
            get { return LazyLoad(ref _value); }
            set { _value = value; }
        }

        public string ValueString
        {
            get { return Value == null ? null : Value.Value; }
        }

        protected override sealed void LoadWholePageElement()
        {
            LoadXml();
        }

        protected override sealed XmlElement RetrieveWholeObject()
        {
            using (new LanguageContext(LanguageVariant))
            {
                const string RETRIEVE_OPTION_LIST =
                    @"<ELT action=""load"" subelements=""1"" guid=""{0}"" ><SELECTIONS action=""list""/></ELT>";

                return
                    (XmlElement)
                    Project.ExecuteRQL(string.Format(RETRIEVE_OPTION_LIST, Guid.ToRQLString()))
                           .GetElementsByTagName("ELT")[0];
            }
        }

        private void LoadXml()
        {
            Guid defaultGuid;
            XmlElement.TryGetGuid("eltdefaultselectionguid", out defaultGuid);

            XmlNodeList elements = XmlElement.GetElementsByTagName("SELECTION");
            _entries =
                (from XmlElement curElement in elements
                 select (IOptionListEntry) new OptionListEntry(defaultGuid, curElement)).ToList();

            Guid selectedGuid;
            _value = XmlElement.TryGetGuid("value", out selectedGuid)
                         ? _entries.FirstOrDefault(entry => entry.Guid == selectedGuid)
                         : _entries.FirstOrDefault(entry => entry.IsDefault);
        }
    }
}