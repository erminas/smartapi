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

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Administration;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Keywords
{
    /// <summary>
    ///     Encapsulates category management for a project.
    ///     Allows enumeration, creation and deletion of categories from a project.
    /// </summary>
    /// <remarks>
    ///     We don't subclass NameIndexedRDList, because renaming to existing category names is allowed (albeit senseless) and could lead to duplicate category names
    /// </remarks>
    public class Categories : RDList<Category>, IProjectObject
    {
        private readonly Project _project;

        internal Categories(Project project) : base(Caching.Enabled)
        {
            RetrieveFunc = GetCategories;
            _project = project;
        }

        public Category CreateOrGet(string categoryName)
        {
            const string ADD_CATEGORY = @"<PROJECT><CATEGORY action=""addnew"" value=""{0}""/></PROJECT>";
            var xmlDoc = _project.ExecuteRQL(ADD_CATEGORY.RQLFormat(categoryName));

            var category = (XmlElement) xmlDoc.SelectSingleNode("//CATEGORY");
            if (category == null)
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not create the category {0} in project {1}",
                                                          categoryName, _project));
            }

            InvalidateCache();
            return new Category(_project, category);
        }

        public void Delete(string categoryName)
        {
            Category category;
            if (TryGetByName(categoryName, out category))
            {
                category.Delete();
            }
        }

        private List<Category> GetCategories()
        {
            const string LIST_CATEGORIES = @"<PROJECT><CATEGORIES action=""list"" /></PROJECT>";
            XmlDocument xmlDoc = _project.ExecuteRQL(LIST_CATEGORIES);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("CATEGORY");
            return (from XmlElement curNode in xmlNodes select new Category(_project, curNode)).ToList();
        }

        public Session Session { get { return _project.Session; } }
        public Project Project { get { return _project; } }
    }
}