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
using System.Xml;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes;
using erminas.SmartAPI.CMS.Project.Filesystem;
using erminas.SmartAPI.Exceptions;

namespace erminas.SmartAPI.CMS
{
    internal class AbstractAttributeContainer : IAttributeContainer
    {
        private readonly Dictionary<string, IRDAttribute> _attributeMap = new Dictionary<string, IRDAttribute>();
        private readonly List<IRDAttribute> _attributes = new List<IRDAttribute>();

        protected AbstractAttributeContainer(ISession session)
        {
            Session = session;
        }

        protected AbstractAttributeContainer(ISession session, XmlElement node)
        {
            Session = session;
            XmlElement = (XmlElement) node.Clone();
        }

        public void AssignAttributes(IEnumerable<IRDAttribute> attributes)
        {
            foreach (var curAttribute in attributes)
            {
                var ownAttribute = GetAttribute(curAttribute.Name);
                ownAttribute.Assign(curAttribute);
            }
        }

        public IEnumerable<IRDAttribute> Attributes
        {
            get { return _attributes.AsReadOnly(); }
        }

        public IRDAttribute GetAttribute(string name)
        {
            return _attributeMap[name];
        }

        public void RefreshAttributeValues()
        {
            foreach (var rdAttribute in Attributes)
            {
                rdAttribute.Refresh();
            }
        }

        public void RegisterAttribute(IRDAttribute attribute)
        {
            if (_attributeMap.ContainsKey(attribute.Name))
            {
                throw new ArgumentException("multiple definitions of attribute: " + attribute.Name);
            }
            _attributes.Add(attribute);
            _attributeMap.Add(attribute.Name, attribute);
        }

        public virtual ISession Session { get; private set; }

        public XmlElement XmlElement { get; set; }

        protected internal virtual T GetAttributeValue<T>(string attributeName)
        {
            var type = typeof (T);
            if (type == typeof (string))
            {
                return (T) (object) ((StringXmlNodeAttribute) GetAttribute(attributeName)).Value;
            }

            if (typeof (IFolder).IsAssignableFrom(type))
            {
                return (T) ((FolderXmlNodeAttribute) GetAttribute(attributeName)).Value;
            }

            if (type == typeof (bool))
            {
                return (T) (object) ((BoolXmlNodeAttribute) GetAttribute(attributeName)).Value;
            }

            throw new SmartAPIInternalException(
                string.Format("In GetAttributeValue<T> for attribute {1}, unexpected attribute type: {0}",
                              typeof (T).Name, attributeName));
        }

        protected internal virtual void SetAttributeValue<T>(string attributeName, T value)
        {
            var type = typeof (T);
            if (type == typeof (string))
            {
                ((StringXmlNodeAttribute) GetAttribute(attributeName)).Value = (string) (object) value;
                return;
            }

            var folder = value as IFolder;
            if (folder != null)
            {
                ((FolderXmlNodeAttribute) GetAttribute(attributeName)).Value = folder;
                return;
            }

            if (type == typeof (bool))
            {
                ((BoolXmlNodeAttribute) GetAttribute(attributeName)).Value = (bool) (object) value;
                return;
            }

            throw new SmartAPIInternalException(
                string.Format("In SetAttributeValue<T> for  attribute {1}, unexpected attribute type: {0}",
                              typeof (T).Name, attributeName));
        }

        protected void CreateAttributes(params string[] attributeNames)
        {
            foreach (string curAttr in attributeNames)
            {
                AttributeFactory.CreateAttribute(this, curAttr);
            }
        }
    }

    public interface IAttributeContainer : ISessionObject
    {
        void AssignAttributes(IEnumerable<IRDAttribute> attributes);
        IEnumerable<IRDAttribute> Attributes { get; }
        IRDAttribute GetAttribute(string name);

        void RefreshAttributeValues();
        void RegisterAttribute(IRDAttribute attribute);
        XmlElement XmlElement { get; set; }
    }
}