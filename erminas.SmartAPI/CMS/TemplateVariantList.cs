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
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    /// <summary>
    ///   A list of template variants.
    /// </summary>
    public class TemplateVariantList : RedDotObject, IEnumerable<TemplateVariant>
    {
        public TemplateVariantList(ContentClass contentClass, XmlElement xmlElement) : base(xmlElement)
        {
            ContentClass = contentClass;
            TemplateVariants = new List<TemplateVariant>();
            LoadXml();
        }

        protected void LoadXml()
        {
            Action = XmlNode.GetAttributeValue("action");
            LanguageVariantId = XmlNode.GetAttributeValue("languagevariantid");
            DialogLanguageId = XmlNode.GetAttributeValue("dialoglanguageid");
            LockDate = XmlNode.GetAttributeValue("lockdate");
            Lockusername = XmlNode.GetAttributeValue("lockusername");
            LockUsermail = XmlNode.GetAttributeValue("lockusermail");
            Draft = XmlNode.GetAttributeValue("draft");
            WaitForRelease = XmlNode.GetAttributeValue("waitforrelease");
            Lock = XmlNode.GetAttributeValue("lock");

            Lock = XmlNode.GetAttributeValue("lock");
            Lock = XmlNode.GetAttributeValue("lock");
            Lock = XmlNode.GetAttributeValue("lock");
            Lock = XmlNode.GetAttributeValue("lock");
            Lock = XmlNode.GetAttributeValue("lock");
            Lock = XmlNode.GetAttributeValue("lock");

            Guid tempGuid;
            if (XmlNode.TryGetGuid("folderguid", out tempGuid))
            {
                FolderGuid = tempGuid;
            }

            if (XmlNode.TryGetGuid("templateguid", out tempGuid))
            {
                TemplateGuid = tempGuid;
            }

            if (XmlNode.TryGetGuid("lockuserguid", out tempGuid))
            {
                LockUserGuid = tempGuid;
            }

            TemplateVariants = (from XmlElement curVariant in XmlNode.GetElementsByTagName("TEMPLATEVARIANT")
                                select new TemplateVariant(ContentClass, curVariant)).ToList();
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