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
using erminas.SmartAPI.CMS.Project.Keywords;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.ContentClasses
{
    public interface IPreassignedKeywords : IRDList<IKeyword>, IProjectObject
    {
        void Add(IKeyword keyword);
        void AddRange(IEnumerable<IKeyword> keywordsToAdd);
        void Clear();
        IContentClass ContentClass { get; }
        void Remove(IKeyword keyword);
        void Set(IEnumerable<IKeyword> keywords);
    }

    internal class PreassignedKeywords : RDList<IKeyword>, IPreassignedKeywords
    {
        private readonly ContentClass _contentClass;

        internal PreassignedKeywords(ContentClass contentClass, Caching caching) : base(caching)
        {
            _contentClass = contentClass;
            RetrieveFunc = GetPreassignedKeywords;
        }

        public void Add(IKeyword keyword)
        {
            AddRange(new[] {keyword});
        }

        public void AddRange(IEnumerable<IKeyword> keywordsToAdd)
        {
            foreach (Keyword curKeyword in keywordsToAdd)
            {
                const string ASSIGN_KEYWORD =
                    @"<TEMPLATE action=""assign"" guid=""{0}""><CATEGORY guid=""{1}""/><KEYWORD guid=""{2}""/></TEMPLATE>";

                XmlDocument xmlDoc =
                    Project.ExecuteRQL(ASSIGN_KEYWORD.RQLFormat(_contentClass, curKeyword.Category, curKeyword),
                                       RqlType.SessionKeyInProject);

                if (!WasKeywordActionSuccessful(xmlDoc))
                {
                    throw new SmartAPIException(Session.ServerLogin,
                                                string.Format("Could not assign keyword {0} to content class {1}",
                                                              curKeyword.Name, _contentClass.Name));
                }
            }
            InvalidateCache();
        }

        public void Clear()
        {
            RemoveRange(this);
        }

        public IContentClass ContentClass
        {
            get { return _contentClass; }
        }

        public IProject Project
        {
            get { return _contentClass.Project; }
        }

        public void Remove(IKeyword keyword)
        {
            RemoveRange(new[] {keyword});
        }

        public ISession Session
        {
            get { return _contentClass.Session; }
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

        private List<IKeyword> GetPreassignedKeywords()
        {
            const string LOAD_PREASSIGNED_KEYWORDS = @"<TEMPLATE guid=""{0}""><KEYWORDS action=""load""/></TEMPLATE>";
            XmlDocument xmlDoc = Project.ExecuteRQL(LOAD_PREASSIGNED_KEYWORDS.RQLFormat(_contentClass),
                                                    RqlType.SessionKeyInProject);

            IEnumerable<IKeyword> keywords = new List<Keyword>();
            foreach (XmlElement node in xmlDoc.GetElementsByTagName("CATEGORY"))
            {
                var curCategory = new Category(Project, node.GetGuid()) {Name = node.GetAttributeValue("value")};
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

        private void RemoveRange(IEnumerable<IKeyword> keywordsToRemove)
        {
            foreach (Keyword curKeyword in keywordsToRemove)
            {
                const string REMOVE_KEYWORD =
                    @"<TEMPLATE action=""unlink"" guid=""{0}""><KEYWORD guid=""{1}""/></TEMPLATE>";

                var xmlDoc = Project.ExecuteRQL(REMOVE_KEYWORD.RQLFormat(_contentClass, curKeyword),
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

        private static bool WasKeywordActionSuccessful(XmlNode node)
        {
            return node.InnerText.Contains("ok");
        }
    }
}