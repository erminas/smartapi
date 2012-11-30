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
using System.Web;
using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    /// <summary>
    ///   A category entry of a project in the RedDot server.
    /// </summary>
    public class Category : PartialRedDotObject
    {
        /// <summary>
        ///   All keywords belonging to this category, indexed by name. This list is cached by default.
        /// </summary>
        public readonly NameIndexedRDList<Keyword> Keywords;

        public readonly Project Project;

        private LanguageVariant _languageVariant;
        private string _name;

        public Category(Project project, XmlElement xmlElement) : base(xmlElement)
        {
            Project = project;
            Keywords = new NameIndexedRDList<Keyword>(GetKeywords, Caching.Enabled);
            LoadXml(xmlElement);
        }

        public Category(Project project, Guid guid)
            : base(guid)
        {
            Project = project;
            Keywords = new NameIndexedRDList<Keyword>(GetKeywords, Caching.Enabled);
        }

        /// <summary>
        ///   Name of the category. Setting it is only clientside until <see cref="Commit" /> gets called.
        /// </summary>
        public override string Name
        {
            get { return LazyLoad(ref _name); }
            set { _name = value; }
        }

        /// <summary>
        ///   The current language variant.
        /// </summary>
        public LanguageVariant LanguageVariant
        {
            get { return LazyLoad(ref _languageVariant); }
        }

        /// <summary>
        ///   Use after setting Name to rename category on the server.
        /// </summary>
        public void Commit()
        {
            const string SAVE_CATEGORY = @"<PROJECT><CATEGORY action=""save"" guid=""{0}"" value=""{1}""/></PROJECT>";
            Project.ExecuteRQL(string.Format(SAVE_CATEGORY, Guid.ToRQLString(), HttpUtility.HtmlEncode(Name)));
        }

        protected override void LoadXml(XmlElement node)
        {
            EnsuredInit(ref _name, "value", HttpUtility.HtmlDecode);
            InitIfPresent(ref _languageVariant, "languagevariantid", x => Project.LanguageVariants[x]);
        }

        protected override XmlElement RetrieveWholeObject()
        {
            const string LOAD_CATEGORY = @"<PROJECT><CATEGORY action=""load"" guid=""{0}""/></PROJECT>";
            return
                (XmlElement)
                Project.ExecuteRQL(string.Format(LOAD_CATEGORY, Guid.ToRQLString())).GetElementsByTagName("CATEGORY")[0];
        }

        private List<Keyword> GetKeywords()
        {
            const string LIST_KEYWORDS =
                @"<PROJECT><CATEGORY guid=""{0}""><KEYWORDS action=""load"" /></CATEGORY></PROJECT>";
            XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(LIST_KEYWORDS, Guid.ToRQLString()));
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("KEYWORD");

            return (from XmlElement curNode in xmlNodes select new Keyword(Project, curNode) {Category = this}).ToList();
        }
    }
}