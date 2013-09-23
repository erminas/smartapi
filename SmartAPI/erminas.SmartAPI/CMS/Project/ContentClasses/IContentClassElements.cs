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
using System.Globalization;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;
using log4net;

namespace erminas.SmartAPI.CMS.Project.ContentClasses
{
    internal class ContentClassElements : NameIndexedRDList<IContentClassElement>, IContentClassElements
    {
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof (ContentClassElements));
        private readonly ContentClass _contentClass;

        internal ContentClassElements(ContentClass contentClass, Caching caching) : base(caching)
        {
            _contentClass = contentClass;
            RetrieveFunc = GetContentClassElements;
        }

        public IContentClass ContentClass
        {
            get { return _contentClass; }
        }

        public IProject Project
        {
            get { return _contentClass.Project; }
        }

        public void Remove(Guid guid)
        {
            const string REMOVE_ELEMENT = @"<TEMPLATE><ELEMENT action=""delete"" guid=""{0}""/></TEMPLATE>";
            XmlDocument xmlDoc = Project.ExecuteRQL(REMOVE_ELEMENT.RQLFormat(guid), RqlType.SessionKeyInProject);
            if (!xmlDoc.IsContainingOk())
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not remove element {0} from content class {1} ",
                                                          guid.ToRQLString(), this));
            }
            InvalidateCache();
        }

        /// <summary>
        ///     Remove an element from this content class
        /// </summary>
        /// <param name="elementName"> Name of the element to remove </param>
        /// <remarks>
        ///     If you want to remove multiple elements, it is faster to get the element guids in advance an use
        ///     <see
        ///         cref="Remove(Guid)" />
        ///     instead of multiple calls to this method, because this method refreshes the cache between multiple calls.
        /// </remarks>
        public void Remove(string elementName)
        {
            IContentClassElement contentClassElementToRemove;
            if (TryGetByName(elementName, out contentClassElementToRemove))
            {
                throw new ArgumentException("Element '" + elementName + "' could not be found in content class '" +
                                            _contentClass.Name + "'");
            }
            Remove(contentClassElementToRemove.Guid);
        }

        public ISession Session
        {
            get { return _contentClass.Session; }
        }

        private IContentClassElement CreateElement(XmlElement curElementNode)
        {
            try
            {
                return ContentClassElement.CreateElement(ContentClass, curElementNode);
            } catch (Exception e)
            {
                var typeStr = GetElementTypeName(curElementNode);
                var elementName = curElementNode.GetAttributeValue("eltname");
                var str = "Could not create element '" + elementName + "' of type '" + typeStr + "'";

                LOGGER.Error(str + ": " + e.Message);
                throw new SmartAPIException(Session.ServerLogin, str, e);
            }
        }

        private List<IContentClassElement> GetContentClassElements()
        {
            const string LOAD_CC_ELEMENTS =
                @"<PROJECT><TEMPLATE action=""load"" guid=""{0}""><ELEMENTS childnodesasattributes=""1"" action=""load""/><TEMPLATEVARIANTS action=""list""/></TEMPLATE></PROJECT>";
            var xmlDoc = _contentClass.Project.ExecuteRQL(LOAD_CC_ELEMENTS.RQLFormat(_contentClass));

            var elementChildren = xmlDoc.GetElementsByTagName("ELEMENT");
            return (from XmlElement curElementNode in elementChildren select CreateElement(curElementNode)).ToList();
        }

        private static string GetElementTypeName(XmlElement curElementNode)
        {
            var elttypeStr = curElementNode.GetAttributeValue("elttype") ??
                             ((int) ElementType.None).ToString(CultureInfo.InvariantCulture);
            int typeValue;
            var typeStr = int.TryParse(elttypeStr, out typeValue) ? ((ElementType) typeValue).ToString() : "unknown";
            return typeStr;
        }
    }

    public interface IContentClassElements : IProjectObject, IIndexedRDList<string, IContentClassElement>
    {
        IContentClass ContentClass { get; }

        /// <summary>
        ///     Remove an element from the content class
        /// </summary>
        /// <param name="elementName"> Name of the element to remove </param>
        void Remove(string elementName);

        void Remove(Guid elementGuid);
    }
}