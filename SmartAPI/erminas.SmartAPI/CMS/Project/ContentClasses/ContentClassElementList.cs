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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using log4net;

namespace erminas.SmartAPI.CMS.Project.ContentClasses
{
    /// <summary>
    ///     A list of content class elements.
    /// </summary>
    public class ContentClassElementList : RedDotProjectObject, IEnumerable<ContentClassElement>
    {
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof (ContentClassElementList));
        private readonly List<ContentClassElement> _elements;

        internal ContentClassElementList(ContentClass contentClass, XmlElement xmlElement)
            : base(contentClass.Project, xmlElement)
        {
            ContentClass = contentClass;
            _elements = new List<ContentClassElement>();
            LoadXml();
        }

        /// <summary>
        ///     The content class this element list belongs to
        /// </summary>
        public ContentClass ContentClass { get; set; }

        /// <summary>
        ///     Number of content class elements contained in this list.
        /// </summary>
        /// <returns> </returns>
        public int Count()
        {
            return _elements.Count;
        }

        /// <summary>
        ///     Get an element on the list by guid.
        /// </summary>
        /// <exception cref="KeyNotFoundException">thrown, if no element with .Guid == guid could be found</exception>
        public ContentClassElement GetByGuid(Guid guid)
        {
            ContentClassElement element = _elements.Find(x => x.Guid == guid);
            if (element == null)
            {
                throw new KeyNotFoundException(string.Format("No element with guid {0} available", guid.ToRQLString()));
            }
            return element;
        }

        /// <summary>
        ///     Get an element of the list by name.
        /// </summary>
        /// <exception cref="KeyNotFoundException">thrown, if no element with .Name == name could be found</exception>
        public ContentClassElement GetByName(string name)
        {
            ContentClassElement element = this[name];
            if (element == null)
            {
                throw new KeyNotFoundException(string.Format("No element with name {0} available", name));
            }
            return element;
        }

        public IEnumerator<ContentClassElement> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        /// <summary>
        ///     Get an element of the list by name. Returns null, if no such element could be found.
        /// </summary>
        /// <param name="name"> Name of the element to get </param>
        public ContentClassElement this[string name]
        {
            get { return _elements.Find(x => x.Name == name); }
        }

        /// <summary>
        ///     Get an element of the list by its position in the list
        /// </summary>
        public ContentClassElement this[int index]
        {
            get { return _elements[index]; }
        }

        public bool TryGetByGuid(Guid guid, out ContentClassElement element)
        {
            element = _elements.Find(x => x.Guid == guid);
            return element != null;
        }

        public bool TryGetByName(string name, out ContentClassElement element)
        {
            element = this[name];
            return element != null;
        }

        internal int RemoveAll(Predicate<IContentClassElement> predicate)
        {
            return _elements.RemoveAll(predicate);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        private void LoadXml()
        {
            XmlNodeList elementChildren = XmlElement.GetElementsByTagName("ELEMENT");
            foreach (XmlElement curElementNode in elementChildren)
            {
                try
                {
                    _elements.Add(ContentClassElement.CreateElement(ContentClass, curElementNode));
                } catch (Exception e)
                {
                    string elttypeStr = curElementNode.GetAttributeValue("elttype") ??
                                        ((int) ElementType.None).ToString(CultureInfo.InvariantCulture);
                    int typeValue;
                    string typeStr = int.TryParse(elttypeStr, out typeValue)
                                         ? ((ElementType) typeValue).ToString()
                                         : "unknown";
                    string str = "Could not create element '" + curElementNode.GetAttributeValue("eltname") +
                                 "' of type '" + typeStr + "'";
                    LOGGER.Error(str + ": " + e.Message);
                    throw new SmartAPIException(Session.ServerLogin, str, e);
                }
            }
        }
    }
}