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
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    public class Workflow : PartialRedDotObject
    {
        public readonly Project Project;

        public Workflow(Project project, XmlElement xmlElement)
            : base(xmlElement)
        {
            Project = project;
            LoadXml();
        }

        public Workflow(Project project, Guid guid)
            : base(guid)
        {
            Project = project;
        }


        private void LoadXml()
        {
            Action = XmlNode.GetAttributeValue("action");
            LanguageVariantId = XmlNode.GetAttributeValue("languagevariantid");
            DialogLanguageId = XmlNode.GetAttributeValue("dialoglanguageid");

            Inherit = XmlNode.GetAttributeValue("inherit");
            StructureWorkflow = XmlNode.GetAttributeValue("structureworkflow");
            Global = XmlNode.GetAttributeValue("global");
        }


        // TODO: Add a more useful action method which retains the 'flow' of the workflow!
        public List<WorkFlowAction> Actions()
        {
            return
                (from XmlElement node in XmlNode.SelectNodes("descendant::NODE") select new WorkFlowAction(node)).ToList
                    ();
        }

        protected override void LoadWholeObject()
        {
            LoadXml();
        }

        protected override XmlElement RetrieveWholeObject()
        {
            const string LOAD_WORKFLOW = @"<WORKFLOW action=""load"" guid=""{0}""/>";

            return
                (XmlElement)
                Project.ExecuteRQL(String.Format(LOAD_WORKFLOW, Guid.ToRQLString())).GetElementsByTagName("WORKFLOW")[0];
        }

        #region Properties

        public string Action { get; set; }

        public string DialogLanguageId { get; set; }

        public string LanguageVariantId { get; set; }

        public string Inherit { get; set; }

        public string StructureWorkflow { get; set; }

        public string Global { get; set; }

        #endregion
    }
}