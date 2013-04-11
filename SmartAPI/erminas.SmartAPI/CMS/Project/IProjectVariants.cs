using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project
{
    public interface IProjectVariants : IIndexedRDList<string, IProjectVariant>, IProjectObject
    {
        /// <summary>
        ///     Get the project variant used as display format (preview).
        /// </summary>
        IProjectVariant DisplayFormat { get; }
    }

    internal class ProjectVariants : NameIndexedRDList<IProjectVariant>, IProjectVariants{
        private readonly IProject _project;

        internal ProjectVariants(IProject project, Caching caching) : base(caching)
        {
            _project = project;
            RetrieveFunc = GetProjectVariants;
        }

        public IProjectVariant DisplayFormat
        {
            get { return this.FirstOrDefault(x => x.IsUsedAsDisplayFormat); }
        }

        private List<IProjectVariant> GetProjectVariants()
        {
            const string LIST_PROJECT_VARIANTS = @"<PROJECT><PROJECTVARIANTS action=""list""/></PROJECT>";
            XmlDocument xmlDoc = Project.ExecuteRQL(LIST_PROJECT_VARIANTS);
            var variants = xmlDoc.GetElementsByTagName("PROJECTVARIANTS")[0] as XmlElement;
            if (variants == null)
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not load project variants of project {0}", this));
            }
            return (from XmlElement variant in variants.GetElementsByTagName("PROJECTVARIANT")
                    select (IProjectVariant)new ProjectVariant(Project, variant)).ToList();
        }

        public Session Session { get { return _project.Session; } }
        public IProject Project { get { return _project; } }
    }
}
