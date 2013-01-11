using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.PageElements
{
    public abstract class AbstractKeywordAssignableMultiLinkElement : AbstractMultiLinkElement, IKeywordAssignable
    {
        protected AbstractKeywordAssignableMultiLinkElement(Project project, Guid guid, LanguageVariant languageVariant) : base(project, guid, languageVariant)
        {
            Init();
        }

        protected AbstractKeywordAssignableMultiLinkElement(Project project, XmlElement xmlElement)
            : base(project, xmlElement)
        {
            Init();
        }

        private void Init()
        {
            AssignedKeywords = new RDList<Keyword>(GetAssignedKeywords, Caching.Enabled) ;
        }

        private List<Keyword> GetAssignedKeywords()
        {
            const string LOAD_KEYWORDS = @"<LINK guid=""{0}""><KEYWORDS action=""load""/></LINK>";
            var xmlDoc = Project.ExecuteRQL(LOAD_KEYWORDS.RQLFormat(this), Project.RqlType.SessionKeyInProject);

            var keywords = xmlDoc.SelectNodes("/IODATA/CATEGORIES/CATEGORY/KEYWORDS/KEYWORD");
            return keywords == null ? new List<Keyword>() : (from XmlElement keyword in keywords select new Keyword(Project, keyword)).ToList();
        }

        public void AssignKeyword(Keyword keyword)
        {
            ExecuteAssignKeyword(keyword);

            ExecutePagebuilderLinkCleanup();

            AssignedKeywords.InvalidateCache();
        }

        private void ExecuteAssignKeyword(Keyword keyword)
        {
            const string ASSING_KEYWORD =
                @"<LINK guid=""{0}"" action=""assign"" allkeywords=""0""><KEYWORDS><KEYWORD guid=""{1}"" changed=""1"" /></KEYWORDS></LINK>";
            Project.ExecuteRQL(ASSING_KEYWORD.RQLFormat(this, keyword), Project.RqlType.SessionKeyInProject);
        }

        public void UnassignKeyword(Keyword keyword)
        {
            ExecuteUnassignKeyword(keyword);

            ExecutePagebuilderLinkCleanup();

            AssignedKeywords.InvalidateCache();
        }

        private void ExecutePagebuilderLinkCleanup()
        {
            const string PAGEBUILDER_LINK =
                @"<PAGEBUILDER><LINKING sessionkey=""{0}""><LINKS><LINK guid=""{1}""/></LINKS><PAGES><PAGE sessionkey=""{0}"" guid=""{2}""/></PAGES></LINKING></PAGEBUILDER>";
            Project.ExecuteRQL(PAGEBUILDER_LINK.RQLFormat(Project.Session.SessionKey, this, Page));
        }

        private void ExecuteUnassignKeyword(Keyword keyword)
        {
            const string UNASSIGN_KEYWORD =
                @"<LINK guid=""{0}"" action=""assign"" allkeywords=""0""><KEYWORDS><KEYWORD guid=""{1}"" delete=""1"" changed=""1"" /></KEYWORDS></LINK>";
            Project.ExecuteRQL(UNASSIGN_KEYWORD.RQLFormat(this, keyword), Project.RqlType.SessionKeyInProject);
        }

        public IRDList<Keyword> AssignedKeywords { get; private set; }
    }
}
