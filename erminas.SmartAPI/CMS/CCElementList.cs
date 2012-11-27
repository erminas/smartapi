﻿/*
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
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using erminas.SmartAPI.Utils;
using erminas.Utilities;
using log4net;

namespace erminas.SmartAPI.CMS
{
    /// <summary>
    ///   A list of content class elements.
    /// </summary>
    public class CCElementList : RedDotObject, IEnumerable<CCElement>
    {
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof (CCElementList));

        public CCElementList(ContentClass project, XmlNode node)
        {
            ContentClass = project;
            Elements = new List<CCElement>();
            LoadXml(node);
        }

        /// <summary>
        ///   Get an element of the list by name. Returns null, if no such element could be found.
        /// </summary>
        /// <param name="name"> Name of the element to get </param>
        public CCElement this[string name]
        {
            get { return Elements.Find(x => x.Name == name); }
        }


        /// <summary>
        ///   Get an element of the list by its position in the list
        /// </summary>
        public CCElement this[int index]
        {
            get { return Elements[index]; }
        }

        /// <summary>
        ///   The content class this element list belongs to
        /// </summary>
        public ContentClass ContentClass { get; set; }

        #region IEnumerable<CCElement> Members

        public IEnumerator<CCElement> GetEnumerator()
        {
            return Elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Elements.GetEnumerator();
        }

        #endregion

        /// <summary>
        ///   Get an element of the list by name.
        /// </summary>
        /// <exception cref="KeyNotFoundException">thrown, if no element with .Name == name could be found</exception>
        public CCElement GetByName(string name)
        {
            CCElement element = this[name];
            if (element == null)
            {
                throw new KeyNotFoundException(string.Format("No element with name {0} available", name));
            }
            return element;
        }

        /// <summary>
        ///   Get an element on the list by guid.
        /// </summary>
        /// <exception cref="KeyNotFoundException">thrown, if no element with .Guid == guid could be found</exception>
        public CCElement GetByGuid(Guid guid)
        {
            CCElement element = Elements.Find(x => x.Guid == guid);
            if (element == null)
            {
                throw new KeyNotFoundException(string.Format("No element with guid {0} available", guid.ToRQLString()));
            }
            return element;
        }

        public bool TryGetByName(string name, out CCElement element)
        {
            element = this[name];
            return element != null;
        }

        public bool TryGetByGuid(Guid guid, out CCElement element)
        {
            element = Elements.Find(x => x.Guid == guid);
            return element != null;
        }

        protected override void LoadXml(XmlNode node)
        {
            Guid tempGuid; // used for parsing
            XmlAttributeCollection attr = node.Attributes;
            if (attr != null)
            {
                //try
                {
                    // Don't break if there is an error in this
                    if (attr["action"] != null)
                    {
                        Action = attr["action"].Value;
                    }
                    if (attr["languagevariantid"] != null)
                    {
                        LanguageVariantId = attr["languagevariantid"].Value;
                    }
                    if (attr["dialoglanguageid"] != null)
                    {
                        DialogLanguageId = attr["dialoglanguageid"].Value;
                    }
                    if (attr["childnodesasattributes"] != null)
                    {
                        ChildnodesAsAttributes = attr["childnodesasattributes"].Value;
                    }
                    if (attr["parenttable"] != null)
                    {
                        ParentTable = attr["parenttable"].Value;
                    }

                    if (attr["parentguid"] != null && Guid.TryParse(attr["parentguid"].Value, out tempGuid))
                    {
                        ParentGuid = tempGuid;
                    }

                    if (node.NodeType == XmlNodeType.Element)
                    {
                        XmlNodeList elementChildren = ((XmlElement) node).GetElementsByTagName("ELEMENT");
                        foreach (XmlNode curElementNode in elementChildren)
                        {
                            try
                            {
                                Elements.Add(CCElement.CreateElement(ContentClass, curElementNode));
                            }
                            catch (Exception e)
                            {
                                string elttypeStr = curElementNode.GetAttributeValue("elttype") ??
                                                    ((int) ElementType.None).ToString();
                                int typeValue;
                                string typeStr = int.TryParse(elttypeStr, out typeValue)
                                                     ? ((ElementType) typeValue).ToString()
                                                     : "unknown";
                                string str = "Could not create element '" + curElementNode.GetAttributeValue("eltname") +
                                             "' of type '" + typeStr + "'";
                                LOGGER.Error(str + ": " + e.Message);
                                throw new Exception(str, e);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Illegal node type for element list");
                    }
                }
            }
        }

        /// <summary>
        ///   Number of content class elements contained in this list.
        /// </summary>
        /// <returns> </returns>
        public int Count()
        {
            return Elements.Count;
        }

        #region Properties

        /// <summary>
        ///   The content class elements in the list
        /// </summary>
        public List<CCElement> Elements { get; set; }

        public string Action { get; set; }

        public string LanguageVariantId { get; set; }

        public string DialogLanguageId { get; set; }

        public string ChildnodesAsAttributes { get; set; }

        public string ParentTable { get; set; }

        public Guid ParentGuid { get; set; }

        #endregion
    }
}