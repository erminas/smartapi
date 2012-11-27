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
using System.Xml;
using erminas.SmartAPI.Utils;
using erminas.Utilities;

namespace erminas.SmartAPI.CMS
{
    public class ProjectVariant : PartialRedDotObject
    {
        private string _name;

        public ProjectVariant(Project project, Guid guid) : base(guid)
        {
            Project = project;
        }

        public ProjectVariant(Project project, XmlNode xmlNode) : base(xmlNode)
        {
            Project = project;
            LoadXml(xmlNode);
        }

        public bool IsUserDisplayVariant
        {
            get
            {
                string value = XmlNode.GetAttributeValue("userdisplayvariant");
                return value == "1";
            }
        }

        public override string Name
        {
            get { return LazyLoad(ref _name); }
            set { _name = value; }
        }

        public bool IsUsedAsDisplayFormat
        {
            get
            {
                string value = XmlNode.GetAttributeValue("checked");
                return value != null && value == "1";
            }
        }

        public Project Project { get; private set; }

        protected override void LoadXml(XmlNode node)
        {
            _name = node.GetAttributeValue("name");
        }

        //public static ProjectVariant Default
        //{
        //    get
        //    {
        //        ProjectVariant displayVariant = null;
        //        foreach (ProjectVariant projectVariant in ProjectVariant.List())
        //        {
        //            if (projectVariant.UsedForDisplay)
        //            {
        //                displayVariant = projectVariant;
        //                break;
        //            }
        //        }
        //        return displayVariant;
        //    }
        //}

        //public static List<ProjectVariant> List()
        //{
        //    List<ProjectVariant> variants = new List<ProjectVariant>();
        //    string rqlStatement =
        //        "<IODATA>" +
        //            "<PROJECT>" +
        //                "<PROJECTVARIANTS action=\"list\"/>" +
        //            "</PROJECT>" +
        //        "</IODATA>";

        //    _xmlDoc.LoadXml(Session.Execute(rqlStatement));
        //    XmlNodeList xmlNodes = _xmlDoc.GetElementsByTagName("PROJECTVARIANT");
        //    foreach (XmlNode xmlNode in xmlNodes)
        //    {
        //        variants.Add(new ProjectVariant(xmlNode));
        //    }
        //    return variants;
        //}
        protected override XmlNode RetrieveWholeObject()
        {
            const string LOAD_PROJECT_VARIANT =
                @"<PROJECT><PROJECTVARIANTS action=""load""><PROJECTVARIANT guid=""{0}"" /></PROJECTVARIANTS></PROJECT>";

            XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(LOAD_PROJECT_VARIANT, Guid.ToRQLString()));
            return xmlDoc.GetElementsByTagName("PROJECTVARIANT")[0];
        }
    }
}