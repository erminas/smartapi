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
using System.Web;
using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    /// <summary>
    ///     A category entry of a project in the RedDot server.
    /// </summary>
    public class Category : PartialRedDotObject
    {
        /// <summary>
        ///     All keywords belonging to this category, indexed by name. This list is cached by default.
        /// </summary>
        public readonly Keywords Keywords;

        public readonly Project Project;

        private LanguageVariant _languageVariant;

        internal Category(Project project, XmlElement xmlElement) : base(xmlElement)
        {
            Project = project;
            Keywords = new Keywords(this);
            LoadXml();
        }

        public Category(Project project, Guid guid) : base(guid)
        {
            Project = project;
            Keywords = new Keywords(this);
        }

        /// <summary>
        ///     Use after setting Name to rename category on the server.
        /// </summary>
        public void Commit()
        {
            const string SAVE_CATEGORY = @"<PROJECT><CATEGORY action=""save"" guid=""{0}"" value=""{1}""/></PROJECT>";
            string htmlEncodedName = HttpUtility.HtmlEncode(Name);
            var xmlDoc = Project.ExecuteRQL(string.Format(SAVE_CATEGORY, Guid.ToRQLString(), htmlEncodedName));

            const string CATEGORY_XPATH_TEMPLATE = "/IODATA/CATEGORY[@value='{0}' and @guid='{1}']";
            string categoryXPath = CATEGORY_XPATH_TEMPLATE.RQLFormat(htmlEncodedName, this);
            var category = xmlDoc.SelectSingleNode(categoryXPath);
            if (category == null)
            {
                throw new SmartAPIException(Project.Session.ServerLogin,
                                            string.Format("Could not rename category to '{0}'", Name));
            }
        }

        /// <summary>
        ///     Delete the category. The operation will fail, if a keyword is still assigned to a page
        /// </summary>
        /// <exception cref="SmartAPIException">Thrown, if the category couldn't be deleted</exception>
        public void Delete()
        {
            const string DELETE_CATEGORY = @"<CATEGORY action=""delete"" guid=""{0}"" force=""0""/>";

            var xmlDoc = Project.ExecuteRQL(DELETE_CATEGORY.RQLFormat(this), Project.RqlType.SessionKeyInProject);
            var category = (XmlElement) xmlDoc.SelectSingleNode("/IODATA/CATEGORY");

            if (category == null)
            {
                throw new SmartAPIException(Project.Session.ServerLogin,
                                            string.Format("Could not delete category {0}", this));
            }

            if (IsCategoryStillUsed(category))
            {
                throw new SmartAPIException(Project.Session.ServerLogin,
                                            string.Format(
                                                "Could not delete category {0}, because a keyword is still assigned to a page",
                                                this));
            }

            Project.Categories.InvalidateCache();
        }

        /// <summary>
        ///     Delete the category, even if its keywords are actively used in connecting pages to containers/lists.
        ///     This requires the session to contain your login password (it does, if you created the session object with valid ServerLogin.AuthData).
        /// </summary>
        /// <exception cref="SmartAPIException">Thrown, if the category could not be deleted</exception>
        public void DeleteForcibly()
        {
            const string DELETE_KEYWORD = @"<CATEGORY action=""delete"" guid=""{0}"" force=""1"" password=""{1}"" />";

            var xmlDoc =
                Project.ExecuteRQL(DELETE_KEYWORD.RQLFormat(this, Project.Session.ServerLogin.AuthData.Password),
                                   Project.RqlType.SessionKeyInProject);
            var category = (XmlElement) xmlDoc.SelectSingleNode("/IODATA/CATEGORY");
            //TODO execute page builder command
            if (category == null)
            {
                throw new SmartAPIException(Project.Session.ServerLogin,
                                            string.Format("Could not delete keyword {0}", this));
            }

            Project.Categories.InvalidateCache();
        }

        /// <summary>
        ///     The current language variant.
        /// </summary>
        public LanguageVariant LanguageVariant
        {
            get { return LazyLoad(ref _languageVariant); }
        }

        /// <summary>
        ///     Renames the category directly on the server.
        ///     Thus it is the same as:
        ///     <example>
        ///         <code>
        /// string newCategoryName = ...; <br />
        /// category.Name = newCategoryName;<br /> 
        /// category.Commit();
        /// </code>
        ///     </example>
        /// </summary>
        public void Rename(string newCategoryName)
        {
            Name = newCategoryName;
            Commit();
        }

        protected override void LoadWholeObject()
        {
            LoadXml();
        }

        protected override XmlElement RetrieveWholeObject()
        {
            const string LOAD_CATEGORY = @"<PROJECT><CATEGORY action=""load"" guid=""{0}""/></PROJECT>";
            return
                (XmlElement)
                Project.ExecuteRQL(string.Format(LOAD_CATEGORY, Guid.ToRQLString())).GetElementsByTagName("CATEGORY")[0];
        }

        private static bool IsCategoryStillUsed(XmlElement category)
        {
            return category.GetIntAttributeValue("countkeywordonpag") > 0 ||
                   category.GetIntAttributeValue("countkeywordonpge") > 0 ||
                   category.GetIntAttributeValue("countkeywordonpgeandpag") > 0;
        }

        private void LoadXml()
        {
            EnsuredInit(ref _name, "value", HttpUtility.HtmlDecode);
            InitIfPresent(ref _languageVariant, "languagevariantid", x => Project.LanguageVariants[x]);
        }
    }
}