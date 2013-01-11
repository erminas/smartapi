using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    /// <summary>
    /// Encapsulates category management for a project.
    /// </summary>
    /// <remarks>
    /// We don't subclass NameIndexedRDList, because renaming to existing names is allowed and could lead to duplicate category names
    /// </remarks>
    public class Categories : RDList<Category>
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
                throw new SmartAPIException(string.Format("Could not create the category {0} in project {1}", categoryName, _project));
            }

            InvalidateCache();
            return new Category(_project, category);
        }

        private List<Category> GetCategories()
        {
            const string LIST_CATEGORIES = @"<PROJECT><CATEGORIES action=""list"" /></PROJECT>";
            XmlDocument xmlDoc = _project.ExecuteRQL(LIST_CATEGORIES);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("CATEGORY");
            return (from XmlElement curNode in xmlNodes select new Category(_project, curNode)).ToList();
        }

        public void Delete(string categoryName)
        {
            Category category;
            if (TryGetByName(categoryName, out category))
            {
                category.Delete();
            }
        }
    }
}
