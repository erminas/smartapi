using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Project.Keywords;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.ContentClasses
{
    public interface IPreassignedKeywords : IRDList<IKeyword>, IProjectObject
    {
        IContentClass ContentClass { get; }
        void Add(IKeyword keyword);
        void AddRange(IEnumerable<IKeyword> keywordsToAdd);
        void Remove(IKeyword keyword);
        void Set(IEnumerable<IKeyword> keywords);
        void Clear();
    }

    internal class PreassignedKeywords : RDList<IKeyword>, IPreassignedKeywords
    {
        private readonly ContentClass _contentClass;
        private List<IKeyword> GetPreassignedKeywords()
        {
            const string LOAD_PREASSIGNED_KEYWORDS = @"<TEMPLATE guid=""{0}""><KEYWORDS action=""load""/></TEMPLATE>";
            XmlDocument xmlDoc = Project.ExecuteRQL(LOAD_PREASSIGNED_KEYWORDS.RQLFormat(_contentClass),
                                                    RqlType.SessionKeyInProject);

            IEnumerable<IKeyword> keywords = new List<Keyword>();
            foreach (XmlElement node in xmlDoc.GetElementsByTagName("CATEGORY"))
            {
                var curCategory = new Category(Project, node.GetGuid()) { Name = node.GetAttributeValue("value") };
                var newKeywords = from XmlElement curKeywordNode in xmlDoc.GetElementsByTagName("KEYWORD")
                                  select
                                      new Keyword(Project, curKeywordNode.GetGuid())
                                      {
                                          Name = curKeywordNode.GetAttributeValue("value"),
                                          Category = curCategory
                                      };
                keywords = keywords.Union(newKeywords);
            }
            return keywords.ToList();
        }
        /// <summary>
        ///     Set the preassigned keywords for this content class on the server.
        ///     PreassignedKeywords contain the updated keywords afterwards.
        ///     Set to an empty IEnumerable or null, to remove all preassigned keywords.
        /// </summary>
        public void Set(IEnumerable<IKeyword> keywords)
        {
            if (keywords == null)
            {
                keywords = new List<Keyword>();
            }
            else
            {
                keywords = keywords.ToList();
            }
            using (new CachingContext<IKeyword>(this, Caching.Enabled))
            {
                var keywordsToAdd = keywords.Except(this).ToList();
                var keywordsToRemove = this.Except(keywords).ToList();

                if (!keywordsToRemove.Any() && !keywordsToAdd.Any())
                {
                    return;
                }

                AddRange(keywordsToAdd);
                RemoveRange(keywordsToRemove);
            }
        }

        internal PreassignedKeywords(ContentClass contentClass, Caching caching)
            : base(caching)
        {
            _contentClass = contentClass;
            RetrieveFunc = GetPreassignedKeywords;
        }

        public ISession Session { get { return _contentClass.Session; } }
        public IProject Project { get { return _contentClass.Project; } }
        public IContentClass ContentClass { get { return _contentClass; } }

        public void Add(IKeyword keyword)
        {
            AddRange(new[] { keyword });
        }


        public void AddRange(IEnumerable<IKeyword> keywordsToAdd)
        {
            foreach (Keyword curKeyword in keywordsToAdd)
            {
                const string ASSIGN_KEYWORD =
                    @"<TEMPLATE action=""assign"" guid=""{0}""><CATEGORY guid=""{1}""/><KEYWORD guid=""{2}""/></TEMPLATE>";

                XmlDocument xmlDoc =
                    Project.ExecuteRQL(ASSIGN_KEYWORD.RQLFormat(_contentClass, curKeyword.Category, curKeyword), RqlType.SessionKeyInProject);

                if (!WasKeywordActionSuccessful(xmlDoc))
                {
                    throw new SmartAPIException(Session.ServerLogin,
                                                string.Format("Could not assign keyword {0} to content class {1}",
                                                              curKeyword.Name, _contentClass.Name));
                }
            }
            InvalidateCache();
        }

        private static bool WasKeywordActionSuccessful(XmlNode node)
        {
            return node.InnerText.Contains("ok");
        }

        public void Remove(IKeyword keyword)
        {
            RemoveRange(new[] { keyword });
        }

        public void Clear()
        {
            RemoveRange(this);
        }

        private void RemoveRange(IEnumerable<IKeyword> keywordsToRemove)
        {
            foreach (Keyword curKeyword in keywordsToRemove)
            {
                const string REMOVE_KEYWORD =
                    @"<TEMPLATE action=""unlink"" guid=""{0}""><KEYWORD guid=""{1}""/></TEMPLATE>";

                var xmlDoc =
                    Project.ExecuteRQL(REMOVE_KEYWORD.RQLFormat(_contentClass, curKeyword),
                        RqlType.SessionKeyInProject);

                if (!WasKeywordActionSuccessful(xmlDoc))
                {
                    throw new SmartAPIException(Session.ServerLogin,
                                                string.Format("Could not unlink keyword {0} from content class {1}",
                                                              curKeyword.Name, _contentClass.Name));
                }
            }
            InvalidateCache();
        }
    }
}