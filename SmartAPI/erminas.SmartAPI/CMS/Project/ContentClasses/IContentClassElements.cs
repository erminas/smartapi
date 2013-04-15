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
using System.Xml;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.ContentClasses
{
    internal class ContentClassElements : IContentClassElements
    {
        private readonly ContentClass _contentClass;
        private Dictionary<string, IContentClassElementList> _elements;

        internal ContentClassElements(ContentClass contentClass)
        {
            _contentClass = contentClass;
        }

        public IContentClass ContentClass
        {
            get { return _contentClass; }
        }

        public void InvalidateCache()
        {
            _elements = null;
        }

        public IContentClassElementList this[string languageAbbreviation]
        {
            get
            {
                EnsureElementsAreLoaded();
                return _elements[languageAbbreviation];
            }
        }

        public IContentClassElementList this[ILanguageVariant languageVariant]
        {
            get { return this[languageVariant.Abbreviation]; }
        }

        public IEnumerable<string> Names
        {
            get
            {
                EnsureElementsAreLoaded();
                return _elements.Keys.ToList();
            }
        }

        public IProject Project
        {
            get { return _contentClass.Project; }
        }

        public void Refresh()
        {
            _elements = null;
            EnsureElementsAreLoaded();
        }

        public void Remove(Guid guid)
        {
            const string REMOVE_ELEMENT = @"<TEMPLATE><ELEMENT action=""delete"" guid=""{0}""/></TEMPLATE>";
            XmlDocument xmlDoc = Project.ExecuteRQL(REMOVE_ELEMENT.RQLFormat(guid), RqlType.SessionKeyInProject);
            if (!xmlDoc.InnerText.Contains("ok"))
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
            EnsureElementsAreLoaded();
            foreach (ContentClassElementList curElements in _elements.Values)
            {
                IContentClassElement contentClassElementToRemove = curElements.FirstOrDefault(x => x.Name == elementName);
                if (contentClassElementToRemove == null)
                {
                    throw new ArgumentException("Element '" + elementName + "' could not be found in content class '" +
                                                _contentClass.Name + "'");
                }
                Remove(contentClassElementToRemove.Guid);
                return;
            }
        }

        public ISession Session
        {
            get { return _contentClass.Session; }
        }

        private void EnsureElementsAreLoaded()
        {
            if (_elements == null)
            {
                _elements = new Dictionary<string, IContentClassElementList>();
                using (new LanguageContext(Project))
                {
                    foreach (var curLanguage in Project.LanguageVariants)
                    {
                        curLanguage.Select();
                        const string LOAD_CC_ELEMENTS =
                            @"<PROJECT><TEMPLATE action=""load"" guid=""{0}""><ELEMENTS childnodesasattributes=""1"" action=""load""/><TEMPLATEVARIANTS action=""list""/></TEMPLATE></PROJECT>";
                        XmlDocument xmlDoc = Project.ExecuteRQL(LOAD_CC_ELEMENTS.RQLFormat(_contentClass));

                        var xmlNode = (XmlElement) xmlDoc.GetElementsByTagName("ELEMENTS")[0];
                        var curElements = new ContentClassElementList(_contentClass, xmlNode);

                        _elements.Add(curLanguage.Abbreviation, curElements);
                    }
                }
            }
        }
    }

    public interface IContentClassElements : ICached, IProjectObject
    {
        IContentClass ContentClass { get; }

        IContentClassElementList this[string languageAbbreviation] { get; }
        IContentClassElementList this[ILanguageVariant languageVariant] { get; }
        IEnumerable<string> Names { get; }

        /// <summary>
        ///     Remove an element from the content class
        /// </summary>
        /// <param name="elementName"> Name of the element to remove </param>
        void Remove(string elementName);

        void Remove(Guid elementGuid);
    }
}