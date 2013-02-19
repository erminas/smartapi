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

namespace erminas.SmartAPI.CMS.Project.Keywords
{
    public class Keyword : PartialRedDotProjectObject
    {
        private Category _category;

        internal Keyword(Project project, XmlElement xmlElement) : base(project, xmlElement)
        {
            LoadXml();
        }

        public Keyword(Project project, Guid guid) : base(project, guid)
        {
        }

        public Category Category
        {
            get { return LazyLoad(ref _category); }
            internal set { _category = value; }
        }

        public void Commit()
        {
            const string SAVE_KEYWORD = @"<PROJECT><KEYWORD action=""save"" guid=""{0}"" value=""{1}""/></PROJECT>";

            string htmlEncodedName = HttpUtility.HtmlEncode(Name);
            var xmlDoc = Project.ExecuteRQL(SAVE_KEYWORD.RQLFormat(this, htmlEncodedName));

            const string KEYWORD_XPATH_TEMPLATE = "/IODATA/KEYWORD[@value='{0}' and @guid='{1}']";
            string keywordXPath = KEYWORD_XPATH_TEMPLATE.RQLFormat(htmlEncodedName, this);
            var keyword = xmlDoc.SelectSingleNode(keywordXPath);
            if (keyword == null)
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not rename keyword to '{0}'", Name));
            }
        }

        /// <summary>
        ///     Delete the keyword. The operation will fail, if it is still actively used in connecting pages to containers/lists.
        /// </summary>
        /// <exception cref="SmartAPIException">Thrown, if the keyword couldn't be deleted</exception>
        public void Delete()
        {
            const string DELETE_KEYWORD = @"<KEYWORD action=""delete"" guid=""{0}"" force=""0""/>";

            var xmlDoc = Project.ExecuteRQL(DELETE_KEYWORD.RQLFormat(this), Project.RqlType.SessionKeyInProject);
            var keyword = (XmlElement) xmlDoc.SelectSingleNode("/IODATA/KEYWORD");

            if (keyword == null)
            {
                throw new SmartAPIException(Session.ServerLogin, string.Format("Could not delete keyword {0}", this));
            }

            if (IsKeywordStillUsed(keyword))
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format(
                                                "Could not delete keyword {0}, because it is still used for page connections",
                                                this));
            }

            Category.Keywords.InvalidateCache();
        }

        /// <summary>
        ///     Delete the keyword, even if it is actively used in connecting pages to containers/lists.
        ///     This requires the session to contain your login password (it does, if you created the session object with valid ServerLogin.AuthData).
        /// </summary>
        /// <exception cref="SmartAPIException">Thrown, if the keyword could not be deleted</exception>
        public void DeleteForcibly()
        {
            const string DELETE_KEYWORD = @"<KEYWORD action=""delete"" guid=""{0}"" force=""1"" password=""{1}"" />";

            var xmlDoc =
                Project.ExecuteRQL(DELETE_KEYWORD.RQLFormat(this, Project.Session.ServerLogin.AuthData.Password),
                                   Project.RqlType.SessionKeyInProject);
            var keyword = (XmlElement) xmlDoc.SelectSingleNode("/IODATA/KEYWORD");
            //TODO execute page builder command
            if (keyword == null)
            {
                throw new SmartAPIException(Session.ServerLogin, string.Format("Could not delete keyword {0}", this));
            }

            Category.Keywords.InvalidateCache();
        }

        public void Rename(string newKeywordName)
        {
            var oldName = Name;
            Name = newKeywordName;
            try
            {
                Commit();
            } catch (Exception)
            {
                Name = oldName;
                throw;
            }
        }

        protected override void LoadWholeObject()
        {
            LoadXml();
        }

        protected override XmlElement RetrieveWholeObject()
        {
            const string LOAD_KEYWORD =
                @"<PROJECT><CATEGORY><KEYWORD action=""load"" guid=""{0}""/></CATEGORY></PROJECT>";
            return (XmlElement) Project.ExecuteRQL(LOAD_KEYWORD.RQLFormat(this)).GetElementsByTagName("KEYWORD")[0];
        }

        private static bool IsKeywordStillUsed(XmlElement keyword)
        {
            return keyword.GetIntAttributeValue("countkeywordonpag").GetValueOrDefault() > 0 ||
                   keyword.GetIntAttributeValue("countkeywordonpge") > 0 ||
                   keyword.GetIntAttributeValue("countkeywordonpgeandpag") > 0;
        }

        private void LoadXml()
        {
            Name = XmlElement.GetAttributeValue("value");
            InitIfPresent(ref _category, "categoryguid", x => new Category(Project, Guid.Parse(x)));
        }
    }
}