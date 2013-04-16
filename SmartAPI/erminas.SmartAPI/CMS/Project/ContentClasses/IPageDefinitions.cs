using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.ContentClasses
{
    public interface IPageDefinitions : IRDList<IPageDefinition>, IProjectObject
    {
        IContentClass ContentClass { get; }
    }

    internal class PageDefinitions : RDList<IPageDefinition>, IPageDefinitions
    {
        private readonly IContentClass _contentClass;

        internal PageDefinitions(IContentClass contentClass, Caching caching) : base(caching)
        {
            _contentClass = contentClass;
            RetrieveFunc = GetPageDefinitions;
        }

        public ISession Session { get { return _contentClass.Session; } }
        public IProject Project { get { return _contentClass.Project; } }
        public IContentClass ContentClass { get { return _contentClass; } }

        private List<IPageDefinition> GetPageDefinitions()
        {
            const string LOAD_PREASSIGNMENT = @"<TEMPLATELIST action=""load"" withpagedefinitions=""1""/>";

            var xmlDoc = Project.ExecuteRQL(LOAD_PREASSIGNMENT);
            const string PAGE_DEFINITIONS_XPATH = "//TEMPLATE[@guid='{0}']/PAGEDEFINITIONS/PAGEDEFINITION";
            var pageDefs = xmlDoc.SelectNodes(PAGE_DEFINITIONS_XPATH.RQLFormat(_contentClass));

            return
                (from XmlElement curPageDef in pageDefs select new PageDefinition(_contentClass, curPageDef))
                    .Cast<IPageDefinition>().ToList();
        }
    }
}
