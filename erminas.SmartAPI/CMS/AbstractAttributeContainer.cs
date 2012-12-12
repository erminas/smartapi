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
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Xml;
using erminas.SmartAPI.CMS.CCElements.Attributes;

namespace erminas.SmartAPI.CMS
{
    public class AbstractAttributeContainer : IAttributeContainer
    {
        [ScriptIgnore] private readonly Dictionary<string, IRDAttribute> _attributeMap =
            new Dictionary<string, IRDAttribute>();

        public AbstractAttributeContainer()
        {
            Attributes = new List<IRDAttribute>();
        }

        public AbstractAttributeContainer(XmlElement node)
        {
            XmlNode = node;
            Attributes = new List<IRDAttribute>();
        }

        #region IAttributeContainer Members

        [ScriptIgnore]
        public List<IRDAttribute> Attributes { get; private set; }

        public void RegisterAttribute(IRDAttribute attribute)
        {
            if (_attributeMap.ContainsKey(attribute.Name))
            {
                throw new ArgumentException("multiple definitions of attribute: " + attribute.Name);
            }
            Attributes.Add(attribute);
            _attributeMap.Add(attribute.Name, attribute);
        }

        public IRDAttribute GetAttribute(string name)
        {
            return _attributeMap[name];
        }

        public void RefreshAttributeValues()
        {
            foreach (IRDAttribute rdAttribute in Attributes)
            {
                rdAttribute.Refresh();
            }
        }

        public void AssignAttributes(List<IRDAttribute> attributes)
        {
            foreach (IRDAttribute curAttribute in attributes)
            {
                IRDAttribute ownAttribute = GetAttribute(curAttribute.Name);
                ownAttribute.Assign(curAttribute);
            }
        }

        [ScriptIgnore]
        public XmlElement XmlNode { get; set; }

        #endregion

        protected void CreateAttributes(params string[] attributeNames)
        {
            foreach (string curAttr in attributeNames)
            {
                AttributeFactory.CreateAttribute(this, curAttr);
            }
        }
    }
}