using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.ContentClasses
{
    internal class TemplateVariants : NameIndexedRDList<ITemplateVariant>, ITemplateVariants
    {
        private readonly IContentClass _contentClass;

        public IContentClass ContentClass
        {
            get { return _contentClass; }
        }

        internal TemplateVariants(IContentClass contentClass, Caching caching) : base(caching)
        {
            _contentClass = contentClass;
            RetrieveFunc = GetTemplateVariants;
        }
        private List<ITemplateVariant> GetTemplateVariants()
        {
            const string LIST_CC_TEMPLATES =
                @"<PROJECT><TEMPLATE guid=""{0}""><TEMPLATEVARIANTS action=""list"" withstylesheets=""0""/></TEMPLATE></PROJECT>";
            var xmlDoc = Project.ExecuteRQL(LIST_CC_TEMPLATES.RQLFormat(_contentClass));
            var variants = xmlDoc.GetElementsByTagName("TEMPLATEVARIANT");
            return
                (from XmlElement curVariant in variants select (ITemplateVariant)new TemplateVariant(_contentClass, curVariant))
                    .ToList();
        }

        public ISession Session { get { return _contentClass.Session; } }
        public IProject Project { get { return _contentClass.Project; } }
    }

    public interface ITemplateVariants : IIndexedRDList<string, ITemplateVariant>, IProjectObject
    {
        IContentClass ContentClass { get; }
    }
}