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
using System.Reflection;
using System.Xml;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.Pages.Elements
{
    public interface IPageElementFactory
    {
        IPageElement CreateElement(IProject project, Guid elementGuid, ILanguageVariant languageVariant);
    }

    public class PageElementFactory : IPageElementFactory
    {
        static PageElementFactory()
        {
            Instance = new PageElementFactory();
        }

        private PageElementFactory()
        {
        }

        public IPageElement CreateElement(IProject project, Guid elementGuid, ILanguageVariant languageVariant)
        {
            return PageElement.CreateElement(project, elementGuid, languageVariant);
        }

        public static IPageElementFactory Instance { get; set; }
    }

    /// <summary>
    ///     Base class for elements of pages. All page element classes have to either get annotated with a
    ///     <see
    ///         cref="PageElementType" />
    ///     attribute denoting their element type or have to be added through the
    ///     <see
    ///         cref="RegisterType" />
    ///     method.
    /// </summary>
    internal abstract class PageElement : PartialRedDotProjectObject, IPageElement
    {
        private const string RETRIEVE_PAGE_ELEMENT = @"<ELT action=""load"" guid=""{0}""/>";

        private static readonly Dictionary<ElementType, Type> TYPES = new Dictionary<ElementType, Type>();

        protected ElementType Type;
        private ILanguageVariant _languageVariant;
        private IPage _page;

        static PageElement()
        {
            foreach (Type curType in typeof (PageElement).Assembly.GetTypes())
            {
                foreach (object curAttr in curType.GetCustomAttributes(typeof (PageElementType), false))
                {
                    if (
                        curType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null,
                                               new[] {typeof (Project), typeof (XmlElement)}, null) == null)
                    {
                        throw new SmartAPIInternalException(
                            string.Format("{0} does not contain a constructor (Project, XmlElement)", curType.Name));
                    }
                    var type = ((PageElementType) curAttr).Type;
                    if (TYPES.ContainsKey(type))
                    {
                        throw new SmartAPIInternalException(string.Format("Multiple definititions of {0}: {1} and {2}",
                                                                          type, TYPES[type].Name, curType.Name));
                    }
                    TYPES.Add(type, curType);
                }
            }
        }

        protected PageElement(IProject project, Guid guid, ILanguageVariant languageVariant) : base(project, guid)
        {
            LanguageVariant = languageVariant;
        }

        protected PageElement(IProject project, XmlElement xmlElement) : base(project, xmlElement)
        {
            LoadXml();
        }

        /// <summary>
        ///     Create an element out of its XML representation (uses the attribute "elttype") to determine the element type and create the appropriate object.
        /// </summary>
        /// <param name="project"> Page that contains the element </param>
        /// <param name="xmlElement"> XML representation of the element </param>
        /// <exception cref="ArgumentException">if the "elttype" attribute of the XML node contains an unknown value</exception>
        public static IPageElement CreateElement(IProject project, XmlElement xmlElement)
        {
            var typeValue = (ElementType) int.Parse(xmlElement.GetAttributeValue("elttype"));
            Type type;
            if (!TYPES.TryGetValue(typeValue, out type))
            {
                throw new ArgumentException(string.Format("Unknown element type: {0}", typeValue));
            }
            return
                (IPageElement)
                Activator.CreateInstance(type, BindingFlags.NonPublic | BindingFlags.Instance, null,
                                         new object[] {project, xmlElement}, CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Create an element out of its XML representation (uses the attribute "elttype") to determine the element type and create the appropriate object.
        /// </summary>
        /// <param name="project"> Project containing the element </param>
        /// <param name="elementGuid"> Guid of the element </param>
        /// <param name="languageVariant">The language variant of the page element</param>
        /// <exception cref="ArgumentException">if the "elttype" attribute of the XML node contains an unknown value</exception>
        public static IPageElement CreateElement(IProject project, Guid elementGuid, ILanguageVariant languageVariant)
        {
            using (new LanguageContext(languageVariant))
            {
                XmlDocument xmlDoc = project.ExecuteRQL(string.Format(RETRIEVE_PAGE_ELEMENT, elementGuid.ToRQLString()));
                var xmlNode = (XmlElement) xmlDoc.GetElementsByTagName("ELT")[0];
                return CreateElement(project, xmlNode);
            }
        }

        public virtual ElementType ElementType
        {
            get
            {
                if (Type == ElementType.None)
                {
                    object[] types = GetType().GetCustomAttributes(typeof (PageElementType), false);
                    if (types.Length != 1)
                    {
                        throw new SmartAPIInternalException(string.Format("Undefined ElementType for {0}",
                                                                          GetType().Name));
                    }
                    Type = ((PageElementType) types[0]).Type;
                }
                return Type;
            }
            set { Type = value; }
        }

        public ILanguageVariant LanguageVariant
        {
            get { return _languageVariant; }
            private set { _languageVariant = value; }
        }

        public IPage Page
        {
            get { return LazyLoad(ref _page); }
        }

        public static void RegisterType(ElementType typeValue, Type type)
        {
            if (!typeof (PageElement).IsAssignableFrom(type))
            {
                //use format to be safe from potentially refactoring names
                throw new ArgumentException(String.Format("TypeId is not a subclass of {0}", typeof (PageElement).Name));
            }

            if (TYPES.ContainsKey(typeValue))
            {
                throw new ArgumentException("There is already a type registered for " + typeValue);
            }

            TYPES.Add(typeValue, type);
        }

        protected override sealed void LoadWholeObject()
        {
            LoadXml();
            LoadWholePageElement();
        }

        protected abstract void LoadWholePageElement();

        protected override XmlElement RetrieveWholeObject()
        {
            using (new LanguageContext(LanguageVariant))
            {
                return
                    (XmlElement)
                    Project.ExecuteRQL(string.Format(RETRIEVE_PAGE_ELEMENT, Guid.ToRQLString()))
                           .GetElementsByTagName("ELT")[0];
            }
        }

        private void LoadXml()
        {
            //language variant must be loaded before the page referenced by pageguid, because it is used in its c'tor
            EnsuredInit(ref _languageVariant, "languagevariantid", Project.LanguageVariants.Get);
            EnsuredInit(ref _page, "pageguid", x => new Page(Project, GuidConvert(x), LanguageVariant));
        }
    }
}