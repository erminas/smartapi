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
using System.Web;
using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    public class Keyword : PartialRedDotObject
    {
        public readonly Project Project;
        private Category _category;

        public Keyword(Project project, XmlElement xmlElement) : base(xmlElement)
        {
            Project = project;
            LoadXml();
        }

        public Keyword(Project project, Guid guid) : base(guid)
        {
            Project = project;
        }

        public Category Category
        {
            get { return LazyLoad(ref _category); }
            internal set { _category = value; }
        }

        public void Commit()
        {
            const string SAVE_KEYWORD =
                @"<PROJECT><CATEGORY><KEYWORD action=""save"" guid=""{0}"" value=""{1}""/></PROJECT>";

            Project.ExecuteRQL(string.Format(SAVE_KEYWORD, Guid.ToRQLString(), HttpUtility.HtmlEncode(Name)));
        }

        private void LoadXml()
        {
            Name = XmlNode.GetAttributeValue("value");
            InitIfPresent(ref _category, "categoryguid", x => new Category(Project, Guid.Parse(x)));
        }

        protected override void LoadWholeObject()
        {
            LoadXml();
        }

        protected override XmlElement RetrieveWholeObject()
        {
            const string LOAD_KEYWORD =
                @"<PROJECT><CATEGORY><KEYWORD action=""load"" guid=""{0}""/></CATEGORY></PROJECT>";
            return
                (XmlElement)
                Project.ExecuteRQL(string.Format(LOAD_KEYWORD, Guid.ToRQLString())).GetElementsByTagName("KEYWORD")[0];
        }
    }
}