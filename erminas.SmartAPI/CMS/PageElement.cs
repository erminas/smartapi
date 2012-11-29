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
using System.Xml;
using erminas.SmartAPI.CMS.PageElements;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    /// <summary>
    ///   Base class for elements of pages. All page element classes have to either get annotated with a <see
    ///    cref="PageElementType" /> attribute denoting their element type or have to be added through the <see
    ///    cref="RegisterType" /> method.
    /// </summary>
    public abstract class PageElement : PartialRedDotObject, IPageElement
    {
        private const string RETRIEVE_PAGE_ELEMENT = @"<ELT action=""load"" guid=""{0}""/>";

        private static readonly Dictionary<ElementType, Type> TYPES =
            new Dictionary<ElementType, Type>();

        protected readonly Project Project;

        private Page _page;
        protected ElementType _type;

        static PageElement()
        {
            TYPES.Add(ElementType.StandardFieldTextLegacy, typeof (StandardFieldText));
            foreach (Type curType in typeof (PageElement).Assembly.GetTypes())
            {
                foreach (object curAttr in curType.GetCustomAttributes(typeof (PageElementType), false))
                {
                    if (curType.GetConstructor(new[] {typeof (Project), typeof (XmlElement)}) == null)
                    {
                        throw new Exception(string.Format("{0} does not contain a constructor (Project, XmlElement)",
                                                          curType.Name));
                    }
                    TYPES.Add(((PageElementType) curAttr).Type, curType);
                }
            }
        }

        protected PageElement(Project project, Guid guid)
            : base(guid)
        {
            Project = project;
        }

        protected PageElement(Project project, XmlElement xmlElement) : base(xmlElement)
        {
            Project = project;
            LoadXml(xmlElement);
        }

        #region IPageElement Members

        public virtual ElementType Type
        {
            get
            {
                if (_type == ElementType.None)
                {
                    object[] types = GetType().GetCustomAttributes(typeof (PageElementType), false);
                    if (types.Length != 1)
                    {
                        throw new Exception(string.Format("Undefined ElementType for {0}", GetType().Name));
                    }
                    _type = ((PageElementType) types[0]).Type;
                }
                return _type;
            }
            set { _type = value; }
        }

        public Page Page
        {
            get { return LazyLoad(ref _page); }
        }

        #endregion

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

        protected override void LoadXml(XmlElement node)
        {
            Name = node.GetAttributeValue("eltname");
            InitIfPresent(ref _page, "pageguid", x => new Page(Project, GuidConvert(x)));
        }

        protected override XmlElement RetrieveWholeObject()
        {
            return
                (XmlElement)
                Project.ExecuteRQL(string.Format(RETRIEVE_PAGE_ELEMENT, Guid.ToRQLString())).GetElementsByTagName("ELT")
                    [0];
        }


        /// <summary>
        ///   Create an element out of its XML representation (uses the attribute "elttype") to determine the element type and create the appropriate object.
        /// </summary>
        /// <param name="project"> Page that contains the element </param>
        /// <param name="xmlElement"> XML representation of the element </param>
        /// <exception cref="ArgumentException">if the "elttype" attribute of the XML node contains an unknown value</exception>
        public static PageElement CreateElement(Project project, XmlElement xmlElement)
        {
            var typeValue = (ElementType) int.Parse(xmlElement.GetAttributeValue("elttype"));
            Type type;
            if (!TYPES.TryGetValue(typeValue, out type))
            {
                throw new ArgumentException(string.Format("Unknown element type: {0}", typeValue));
            }

            return
                (PageElement)
// ReSharper disable PossibleNullReferenceException
                type.GetConstructor(new[] {typeof (Project), typeof (XmlElement)}).Invoke(new object[]
// ReSharper restore PossibleNullReferenceException
                                                                                           {project, xmlElement});
        }

        /// <summary>
        ///   Create an element out of its XML representation (uses the attribute "elttype") to determine the element type and create the appropriate object.
        /// </summary>
        /// <param name="project"> Project containing the element </param>
        /// <param name="elementGuid"> Guid of the element </param>
        /// <exception cref="ArgumentException">if the "elttype" attribute of the XML node contains an unknown value</exception>
        public static PageElement CreateElement(Project project, Guid elementGuid)
        {
            XmlDocument xmlDoc = project.ExecuteRQL(string.Format(RETRIEVE_PAGE_ELEMENT, elementGuid.ToRQLString()));
            var xmlNode = (XmlElement) xmlDoc.GetElementsByTagName("ELT")[0];
            return CreateElement(project, xmlNode);
        }
    }
}