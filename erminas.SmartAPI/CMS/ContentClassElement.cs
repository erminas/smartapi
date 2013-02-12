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
using System.Globalization;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.CCElements;
using erminas.SmartAPI.CMS.CCElements.Attributes;
using erminas.SmartAPI.Utils;
using Attribute = erminas.SmartAPI.CMS.CCElements.Attribute;

namespace erminas.SmartAPI.CMS
{
    /// <summary>
    ///     Base class for content class element types.
    /// </summary>
    /// <remarks>
    ///     For every attribute/property that can be compared and/or saved there has to be an <see cref="IRDAttribute" /> created and registered, so that the comparison/assignement can be made independent of the element type.
    /// </remarks>
    public abstract class ContentClassElement : RedDotObject, IContentClassElement
    {
        private const string LANGUAGEVARIANTID = "languagevariantid";
        private LanguageVariant _languageVariant;

        protected ContentClassElement(ContentClass contentClass, XmlElement xmlElement) : base(xmlElement)
        {
            CreateAttributes("eltname", LANGUAGEVARIANTID);
            ContentClass = contentClass;
            LoadXml();
        }

        /// <summary>
        ///     Element category of the lement
        /// </summary>
        public abstract ContentClassCategory Category { get; }

        /// <summary>
        ///     Save element on the server. Saves only the attributes!
        /// </summary>
        public virtual void Commit()
        {
            //RQL for committing changes
            //One parameter: xml representation of the element, containing an attribute "action" with value "save"
            const string COMMIT_ELEMENT = "<TEMPLATE><ELEMENTS>{0}</ELEMENTS></TEMPLATE>";
            var node = (XmlElement) XmlElement.Clone();
            using (new LanguageContext(LanguageVariant))
            {
                XmlDocument rqlResult =
                    ContentClass.Project.ExecuteRQL(string.Format(COMMIT_ELEMENT, GetSaveString(node)),
                                                    Project.RqlType.SessionKeyInProject);
                try
                {
                    var resultElement = (XmlElement) rqlResult.GetElementsByTagName("ELEMENT")[0];
                    string tmpGuidStr = resultElement.Attributes["guid"].Value;
                    var newGuid = new Guid(tmpGuidStr);
                    if (!newGuid.Equals(Guid))
                    {
                        throw new Exception("unexpected guid in return value");
                    }
                    //if needed could check wether the element has changed on the server, via the checked attribute
                    //-1 = changed 0 = unchanged
                } catch (Exception e)
                {
                    throw new Exception("could not save changes to " + Name, e);
                }
            }
        }

        public ContentClass ContentClass { get; set; }

        /// <summary>
        ///     Language variant of the element (a separate instance exists for every language variant on the server).
        /// </summary>
        public LanguageVariant LanguageVariant
        {
            get
            {
                return _languageVariant ??
                       (_languageVariant =
                        ContentClass.Project.LanguageVariants[XmlElement.GetAttributeValue(LANGUAGEVARIANTID)]);
            }
        }

        public override string Name { get; set; }

        /// <summary>
        ///     TypeId of the element.
        /// </summary>
        public ElementType Type { get; private set; }

        /// <summary>
        ///     Copies the element to another content class by creating a new element and copying the attribute values to it.
        ///     Make sure to set the language variant in the target project into which the element should be copied, first.
        /// </summary>
        /// <param name="contentClass"> target content class, into which the element should be copied </param>
        /// <returns> the created copy </returns>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>Override this method, if you need to set other values than the direct attributes of the element (e.g. setting text values of TextHtml elements)</description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 The target content class is only modified on the server, thus the content class object does not contain the newly created element.
        ///                 If you need an updated version of the content class, you have to retrieve it again with
        ///                 <code>new ContentClass(Project, Guid);</code>
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        internal ContentClassElement CopyToContentClass(ContentClass contentClass)
        {
            ContentClassElement newContentClassElement = CreateElement(contentClass, Type);
            foreach (IRDAttribute attr in Attributes)
            {
                IRDAttribute newAttr = newContentClassElement.Attributes.First(x => x.Name == attr.Name);
                try
                {
                    newAttr.Assign(attr);
                } catch (Exception e)
                {
                    throw new Exception(
                        string.Format(
                            "Unable to assign attribute {0} of element {1} of content class {2} in project {3}",
                            attr.Name, Name, contentClass.Name, contentClass.Project.Name), e);
                }
            }

            var node = (XmlElement) newContentClassElement.XmlElement.Clone();
            node.Attributes.RemoveNamedItem("guid");
            string creationString = GetSaveString(node);

            // <summary>
            // RQL for creating an element from a content class.
            // Two parameters:
            // 1. Content class guid
            // 2. Element to create, make sure it contains an attribute "action" with the value "save"!
            // </summary>
            const string CREATE_ELEMENT = @"<TEMPLATE guid=""{0}"">{1}</TEMPLATE>";

            XmlDocument rqlResult =
                contentClass.Project.ExecuteRQL(string.Format(CREATE_ELEMENT, contentClass.Guid.ToRQLString(),
                                                              creationString));
            var resultElementNode = (XmlElement) rqlResult.GetElementsByTagName("ELEMENT")[0];
            if (resultElementNode == null)
            {
                throw new Exception("error during creation of element: " + Name);
            }
            newContentClassElement.Guid = resultElementNode.GetGuid();

            return newContentClassElement;
        }

        /// <summary>
        ///     Create an element out of its XML representation (uses the attribute "elttype") to determine the element type and create the appropriate object.
        /// </summary>
        /// <param name="contentClass"> parent content class that contains the element </param>
        /// <param name="xmlElement"> XML representation of the element </param>
        /// <exception cref="ArgumentException">if the "elttype" attribute of the XML node contains an unknown value</exception>
        internal static ContentClassElement CreateElement(ContentClass contentClass, XmlElement xmlElement)
        {
            var type = (ElementType) int.Parse(xmlElement.GetAttributeValue("elttype"));
            switch (type)
            {
                case ElementType.DatabaseContent:
                    return new DatabaseContent(contentClass, xmlElement);
                case ElementType.TextHtml:
                    return new TextHtml(contentClass, xmlElement);
                case ElementType.TextAscii:
                    return new TextAscii(contentClass, xmlElement);
                case ElementType.StandardFieldText:
                case ElementType.StandardFieldTextLegacy:
                    return new StandardFieldText(contentClass, xmlElement);
                case ElementType.StandardFieldNumeric:
                    return new StandardFieldNumeric(contentClass, xmlElement);
                case ElementType.StandardFieldDate:
                    return new StandardFieldDate(contentClass, xmlElement);
                case ElementType.StandardFieldTime:
                    return new StandardFieldTime(contentClass, xmlElement);
                case ElementType.StandardFieldUserDefined:
                    return new StandardFieldUserDefined(contentClass, xmlElement);
                case ElementType.StandardFieldEmail:
                    return new StandardFieldEmail(contentClass, xmlElement);
                case ElementType.StandardFieldUrl:
                    return new StandardFieldURL(contentClass, xmlElement);
                case ElementType.Headline:
                    return new Headline(contentClass, xmlElement);
                case ElementType.Background:
                    return new Background(contentClass, xmlElement);
                case ElementType.Image:
                    return new Image(contentClass, xmlElement);
                case ElementType.Media:
                    return new Media(contentClass, xmlElement);
                case ElementType.ListEntry:
                    return new ListEntry(contentClass, xmlElement);
                case ElementType.Transfer:
                    return new Transfer(contentClass, xmlElement);
                case ElementType.Ivw:
                    return new IVW(contentClass, xmlElement);
                case ElementType.OptionList:
                    return new OptionList(contentClass, xmlElement);
                case ElementType.Attribute:
                    return new Attribute(contentClass, xmlElement);
                case ElementType.Info:
                    return new Info(contentClass, xmlElement);
                case ElementType.Browse:
                    return new Browse(contentClass, xmlElement);
                case ElementType.Area:
                    return new Area(contentClass, xmlElement);
                case ElementType.AnchorAsImage:
                    return new ImageAnchor(contentClass, xmlElement);
                case ElementType.AnchorAsText:
                    return new TextAnchor(contentClass, xmlElement);
                case ElementType.Container:
                    return new Container(contentClass, xmlElement);
                case ElementType.Frame:
                    return new Frame(contentClass, xmlElement);
                case ElementType.SiteMap:
                    return new SiteMap(contentClass, xmlElement);
                case ElementType.HitList:
                    return new HitList(contentClass, xmlElement);
                case ElementType.List:
                    return new List(contentClass, xmlElement);
                case ElementType.ProjectContent:
                    return new ProjectContent(contentClass, xmlElement);
                default:
                    throw new ArgumentException("unknown element type: " + type);
            }
        }

        /// <summary>
        ///     Create an empty element of a specific type as child of a content class. Does not insert the element into the contentclass itself, but just provides a vanilla element with an XML node that contains only the "elttype" and the empty "guid" attribute.
        /// </summary>
        /// <param name="contentClass"> parent content class of the element </param>
        /// <param name="elementType"> type of the element </param>
        /// <returns> </returns>
        private static ContentClassElement CreateElement(ContentClass contentClass, ElementType elementType)
        {
            var doc = new XmlDocument();
            XmlElement element = doc.CreateElement("ELEMENT");
            XmlAttribute typeAttr = doc.CreateAttribute("elttype");
            XmlAttribute guidAttr = doc.CreateAttribute("guid");
            typeAttr.Value = ((int) elementType).ToString(CultureInfo.InvariantCulture);
            guidAttr.Value = new Guid().ToRQLString();
            element.Attributes.Append(typeAttr);
            element.Attributes.Append(guidAttr);

            return CreateElement(contentClass, element);
        }

        private void LoadXml()
        {
            Name = XmlElement.GetAttributeValue("eltname");
            Type = (ElementType) XmlElement.GetIntAttributeValue("elttype").GetValueOrDefault();
        }
    }

    /// <summary>
    ///     Category of the element.
    /// </summary>
    public enum ContentClassCategory
    {
        Content,
        Structural,
        Meta
    }
}