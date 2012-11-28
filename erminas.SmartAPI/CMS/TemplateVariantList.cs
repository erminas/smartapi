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
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using erminas.SmartAPI.Exceptions;

namespace erminas.SmartAPI.CMS
{
    /// <summary>
    ///   A list of template variants.
    /// </summary>
    public class TemplateVariantList : RedDotObject, IEnumerable<TemplateVariant>
    {
        public TemplateVariantList(ContentClass contentClass, XmlNode node)
            : base(node)
        {
            ContentClass = contentClass;
            TemplateVariants = new List<TemplateVariant>();
            LoadXml(node);
        }

        protected override void LoadXml(XmlNode node)
        {
            XmlAttributeCollection attr = node.Attributes;
            if (attr != null)
            {
                try
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
                    if (attr["lockdate"] != null)
                    {
                        LockDate = attr["lockdate"].Value;
                    }
                    if (attr["lockusername"] != null)
                    {
                        Lockusername = attr["lockusername"].Value;
                    }
                    if (attr["lockusermail"] != null)
                    {
                        LockUsermail = attr["lockusermail"].Value;
                    }
                    if (attr["draft"] != null)
                    {
                        Draft = attr["draft"].Value;
                    }
                    if (attr["waitforrelease"] != null)
                    {
                        WaitForRelease = attr["waitforrelease"].Value;
                    }
                    if (attr["lock"] != null)
                    {
                        Lock = attr["lock"].Value;
                    }
                    Guid tempGuid; // used for parsing
                    if (attr["folderguid"] != null && Guid.TryParse(attr["folderguid"].Value, out tempGuid))
                    {
                        FolderGuid = tempGuid;
                    }
                    if (attr["templateguid"] != null && Guid.TryParse(attr["templateguid"].Value, out tempGuid))
                    {
                        TemplateGuid = tempGuid;
                    }

                    if (attr["lockuserguid"] != null && Guid.TryParse(attr["lockuserguid"].Value, out tempGuid))
                    {
                        LockUserGuid = tempGuid;
                    }

                    if (node.NodeType == XmlNodeType.Element)
                    {
                        XmlNodeList templateVariantNodes = ((XmlElement) node).GetElementsByTagName("TEMPLATEVARIANT");
                        foreach (XmlNode curElementNode in templateVariantNodes)
                        {
                            TemplateVariants.Add(new TemplateVariant(ContentClass, curElementNode));
                        }
                    }
                    else
                    {
                        throw new Exception("Illegal node type for template variant list");
                    }
                }
                catch (Exception e)
                {
                    // couldn't read data
                    throw new RedDotDataException("Couldn't read content class data..", e);
                }
            }
        }

        #region TemplateVariantAccessMethods

        /// <summary>
        ///   Get a template variant by name
        /// </summary>
        /// <param name="name"> Name of the template variant to get </param>
        public TemplateVariant this[string name]
        {
            get { return TemplateVariants.Find(variant => name.Equals(variant.Name)); }
        }

        public IEnumerator<TemplateVariant> GetEnumerator()
        {
            return TemplateVariants.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return TemplateVariants.GetEnumerator();
        }

        #endregion

        #region Properties

        public List<TemplateVariant> TemplateVariants { get; set; }

        public string Action { get; set; }

        public string LanguageVariantId { get; set; }

        public string DialogLanguageId { get; set; }

        public string LockDate { get; set; }

        public string Lockusername { get; set; }

        public string LockUsermail { get; set; }

        public string Draft { get; set; }

        public string WaitForRelease { get; set; }

        public string Lock { get; set; }

        public Guid FolderGuid { get; set; }

        public Guid TemplateGuid { get; set; }

        public Guid LockUserGuid { get; set; }

        public ContentClass ContentClass { get; set; }

        #endregion
    }
}