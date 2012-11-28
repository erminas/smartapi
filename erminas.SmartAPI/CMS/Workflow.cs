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
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    public class Workflow : PartialRedDotObject
    {
        public readonly Project Project;

        public Workflow(Project project, XmlElement xmlElement) : base(xmlElement)
        {
            Project = project;
            LoadXml(xmlElement);
        }

        public Workflow(Project project, Guid guid) : base(guid)
        {
            Project = project;
        }


        protected override void LoadXml(XmlElement node)
        {
            XmlAttributeCollection attr = node.Attributes;
            try
            {
                if (attr["action"] != null)
                {
                    Action = attr["action"].Value;
                }
                if (attr["dialoglanguageid"] != null)
                {
                    DialogLanguageId = attr["dialoglanguageid"].Value;
                }
                if (attr["languagevariantid"] != null)
                {
                    LanguageVariantId = attr["languagevariantid"].Value;
                }
                if (attr["name"] != null)
                {
                    Name = attr["name"].Value;
                }
                if (attr["inherit"] != null)
                {
                    Inherit = attr["inherit"].Value;
                }
                if (attr["structureworkflow"] != null)
                {
                    StructureWorkflow = attr["structureworkflow"].Value;
                }
                if (attr["global"] != null)
                {
                    Global = attr["global"].Value;
                }

                Guid tempGuid; // used for parsing
                if (attr["guid"] != null && Guid.TryParse(attr["guid"].Value, out tempGuid))
                {
                    Guid = tempGuid;
                }
            }
            catch (Exception e)
            {
                // couldn't read data
                throw new RedDotDataException("Couldn't read content class data..", e);
            }
        }


        // TODO: Add a more useful action method which retains the 'flow' of the workflow!
        public List<WorkFlowAction> Actions()
        {
            return
                (from XmlElement node in XmlNode.SelectNodes("descendant::NODE") select new WorkFlowAction(node)).ToList
                    ();
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