using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    /// <summary>
    /// Encapsulates keyword management for a category.
    /// </summary>
    /// <remarks>
    /// We don't subclass NameIndexedRDList, because renaming to existing names is allowed and could lead to duplicate keyword names.
    /// </remarks>
    public class Keywords : RDList<Keyword>
    {
        public readonly Category Category;

        internal Keywords(Category category) : base(Caching.Enabled)
        {
            Category = category;
            RetrieveFunc = GetKeywords;
        }

        public Keyword CreateOrGet(string keywordName)
        {
            const string SAVE_KEYWORD =
               @"<CATEGORY guid=""{0}""><KEYWORD action=""save"" value=""{1}""/></CATEGORY>";

            var xmlDoc = Category.Project.ExecuteRQL(SAVE_KEYWORD.RQLFormat(Category, HttpUtility.HtmlEncode(keywordName)), Project.RqlType.SessionKeyInProject);
            var keyword = (XmlElement)xmlDoc.SelectSingleNode("/IODATA/KEYWORD");
            if (keyword == null)
            {
                throw new SmartAPIException(string.Format("Could not create the keyword '{0}'", keywordName));
            }

            InvalidateCache();
            return new Keyword(Category.Project, keyword);
        }

        public void Delete(string keywordName)
        {
            Keyword keyword;
            if (!TryGetByName(keywordName, out keyword))
            {
                return;
            }

            keyword.Delete();
            InvalidateCache();
        }

        public void DeleteForcibly(string keywordName)
        {
            Keyword keyword;
            if (!TryGetByName(keywordName, out keyword))
            {
                return;
            }

            keyword.DeleteForcibly();
            InvalidateCache();
        }

        private List<Keyword> GetKeywords()
        {
            const string LIST_KEYWORDS =
                @"<PROJECT><CATEGORY guid=""{0}""><KEYWORDS action=""load"" /></CATEGORY></PROJECT>";
            XmlDocument xmlDoc = Category.Project.ExecuteRQL(LIST_KEYWORDS.RQLFormat(Category));
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("KEYWORD");

            var kategoryKeyword = new List<Keyword>
                {
                    new Keyword(Category.Project, Category.Guid) {Name = "[category]", Category = Category}
                };
            return
                (from XmlElement curNode in xmlNodes select new Keyword(Category.Project, curNode) {Category = Category})
                    .Union(kategoryKeyword).ToList();
        }
    }
}