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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.CCElements.Attributes
{
    public class OptionListSelectionAttribute : IRDAttribute
    {
        private const string IDENTIFIER = "identifier";
        private const string SELECTION = "SELECTION";

        private readonly OptionList _parent;
        private string _value;

        public OptionListSelectionAttribute(OptionList parent, string name, XmlElement xmlElement)
        {
            _parent = parent;
            Name = name;
            XmlNode settingsNode = xmlElement.GetElementsByTagName("SELECTIONS")[0];
            if (settingsNode != null)
            {
                _value = xmlElement.OuterXml;
            }
            parent.RegisterAttribute(this);
        }

        public string Value
        {
            get
            {
                if (_value == null)
                {
                    const string LOAD_OPTION_LIST_DATA =
                        @"<TEMPLATE><ELEMENT action=""load"" guid=""{0}""><SELECTIONS action=""load"" guid=""{0}""/></ELEMENT></TEMPLATE>";
                    XmlDocument xmlDoc =
                        _parent.ContentClass.Project.ExecuteRQL(
                            String.Format(LOAD_OPTION_LIST_DATA, _parent.Guid.ToRQLString()),
                            Project.RqlType.SessionKeyInProject);
                    XmlNode selectionsNode = xmlDoc.GetElementsByTagName("SELECTIONS")[0];
                    if (selectionsNode == null)
                    {
                        throw new Exception("could not load option list data for '" + _parent.Name + "' (" +
                                            _parent.Guid.ToRQLString() + " )");
                    }
                    _value = selectionsNode.OuterXml;
                }
                return _value;
            }
        }

        #region IRDAttribute Members

        public string Name { get; private set; }

        public bool IsAssignableFrom(IRDAttribute o, out string reason)
        {
            reason = String.Empty;
            return o is OptionListSelectionAttribute;
        }

        public void Assign(IRDAttribute o)
        {
            var other = (OptionListSelectionAttribute) o;
            XDocument sourceDoc = XDocument.Parse(other.Value);
            XDocument targetDoc = XDocument.Parse(Value);

            var resultSelections = new XElement("SELECTIONS");

            //selections
            //-> selection [identifier]
            //   -> item [languageid, name, value]

            string sourceDefault = other.GetDefaultValueStringFromParent();

            //match guids of selections, by finding at least one item with same [languageid, name]
            List<SourceTarget> matchedItems = (from targetItem in targetDoc.Descendants("ITEM")
                                               where !string.IsNullOrEmpty(targetItem.Attribute("name").Value)
                                               join sourceItem in sourceDoc.Descendants("ITEM") on
                                                   new
                                                       {
                                                           name = targetItem.Attribute("name").Value,
                                                           lang = targetItem.Attribute("languageid").Value
                                                       } equals
                                                   new
                                                       {
                                                           name = sourceItem.Attribute("name").Value,
                                                           lang = sourceItem.Attribute("languageid").Value
                                                       }
                                               select
                                                   new SourceTarget
                                                       {
                                                           Source = sourceItem.Parent,
                                                           Target = targetItem.Parent
                                                       }).Distinct().ToList();

            string targetDefaultSelection = (from match in matchedItems
                                             where match.Source.Attribute(IDENTIFIER).Value.Equals(sourceDefault)
                                             select match.Target.Attribute(IDENTIFIER).Value).FirstOrDefault();

            //add new elements
            resultSelections.Add(from sourceSelection in sourceDoc.Descendants(SELECTION)
                                 where
                                     !(from mItem in matchedItems select mItem.Source.Attribute(IDENTIFIER).Value)
                                          .Contains(sourceSelection.Attribute(IDENTIFIER).Value)
                                 select
                                     new XElement(SELECTION,
                                                  new XAttribute(IDENTIFIER,
                                                                 sourceSelection.Attribute(IDENTIFIER)
                                                                                .Value.Equals(sourceDefault)
                                                                     ? "1"
                                                                     : "NaN"), sourceSelection.Elements()));

            //"change" matched items
            resultSelections.Add(from x in matchedItems
                                 select new XElement(SELECTION, x.Target.Attribute(IDENTIFIER), x.Source.Descendants()));

            targetDefaultSelection = targetDefaultSelection ?? "1";

            _parent.XmlElement.SetAttributeValue(Name, HttpUtility.HtmlEncode(resultSelections.ToString()));
            _value = null;
            //retrieve value again, to have guids set, beware: until commit on parent is called, the old list will be returned
            _parent.DefaultValueString = targetDefaultSelection;
        }

        public override bool Equals(object o)
        {
            var other = o as OptionListSelectionAttribute;
            if (other == null)
            {
                return false;
            }
            XDocument doc = XDocument.Parse(Value);
            XDocument otherDoc = XDocument.Parse(other.Value);

            if (doc.Elements("SELECTION").Count() != otherDoc.Elements("SELECTION").Count())
            {
                return false;
            }

            IEnumerable<XElement> docItems = doc.Descendants("ITEM").ToList();
            IEnumerable<XElement> otherDocItems = otherDoc.Descendants("ITEM").ToList();
            if (docItems.Count() != otherDocItems.Count())
            {
                return false;
            }

            IEnumerator<XElement> eDoc = docItems.GetEnumerator();
            IEnumerator<XElement> eOtherDoc = otherDocItems.GetEnumerator();
            while (eDoc.MoveNext())
            {
                eOtherDoc.MoveNext();
                if (eDoc.Current.Attribute("name").Value != eOtherDoc.Current.Attribute("name").Value ||
                    eDoc.Current.Value != eOtherDoc.Current.Value)
                {
                    return false;
                }
            }
            string defaultValueGuid = GetDefaultValueStringFromParent();
            IEnumerable<IEnumerable<string>> defaultNames = from selection in doc.Elements("SELECTION")
                                                            where
                                                                selection.Attribute(IDENTIFIER).Value ==
                                                                defaultValueGuid
                                                            select
                                                                (from item in selection.Descendants("ITEM")
                                                                 select item.Attribute("name").Value);

            string otherValueGuid = other.GetDefaultValueStringFromParent();
            IEnumerable<IEnumerable<string>> otherDefaultNames = from selection in otherDoc.Elements("SELECTION")
                                                                 where
                                                                     selection.Attribute(IDENTIFIER).Value ==
                                                                     otherValueGuid
                                                                 select
                                                                     (from item in selection.Descendants("ITEM")
                                                                      select item.Attribute("name").Value);

            return defaultNames.SequenceEqual(otherDefaultNames);
        }

        public void Refresh()
        {
            _value = null;
        }

        public string Description
        {
            get { return "Selection"; }
        }

        public object DisplayObject
        {
            get
            {
                XDocument doc = XDocument.Parse(Value);
                string defaultValueString = GetDefaultValueStringFromParent();
                Dictionary<string, List<OptionListEntry>> entries = (from item in doc.Descendants("ITEM")
                                                                     group item by item.Attribute("languageid").Value
                                                                     into langgroups
                                                                     select
                                                                         new
                                                                             {
                                                                                 Language = langgroups.Key,
                                                                                 Entries = (from entry in langgroups
                                                                                            select
                                                                                                new OptionListEntry(
                                                                                                entry.Attribute("name")
                                                                                                     .Value, entry.Value,
                                                                                                entry.Parent.Attribute(
                                                                                                    IDENTIFIER).Value ==
                                                                                                defaultValueString)).ToList()
                                                                             }).ToDictionary(key => key.Language,
                                                                                             value => value.Entries);

                return new OptionListValue {Entries = entries};
            }
        }

        #endregion

        public string GetDefaultValueStringFromParent()
        {
            return _parent.DefaultValueString;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        #region Nested type: OptionListEntry

        public struct OptionListEntry
        {
            public readonly bool IsDefault;
            public readonly string Name;
            public readonly string Value;

            public OptionListEntry(string name, string value, bool isDefault)
            {
                Name = name;
                Value = value;
                IsDefault = isDefault;
            }
        }

        #endregion

        #region Nested type: OptionListValue

        public class OptionListValue
        {
            public Dictionary<string, List<OptionListEntry>> Entries { get; set; }
        }

        #endregion

        #region Nested type: SourceTarget

        private class SourceTarget
        {
            public XElement Source { get; set; }
            public XElement Target { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as SourceTarget;
                return other != null && Source.Attribute(IDENTIFIER) == other.Source.Attribute(IDENTIFIER);
            }

            public override int GetHashCode()
            {
                return Source.Attribute(IDENTIFIER).GetHashCode();
            }
        }

        #endregion
    }
}